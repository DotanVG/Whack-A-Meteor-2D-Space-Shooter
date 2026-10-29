using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class EncounterTests
{
    private EncounterCatalog catalogue;

    [SetUp]
    public void LoadCatalogue() { catalogue = Resources.Load<EncounterCatalog>("Encounters/Catalogue"); Assert.That(catalogue, Is.Not.Null); }

    [Test]
    public void Committed_encounter_data_has_three_themes_three_meteors_and_two_behaviours()
    {
        Assert.That(catalogue.Validate(), Is.Empty);
        Assert.That(catalogue.levels.Select(item => item.id).Distinct().Count(), Is.EqualTo(3));
        Assert.That(catalogue.meteors.Count, Is.EqualTo(3));
        Assert.That(catalogue.enemies.Select(item => item.behaviour).Distinct().Count(), Is.EqualTo(2));
        Assert.That(catalogue.meteors.Single(item => item.id == "swift_shard").prefabSize, Is.EqualTo(3));
        Assert.That(catalogue.meteors.Single(item => item.id == "armored_ore").metalReward, Is.GreaterThan(0));
    }

    [Test]
    public void Difficulty_and_rewards_are_monotonic_across_all_theme_boundaries_and_caps()
    {
        var previous = catalogue.Evaluate(1);
        for (int wave = 2; wave <= 1000; wave++)
        {
            var next = catalogue.Evaluate(wave);
            Assert.That(next.meteorInterval, Is.LessThanOrEqualTo(previous.meteorInterval), "Wave " + wave);
            Assert.That(next.enemyInterval, Is.LessThanOrEqualTo(previous.enemyInterval));
            Assert.That(next.speedMultiplier, Is.GreaterThanOrEqualTo(previous.speedMultiplier));
            Assert.That(next.healthBonus, Is.GreaterThanOrEqualTo(previous.healthBonus));
            Assert.That(next.stardustReward, Is.GreaterThanOrEqualTo(previous.stardustReward));
            Assert.That(next.metalReward, Is.GreaterThanOrEqualTo(previous.metalReward));
            Assert.That(next.meteorInterval, Is.GreaterThanOrEqualTo(catalogue.minimumMeteorInterval));
            Assert.That(next.speedMultiplier, Is.LessThanOrEqualTo(catalogue.maximumSpeedMultiplier));
            Assert.That(next.healthBonus, Is.LessThanOrEqualTo(catalogue.maximumHealthBonus));
            previous = next;
        }
    }

    [Test]
    public void Milestones_and_themes_repeat_without_resetting_difficulty()
    {
        Assert.That(catalogue.Evaluate(1).level.id, Is.EqualTo("copper_belt"));
        Assert.That(catalogue.Evaluate(6).level.id, Is.EqualTo("raider_crossing"));
        Assert.That(catalogue.Evaluate(11).level.id, Is.EqualTo("iron_storm"));
        Assert.That(catalogue.Evaluate(16).level.id, Is.EqualTo("copper_belt"));
        for (int wave = 1; wave <= 30; wave++) Assert.That(catalogue.Evaluate(wave).elite, Is.EqualTo(wave % 5 == 0));
        Assert.That(catalogue.Evaluate(5).eliteHealth, Is.EqualTo(25));
        Assert.That(catalogue.Evaluate(10).eliteHealth, Is.EqualTo(30));
        Assert.That(catalogue.Evaluate(1).enemiesEnabled, Is.False);
        Assert.That(catalogue.Evaluate(2).enemiesEnabled, Is.True);
    }

    [Test]
    public void Survival_reward_formula_is_explicit_and_bounded()
    {
        Assert.That(catalogue.Evaluate(1).stardustReward, Is.EqualTo(52));
        Assert.That(catalogue.Evaluate(1).metalReward, Is.EqualTo(8));
        Assert.That(catalogue.Evaluate(5).stardustReward, Is.EqualTo(100));
        Assert.That(catalogue.Evaluate(1000).stardustReward, Is.EqualTo(catalogue.Evaluate(100).stardustReward));
        Assert.That(catalogue.Evaluate(1000).metalReward, Is.EqualTo(catalogue.Evaluate(100).metalReward));
        Assert.That(catalogue.Evaluate(0).number, Is.EqualTo(1));
        Assert.That(catalogue.Evaluate(int.MaxValue).speedMultiplier, Is.EqualTo(catalogue.maximumSpeedMultiplier));
    }

    [Test]
    public void Validation_rejects_bad_curve_and_missing_profiles_without_mutating_committed_assets()
    {
        var temporary = UnityEngine.Object.Instantiate(catalogue);
        try
        {
            temporary.baseMeteorInterval = float.NaN;
            temporary.wavesPerLevel = 0;
            temporary.meteors.Clear();
            Assert.That(temporary.Validate(), Is.Not.Empty);
            Assert.That(catalogue.Validate(), Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(temporary); }
    }

    [Test]
    public void Durability_ignores_invalid_damage_and_reports_a_kill_only_once()
    {
        var health = new Durability(5);
        Assert.That(health.Apply(0), Is.False);
        Assert.That(health.Apply(-2), Is.False);
        Assert.That(health.Current, Is.EqualTo(5));
        Assert.That(health.Apply(1), Is.False);
        Assert.That(health.Apply(2), Is.False);
        Assert.That(health.Current, Is.EqualTo(2));
        Assert.That(health.Apply(int.MaxValue), Is.True);
        Assert.That(health.Apply(1), Is.False);
        Assert.That(health.Current, Is.Zero);
    }

    [Test]
    public void Spawn_configuration_survives_enemy_Start_and_boss_Start_order()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "Assembly-CSharp");
        var enemyType = assembly.GetType("EnemyHealth", true);
        var bossType = assembly.GetType("BossController", true);
        var go = new GameObject("Encounter initialization test");
        go.SetActive(false);
        try
        {
            var health = go.AddComponent(enemyType);
            var boss = go.AddComponent(bossType);
            bossType.GetMethod("Configure").Invoke(boss, new object[] { catalogue, catalogue.Evaluate(5) });
            enemyType.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(health, null);
            bossType.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(boss, null);
            Assert.That(enemyType.GetProperty("Health").GetValue(health), Is.EqualTo(25));
            Assert.That(enemyType.GetField("maxHealth").GetValue(health), Is.EqualTo(25));
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    [Test]
    public void Both_weapon_paths_respect_armored_meteor_health()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "Assembly-CSharp");
        var type = assembly.GetType("MeteorSplit", true);
        var go = new GameObject("Encounter armor test");
        go.SetActive(false);
        var previous = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Where(item => item.GetType().Name == "CombatFeedback").ToArray();
        try
        {
            var meteor = go.AddComponent(type);
            type.GetMethod("Configure").Invoke(meteor, new object[] { catalogue.meteors.Single(item => item.id == "armored_ore"), 0 });
            type.GetMethod("OnProjectileHit").Invoke(meteor, new object[] { 1 });
            type.GetMethod("OnHammerHit").Invoke(meteor, new object[] { 2 });
            Assert.That(type.GetProperty("Health").GetValue(meteor), Is.EqualTo(2));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            foreach (var feedback in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Where(item => item.GetType().Name == "CombatFeedback" && !previous.Contains(item)))
                UnityEngine.Object.DestroyImmediate(feedback.gameObject);
        }
    }
}
