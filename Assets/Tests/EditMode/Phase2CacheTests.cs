using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class Phase2CacheTests
{
    private SkillTree tree;
    private SkillDefinition first, second, third;

    [SetUp]
    public void SetUp()
    {
        tree = ScriptableObject.CreateInstance<SkillTree>();
        first = ScriptableObject.CreateInstance<SkillDefinition>();
        second = ScriptableObject.CreateInstance<SkillDefinition>();
        third = ScriptableObject.CreateInstance<SkillDefinition>();
        first.tier = 2;
        second.tier = third.tier = 1;
        tree.definitions.AddRange(new[] { first, second, third });
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(tree);
        UnityEngine.Object.DestroyImmediate(first);
        UnityEngine.Object.DestroyImmediate(second);
        UnityEngine.Object.DestroyImmediate(third);
    }

    [Test]
    public void Ordered_definitions_reuses_a_read_only_stable_tier_view()
    {
        var ordered = tree.OrderedDefinitions();
        Assert.That(ordered, Is.EqualTo(new[] { second, third, first }));
        Assert.That(tree.OrderedDefinitions(), Is.SameAs(ordered));
        Assert.That(ordered, Is.Not.InstanceOf<List<SkillDefinition>>());
        Assert.Throws<NotSupportedException>(() => ((IList<SkillDefinition>)ordered)[0] = first);
        Assert.That(tree.definitions, Is.EqualTo(new[] { first, second, third }));
    }

    [Test]
    public void Ordered_definitions_refreshes_after_count_or_list_assignment_changes()
    {
        var original = tree.OrderedDefinitions();
        tree.definitions.Remove(third);
        var removed = tree.OrderedDefinitions();
        Assert.That(removed, Is.Not.SameAs(original));
        Assert.That(removed, Is.EqualTo(new[] { second, first }));
        tree.definitions = new List<SkillDefinition> { third, first };
        var assigned = tree.OrderedDefinitions();
        Assert.That(assigned, Is.Not.SameAs(removed));
        Assert.That(assigned, Is.EqualTo(new[] { third, first }));
        Assert.That(tree.OrderedDefinitions(), Is.SameAs(assigned));
    }

    [Test]
    public void Ordered_definitions_detects_same_count_reordering_replacement_and_tier_edits()
    {
        var original = tree.OrderedDefinitions();
        tree.definitions.Reverse();
        var reversed = tree.OrderedDefinitions();
        Assert.That(reversed, Is.Not.SameAs(original));
        Assert.That(reversed, Is.EqualTo(new[] { third, second, first }));
        tree.definitions[1] = first;
        var replaced = tree.OrderedDefinitions();
        Assert.That(replaced, Is.EqualTo(new[] { third, first, first }));
        Assert.That(replaced, Is.Not.SameAs(reversed));
        first.tier = 0;
        var retiered = tree.OrderedDefinitions();
        Assert.That(retiered, Is.Not.SameAs(replaced));
        Assert.That(retiered, Is.EqualTo(new[] { first, first, third }));
    }

    [TestCase("OnValidate")]
    [TestCase("OnEnable")]
    public void Ordered_definitions_rebuilds_after_authoring_or_enable_invalidation(string callback)
    {
        var original = tree.OrderedDefinitions();
        typeof(SkillTree).GetMethod(callback, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tree, null);
        var refreshed = tree.OrderedDefinitions();
        Assert.That(refreshed, Is.Not.SameAs(original));
        Assert.That(refreshed, Is.EqualTo(original));
        Assert.That(tree.OrderedDefinitions(), Is.SameAs(refreshed));
    }

    [Test]
    public void Skill_service_definitions_returns_the_same_cached_instance()
    {
        var property = RuntimeType("SkillService").GetProperty("Definitions");
        var definitions = property.GetValue(null);
        Assert.That(property.GetValue(null), Is.SameAs(definitions));
        Assert.That(definitions, Is.SameAs(Resources.Load<SkillTree>("Skills/SkillTree").OrderedDefinitions()));
    }

    private static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(name)).First(type => type != null);

    private sealed class WaveFixture : IDisposable
    {
        private readonly GameObject host = new GameObject("Wave status cache test");
        private readonly Type managerType = RuntimeType("WaveManager");
        private readonly ScriptableObject level = ScriptableObject.CreateInstance(RuntimeType("LevelDefinition"));
        private readonly object wave = Activator.CreateInstance(RuntimeType("WaveDefinition"));
        private readonly Component manager;

        public WaveFixture()
        {
            host.SetActive(false);
            manager = host.AddComponent(managerType);
            SetSector("Asteroid Belt");
            wave.GetType().GetField("level").SetValue(wave, level);
            SetWaveValue("stardustReward", 52);
            SetWaveValue("metalReward", 8);
        }

        public string Text => (string)managerType.GetProperty("StatusText").GetValue(manager);
        public void SetSector(string name) => level.GetType().GetField("displayName").SetValue(level, name);
        public void SetNumber(int number) => managerType.GetField("currentWave").SetValue(manager, number);
        public void SetWaveValue(string field, object value)
        {
            wave.GetType().GetField(field).SetValue(wave, value);
            managerType.GetProperty("Current").SetValue(manager, wave);
        }
        public string Update(bool regroup, float remaining)
        {
            managerType.GetMethod("UpdateStatusText", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(manager, new object[] { regroup, remaining });
            return Text;
        }
        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Object.DestroyImmediate(level);
        }
    }

    [Test]
    public void Active_wave_status_preserves_format_and_reuses_text_until_the_displayed_second_changes()
    {
        using (var wave = new WaveFixture())
        {
            Assert.That(wave.Text, Is.EqualTo("Prepare for launch"));
            string text = wave.Update(false, 8.9f);
            Assert.That(text, Is.EqualTo("Asteroid Belt  |  Wave 1  |  9s"));
            Assert.That(wave.Update(false, 8.01f), Is.SameAs(text));
            Assert.That(wave.Update(false, 8f), Is.EqualTo("Asteroid Belt  |  Wave 1  |  8s"));
            Assert.That(wave.Text, Is.Not.SameAs(text));
        }
    }

    [Test]
    public void Active_wave_status_refreshes_for_wave_sector_and_elite_but_not_hidden_rewards()
    {
        using (var wave = new WaveFixture())
        {
            string original = wave.Update(false, 9f);
            wave.SetWaveValue("stardustReward", 99);
            Assert.That(wave.Update(false, 9f), Is.SameAs(original));
            wave.SetNumber(5);
            Assert.That(wave.Update(false, 9f), Is.EqualTo("Asteroid Belt  |  Wave 5  |  9s"));
            wave.SetWaveValue("elite", true);
            Assert.That(wave.Update(false, 9f), Is.EqualTo("Asteroid Belt  |  Wave 5 ELITE  |  9s"));
            wave.SetSector("Solar Storm");
            Assert.That(wave.Update(false, 9f), Is.EqualTo("Solar Storm  |  Wave 5 ELITE  |  9s"));
        }
    }

    [Test]
    public void Regroup_status_preserves_format_reuses_text_and_refreshes_for_rewards_and_state()
    {
        using (var wave = new WaveFixture())
        {
            wave.Update(false, 3f);
            string regroup = wave.Update(true, 2.9f);
            Assert.That(regroup, Is.EqualTo("Wave 1 survived: +52 Stardust / +8 Metal  |  Regroup 3s"));
            Assert.That(wave.Update(true, 2.01f), Is.SameAs(regroup));
            wave.SetSector("Not displayed during regroup");
            Assert.That(wave.Update(true, 2.5f), Is.SameAs(regroup));
            wave.SetWaveValue("metalReward", 10);
            Assert.That(wave.Update(true, 2.5f), Is.EqualTo("Wave 1 survived: +52 Stardust / +10 Metal  |  Regroup 3s"));
            wave.SetWaveValue("stardustReward", 60);
            Assert.That(wave.Update(true, 2f), Is.EqualTo("Wave 1 survived: +60 Stardust / +10 Metal  |  Regroup 2s"));
            Assert.That(wave.Update(false, 2f), Is.EqualTo("Not displayed during regroup  |  Wave 1  |  2s"));
        }
    }
}
