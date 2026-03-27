using UnityEngine;
using TMPro;

/// <summary>
/// UIManager — central hub for all in-game HUD elements.
/// Keeps UI logic out of gameplay scripts (PlayerHealthGL, ScoreManager).
///
/// SETUP:
///   1. Attach to a persistent "UIManager" GameObject in the scene.
///   2. Assign all TextMeshProUGUI references in the Inspector.
///   3. Assign the GameOverPanel (a UI Panel, hidden by default).
///
/// PANELS:
///   - HUD: score, HP, wave number — always visible during gameplay
///   - GameOverPanel: shown when player dies — has Restart button
///
/// PLANNED UPGRADES:
///   - Main menu / title screen
///   - Upgrade shop panel between waves
///   - Animated score pop (+10 floating text at enemy death position)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI waveText;

    [Header("Game Over")]
    public GameObject gameOverPanel;        // Assign a Panel UI object, set inactive by default
    public TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>Called by ScoreManager.UpdateUI every time score changes.</summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    /// <summary>Called by PlayerHealthGL.UpdateUI every time HP changes.</summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = $"HP: {current}/{max}";
    }

    /// <summary>Called by WaveManager when a new wave starts.</summary>
    public void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = $"Wave {wave}";
    }

    /// <summary>
    /// Called by PlayerHealthGL.Die().
    /// Freezes time and shows the Game Over panel.
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"Score: {finalScore}";
        Time.timeScale = 0f; // Freeze — resume on restart
    }

    /// <summary>
    /// Called by the Restart button (wire via Inspector → Button OnClick).
    /// Reloads the active scene and un-freezes time.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
