using System;
using System.Collections.Generic;

/// <summary>Injectable save boundary; keys are relative to the adapter's namespace.</summary>
public interface ISkillSave
{
    int GetInt(string key, int fallback = 0);
    void SetInt(string key, int value);
    void DeleteKey(string key);
    void Save();
}

/// <summary>A two-currency wallet with an atomic affordability check and deduction.</summary>
public interface ISkillWallet
{
    int Stardust { get; }
    int Metal { get; }
    bool TrySpend(int stardust, int metal);
}

/// <summary>Purchases, migration and aggregation independent of scene/component lifetime.</summary>
public sealed class SkillProgress
{
    public const string SaveVersionKey = "Skills.SaveVersion";
    private readonly SkillTree tree;
    private readonly ISkillSave save;
    private readonly ISkillWallet wallet;
    private readonly Func<bool> allOwned;
    private readonly Dictionary<string, SkillDefinition> definitions = new Dictionary<string, SkillDefinition>(StringComparer.Ordinal);
    private readonly Dictionary<string, int> owned = new Dictionary<string, int>(StringComparer.Ordinal);

    /// <summary>Fired once after a successful purchase or reset.</summary>
    public event Action OnSkillsChanged;

    /// <summary>Loads and migrates once; dependencies allow tests without developer saves.</summary>
    public SkillProgress(SkillTree tree, ISkillSave save, ISkillWallet wallet, Func<bool> allOwned = null)
    {
        this.tree = tree ?? throw new ArgumentNullException(nameof(tree));
        this.save = save ?? throw new ArgumentNullException(nameof(save));
        this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        this.allOwned = allOwned ?? (() => false);
        var errors = tree.Validate();
        if (errors.Count != 0) throw new ArgumentException(string.Join("; ", errors), nameof(tree));
        foreach (var definition in tree.definitions) definitions.Add(definition.id, definition);
        Load();
    }

    /// <summary>Returns the new persistent key for a stable skill id.</summary>
    public static string Key(string id) => "Skills." + id;

    /// <summary>Reloads new data. Legacy keys are read only before the migration marker exists.</summary>
    public void Load()
    {
        bool migrate = save.GetInt(SaveVersionKey) < 1;
        foreach (var definition in tree.definitions)
        {
            int level = Math.Max(0, Math.Min(definition.MaxLevel, save.GetInt(Key(definition.id))));
            if (migrate)
            {
                for (int index = 0; index < definition.MaxLevel; index++)
                    if (definition.levels[index].legacyId >= 0 && save.GetInt("Skill." + definition.levels[index].legacyId) == 1)
                        level = Math.Max(level, index + 1);
                save.SetInt(Key(definition.id), level);
            }
            owned[definition.id] = level;
        }
        if (migrate)
        {
            // Write the marker last. Kept legacy keys are rollback data, never read again.
            save.SetInt(SaveVersionKey, 1);
            save.Save();
        }
    }

    /// <summary>Highest level implies all earlier levels in the same skill, including corrupt old saves.</summary>
    public int GetLevel(string id)
    {
        if (id == null || !definitions.TryGetValue(id, out var definition)) return 0;
        return allOwned() ? definition.MaxLevel : owned[id];
    }

    /// <summary>True when at least level one is owned.</summary>
    public bool IsOwned(string id) => GetLevel(id) > 0;

    /// <summary>Prerequisites require their final level, matching the old shop's row chains.</summary>
    public bool PrerequisitesMet(string id)
    {
        if (id == null || !definitions.TryGetValue(id, out var definition)) return false;
        foreach (string prerequisite in definition.prerequisiteIds)
            if (GetLevel(prerequisite) < definitions[prerequisite].MaxLevel) return false;
        return true;
    }

    /// <summary>Checks the next level without spending currency.</summary>
    public bool CanPurchase(string id) => CanPurchase(id, out _);

    /// <summary>Returns the same currency feedback as the legacy shop, with prerequisite/ownership reasons.</summary>
    public bool CanPurchase(string id, out string reason)
    {
        reason = "";
        if (id == null || !definitions.TryGetValue(id, out var definition)) { reason = "Unknown skill!"; return false; }
        int level = GetLevel(id);
        if (level >= definition.MaxLevel) { reason = "Already owned!"; return false; }
        if (!PrerequisitesMet(id)) { reason = "Requires previous skill!"; return false; }
        var next = definition.levels[level];
        if (wallet.Metal < next.costMetal) { reason = "Not enough Metal!"; return false; }
        if (wallet.Stardust < next.costStardust) { reason = "Not enough Stardust!"; return false; }
        return true;
    }

    /// <summary>Buys exactly the next level and saves both wallet and progress together.</summary>
    public bool TryPurchase(string id) => TryPurchase(id, out _);

    /// <summary>Failed and duplicate purchases never mutate the wallet or save.</summary>
    public bool TryPurchase(string id, out string reason)
    {
        if (!CanPurchase(id, out reason)) return false;
        var definition = definitions[id];
        int level = GetLevel(id);
        var next = definition.levels[level];
        if (!wallet.TrySpend(next.costStardust, next.costMetal)) { reason = "Wallet changed; try again!"; return false; }
        owned[id] = level + 1;
        save.SetInt(Key(id), level + 1);
        save.Save();
        OnSkillsChanged?.Invoke();
        return true;
    }

    /// <summary>Clears current and legacy progress, even if the migration marker is later lost.</summary>
    public void ResetAll()
    {
        foreach (var definition in tree.definitions)
        {
            owned[definition.id] = 0;
            save.DeleteKey(Key(definition.id));
        }
        for (int legacyId = 0; legacyId < 18; legacyId++) save.DeleteKey("Skill." + legacyId);
        save.SetInt(SaveVersionKey, 1);
        save.Save();
        OnSkillsChanged?.Invoke();
    }

    /// <summary>Only the highest owned level contributes. Absolute Set values preserve legacy float bits.</summary>
    public float GetModifier(SkillStat stat)
    {
        float result = BaseValue(stat);
        foreach (var definition in tree.definitions)
        {
            int level = GetLevel(definition.id);
            if (level == 0) continue;
            foreach (var modifier in definition.levels[level - 1].modifiers)
            {
                if (modifier.stat != stat) continue;
                switch (modifier.operation)
                {
                    case SkillOperation.Set: result = modifier.value; break;
                    case SkillOperation.Add: result += modifier.value; break;
                    case SkillOperation.Multiply: result *= modifier.value; break;
                }
            }
        }
        return result;
    }

    /// <summary>Unupgraded gameplay defaults; upgrade values live exclusively in assets.</summary>
    public static float BaseValue(SkillStat stat)
    {
        switch (stat)
        {
            case SkillStat.HammerScore: return 2f;
            case SkillStat.ShieldCharges:
            case SkillStat.LightningBounces:
            case SkillStat.LightningBonusDamage:
            case SkillStat.TargetPriority:
            case SkillStat.SlamWave:
            case SkillStat.ProjectilePierce:
            case SkillStat.ExtraProjectiles:
            case SkillStat.CriticalChance:
            case SkillStat.HammerDamage:
            case SkillStat.ShieldPulseDamage:
            case SkillStat.PickupMagnetRadius: return 0f;
            default: return 1f;
        }
    }
}
