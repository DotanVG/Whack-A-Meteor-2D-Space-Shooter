using UnityEngine;

/// <summary>Initializes durability before hits and awards each death exactly once.</summary>
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public GameObject explosionPrefab;
    public GameObject bossExplosionPrefab;
    private Durability durability;
    private bool configured;
    private bool dead;
    public int Health => durability.Current;

    private void Awake() { durability = new Durability(maxHealth); }

    private void Start()
    {
        if (configured || GetComponent<BossController>()) return;
        var faction = GetComponent<EnemyFaction>();
        if (faction && BalanceService.Instance)
            maxHealth = BalanceService.Instance.GetInt("enemy.hp_" + faction.faction.ToString().ToLower(), maxHealth);
        Initialize(maxHealth);
    }

    /// <summary>Called immediately after spawning, before the first physics step.</summary>
    public void Initialize(int health)
    {
        maxHealth = Mathf.Max(1, health);
        durability = new Durability(maxHealth);
        configured = true;
        dead = false;
    }

    /// <summary>Positive damage only; further hits during deferred destruction do nothing.</summary>
    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        CombatFeedback.Hit(gameObject, amount);
        if (!durability.Apply(amount)) return;
        dead = true;
        Die();
    }

    private void Die()
    {
        int points = BalanceService.Instance?.GetInt("score.enemy", GameConstants.ScoreEnemy) ?? GameConstants.ScoreEnemy;
        GameManager.Instance?.AddScore(points);
        var faction = GetComponent<EnemyFaction>();
        var boss = GetComponent<BossController>();
        if (boss) boss.OnBossDeath();
        else if (EconomyService.Instance) EconomyService.Instance.EarnMetalFromEnemy(faction ? (Faction?)faction.faction : null);
        PowerupSpawner.Instance?.TryDrop(transform.position);
        var effect = boss && bossExplosionPrefab ? bossExplosionPrefab : explosionPrefab;
        if (effect) Instantiate(effect, transform.position, Quaternion.identity);
        ScorePopup.Spawn(transform.position, points);
        GameLogger.EnemyKilledByFaction(faction ? faction.faction.ToString() : "Unknown", transform.position, points, GameManager.Instance?.Score ?? 0);
        Destroy(gameObject);
    }
}
