using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 50f;  // Speed at which the projectile moves
    public float lifeTime = 2f; // Lifetime of the projectile
    public AudioClip enemyHitClip;
    public GameObject hitParticles;

    void Start()
    {
        // Initialize the projectile's velocity in the direction it's facing
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;

        // Destroy the projectile after 'lifeTime' seconds if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hit an enemy
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (enemyHitClip != null)
                AudioSource.PlayClipAtPoint(enemyHitClip, transform.position);
            if (hitParticles != null)
                Instantiate(hitParticles, other.transform.position, Quaternion.identity);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(GameConstants.ScoreEnemy);
                GameLogger.EnemyKilledByProjectile(
                    (Vector2)other.transform.position,
                    GameConstants.ScoreEnemy,
                    GameManager.Instance.Score);
            }
            // Metal from enemy kills — wired when new enemy ship types are added (Phase 2+)
            // EconomyService.Instance?.EarnMetalFromEnemy();
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        // Check if the projectile hit a meteor
        else if (other.gameObject.CompareTag("BigBrownMeteor") ||
                 other.gameObject.CompareTag("BigGreyMeteor") ||
                 other.gameObject.CompareTag("MediumBrownMeteor") ||
                 other.gameObject.CompareTag("MediumGreyMeteor") ||
                 other.gameObject.CompareTag("SmallBrownMeteor") ||
                 other.gameObject.CompareTag("SmallGreyMeteor") ||
                 other.gameObject.CompareTag("TinyBrownMeteor") ||
                 other.gameObject.CompareTag("TinyGreyMeteor"))
        {
            MeteorSplit meteorSplit = other.gameObject.GetComponent<MeteorSplit>();
            if (meteorSplit != null)
            {
                meteorSplit.OnProjectileHit();
            }
            Destroy(gameObject); // Destroy the projectile
        }
    }
}