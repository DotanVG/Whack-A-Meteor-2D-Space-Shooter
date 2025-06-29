using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MeteorSpawner spawner;
    public PlayerHealth player;

    public int Score { get; private set; }
    public int Lives { get; private set; }

    private float countdown = 3f;
    private bool showingCountdown = true;
    private bool isPaused = false;
    private bool isGameOver = false;
    private float gameOverTimer = 0f;

    private float livesHitTimer = 0f;
    private bool livesRecentlyHit = false;

    private GUIStyle centerStyle;
    private GUIStyle hudStyle;
    private GUIStyle gameOverStyle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // initialize GUI styles without using GUI.skin since Awake can run
        // outside of the OnGUI context
        hudStyle = new GUIStyle();
        hudStyle.fontSize = 16; // smaller HUD text
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.normal.textColor = Color.white;

        centerStyle = new GUIStyle();
        centerStyle.alignment = TextAnchor.MiddleCenter;
        centerStyle.fontSize = 30; // half size countdown and pause text
        centerStyle.fontStyle = FontStyle.Bold;
        centerStyle.normal.textColor = Color.white;

        gameOverStyle = new GUIStyle();
        gameOverStyle.alignment = TextAnchor.MiddleCenter;
        gameOverStyle.fontSize = 40; // smaller game over text
        gameOverStyle.fontStyle = FontStyle.Bold;
        gameOverStyle.normal.textColor = Color.white;
    }

    void Start()
    {
        Lives = GameConstants.StartingLives;
        Score = 0;
        isPaused = false;
        isGameOver = false;
        showingCountdown = true;
        countdown = 3f;
        if (spawner == null)
        {
            spawner = FindObjectOfType<MeteorSpawner>();
        }
        if (player == null)
        {
            player = FindObjectOfType<PlayerHealth>();
        }
    }

    void Update()
    {
        if (showingCountdown)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                showingCountdown = false;
                if (spawner != null)
                {
                    spawner.StartSpawning();
                }
            }
        }
        else if (!isPaused && !isGameOver && Lives <= 0)
        {
            StartGameOver();
        }

        if (isGameOver)
        {
            gameOverTimer += Time.unscaledDeltaTime;

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("Game");
            }
            else if (gameOverTimer >= 3f)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
            return;
        }

        if (!isGameOver && (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)))
        {
            TogglePause();
        }

        if (!isGameOver && isPaused && Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene("MainMenu");
            Time.timeScale = 1f;
        }

        // Update lives hit animation timer
        if (livesRecentlyHit)
        {
            livesHitTimer += Time.deltaTime;
            if (livesHitTimer > 2f) // 1s shake+scale, 1s blink
            {
                livesRecentlyHit = false;
                livesHitTimer = 0f;
            }
        }
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        Lives -= 1;
        // Only animate if lives remain
        if (Lives > 0)
        {
            livesHitTimer = 0f;
            livesRecentlyHit = true;
        }
        if (Lives <= 0)
        {
            StartGameOver();
        }
    }

    void StartGameOver()
    {
        isGameOver = true;
        gameOverTimer = 0f;
        Time.timeScale = 0f;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void EndGame()
    {
        StartGameOver();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 50), $"SCORE: {Score}", hudStyle);

        // Animate LIVES label if recently hit
        Rect livesRect = new Rect(Screen.width - 120, 20, 100, 50);
        GUIStyle livesStyle = hudStyle;

        if (livesRecentlyHit)
        {
            float animTime = livesHitTimer;
            Matrix4x4 oldMatrix = GUI.matrix;
            Vector2 center = new Vector2(livesRect.x + livesRect.width / 2, livesRect.y + livesRect.height / 2);

            if (animTime < 1f)
            {
                // Scale up and shake for 1 second
                float scale = 1.0f + Mathf.Sin(animTime * 12f) * 0.18f + Mathf.Lerp(0.25f, 0f, animTime / 1f);
                float shake = Mathf.Sin(animTime * 40f) * 6f;
                GUI.matrix = Matrix4x4.TRS(
                    new Vector3(center.x + shake, center.y, 0),
                    Quaternion.identity,
                    new Vector3(scale, scale, 1)
                ) * Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one);
            }
            else if (animTime < 2f)
            {
                // Blink for 1 second
                float blink = Mathf.PingPong((animTime - 1f) * 6f, 1f);
                Color prevColor = GUI.color;
                GUI.color = new Color(1, 1, 1, blink > 0.5f ? 1f : 0.2f);
                GUI.Label(livesRect, $"LIVES: {Lives}", livesStyle);
                GUI.color = prevColor;
                return; // Don't draw again below
            }

            GUI.Label(livesRect, $"LIVES: {Lives}", livesStyle);
            GUI.matrix = oldMatrix;
        }
        else
        {
            GUI.Label(livesRect, $"LIVES: {Lives}", livesStyle);
        }

        if (showingCountdown)
        {
            int number = Mathf.CeilToInt(countdown);
            float alpha = countdown - (number - 1);
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), number.ToString(), centerStyle);
            GUI.color = prev;
        }

        if (isPaused)
        {
            float pulse = (Mathf.Sin(Time.realtimeSinceStartup * 3f) + 1f) / 2f;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, pulse);
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 60, 100, 30), "PAUSE", centerStyle);
            GUI.color = prev;
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2, 100, 30), "Resume"))
            {
                TogglePause();
            }
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 40, 100, 30), "Quit Game"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
        }

        if (isGameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 30, 300, 60), "GAME OVER", gameOverStyle);
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 40, 300, 30), "R - Restart    Enter/Esc - Menu", centerStyle);
        }
    }
}
