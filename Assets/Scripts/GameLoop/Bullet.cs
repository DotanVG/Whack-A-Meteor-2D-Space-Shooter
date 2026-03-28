using UnityEngine;

/// <summary>
/// Bullet — attaches to the bullet prefab.
/// Handles collision with enemies and applies damage via EnemyHealth.
///
/// SETUP:
///   - The bullet prefab needs a Collider with "Is Trigger" checked.
///   - The bullet prefab needs a Rigidbody (velocity is set by AutoShooter.Shoot).
///   - Enemy GameObjects must be tagged "Enemy" and have EnemyHealth attached.
///
/// NOTE: Using OnTriggerEnter (not OnCollisionEnter) so bullets pass through
/// geometry cleanly. If you want physics bouncing off walls, switch to OnCollisionEnter.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("Damage dealt to the enemy on hit. Can be upgraded via future WeaponStats ScriptableObject.")]
    public int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Delegate damage calculation to the enemy — keeps concerns separated
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // Destroy the bullet on impact regardless of whether the enemy had health
            Destroy(gameObject);
        }
    }
}
