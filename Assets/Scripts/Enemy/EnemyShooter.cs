using UnityEngine;

/// <summary>
/// EnemyShooter — attached to Red faction enemies.
/// Periodically fires an EnemyProjectile aimed at the player.
/// Staggered by half the fire rate on spawn so enemies don't all shoot simultaneously.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    public GameObject enemyProjectilePrefab; // Assign EnemyProjectile_Green.prefab for standard enemies
    [Tooltip("If set, used instead when this enemy is a Boss (Red laser).")]
    public GameObject bossProjectilePrefab;

    private float _nextShot;
    private Transform _player;
    private bool _isBoss;
    private EnemyProfile profile;

    /// <summary>Uses authored shot cadence; divers do not fire aimed shots.</summary>
    public void Configure(EnemyProfile data) { profile = data; }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _isBoss = GetComponent<BossController>() != null;
        float rate = profile ? profile.fireInterval : BalanceService.Instance?.GetFloat("enemy.red_fire_rate", 2f) ?? 2f;
        _nextShot = Time.time + rate * Random.Range(0.3f, 0.8f); // stagger first shot
    }

    void Update()
    {
        if (enemyProjectilePrefab == null || _player == null) return;
        if (GameManager.Instance && !GameManager.Instance.IsSessionRunning) return;
        if (profile && profile.behaviour != EnemyBehaviour.AimedShooter) return;

        float rate = profile ? profile.fireInterval : BalanceService.Instance?.GetFloat("enemy.red_fire_rate", 2f) ?? 2f;
        if (Time.time < _nextShot) return;

        Shoot();
        _nextShot = Time.time + rate;
    }

    void Shoot()
    {
        GameObject prefab = (_isBoss && bossProjectilePrefab != null) ? bossProjectilePrefab : enemyProjectilePrefab;
        if (prefab == null) return;
        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        Quaternion aimRot = Quaternion.Euler(0f, 0f, -angle);
        Instantiate(prefab, transform.position, aimRot);
    }
}
