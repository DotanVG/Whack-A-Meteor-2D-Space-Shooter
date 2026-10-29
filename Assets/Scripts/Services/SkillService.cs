using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Scene facade over the shared, data-driven progress API.</summary>
[DefaultExecutionOrder(-90)]
public class SkillService : MonoBehaviour
{
    public static SkillService Instance { get; private set; }
    private static SkillProgress progress;
    private static SkillTree tree;

    /// <summary>Available in standalone Shop/Settings scenes without GameServices.</summary>
    public static SkillProgress Current
    {
        get
        {
            if (progress == null)
            {
                var save = new PlayerPrefsSkillSave();
                progress = new SkillProgress(Tree, save, new SkillWallet(save), () => DevMode.AllSkillsOwned);
                progress.OnSkillsChanged += () =>
                {
                    // Standalone shopping uses the same persisted wallet. Refresh a live cache too.
                    EconomyService.Instance?.ReloadWallet();
                    OnSkillsChanged?.Invoke();
                };
            }
            return progress;
        }
    }

    /// <summary>The only runtime catalogue; no hard-coded skill table.</summary>
    public static SkillTree Tree
    {
        get
        {
            if (tree == null) tree = Resources.Load<SkillTree>("Skills/SkillTree");
            if (tree == null) throw new InvalidOperationException("Missing Resources/Skills/SkillTree asset.");
            return tree;
        }
    }

    /// <summary>Definitions retain the original shop's visual order.</summary>
    public static IReadOnlyList<SkillDefinition> Definitions => Tree.definitions;
    public static event Action OnSkillsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        progress = null;
        tree = null;
        OnSkillsChanged = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadSkills();
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    /// <summary>Refreshes shared progress when entering gameplay after shopping.</summary>
    public void LoadSkills() => Current.Load();
    public bool IsOwned(string id) => Current.IsOwned(id);
    public int GetLevel(string id) => Current.GetLevel(id);
    public bool CanPurchase(string id) => Current.CanPurchase(id);
    public bool CanPurchase(string id, out string reason) => Current.CanPurchase(id, out reason);
    public bool TryPurchase(string id) => Current.TryPurchase(id);
    public bool TryPurchase(string id, out string reason) => Current.TryPurchase(id, out reason);
    public void ResetAll() => Current.ResetAll();
    public float GetModifier(SkillStat stat) => Current.GetModifier(stat);

    // Thin compatibility wrappers keep existing null-safe gameplay call sites unchanged.
    public float GetFireRateMultiplier() => GetModifier(SkillStat.FireRate);
    public float GetSpreadMultiplier() => GetModifier(SkillStat.Spread);
    public float GetProjSpeedMultiplier() => GetModifier(SkillStat.ProjectileSpeed);
    public bool GetTargetPriorityEnabled() => GetModifier(SkillStat.TargetPriority) != 0f;
    public float GetHammerRadiusMultiplier() => GetModifier(SkillStat.HammerRadius);
    public int GetHammerScoreMult() => (int)GetModifier(SkillStat.HammerScore);
    public bool GetSlamWaveEnabled() => GetModifier(SkillStat.SlamWave) != 0f;
    public float GetMoveSpeedMultiplier() => GetModifier(SkillStat.MoveSpeed);
    public float GetBoostDurationMultiplier() => GetModifier(SkillStat.BoostDuration);
    public int GetShieldCharges() => (int)GetModifier(SkillStat.ShieldCharges);
    public float GetInvincibilityMultiplier() => GetModifier(SkillStat.Invincibility);
    public int GetLightningBounces() => (int)GetModifier(SkillStat.LightningBounces);
    public int GetLightningBonusDamage() => (int)GetModifier(SkillStat.LightningBonusDamage);
}
