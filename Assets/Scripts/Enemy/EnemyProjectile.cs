using UnityEngine;

/// <summary>
/// EnemyProjectile — fired by Red faction enemies toward the player.
/// Moves in transform.up direction. Destroys itself on player contact (PlayerHealth handles damage).
/// Auto-destroys after lifeTime if it misses.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed    = 5f;
    public float lifeTime = 3f;

    void Start()
    {
        speed    = BalanceService.Instance?.GetFloat("enemy.red_projectile_speed", speed)    ?? speed;
        lifeTime = BalanceService.Instance?.GetFloat("enemy.red_projectile_lifetime", lifeTime) ?? lifeTime;
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Destroy(gameObject); // PlayerHealth.OnTriggerEnter2D handles the damage
    }
}
