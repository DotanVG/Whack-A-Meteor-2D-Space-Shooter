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

    /// <summary>
    /// Called when health hits zero.
    /// Notifies ScoreManager, then destroys this GameObject.
    /// TODO: instantiate an explosion VFX prefab here before Destroy.
    /// TODO: spawn a collectible/powerup at transform.position.
    /// </summary>
    void Die()
    {
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(10);
        }

        // TODO: Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    /// <summary>
    /// Briefly tints the mesh renderer red to signal a hit.
    /// NOTE: uses material.color which creates a material instance per mesh.
    /// Fine for prototyping — switch to MaterialPropertyBlock before shipping.
    /// </summary>
    private System.Collections.IEnumerator FlashRed()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = original;
    }
}
