using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class SkillProgressTests
{
    [Serializable]
    public class LegacyTable { public LegacyRow[] rows; }
    [Serializable]
    public class LegacyRow
    {
        public int legacyId;
        public string displayName;
        public string label;
        public int costStardust;
        public int costMetal;
        public int prereqId;
        public int branch;
        public string newId;
        public int level;
    }

    private sealed class MemorySave : ISkillSave
    {
        public readonly Dictionary<string, int> values = new Dictionary<string, int>();
        public int legacyReads;
        public int saves;
        public int GetInt(string key, int fallback = 0)
        {
            if (key.StartsWith("Skill.", StringComparison.Ordinal)) legacyReads++;
            return values.TryGetValue(key, out int value) ? value : fallback;
        }
        public void SetInt(string key, int value) => values[key] = value;
        public void DeleteKey(string key) => values.Remove(key);
        public void Save() => saves++;
    }

    private SkillTree tree;
    private LegacyRow[] golden;
    private IEnumerable<SkillDefinition> LegacyDefinitions => tree.definitions.Where(item => item.levels.Any(level => level.legacyId >= 0));
    private MemorySave save;
    private SkillProgress progress;

    [SetUp]
    public void SetUp()
    {
        tree = Resources.Load<SkillTree>("Skills/SkillTree");
        Assert.That(tree, Is.Not.Null, "Handwritten YAML assets must load in Unity");
        golden = JsonUtility.FromJson<LegacyTable>(AssetDatabase.LoadAssetAtPath<TextAsset>(
            "Assets/Tests/EditMode/LegacySkillTable.json").text).rows;
        save = new MemorySave();
        save.SetInt("Economy.Stardust", 100000);
        save.SetInt("Economy.Metal", 100000);
        progress = NewProgress();
    }

    private SkillProgress NewProgress(Func<bool> allOwned = null) =>
        new SkillProgress(tree, save, new SkillWallet(save), allOwned);

    [Test]
    public void Tree_matches_every_legacy_shop_row_cost_label_order_and_prerequisite()
    {
        Assert.That(tree.Validate(), Is.Empty);
        Assert.That(LegacyDefinitions.Count(), Is.EqualTo(12));
        int row = 0;
        foreach (var definition in LegacyDefinitions)
        {
            for (int index = 0; index < definition.MaxLevel; index++)
            {
                var expected = golden[row++];
                var actual = definition.levels[index];
                Assert.That(definition.id, Is.EqualTo(expected.newId));
                Assert.That(definition.displayName, Is.EqualTo(expected.displayName));
                Assert.That((int)definition.branch, Is.EqualTo(expected.branch));
                Assert.That(actual.legacyId, Is.EqualTo(expected.legacyId));
                Assert.That(actual.label, Is.EqualTo(expected.label));
                Assert.That(actual.costStardust, Is.EqualTo(expected.costStardust));
                Assert.That(actual.costMetal, Is.EqualTo(expected.costMetal));
                Assert.That(index + 1, Is.EqualTo(expected.level));
                if (index > 0)
                    Assert.That(definition.levels[index - 1].legacyId, Is.EqualTo(expected.prereqId));
                else if (expected.prereqId < 0)
                    Assert.That(definition.prerequisiteIds, Is.Empty);
                else
                    Assert.That(definition.prerequisiteIds, Is.EqualTo(new[] {
                        golden.Single(item => item.legacyId == expected.prereqId).newId }));
            }
        }
        Assert.That(row, Is.EqualTo(18));
    }

    [Test]
    public void Purchases_follow_all_18_rows_and_spend_the_displayed_cost_once()
    {
        int events = 0;
        progress.OnSkillsChanged += () => events++;
        foreach (var row in golden)
        {
            int stardust = save.GetInt("Economy.Stardust");
            int metal = save.GetInt("Economy.Metal");
            Assert.That(progress.CanPurchase(row.newId, out string reason), Is.True, reason);
            Assert.That(progress.TryPurchase(row.newId), Is.True, row.newId);
            Assert.That(progress.GetLevel(row.newId), Is.EqualTo(row.level));
            Assert.That(save.GetInt("Economy.Stardust"), Is.EqualTo(stardust - row.costStardust));
            Assert.That(save.GetInt("Economy.Metal"), Is.EqualTo(metal - row.costMetal));
        }
        Assert.That(events, Is.EqualTo(18));
        foreach (var definition in LegacyDefinitions)
        {
            int before = save.GetInt("Economy.Stardust");
            Assert.That(progress.TryPurchase(definition.id, out string reason), Is.False);
            Assert.That(reason, Is.EqualTo("Already owned!"));
            Assert.That(save.GetInt("Economy.Stardust"), Is.EqualTo(before));
        }
        Assert.That(events, Is.EqualTo(18));
        var reloaded = NewProgress();
        foreach (var definition in LegacyDefinitions)
            Assert.That(reloaded.GetLevel(definition.id), Is.EqualTo(definition.MaxLevel));
    }

    [TestCase("Economy.Stardust", "Not enough Stardust!")]
    [TestCase("Economy.Metal", "Not enough Metal!")]
    public void Insufficient_currency_never_spends_either_currency(string key, string reason)
    {
        // Slam has both costs; grant only its prerequisite without granting unrelated skills.
        save.SetInt(SkillProgress.Key("hammer.score"), 1);
        progress.Load();
        save.SetInt(key, 0);
        int dust = save.GetInt("Economy.Stardust"), metal = save.GetInt("Economy.Metal");
        int saves = save.saves;
        Assert.That(progress.TryPurchase("hammer.slam_wave", out string actual), Is.False);
        Assert.That(actual, Is.EqualTo(reason));
        Assert.That(save.GetInt("Economy.Stardust"), Is.EqualTo(dust));
        Assert.That(save.GetInt("Economy.Metal"), Is.EqualTo(metal));
        Assert.That(progress.IsOwned("hammer.slam_wave"), Is.False);
        Assert.That(save.saves, Is.EqualTo(saves));
    }

    [Test]
    public void Prerequisites_require_previous_skill_final_level_and_levels_cannot_be_skipped()
    {
        Assert.That(progress.TryPurchase("autoshooter.accuracy"), Is.False);
        Assert.That(progress.TryPurchase("autoshooter.fire_rate"), Is.True);
        Assert.That(progress.GetLevel("autoshooter.fire_rate"), Is.EqualTo(1));
        Assert.That(progress.TryPurchase("autoshooter.accuracy"), Is.False);
        Assert.That(progress.TryPurchase("autoshooter.fire_rate"), Is.True);
        Assert.That(progress.TryPurchase("autoshooter.accuracy"), Is.True);
        Assert.That(progress.TryPurchase("hammer.lightning"), Is.False);
    }

    [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
    [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)]
    [TestCase(12)] [TestCase(13)] [TestCase(14)] [TestCase(15)] [TestCase(16)] [TestCase(17)]
    public void Migration_maps_each_legacy_id_including_out_of_order_purchases(int legacyId)
    {
        save.DeleteKey(SkillProgress.SaveVersionKey);
        save.SetInt("Skill." + legacyId, 1);
        var migrated = NewProgress();
        var expected = golden.Single(row => row.legacyId == legacyId);
        foreach (var definition in tree.definitions)
            Assert.That(migrated.GetLevel(definition.id), Is.EqualTo(
                definition.id == expected.newId ? expected.level : 0));
        Assert.That(save.GetInt(SkillProgress.Key(expected.newId)), Is.EqualTo(expected.level));
        Assert.That(save.GetInt("Skill." + legacyId), Is.EqualTo(1), "Keep rollback keys");
    }

    [Test]
    public void Migration_is_idempotent_preserves_new_progress_and_stops_reading_legacy_keys()
    {
        save.DeleteKey(SkillProgress.SaveVersionKey);
        save.SetInt("Skill.15", 1);
        save.SetInt(SkillProgress.Key("hammer.lightning"), 3);
        Assert.That(NewProgress().GetLevel("hammer.lightning"), Is.EqualTo(3));
        int reads = save.legacyReads, saves = save.saves;
        save.SetInt("Skill.1", 1);
        Assert.That(NewProgress().GetLevel("autoshooter.fire_rate"), Is.Zero);
        Assert.That(save.legacyReads, Is.EqualTo(reads));
        Assert.That(save.saves, Is.EqualTo(saves));
    }

    [Test]
    public void Empty_prefs_and_invalid_legacy_values_grant_nothing()
    {
        save = new MemorySave();
        foreach (var row in golden) save.SetInt("Skill." + row.legacyId, row.legacyId % 2 == 0 ? -1 : 2);
        foreach (var definition in tree.definitions) Assert.That(NewProgress().GetLevel(definition.id), Is.Zero);
        save = new MemorySave();
        var empty = NewProgress();
        foreach (var definition in tree.definitions) Assert.That(empty.GetLevel(definition.id), Is.Zero);
    }

    [Test]
    public void Corrupt_new_levels_are_clamped_and_unknown_ids_are_safe()
    {
        save.SetInt(SkillProgress.Key("hammer.lightning"), int.MaxValue);
        save.SetInt(SkillProgress.Key("ship.move_speed"), -20);
        progress.Load();
        Assert.That(progress.GetLevel("hammer.lightning"), Is.EqualTo(3));
        Assert.That(progress.GetLevel("ship.move_speed"), Is.Zero);
        Assert.That(progress.GetLevel(null), Is.Zero);
        Assert.That(progress.IsOwned("unknown.id"), Is.False);
        Assert.That(progress.TryPurchase("unknown.id"), Is.False);
        Assert.That(progress.CanPurchase(null), Is.False);
    }

    [Test]
    public void Reset_emits_once_and_does_not_resurrect_rollback_keys()
    {
        save.DeleteKey(SkillProgress.SaveVersionKey);
        foreach (var row in golden) save.SetInt("Skill." + row.legacyId, 1);
        var migrated = NewProgress();
        int events = 0;
        migrated.OnSkillsChanged += () => events++;
        migrated.ResetAll();
        Assert.That(events, Is.EqualTo(1));
        for (int legacyId = 0; legacyId < 18; legacyId++)
            Assert.That(save.values.ContainsKey("Skill." + legacyId), Is.False);
        save.DeleteKey(SkillProgress.SaveVersionKey);
        var reloaded = NewProgress();
        foreach (var definition in tree.definitions) Assert.That(reloaded.GetLevel(definition.id), Is.Zero);
    }

    [Test]
    public void Dev_override_grants_maximum_effects_without_writing_purchases()
    {
        bool enabled = true;
        var dev = NewProgress(() => enabled);
        foreach (var definition in tree.definitions)
            Assert.That(dev.GetLevel(definition.id), Is.EqualTo(definition.MaxLevel));
        Assert.That(dev.GetModifier(SkillStat.LightningBounces), Is.EqualTo(6f));
        Assert.That(dev.TryPurchase("autoshooter.fire_rate"), Is.False);
        enabled = false;
        foreach (var definition in tree.definitions) Assert.That(dev.GetLevel(definition.id), Is.Zero);
    }

    [Test]
    public void All_262144_owned_combinations_match_legacy_modifier_bits() => CheckAllLegacyMasks(false);

    [Test]
    public void All_262144_legacy_masks_migrate_from_fresh_saves_and_match_modifier_bits() => CheckAllLegacyMasks(true);

    private void CheckAllLegacyMasks(bool migrate)
    {
        // Exhaustive comparison, including impossible/corrupt combinations, against the old getter formulas.
        var stats = ((SkillStat[])Enum.GetValues(typeof(SkillStat))).Where(stat => (int)stat <= (int)SkillStat.SlamWave).ToArray();
        var expected = new float[stats.Length];
        for (int mask = 0; mask < (1 << 18); mask++)
        {
            if (migrate)
            {
                save = new MemorySave();
                for (int legacyId = 0; legacyId < 18; legacyId++)
                    if (Has(mask, legacyId)) save.SetInt("Skill." + legacyId, 1);
                progress = NewProgress();
                if (save.GetInt(SkillProgress.SaveVersionKey) != 1 || save.legacyReads == 0)
                    Assert.Fail("Migration did not run at mask " + mask);
            }
            else
            foreach (var definition in LegacyDefinitions)
            {
                int highest = 0;
                for (int level = 0; level < definition.MaxLevel; level++)
                    if (Has(mask, definition.levels[level].legacyId)) highest = level + 1;
                save.SetInt(SkillProgress.Key(definition.id), highest);
            }
            if (!migrate) progress.Load();
            expected[0] = Has(mask, 1) ? 2f : Has(mask, 0) ? 1.5f : 1f;
            expected[1] = Has(mask, 2) ? .5f : 1f;
            expected[2] = Has(mask, 3) ? 1.3f : 1f;
            expected[3] = Has(mask, 5) ? 1.65f : Has(mask, 4) ? 1.3f : 1f;
            expected[4] = Has(mask, 6) ? 3f : 2f;
            expected[5] = Has(mask, 9) ? 1.3f : Has(mask, 8) ? 1.15f : 1f;
            expected[6] = Has(mask, 10) ? 1.5f : 1f;
            expected[7] = Has(mask, 14) ? 2f : Has(mask, 13) ? 1f : 0f;
            expected[8] = Has(mask, 11) ? 1.5f : 1f;
            expected[9] = Has(mask, 17) ? 6f : Has(mask, 16) ? 4f : Has(mask, 15) ? 2f : 0f;
            expected[10] = Has(mask, 17) ? 1f : 0f;
            expected[11] = Has(mask, 12) ? 1f : 0f;
            expected[12] = Has(mask, 7) ? 1f : 0f;
            foreach (var stat in stats)
                if (BitConverter.SingleToInt32Bits(progress.GetModifier(stat)) != BitConverter.SingleToInt32Bits(expected[(int)stat]))
                    Assert.Fail("Legacy mismatch at mask " + mask + " stat " + stat);
        }
    }

    private static bool Has(int mask, int id) => (mask & (1 << id)) != 0;

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Negative_migration_marker_runs_migration(int marker)
    {
        save.SetInt(SkillProgress.SaveVersionKey, marker);
        save.SetInt("Skill.1", 1);
        Assert.That(NewProgress().GetLevel("autoshooter.fire_rate"), Is.EqualTo(2));
        Assert.That(save.GetInt(SkillProgress.SaveVersionKey), Is.EqualTo(1));
    }

    [Test]
    public void Partial_migration_max_merges_new_and_legacy_levels_without_a_marker()
    {
        save.DeleteKey(SkillProgress.SaveVersionKey);
        save.SetInt(SkillProgress.Key("autoshooter.fire_rate"), 1);
        save.SetInt(SkillProgress.Key("hammer.lightning"), 3);
        save.SetInt("Skill.1", 1);
        save.SetInt("Skill.15", 1);
        var migrated = NewProgress();
        Assert.That(migrated.GetLevel("autoshooter.fire_rate"), Is.EqualTo(2));
        Assert.That(migrated.GetLevel("hammer.lightning"), Is.EqualTo(3));
    }

    [Test]
    public void Garbage_string_marker_uses_missing_int_fallback_and_migrates()
    {
        string prefix = "Wam.Phase2.Tests." + Guid.NewGuid().ToString("N") + ".";
        var isolated = new PlayerPrefsSkillSave(prefix);
        try
        {
            PlayerPrefs.SetString(prefix + SkillProgress.SaveVersionKey, "garbage");
            isolated.SetInt("Skill.1", 1);
            var migrated = new SkillProgress(tree, isolated, new SkillWallet(isolated));
            Assert.That(migrated.GetLevel("autoshooter.fire_rate"), Is.EqualTo(2));
            Assert.That(isolated.GetInt(SkillProgress.SaveVersionKey), Is.EqualTo(1));
        }
        finally
        {
            foreach (var definition in tree.definitions) isolated.DeleteKey(SkillProgress.Key(definition.id));
            isolated.DeleteKey("Skill.1");
            isolated.DeleteKey(SkillProgress.SaveVersionKey);
            isolated.Save();
        }
    }

    [TestCase("a.b", true)] [TestCase("a0.b_1.c2", true)]
    [TestCase("a.b\n", true)] [TestCase("a_b.c", false)]
    [TestCase("A.b", false)] [TestCase("a.B", false)]
    [TestCase("a.1b", false)] [TestCase("a._b", false)]
    [TestCase("a..b", false)] [TestCase("a.b.", false)]
    [TestCase("ab", false)] [TestCase("a.b-c", false)]
    [TestCase("a.b\r\n", false)] [TestCase("a.b\nc", false)]
    [TestCase("", false)] [TestCase(null, false)]
    public void Id_char_validation_preserves_the_original_expression(string id, bool accepted)
    {
        Assert.That(SkillTree.IsValidId(id), Is.EqualTo(accepted));
    }

    [Test]
    public void Tier_sort_preserves_current_visible_order_and_is_stable()
    {
        var ordered = tree.OrderedDefinitions();
        foreach (SkillBranch branch in Enum.GetValues(typeof(SkillBranch)))
            Assert.That(ordered.Where(item => item.branch == branch).Select(item => item.id),
                Is.EqualTo(tree.definitions.Where(item => item.branch == branch).Select(item => item.id)));
        var temporary = ScriptableObject.CreateInstance<SkillTree>();
        try
        {
            temporary.definitions.AddRange(tree.definitions.AsEnumerable().Reverse());
            var sorted = temporary.OrderedDefinitions();
            Assert.That(sorted.Select(item => item.tier), Is.Ordered);
            foreach (var group in sorted.GroupBy(item => item.tier))
                Assert.That(group, Is.EqualTo(temporary.definitions.Where(item => item.tier == group.Key)));
        }
        finally { UnityEngine.Object.DestroyImmediate(temporary); }
    }

    [Test]
    public void Validation_rejects_duplicate_ids_missing_prerequisites_cycles_and_negative_costs()
    {
        var temporary = ScriptableObject.CreateInstance<SkillTree>();
        var first = UnityEngine.Object.Instantiate(tree.definitions[0]);
        var second = UnityEngine.Object.Instantiate(tree.definitions[1]);
        try
        {
            temporary.definitions.Add(first);
            temporary.definitions.Add(second);
            second.id = first.id;
            Assert.That(temporary.Validate().Any(error => error.StartsWith("Duplicate id")), Is.True);
            second.id = "test.second";
            first.prerequisiteIds.Add("missing.id");
            Assert.That(temporary.Validate().Any(error => error.StartsWith("Missing prerequisite")), Is.True);
            first.prerequisiteIds.Clear();
            first.prerequisiteIds.Add(second.id);
            second.prerequisiteIds.Clear();
            second.prerequisiteIds.Add(first.id);
            Assert.That(temporary.Validate().Any(error => error.StartsWith("Prerequisite cycle")), Is.True);
            first.levels[0].costMetal = -1;
            Assert.That(temporary.Validate().Any(error => error.StartsWith("Negative cost")), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
            UnityEngine.Object.DestroyImmediate(temporary);
        }
    }

    [Test]
    public void PlayerPrefs_adapter_roundtrips_in_a_test_only_namespace()
    {
        string prefix = "Wam.Phase1.Tests." + Guid.NewGuid().ToString("N") + ".";
        var isolated = new PlayerPrefsSkillSave(prefix);
        try
        {
            isolated.SetInt("Economy.Stardust", 1000);
            isolated.SetInt("Skill.1", 1);
            var testProgress = new SkillProgress(tree, isolated, new SkillWallet(isolated));
            Assert.That(testProgress.GetLevel("autoshooter.fire_rate"), Is.EqualTo(2));
            Assert.That(testProgress.TryPurchase("autoshooter.accuracy"), Is.True);
            Assert.That(new SkillProgress(tree, isolated, new SkillWallet(isolated)).IsOwned("autoshooter.accuracy"), Is.True);
            Assert.That(isolated.GetInt("Economy.Stardust"), Is.EqualTo(400));
        }
        finally
        {
            foreach (var definition in tree.definitions) isolated.DeleteKey(SkillProgress.Key(definition.id));
            for (int id = 0; id < 18; id++) isolated.DeleteKey("Skill." + id);
            isolated.DeleteKey(SkillProgress.SaveVersionKey);
            isolated.DeleteKey("Economy.Stardust");
            isolated.DeleteKey("Economy.Metal");
            isolated.Save();
        }
    }
}
