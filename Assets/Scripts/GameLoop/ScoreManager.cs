using UnityEngine;

/// <summary>
/// ScoreManager — singleton that tracks the player's score.
/// UI rendering is delegated to UIManager.
///
/// SETUP:
///   - Attach to the "GameManager" GameObject.
///   - UIManager must exist in the scene (handles the score TextMeshPro element).
///
/// PLANNED UPGRADES:
///   - High score persistence via PlayerPrefs
///   - Score multiplier system (combo chain increases multiplier, resets on hit)
///   - Different point values per enemy type (boss, fast meteor, etc.)
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int _score = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UIManager.Instance?.UpdateScore(_score);
    }

    /// <summary>Adds points and refreshes the HUD via UIManager.</summary>
    public void AddScore(int amount)
    {
        _score += amount;
        UIManager.Instance?.UpdateScore(_score);
        // TODO: trigger floating +score text at enemy position
    }

    /// <summary>Returns current score — used by PlayerHealthGL.Die() to pass to Game Over screen.</summary>
    public int GetScore() => _score;
}
