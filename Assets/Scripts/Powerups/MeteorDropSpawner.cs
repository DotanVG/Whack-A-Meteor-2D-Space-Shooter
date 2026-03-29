using UnityEngine;

/// <summary>
/// MeteorDropSpawner — singleton that handles item drops from meteor kills.
///
/// Meteors can drop:
///   • Pill_Red   (health)      — very rare, balance key: meteor.pill_health_chance
///   • Pill_Blue  (laser boost) — rare,      balance key: meteor.pill_laser_chance
///   • BoltTier   collectible   — uncommon,  balance key: meteor.bolt_tier_chance
///   • ShieldTier collectible   — uncommon,  balance key: meteor.shield_tier_chance
///   • StarTier   collectible   — uncommon,  balance key: meteor.star_tier_chance
///
/// Called from MeteorSplit.OnProjectileHit() and OnHammerHit() via TryDrop().
/// Assign the 5 prefabs in the Inspector on the GameManager GameObject alongside PowerupSpawner.
/// </summary>
public class MeteorDropSpawner : MonoBehaviour
{
    public static MeteorDropSpawner Instance { get; private set; }

    [Header("Pill Prefabs")]
    public GameObject pillHealthPrefab;      // pill_red
    public GameObject pillLaserBoostPrefab;  // pill_blue

    [Header("Tier Collectible Prefabs")]
    public GameObject boltTierPrefab;        // powerupBlue_bolt
    public GameObject shieldTierPrefab;      // powerupBlue_shield
    public GameObject starTierPrefab;        // powerupBlue_star

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// Roll for drops on a meteor kill. Safe to call even if prefabs are unassigned.
    /// </summary>
    public void TryDrop(Vector2 position, string meteorTag)
    {
        if (!GameFeatureFlags.UsePowerups) return;

        float pillHealthChance  = Get("meteor.pill_health_chance",  0.03f);
        float pillLaserChance   = Get("meteor.pill_laser_chance",   0.05f);
        float boltChance        = Get("meteor.bolt_tier_chance",    0.08f);
        float shieldChance      = Get("meteor.shield_tier_chance",  0.06f);
        float starChance        = Get("meteor.star_tier_chance",    0.07f);

        // Big meteors have doubled drop chances (more rewarding to kill)
        if (meteorTag.StartsWith("Big"))
        {
            float bigMult = Get("meteor.big_drop_mult", 2f);
            pillHealthChance *= bigMult;
            pillLaserChance  *= bigMult;
            boltChance       *= bigMult;
            shieldChance     *= bigMult;
            starChance       *= bigMult;
        }

        TrySpawn(pillHealthPrefab,  position, pillHealthChance);
        TrySpawn(pillLaserBoostPrefab, position, pillLaserChance);
        TrySpawn(boltTierPrefab,    position, boltChance);
        TrySpawn(shieldTierPrefab,  position, shieldChance);
        TrySpawn(starTierPrefab,    position, starChance);
    }

    void TrySpawn(GameObject prefab, Vector2 pos, float chance)
    {
        if (prefab == null) return;
        if (Random.value <= chance)
            Instantiate(prefab, pos, Quaternion.identity);
    }

    static float Get(string key, float def)
        => BalanceService.Instance?.GetFloat(key, def) ?? def;
}
