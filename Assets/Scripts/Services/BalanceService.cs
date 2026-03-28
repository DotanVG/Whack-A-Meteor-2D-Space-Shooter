using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BalanceService — single source for all tunable game numbers.
///
/// Phase 1: loads balance_master.csv from Resources/Data/ at boot.
///          All gameplay scripts read values via GetFloat/GetInt instead of
///          raw GameConstants so you can retune without code edits.
///
/// Key format:   "category.key"     e.g. "weapon.autoshooter_fire_rate"
/// Level format: "category.key.N"   e.g. "weapon.autoshooter_fire_rate.2"
///               (level 1 is also stored under the un-levelled key as default)
///
/// CSV hot-reload: call Reload() at runtime to re-parse the CSV without
/// restarting. Useful during balance iteration in Editor play mode.
/// </summary>
public class BalanceService : MonoBehaviour
{
    public static BalanceService Instance { get; private set; }

    private readonly Dictionary<string, float> _table = new Dictionary<string, float>(128);

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
            var csvTable = CSVBalanceParser.Load();
            // CSV values override defaults — merge into _table
            foreach (var kv in csvTable)
                _table[kv.Key] = kv.Value;
        }

        Debug.Log($"[BalanceService] Initialized — {_table.Count} entries " +
                  $"(CSV: {GameFeatureFlags.UseCSVBalance})");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns a float value. Falls back to <paramref name="defaultVal"/> if key missing.</summary>
    public float GetFloat(string key, float defaultVal = 0f)
        => _table.TryGetValue(key, out float v) ? v : defaultVal;

    /// <summary>Returns an int value.</summary>
    public int GetInt(string key, int defaultVal = 0)
        => Mathf.RoundToInt(GetFloat(key, defaultVal));

    /// <summary>
    /// Levelled lookup. Tries "category.key.level" first, falls back to "category.key",
    /// then to <paramref name="defaultVal"/>.
    /// </summary>
    public float GetFloat(string key, int level, float defaultVal = 0f)
        => GetFloat($"{key}.{level}", GetFloat(key, defaultVal));

    /// <summary>
    /// Re-parses the CSV at runtime (Editor only). Useful during balance iteration
    /// without restarting play mode.
    /// </summary>
    public void Reload()
    {
        if (!GameFeatureFlags.UseCSVBalance)
        {
            Debug.LogWarning("[BalanceService] Reload() called but UseCSVBalance is false.");
            return;
        }
        LoadDefaults();
        var csvTable = CSVBalanceParser.Load();
        foreach (var kv in csvTable)
            _table[kv.Key] = kv.Value;
        Debug.Log($"[BalanceService] Reloaded — {_table.Count} entries.");
    }

    // ── Hardcoded defaults (mirrors GameConstants — always loaded first) ───────

    void LoadDefaults()
    {
        _table.Clear();

        // Ship
        Set("ship.lives_max",                  GameConstants.StartingLives);
        Set("ship.invincibility_duration",      GameConstants.InvincibilityDuration);

        // Scores
        Set("score.big_meteor",                 GameConstants.ScoreBigMeteor);
        Set("score.medium_meteor",              GameConstants.ScoreMediumMeteor);
        Set("score.small_meteor",               GameConstants.ScoreSmallMeteor);
        Set("score.tiny_meteor",                GameConstants.ScoreTinyMeteor);
        Set("score.enemy",                      GameConstants.ScoreEnemy);

        // Auto-shooter level 1 defaults
        Set("weapon.autoshooter_fire_rate",         1.0f);
        Set("weapon.autoshooter_fire_rate.1",       1.0f);
        Set("weapon.autoshooter_accuracy_spread",   12f);
        Set("weapon.autoshooter_accuracy_spread.1", 12f);
        Set("weapon.autoshooter_projectile_speed",  50f);
        Set("weapon.autoshooter_range",             0f);

        // Hammer level 1 defaults
        Set("hammer.aoe_radius",       1.5f);
        Set("hammer.aoe_radius.1",     1.5f);
        Set("hammer.score_multiplier", 2f);

        // Meteor cap
        Set("meteor.max_active_count", 80f);

        // Economy — multipliers
        Set("economy.stardust_drop_mult", 1.0f);
        Set("economy.metal_drop_mult",    1.0f);

        // Economy — Stardust drops per meteor size (projectile or hammer kill)
        Set("economy.stardust_big",    3f);
        Set("economy.stardust_medium", 2f);
        Set("economy.stardust_small",  1f);
        Set("economy.stardust_tiny",   1f);

        // Economy — Metal drops from enemy kills
        Set("economy.metal_enemy", 2f);

        // Progression — XP curve (xp_base * xp_growth^(level-1))
        // Lv1→2: 5000 XP  |  Lv2→3: 7500  |  Lv3→4: 11250  |  Lv5→6: ~25k
        Set("progression.xp_base",   5000f);
        Set("progression.xp_growth",    1.5f);

        // Wave
        Set("wave.time_between_waves",    5f);
        Set("wave.spawn_rate_decrement",  0.15f);
        Set("wave.speed_increment",       0.5f);
        Set("wave.min_spawn_rate",        0.3f);
    }

    void Set(string key, float value) => _table[key] = value;
}
