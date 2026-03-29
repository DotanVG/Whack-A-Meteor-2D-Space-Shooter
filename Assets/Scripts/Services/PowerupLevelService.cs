using UnityEngine;

/// <summary>
/// PowerupLevelService — tracks per-run levels for the three "tiered" collectible powerup types.
///
/// Level progression: 0 (none) → 1 (bronze) → 2 (silver) → 3 (gold/max).
/// Picking up a matching powerup collectible increments the level by 1, up to max.
///
/// These levels persist for the duration of the run only (reset on death).
/// They are read by PlayerPowerupHandler.OnGUI() to draw the HUD badge.
///
/// Tier icons in HUD (Sprites/Powerups):
///   bolt  levels: bolt_silver (lv1), bolt_gold (lv2-3)
///   shield levels: bolt_silver (lv1), bolt_gold (lv2-3)  [uses powerupBlue_shield sprite as icon]
///   star  levels: star_bronze (lv1), star_silver (lv2), star_gold (lv3)
/// </summary>
public class PowerupLevelService : MonoBehaviour
{
    public static PowerupLevelService Instance { get; private set; }

    public const int MaxLevel = 3;

    public int BoltLevel   { get; private set; }  // DoubleFire tier
    public int ShieldLevel { get; private set; }  // Shield tier
    public int StarLevel   { get; private set; }  // ScoreMultiplier tier

    public static event System.Action OnLevelsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void IncrementBolt()
    {
        if (BoltLevel < MaxLevel) BoltLevel++;
        OnLevelsChanged?.Invoke();
        Debug.Log($"[PowerupLevel] Bolt Lv{BoltLevel}");
    }

    public void IncrementShield()
    {
        if (ShieldLevel < MaxLevel) ShieldLevel++;
        OnLevelsChanged?.Invoke();
        Debug.Log($"[PowerupLevel] Shield Lv{ShieldLevel}");
    }

    public void IncrementStar()
    {
        if (StarLevel < MaxLevel) StarLevel++;
        OnLevelsChanged?.Invoke();
        Debug.Log($"[PowerupLevel] Star Lv{StarLevel}");
    }

    /// <summary>Reset all tier levels — call on game-over / new run.</summary>
    public void ResetForNewRun()
    {
        BoltLevel   = 0;
        ShieldLevel = 0;
        StarLevel   = 0;
        OnLevelsChanged?.Invoke();
    }
}
