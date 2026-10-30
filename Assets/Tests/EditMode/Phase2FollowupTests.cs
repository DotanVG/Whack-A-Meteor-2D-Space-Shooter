using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Phase2FollowupTests
{
    private static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Single(assembly => assembly.GetName().Name == "Assembly-CSharp").GetType(name, true);
    private static object Call(object target, string method, params object[] arguments) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
    private static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private readonly List<GameObject> objects = new List<GameObject>();
    private Scene scene;
    private HashSet<GameObject> existingFeedback;

    [SetUp]
    public void SetUp()
    {
        scene = SceneManager.CreateScene("Followup isolated physics", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        existingFeedback = new HashSet<GameObject>(UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
            .Where(item => item.GetType().Name == "CombatFeedback").Select(item => item.gameObject));
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var host in objects) if (host) UnityEngine.Object.DestroyImmediate(host);
        objects.Clear();
        foreach (var feedback in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
            .Where(item => item.GetType().Name == "CombatFeedback" && !existingFeedback.Contains(item.gameObject)))
            UnityEngine.Object.DestroyImmediate(feedback.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
    }

    private GameObject Host(string name, Vector2 position)
    {
        var host = new GameObject(name);
        objects.Add(host);
        SceneManager.MoveGameObjectToScene(host, scene);
        host.transform.position = position;
        return host;
    }

    private Component Shot(float criticalChance = 0f)
    {
        var host = Host("QA shot", Vector2.zero);
        var collider = host.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(.16f, .4f);
        collider.isTrigger = true;
        var shot = host.AddComponent(RuntimeType("ProjectileController"));
        Call(shot, "Awake");
        host.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        host.GetComponent<Rigidbody2D>().velocity = Vector2.up * 65f;
        Set(shot, "hits", new ProjectileHits(2)); // Leave one slot so EditMode does not need deferred Destroy.
        Set(shot, "criticalChance", criticalChance);
        return shot;
    }

    private Component Enemy(Vector2 position)
    {
        var host = Host("QA thin enemy", position);
        host.AddComponent<BoxCollider2D>().size = new Vector2(.08f, .08f);
        var enemy = host.AddComponent(RuntimeType("EnemyHealth"));
        enemy.GetType().GetMethod("Initialize").Invoke(enemy, new object[] { 100 });
        return enemy;
    }

    [Test]
    public void Projectile_prefab_instances_enable_continuous_without_changing_authored_speed()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Projectile.prefab");
        var host = UnityEngine.Object.Instantiate(prefab);
        objects.Add(host);
        SceneManager.MoveGameObjectToScene(host, scene);
        var type = RuntimeType("ProjectileController");
        var shot = host.GetComponent(type);
        float authoredSpeed = (float)type.GetField("speed").GetValue(prefab.GetComponent(type));
        Call(shot, "Awake");
        Assert.That(host.GetComponent<Rigidbody2D>().collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));
        host.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        Call(shot, "OnEnable");
        Assert.That(host.GetComponent<Rigidbody2D>().collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));
        Assert.That(type.GetField("speed").GetValue(shot), Is.EqualTo(authoredSpeed));
    }

    [Test]
    public void Fast_trigger_sweep_hits_thin_targets_and_callback_or_repeat_sweep_never_hits_twice()
    {
        var shot = Shot();
        var near = Enemy(new Vector2(0f, .55f));
        var far = Enemy(new Vector2(0f, 1.05f));
        var outside = Enemy(new Vector2(0f, 2f));
        // A child collider represents the same damage target and must not spend another hit.
        var child = Host("QA compound collider", new Vector2(0f, .6f));
        child.transform.SetParent(near.transform, true);
        child.AddComponent<BoxCollider2D>().size = new Vector2(.08f, .08f);
        scene.GetPhysicsScene2D().Simulate(.0001f);
        Call(shot, "FixedUpdate");
        Call(shot, "OnTriggerEnter2D", near.GetComponent<Collider2D>());
        Call(shot, "FixedUpdate");
        Assert.That(near.GetType().GetProperty("Health").GetValue(near), Is.EqualTo(99));
        Assert.That(far.GetType().GetProperty("Health").GetValue(far), Is.EqualTo(99));
        Assert.That(outside.GetType().GetProperty("Health").GetValue(outside), Is.EqualTo(100));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Projectile_hit_emits_exactly_one_correct_damage_label(bool meteor, bool critical)
    {
        var shot = Shot(critical ? 1f : 0f);
        Component target;
        if (meteor)
        {
            var host = Host("QA armored meteor", Vector2.up);
            host.AddComponent<BoxCollider2D>();
            target = host.AddComponent(RuntimeType("MeteorSplit"));
            // Keep the target alive to isolate label/damage behavior from loot and splits.
            var healthField = target.GetType().GetField("durability", BindingFlags.Instance | BindingFlags.NonPublic);
            healthField.SetValue(target, Activator.CreateInstance(healthField.FieldType, new object[] { 100 }));
        }
        else target = Enemy(Vector2.up);
        Call(shot, "OnTriggerEnter2D", target.GetComponent<Collider2D>());
        var feedbackType = RuntimeType("CombatFeedback");
        var hub = feedbackType.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        var labels = (IList)feedbackType.GetField("labels", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hub);
        Assert.That(labels.Count, Is.EqualTo(1));
        Assert.That(labels[0].GetType().GetField("text").GetValue(labels[0]), Is.EqualTo(critical ? "CRIT 2" : "1"));
        Assert.That(labels[0].GetType().GetField("color").GetValue(labels[0]), Is.EqualTo(critical ? Color.yellow : Color.white));
        Assert.That(target.GetType().GetProperty("Health").GetValue(target), Is.EqualTo(critical ? 98 : 99));
    }

    [Test]
    public void Magnet_without_handler_returns_to_normal_drift_path_instead_of_pulling()
    {
        var pickup = Host("QA pickup", Vector2.right).AddComponent(RuntimeType("PowerupBase"));
        var player = Host("QA player without handler", Vector2.zero);
        Set(pickup, "_player", player.transform);
        Set(pickup, "_magnetRadius", 4f);
        Vector3 before = pickup.transform.position;
        Assert.That(Call(pickup, "TryMagnet", .1f), Is.EqualTo(false));
        Assert.That(pickup.transform.position, Is.EqualTo(before));
        var handler = player.AddComponent(RuntimeType("PlayerPowerupHandler"));
        Set(pickup, "_playerHandler", handler);
        Assert.That(Call(pickup, "TryMagnet", .01f), Is.EqualTo(true));
        Assert.That(pickup.transform.position.x, Is.LessThan(before.x));
    }

    [Test]
    public void Collected_pickup_cannot_pull_or_apply_its_effect_again()
    {
        var pickup = Host("QA collected pickup", Vector2.zero).AddComponent(RuntimeType("PowerupBase"));
        var player = Host("QA player", Vector2.zero);
        var handler = player.AddComponent(RuntimeType("PlayerPowerupHandler"));
        Set(pickup, "_player", player.transform);
        Set(pickup, "_playerHandler", handler);
        Set(pickup, "_magnetRadius", 4f);
        Set(pickup, "_collected", true);
        pickup.GetType().GetField("type").SetValue(pickup, Enum.Parse(RuntimeType("PowerupType"), "SpeedBoost"));
        Call(pickup, "Collect", handler);
        Assert.That(Call(pickup, "TryMagnet", .1f), Is.EqualTo(false));
        Assert.That(handler.GetType().GetProperty("IsSpeedBoosted").GetValue(handler), Is.EqualTo(false));
    }

    [Test]
    public void Shield_pulse_excludes_absorbed_root_and_child_but_keeps_nearby_hazards()
    {
        var absorbed = Host("QA absorbed hazard", Vector2.zero);
        var child = Host("QA hazard collider", Vector2.zero);
        child.transform.SetParent(absorbed.transform);
        var nearby = Host("QA nearby hazard", Vector2.right);
        var filter = RuntimeType("ShieldController").GetMethod("IsPulseTarget");
        Assert.That(filter.Invoke(null, new object[] { absorbed, absorbed }), Is.EqualTo(false));
        Assert.That(filter.Invoke(null, new object[] { absorbed, child }), Is.EqualTo(false));
        Assert.That(filter.Invoke(null, new object[] { child, absorbed }), Is.EqualTo(false));
        Assert.That(filter.Invoke(null, new object[] { nearby, child }), Is.EqualTo(true));
        Assert.That(filter.Invoke(null, new object[] { nearby, null }), Is.EqualTo(true));
        Assert.That(filter.Invoke(null, new object[] { null, absorbed }), Is.EqualTo(false));
    }
}
