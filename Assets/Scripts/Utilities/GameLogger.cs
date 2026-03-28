using UnityEngine;

/// <summary>
/// GameLogger — consistent, filterable debug logging for game development.
///
/// Every log is prefixed so you can filter the Unity Console by category:
///   [Player/Damage]   [Player/Boost]
///   [Meteor/Kill]     [Meteor/Split]   [Meteor/Collision]
///   [Enemy/Kill]      [Enemy/Damage]
///   [AutoShooter]
///   [Wave]
///   [Session]
///
/// High-frequency logs (AutoShooter per-shot) are gated by VerboseAutoShooter.
/// All other logs are always on so you never miss a game event during QA.
/// </summary>
public static class GameLogger
{
    // ── Verbose gates (toggle in Inspector via GameLoggerSettings or here) ────
    /// <summary>Log every AutoShooter shot. Off by default — 1 shot/sec is noisy.</summary>
    public static bool VerboseAutoShooter = false;

    // ── Player ────────────────────────────────────────────────────────────────

    public static void PlayerDamage(string sourceTag, Vector2 hitPosition, int livesRemaining, float sessionTime)
    {
        string size    = MeteorSize(sourceTag);
        string variety = MeteorVariety(sourceTag);
        string source  = size != "Unknown"
            ? $"meteor [{size} {variety}]"
            : sourceTag == "Enemy" ? "enemy ship" : $"unknown ({sourceTag})";

        string urgency = livesRemaining == 0 ? "  ← FATAL" : livesRemaining == 1 ? "  ← CRITICAL" : "";
        Debug.LogWarning($"[Player/Damage] Damaged by {source} at {hitPosition} | " +
                         $"Lives left: {livesRemaining}{urgency} | T+{sessionTime:F1}s");
    }

    public static void PlayerInvincibleHit(string sourceTag)
    {
        Debug.Log($"[Player/Damage] Hit blocked — invincibility active (source: {sourceTag})");
    }

    public static void PlayerBoost(float cooldown, float speedMultiplier)
    {
        Debug.Log($"[Player/Boost] Boost activated × {speedMultiplier:F1} | Cooldown: {cooldown:F1}s");
    }

    public static void PlayerGameOver(int finalScore, int wave, float sessionTime)
    {
        Debug.Log($"[Session] GAME OVER | Score: {finalScore} | Wave: {wave} | Time: {sessionTime:F1}s");
    }

    // ── Meteor ────────────────────────────────────────────────────────────────

    public static void MeteorKilledByProjectile(string tag, int scoreAwarded, int totalScore, bool isSplit)
    {
        string size    = MeteorSize(tag);
        string variety = MeteorVariety(tag);
        string fate    = isSplit ? "→ splits" : "→ terminal (no split)";
        Debug.Log($"[Meteor/Kill] {size} {variety} destroyed by PROJECTILE | " +
                  $"+{scoreAwarded} pts (total: {totalScore}) | {fate}");
    }

    public static void MeteorKilledByHammer(string tag, int scoreAwarded, int totalScore, bool isSplit)
    {
        string size    = MeteorSize(tag);
        string variety = MeteorVariety(tag);
        string fate    = isSplit ? "→ splits" : "→ terminal (no split)";
        Debug.Log($"[Meteor/Kill] {size} {variety} destroyed by HAMMER (2× bonus) | " +
                  $"+{scoreAwarded} pts (total: {totalScore}) | {fate}");
    }

    public static void MeteorSplitSpawned(string parentTag, string childTag, int count)
    {
        Debug.Log($"[Meteor/Split] {MeteorSize(parentTag)} {MeteorVariety(parentTag)} " +
                  $"→ {count}× {MeteorSize(childTag)} {MeteorVariety(childTag)}");
    }

    public static void MeteorCollisionSplit(string tagA, string tagB, float relativeSpeed)
    {
        Debug.Log($"[Meteor/Collision] {MeteorSize(tagA)} {MeteorVariety(tagA)} collided with " +
                  $"{MeteorSize(tagB)} {MeteorVariety(tagB)} | Relative speed: {relativeSpeed:F2} → split");
    }

