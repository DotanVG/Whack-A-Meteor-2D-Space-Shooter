using UnityEngine;

/// <summary>
/// EnemyHealth — attaches to each meteor / enemy GameObject.
/// Manages incoming damage, hit feedback, and death behaviour.
///
/// SETUP:
///   - Attach to your meteor prefab alongside MeteorMover.
///   - The GameObject must be tagged "Enemy" so AutoShooter and Bullet can find it.
///
/// PLANNED UPGRADES:
///   - Spawn explosion VFX on Die() (particle system prefab via Inspector field)
///   - Drop power-up / currency pickup on death
///   - Variable score value per enemy type (e.g. boss = 100 pts)
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Tooltip("Total hits this enemy can take before dying.")]
    public int maxHealth = 3;

    private int _currentHealth;

    private void Start()
    {
        EnemyFaction ef = GetComponent<EnemyFaction>();
        if (ef != null)
        {
            string key = $"enemy.hp_{ef.faction.ToString().ToLower()}";
            maxHealth = BalanceService.Instance?.GetInt(key, maxHealth) ?? maxHealth;
        }
        _currentHealth = maxHealth;
    }

    /// <summary>
    /// Called by Bullet.cs on collision. Reduces health and triggers die if <= 0.
    /// Public so other damage sources (e.g. player ramming, area-of-effect) can call it directly.
    /// </summary>
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        // Visual hit feedback — flashes the renderer red for 0.1s
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public GameObject explosionPrefab;     // Regular enemy explosion (laserBlue08-11 anim)
    public GameObject bossExplosionPrefab; // Boss explosion (laserRed08-11 anim); falls back to explosionPrefab if null

    /// <summary>
    /// Called when health hits zero. Awards score, grants Metal, triggers VFX/drops, destroys self.
    /// </summary>
    void Die()
    {
        int points = BalanceService.Instance?.GetInt("score.enemy", GameConstants.ScoreEnemy) ?? GameConstants.ScoreEnemy;
        GameManager.Instance?.AddScore(points);

        EnemyFaction ef = GetComponent<EnemyFaction>();

        // Boss handles its own Metal award and logging
        BossController boss = GetComponent<BossController>();
        if (boss != null) boss.OnBossDeath();
        else EconomyService.Instance?.EarnMetalFromEnemy(ef?.faction);

        // Phase 6: drop powerup
        PowerupSpawner.Instance?.TryDrop(transform.position);

        // Explosion VFX — boss uses red laser anim, regular enemies use blue laser anim
        GameObject vfxPrefab = (boss != null && bossExplosionPrefab != null) ? bossExplosionPrefab : explosionPrefab;
        if (vfxPrefab != null)
            Instantiate(vfxPrefab, transform.position, Quaternion.identity);

        ScorePopup.Spawn(transform.position, points);

        GameLogger.EnemyKilledByFaction(
            ef != null ? ef.faction.ToString() : "Unknown",
            transform.position, points,
            GameManager.Instance?.Score ?? 0);

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }
}
