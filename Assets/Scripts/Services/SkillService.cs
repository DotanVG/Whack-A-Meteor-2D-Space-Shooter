using UnityEngine;

/// <summary>
/// SkillService — single source of truth for skill ownership and gameplay multipliers.
///
/// Spawned as a child by GameServices in the Game scene.
/// ShopController reads/writes PlayerPrefs directly (standalone scene, no GameServices).
/// All gameplay scripts query Instance?.GetXxx() — null-safe; returns base value when absent.
///
/// PlayerPrefs keys: "Skill.0" … "Skill.14"  (1 = owned, 0 = not owned)
/// </summary>
[DefaultExecutionOrder(-90)]
public class SkillService : MonoBehaviour
{
    public static SkillService Instance { get; private set; }

    // ── Skill definitions ─────────────────────────────────────────────────────

    public struct SkillDef
    {
        public int    id;
        public string name;
        public string levelLabel;
        public int    costStardust;
        public int    costMetal;
        public int    prereqId;   // -1 = root (no prerequisite)
        public int    column;     // 0 = AutoShooter, 1 = Hammer, 2 = Ship
    }

    /// <summary>
    /// Visual order within each column determines shop row order.
    /// Columns: 0 = AutoShooter (5 nodes), 1 = Hammer (4 nodes), 2 = Ship (6 nodes).
    /// </summary>
    public static readonly SkillDef[] All =
    {
        // ── AutoShooter (column 0) ────────────────────────────────────────────
        new SkillDef { id=0,  name="Fire Rate",       levelLabel="Lv 1", costStardust=500,  costMetal=0,   prereqId=-1, column=0 },
        new SkillDef { id=1,  name="Fire Rate",       levelLabel="Lv 2", costStardust=800,  costMetal=0,   prereqId=0,  column=0 },
        new SkillDef { id=2,  name="Accuracy",        levelLabel="Lv 1", costStardust=600,  costMetal=0,   prereqId=1,  column=0 },
        new SkillDef { id=3,  name="Proj. Speed",     levelLabel="Lv 1", costStardust=700,  costMetal=0,   prereqId=2,  column=0 },
        new SkillDef { id=12, name="Target Priority", levelLabel="",     costStardust=900,  costMetal=0,   prereqId=3,  column=0 },

        // ── Hammer (column 1) ─────────────────────────────────────────────────
        new SkillDef { id=4,  name="AOE Radius",      levelLabel="Lv 1", costStardust=500,  costMetal=0,   prereqId=-1, column=1 },
        new SkillDef { id=5,  name="AOE Radius",      levelLabel="Lv 2", costStardust=800,  costMetal=0,   prereqId=4,  column=1 },
        new SkillDef { id=6,  name="Score Mult",      levelLabel="Lv 1", costStardust=1000, costMetal=0,   prereqId=5,  column=1 },
        new SkillDef { id=7,  name="Slam Wave",       levelLabel="Lv 1", costStardust=500,  costMetal=800, prereqId=6,  column=1 },

        // ── Ship (column 2) ───────────────────────────────────────────────────
        new SkillDef { id=8,  name="Move Speed",      levelLabel="Lv 1", costStardust=400,  costMetal=0,   prereqId=-1, column=2 },
        new SkillDef { id=9,  name="Move Speed",      levelLabel="Lv 2", costStardust=650,  costMetal=0,   prereqId=8,  column=2 },
        new SkillDef { id=10, name="Boost Duration",  levelLabel="Lv 1", costStardust=750,  costMetal=0,   prereqId=9,  column=2 },
        new SkillDef { id=13, name="Shield",          levelLabel="Lv 1", costStardust=600,  costMetal=0,   prereqId=10, column=2 },
        new SkillDef { id=14, name="Shield",          levelLabel="Lv 2", costStardust=900,  costMetal=400, prereqId=13, column=2 },
        new SkillDef { id=11, name="Invincibility",   levelLabel="Lv 1", costStardust=1200, costMetal=600, prereqId=14, column=2 },
    };

    public const int SkillCount = 15;

    private bool[] _owned = new bool[SkillCount];

    public static event System.Action OnSkillsChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadSkills();
    }

    public void LoadSkills()
    {
        for (int i = 0; i < SkillCount; i++)
            _owned[i] = PlayerPrefs.GetInt($"Skill.{i}", 0) == 1;
    }

    // ── Ownership queries ─────────────────────────────────────────────────────

    public bool IsOwned(int id) => id >= 0 && id < SkillCount && _owned[id];

    // ── Multiplier / flag getters (null-safe via Instance?.) ──────────────────

    /// <summary>AutoShooter: shots per second multiplier.</summary>
    public float GetFireRateMultiplier()
    {
        if (IsOwned(1)) return 2.0f;
        if (IsOwned(0)) return 1.5f;
        return 1.0f;
    }

    /// <summary>AutoShooter: spread angle multiplier (lower = tighter).</summary>
    public float GetSpreadMultiplier()    => IsOwned(2) ? 0.5f : 1.0f;

    /// <summary>AutoShooter: projectile speed multiplier.</summary>
    public float GetProjSpeedMultiplier() => IsOwned(3) ? 1.3f : 1.0f;

    /// <summary>AutoShooter: when true, prefers enemy ships over meteors.</summary>
    public bool GetTargetPriorityEnabled() => IsOwned(12);

    /// <summary>Hammer: AOE hit-detection radius multiplier.</summary>
    public float GetHammerRadiusMultiplier()
    {
        if (IsOwned(5)) return 1.65f;
        if (IsOwned(4)) return 1.3f;
        return 1.0f;
    }

    /// <summary>Hammer: score multiplier (base 2×, upgraded to 3×).</summary>
    public int GetHammerScoreMult() => IsOwned(6) ? 3 : 2;

    /// <summary>Hammer: true when Slam Wave (radial burst on swing) is unlocked.</summary>
    public bool GetSlamWaveEnabled() => IsOwned(7);

    /// <summary>Ship: movement speed multiplier.</summary>
    public float GetMoveSpeedMultiplier()
    {
        if (IsOwned(9)) return 1.3f;
        if (IsOwned(8)) return 1.15f;
        return 1.0f;
    }

    /// <summary>Ship: boost duration multiplier.</summary>
    public float GetBoostDurationMultiplier() => IsOwned(10) ? 1.5f : 1.0f;

    /// <summary>Ship: number of shield charges available per run (0, 1, or 2).</summary>
    public int GetShieldCharges()
    {
        if (IsOwned(14)) return 2;
        if (IsOwned(13)) return 1;
        return 0;
    }

    /// <summary>Ship: post-hit invincibility duration multiplier.</summary>
    public float GetInvincibilityMultiplier() => IsOwned(11) ? 1.5f : 1.0f;

    // ── Reset (called by SettingsController) ──────────────────────────────────

    public void ResetAll()
    {
        for (int i = 0; i < SkillCount; i++)
        {
            _owned[i] = false;
            PlayerPrefs.DeleteKey($"Skill.{i}");
        }
        PlayerPrefs.Save();
        OnSkillsChanged?.Invoke();
        Debug.Log("[SkillService] All skills reset.");
    }
}