    // ── Enemy ─────────────────────────────────────────────────────────────────

    public static void EnemyKilledByProjectile(Vector2 position, int scoreAwarded, int totalScore)
    {
        Debug.Log($"[Enemy/Kill] Enemy ship destroyed by PROJECTILE at {position} | " +
                  $"+{scoreAwarded} pts (total: {totalScore})");
    }

    public static void EnemyKilledByHammer(Vector2 position, int scoreAwarded, int totalScore)
    {
        Debug.Log($"[Enemy/Kill] Enemy ship destroyed by HAMMER at {position} | " +
                  $"+{scoreAwarded} pts (total: {totalScore})");
    }

    public static void EnemyRammedPlayer(Vector2 position, int livesRemaining)
    {
        string urgency = livesRemaining == 0 ? "  ← FATAL" : livesRemaining == 1 ? "  ← CRITICAL" : "";
        Debug.LogWarning($"[Enemy/Damage] Enemy ship rammed player at {position} | " +
                         $"Lives left: {livesRemaining}{urgency}");
    }

    // ── AutoShooter ───────────────────────────────────────────────────────────

    public static void AutoShooterFired(string targetTag, float distance, float spreadDeg)
    {
        if (!VerboseAutoShooter) return;
        Debug.Log($"[AutoShooter] Fired → {targetTag} | Dist: {distance:F1}u | Spread: ±{spreadDeg:F1}°");
    }

    // ── Enemy (faction) ───────────────────────────────────────────────────────

    public static void EnemyKilledByFaction(string faction, Vector2 position, int scoreAwarded, int totalScore)
    {
        Debug.Log($"[Enemy/Faction] {faction} ship destroyed at {position} | " +
                  $"+{scoreAwarded} pts (total: {totalScore})");
    }

    public static void SpawnDirectorWave(int wave, int black, int blue, int green, int red)
    {
        if (black == 0 && blue == 0 && green == 0 && red == 0)
            Debug.Log($"[SpawnDirector] Wave {wave} — boss wave (no regular enemies)");
        else
            Debug.Log($"[SpawnDirector] Wave {wave} factions — Black:{(black>0?"✓":"✗")} Blue:{(blue>0?"✓":"✗")} " +
                      $"Green:{(green>0?"✓":"✗")} Red:{(red>0?"✓":"✗")}");
    }

    public static void BossKilled(int wave, float sessionTime)
    {
        Debug.Log($"[Boss] Boss defeated on wave {wave} | T+{sessionTime:F1}s");
    }

    // ── Power-ups ─────────────────────────────────────────────────────────────

    public static void PowerupCollected(PowerupType type, Vector2 position)
    {
        Debug.Log($"[Powerup] {type} collected at {position}");
    }

    // ── Wave ──────────────────────────────────────────────────────────────────

    public static void WaveStarted(int wave, float spawnRate, float meteorSpeed, int maxMeteors)
    {
        Debug.Log($"[Wave] ── Wave {wave} started ── " +
                  $"SpawnInterval: {spawnRate:F2}s");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static string MeteorSize(string tag) => tag switch
    {
        "BigBrownMeteor"    or "BigGreyMeteor"    => "Big",
        "MediumBrownMeteor" or "MediumGreyMeteor" => "Medium",
        "SmallBrownMeteor"  or "SmallGreyMeteor"  => "Small",
        "TinyBrownMeteor"   or "TinyGreyMeteor"   => "Tiny",
        _ => "Unknown"
    };

    static string MeteorVariety(string tag) => tag switch
    {
        "BigBrownMeteor"    or "MediumBrownMeteor" or "SmallBrownMeteor" or "TinyBrownMeteor" => "Brown",
        "BigGreyMeteor"     or "MediumGreyMeteor"  or "SmallGreyMeteor"  or "TinyGreyMeteor"  => "Grey",
        _ => ""
    };
}
