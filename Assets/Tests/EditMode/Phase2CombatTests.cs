using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class Phase2CombatTests
{
    private sealed class MemorySave : ISkillSave
    {
        private readonly Dictionary<string, int> values = new Dictionary<string, int>();
        public int GetInt(string key, int fallback = 0) => values.TryGetValue(key, out int value) ? value : fallback;
        public void SetInt(string key, int value) => values[key] = value;
        public void DeleteKey(string key) => values.Remove(key);
        public void Save() { }
    }

    private static SkillProgress Progress(bool owned = false)
    {
        var save = new MemorySave();
        return new SkillProgress(Resources.Load<SkillTree>("Skills/SkillTree"), save, new SkillWallet(save), () => owned);
    }

    [Test]
    public void New_skills_validate_and_existing_or_legacy_saves_grant_none()
    {
        var tree = Resources.Load<SkillTree>("Skills/SkillTree");
        Assert.That(tree.Validate(), Is.Empty);
        var additions = tree.definitions.Where(item => item.levels.All(level => level.legacyId == -1)).ToArray();
        Assert.That(additions.Length, Is.EqualTo(7));
        var save = new MemorySave();
        for (int id = 0; id < 18; id++) save.SetInt("Skill." + id, 1);
        save.SetInt("Skill.-1", 1); // No non-legacy skill may accidentally inherit the sentinel key.
        var progress = new SkillProgress(tree, save, new SkillWallet(save));
        foreach (var definition in additions)
        {
            Assert.That(progress.GetLevel(definition.id), Is.Zero, definition.id);
            Assert.That(definition.MaxLevel, Is.EqualTo(1));
        }
    }

    [TestCase(SkillStat.ProjectilePierce, 0f, 2f)]
    [TestCase(SkillStat.ExtraProjectiles, 0f, 1f)]
    [TestCase(SkillStat.CriticalChance, 0f, .2f)]
    [TestCase(SkillStat.HammerDamage, 0f, 1f)]
    [TestCase(SkillStat.LightningRange, 1f, 1.5f)]
    [TestCase(SkillStat.ShieldPulseDamage, 0f, 2f)]
    [TestCase(SkillStat.PickupMagnetRadius, 0f, 4f)]
    public void New_modifiers_have_safe_defaults_and_aggregate_from_the_shared_tree(SkillStat stat, float baseline, float upgraded)
    {
        Assert.That(Progress().GetModifier(stat), Is.EqualTo(baseline));
        Assert.That(Progress(true).GetModifier(stat), Is.EqualTo(upgraded));
    }

    [Test]
    public void Every_new_skill_requires_prerequisites_spends_displayed_cost_and_survives_reload()
    {
        var tree = Resources.Load<SkillTree>("Skills/SkillTree");
        foreach (var definition in tree.definitions.Where(item => item.levels.All(level => level.legacyId == -1)))
        {
            var save = new MemorySave();
            save.SetInt("Economy.Stardust", 100000);
            save.SetInt("Economy.Metal", 100000);
            var progress = new SkillProgress(tree, save, new SkillWallet(save));
            Assert.That(progress.CanPurchase(definition.id), Is.False, definition.id);
            foreach (string prerequisite in definition.prerequisiteIds)
                save.SetInt(SkillProgress.Key(prerequisite), tree.definitions.Single(item => item.id == prerequisite).MaxLevel);
            progress.Load();
            Assert.That(progress.TryPurchase(definition.id), Is.True, definition.id);
            Assert.That(save.GetInt("Economy.Stardust"), Is.EqualTo(100000 - definition.levels[0].costStardust));
            Assert.That(save.GetInt("Economy.Metal"), Is.EqualTo(100000 - definition.levels[0].costMetal));
            Assert.That(progress.TryPurchase(definition.id), Is.False);
            Assert.That(new SkillProgress(tree, save, new SkillWallet(save)).GetLevel(definition.id), Is.EqualTo(1));
        }
    }

    [TestCase(1, 0f, 0f, 1)] [TestCase(1, .2f, 0f, 2)]
    [TestCase(1, .2f, .1999f, 2)] [TestCase(1, .2f, .2f, 1)]
    [TestCase(1, 2f, .99f, 2)] [TestCase(1, -1f, 0f, 1)]
    [TestCase(0, 1f, 0f, 0)] [TestCase(int.MaxValue, 1f, 0f, int.MaxValue)]
    public void Critical_damage_obeys_roll_boundary_and_saturates(int damage, float chance, float roll, int expected)
        => Assert.That(CombatRules.Damage(damage, chance, roll), Is.EqualTo(expected));

    [Test]
    public void Piercing_counts_distinct_targets_and_never_exceeds_its_budget()
    {
        var hits = new ProjectileHits(2);
        Assert.That(hits.TryHit(10), Is.True);
        Assert.That(hits.TryHit(10), Is.False);
        Assert.That(hits.TryHit(11), Is.True);
        Assert.That(hits.Exhausted, Is.False);
        Assert.That(hits.TryHit(12), Is.True);
        Assert.That(hits.Exhausted, Is.True);
        Assert.That(hits.TryHit(13), Is.False);
        var baseline = new ProjectileHits(-3);
        Assert.That(baseline.TryHit(1), Is.True);
        Assert.That(baseline.TryHit(2), Is.False);
    }

    [Test]
    public void Projectile_preparation_uses_shared_skills_and_twins_cannot_recurse()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "Assembly-CSharp");
        var service = assembly.GetType("SkillService", true);
        var setProgress = service.GetMethod("SetProgressForTests");
        var progress = Progress(true);
        object previous = setProgress.Invoke(null, new object[] { progress });
        var first = new GameObject("Shot preparation test");
        var second = new GameObject("Twin preparation test");
        first.SetActive(false);
        second.SetActive(false);
        try
        {
            var shotType = assembly.GetType("ProjectileController", true);
            var shot = first.AddComponent(shotType);
            var twin = second.AddComponent(shotType);
            object shared = service.GetProperty("Current").GetValue(null);
            Assert.That(shared, Is.SameAs(progress));
            var prepare = shotType.GetMethod("PrepareShot");
            Assert.That(prepare.Invoke(shot, new[] { shared, (object)false }), Is.EqualTo(true));
            Assert.That(prepare.Invoke(twin, new[] { shared, (object)true }), Is.EqualTo(false));
            Assert.That(prepare.Invoke(shot, new[] { shared, (object)false }), Is.EqualTo(false));
            Assert.That(shotType.GetProperty("EffectiveSpeed").GetValue(shot), Is.EqualTo(65f));
            Assert.That(shotType.GetMethod("DamageForRoll").Invoke(shot, new object[] { 0f }), Is.EqualTo(2));
            Assert.That(shotType.GetMethod("DamageForRoll").Invoke(shot, new object[] { .2f }), Is.EqualTo(1));
        }
        finally
        {
            setProgress.Invoke(null, new[] { previous });
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }
    }

    [Test]
    public void Active_shield_absorption_blocks_repeat_hits_even_after_last_charge()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "Assembly-CSharp");
        var shieldType = assembly.GetType("ShieldController", true);
        var gameObject = new GameObject("Shield window test");
        gameObject.SetActive(false);
        try
        {
            var shield = gameObject.AddComponent(shieldType);
            var state = shieldType.GetMethod("SetStateForTests");
            var absorb = shieldType.GetMethod("TryAbsorbHit");
            state.Invoke(shield, new object[] { 0, true });
            Assert.That(absorb.Invoke(shield, null), Is.EqualTo(true));
            Assert.That(shieldType.GetProperty("Charges").GetValue(shield), Is.EqualTo(0));
            state.Invoke(shield, new object[] { 2, true });
            Assert.That(absorb.Invoke(shield, null), Is.EqualTo(true));
            Assert.That(shieldType.GetProperty("Charges").GetValue(shield), Is.EqualTo(2));
            state.Invoke(shield, new object[] { 0, false });
            Assert.That(absorb.Invoke(shield, null), Is.EqualTo(false));
        }
        finally { UnityEngine.Object.DestroyImmediate(gameObject); }
    }
}
