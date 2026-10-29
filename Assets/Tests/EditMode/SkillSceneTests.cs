using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillSceneTests
{
    [TestCase("Assets/Scenes/Game.unity")]
    [TestCase("Assets/Scenes/HyperdriveShop.unity")]
    public void Scene_loads_without_missing_scripts_or_serialized_references(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects()) CheckHierarchy(root, path);
        Debug.Log("[Phase1 QA] Scene checked: " + path);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    [Test]
    public void Every_prefab_loads_without_missing_scripts_or_serialized_references()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            CheckHierarchy(prefab, path);
            count++;
        }
        Debug.Log("[Phase1 QA] Prefabs checked: " + count);
    }

    [Test]
    public void Shop_rows_use_shared_progress_without_a_GameServices_component()
    {
        // Reflection avoids introducing an asmdef over the entire legacy Assembly-CSharp.
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "Assembly-CSharp");
        var serviceType = assembly.GetType("SkillService", true);
        var shopType = assembly.GetType("ShopController", true);
        var field = serviceType.GetField("progress", BindingFlags.NonPublic | BindingFlags.Static);
        object previous = field.GetValue(null);
        var memory = new System.Collections.Generic.Dictionary<string, int>();
        var save = new ShopSave(memory);
        save.SetInt("Economy.Stardust", 100000);
        save.SetInt("Economy.Metal", 100000);
        var progress = new SkillProgress(Resources.Load<SkillTree>("Skills/SkillTree"), save, new SkillWallet(save));
        var gameObject = new GameObject("Phase1 Shop Test");
        gameObject.SetActive(false);
        try
        {
            field.SetValue(null, progress);
            var shop = gameObject.AddComponent(shopType);
            var stateMethod = shopType.GetMethod("GetNodeState", BindingFlags.NonPublic | BindingFlags.Instance);
            var purchaseMethod = shopType.GetMethod("TryPurchase", BindingFlags.NonPublic | BindingFlags.Instance);
            var definitions = Resources.Load<SkillTree>("Skills/SkillTree").definitions;
            var fire = definitions.Single(item => item.id == "autoshooter.fire_rate");
            Assert.That(stateMethod.Invoke(shop, new object[] { fire, 1 }).ToString(), Is.EqualTo("Available"));
            Assert.That(stateMethod.Invoke(shop, new object[] { fire, 2 }).ToString(), Is.EqualTo("Locked"));
            purchaseMethod.Invoke(shop, new object[] { fire });
            Assert.That(progress.GetLevel(fire.id), Is.EqualTo(1));
            Assert.That(stateMethod.Invoke(shop, new object[] { fire, 1 }).ToString(), Is.EqualTo("Owned"));
            Assert.That(stateMethod.Invoke(shop, new object[] { fire, 2 }).ToString(), Is.EqualTo("Available"));
            // The formerly mismatched legacy IDs 12/13/14/15 must buy their displayed definitions.
            foreach (var definition in definitions)
                while (progress.GetLevel(definition.id) < definition.MaxLevel)
                {
                    int before = progress.GetLevel(definition.id);
                    purchaseMethod.Invoke(shop, new object[] { definition });
                    Assert.That(progress.GetLevel(definition.id), Is.EqualTo(before + 1));
                }
            Assert.That(progress.GetLevel("autoshooter.target_priority"), Is.EqualTo(1));
            Assert.That(progress.GetLevel("hammer.lightning"), Is.EqualTo(3));
            Assert.That(progress.GetLevel("ship.shield"), Is.EqualTo(2));
        }
        finally
        {
            field.SetValue(null, previous);
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static void CheckHierarchy(GameObject root, string path)
    {
        foreach (var component in root.GetComponentsInChildren<Component>(true))
        {
            Assert.That(component, Is.Not.Null, "Missing script: " + path);
            var serialized = new SerializedObject(component);
            var property = serialized.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                    Assert.Fail("Missing reference: " + path + " " + component.name + "." + property.propertyPath);
        }
    }

    private sealed class ShopSave : ISkillSave
    {
        private readonly System.Collections.Generic.Dictionary<string, int> values;
        public ShopSave(System.Collections.Generic.Dictionary<string, int> values) { this.values = values; }
        public int GetInt(string key, int fallback = 0) => values.TryGetValue(key, out int value) ? value : fallback;
        public void SetInt(string key, int value) => values[key] = value;
        public void DeleteKey(string key) => values.Remove(key);
        public void Save() { }
    }
}
