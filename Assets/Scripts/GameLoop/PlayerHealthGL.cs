using UnityEngine;

/// <summary>
/// PlayerHealthGL — 3D player health for the game-loop-foundation feature.
/// Handles damage when a meteor collides with the ship and updates the HUD via UIManager.
/// Renamed from PlayerHealth to avoid conflict with the existing 2D PlayerHealth.
///
/// SETUP:
///   - The Player GameObject needs a Collider with "Is Trigger" checked.
///   - Player must be tagged "Player".
///   - UI is handled by UIManager (no need to assign healthText here directly).
///
/// PLANNED UPGRADES:
///   - Invincibility frames after taking a hit (brief i-frames to avoid instant death from clusters)
///   - Shield system as a first upgrade tier (absorbs first N hits)
/// </summary>
public class PlayerHealthGL : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Starting and maximum health. Can later be modified by upgrades.")]
    public int maxHealth = 5;

    private int _currentHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealth(_currentHealth, maxHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Public so future hazards (traps, area damage) can also call this directly.
    /// </summary>
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        UIManager.Instance?.UpdateHealth(_currentHealth, maxHealth);

        if (_currentHealth <= 0) Die();
    }

    /// <summary>
    /// Delegates Game Over display to UIManager.
    /// ScoreManager.Instance.GetScore() is used to pass final score.
    /// TODO: add death VFX / camera shake here before calling ShowGameOver.
    /// </summary>
    void Die()
    {
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        UIManager.Instance?.ShowGameOver(finalScore);
    }
}
