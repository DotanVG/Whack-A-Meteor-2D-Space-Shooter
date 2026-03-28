using UnityEngine;

/// <summary>
/// EnemyShooter — attached to Red faction enemies.
/// Periodically fires an EnemyProjectile aimed at the player.
/// Staggered by half the fire rate on spawn so enemies don't all shoot simultaneously.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    public GameObject enemyProjectilePrefab; // Assign EnemyProjectile.prefab in Inspector

    private float _nextShot;
    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        float rate = BalanceService.Instance?.GetFloat("enemy.red_fire_rate", 2f) ?? 2f;
        _nextShot = Time.time + rate * Random.Range(0.3f, 0.8f); // stagger first shot
    }

    void Update()
    {
        if (enemyProjectilePrefab == null || _player == null) return;

        float rate = BalanceService.Instance?.GetFloat("enemy.red_fire_rate", 2f) ?? 2f;
        if (Time.time < _nextShot) return;

        Shoot();
        _nextShot = Time.time + rate;
    }

    void Shoot()
    {
        // Aim toward player: rotate so transform.up points at the player
        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        Quaternion aimRot = Quaternion.Euler(0f, 0f, -angle);
        Instantiate(enemyProjectilePrefab, transform.position, aimRot);
    }
}
