using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BalanceService — single source for all tunable game numbers.
///
/// Phase 0: ships hardcoded defaults from GameConstants so existing gameplay
///          is unchanged. All callers should use BalanceService.Get* instead
///          of raw GameConstants so that Phase 1 (CSV import) is a drop-in swap.
///
/// Phase 1 upgrade path:
///   - Set GameFeatureFlags.UseCSVBalance = true
///   - Drop balance_master.csv into Assets/Data/
///   - CSVParser populates _table at boot
///   - No call-site changes needed
///
/// Key format:  "category.key"  e.g. "ship.lives_max", "weapon.fire_rate"
/// Level keys:  "category.key.N" where N = upgrade level (1-based)
/// </summary>
public class BalanceService : MonoBehaviour
{
    public static BalanceService Instance { get; private set; }

    // ── Internal table ────────────────────────────────────────────────────────
    // Key: "category.key[.level]"   Value: float
    private readonly Dictionary<string, float> _table = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadDefaults();

        if (GameFeatureFlags.UseCSVBalance)
        {
            // TODO Phase 1: LoadFromCSV("Assets/Data/balance_master.csv");
            Debug.Log("[BalanceService] CSV balance enabled — loader not yet implemented.");
        }

        Debug.Log($"[BalanceService] Initialized with {_table.Count} entries (hardcoded defaults).");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns a float balance value. Falls back to <paramref name="defaultVal"/> if missing.</summary>
    public float GetFloat(string key, float defaultVal = 0f)
        => _table.TryGetValue(key, out float v) ? v : defaultVal;

    /// <summary>Returns an int balance value.</summary>
    public int GetInt(string key, int defaultVal = 0)
        => Mathf.RoundToInt(GetFloat(key, defaultVal));

    /// <summary>Levelled lookup — key format "category.key", level is 1-based.</summary>
    public float GetFloat(string key, int level, float defaultVal = 0f)
        => GetFloat($"{key}.{level}", GetFloat(key, defaultVal));

    // ── Hardcoded defaults (mirrors GameConstants) ────────────────────────────

    void LoadDefaults()
    {
        // Ship
        Set("ship.lives_max",               GameConstants.StartingLives);
        Set("ship.invincibility_duration",   GameConstants.InvincibilityDuration);

        // Scores (kept in sync with GameConstants)
        Set("score.big_meteor",              GameConstants.ScoreBigMeteor);
        Set("score.medium_meteor",           GameConstants.ScoreMediumMeteor);
        Set("score.small_meteor",            GameConstants.ScoreSmallMeteor);
        Set("score.tiny_meteor",             GameConstants.ScoreTinyMeteor);
        Set("score.enemy",                   GameConstants.ScoreEnemy);

        // Auto-shooter defaults
        Set("weapon.autoshooter_fire_rate",      1.0f);
        Set("weapon.autoshooter_accuracy_spread", 12f);
        Set("weapon.autoshooter_projectile_speed", 50f);
        Set("weapon.autoshooter_range",           0f);   // 0 = unlimited

        // Hammer defaults
        Set("hammer.aoe_radius",             1.5f);
        Set("hammer.score_multiplier",       2f);

        // Economy defaults (Phase 2 will use these)
        Set("economy.stardust_drop_mult",    1.0f);
        Set("economy.scrap_drop_mult",       1.0f);

        // Wave defaults
        Set("wave.time_between_waves",       5f);
        Set("wave.spawn_rate_decrement",     0.15f);
        Set("wave.speed_increment",          0.5f);
        Set("wave.min_spawn_rate",           0.3f);
    }

    void Set(string key, float value) => _table[key] = value;
}
