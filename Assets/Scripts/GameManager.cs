using UnityEngine;
using UnityEngine.SceneManagement;

// Guarantee GameManager.Start() runs before any service script.
// Services read Lives/Score in their own Start(), so GameManager must go first.
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Service-layer events (RunStateService subscribes to these) ────────────
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<int> OnLivesChanged;
    public static event System.Action      OnGameOver;
    public static event System.Action      OnRunStarted;

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
    private int lostLifeIndex = -1;

    // Session timer — starts when countdown ends, used by logging
    public float SessionTime { get; private set; } = 0f;
    private bool sessionRunning = false;

    public Texture2D lifeIcon;
    public Texture2D[] digitSprites;
    public Vector2 digitSize = new Vector2(16, 22);
    public Vector2 lifeIconSize = new Vector2(32, 32);

    private GUIStyle centerStyle;
    private GUIStyle hudStyle;
    private GUIStyle gameOverStyle;
    private InputManager inputManager;

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
        // Fire initial state so RunStateService mirrors correct values on first frame
        OnLivesChanged?.Invoke(Lives);
        OnScoreChanged?.Invoke(Score);
        OnRunStarted?.Invoke();
        if (spawner == null)
        {
            spawner = FindObjectOfType<MeteorSpawner>();
        }
        if (player == null)
        {
            player = FindObjectOfType<PlayerHealth>();
        }

        // Auto-create InputManager if it doesn't exist
        inputManager = InputManager.GetOrCreateInstance();
    }

    void Update()
    {
        if (sessionRunning && !isPaused && !isGameOver)
            SessionTime += Time.deltaTime;

        if (showingCountdown)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                showingCountdown = false;
                sessionRunning = true;
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

            if (inputManager != null)
            {
                if (inputManager.GetCancel() || inputManager.GetSubmit())
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("MainMenu");
                }
                else if (inputManager.GetRestart())
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("Game");
                }
            }
            else
            {
                // Fallback to old input system
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
            }

            if (gameOverTimer >= 3f)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
            return;
        }

        if (inputManager != null)
        {
            if (!isGameOver && inputManager.GetPause())
            {
                TogglePause();
            }

            if (!isGameOver && isPaused && inputManager.GetCancel())
            {
                SceneManager.LoadScene("MainMenu");
                Time.timeScale = 1f;
            }
        }
        else
        {
            // Fallback to old input system
            if (!isGameOver && (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)))
            {
                TogglePause();
            }

            if (!isGameOver && isPaused && Input.GetKeyDown(KeyCode.Q))
            {
                SceneManager.LoadScene("MainMenu");
                Time.timeScale = 1f;
            }
        }

        // Update lives hit animation timer
        if (livesRecentlyHit)
        {
            livesHitTimer += Time.deltaTime;
            if (livesHitTimer > 2f) // 1s shake+scale, 1s blink
            {
                livesRecentlyHit = false;
                livesHitTimer = 0f;
                lostLifeIndex = -1;
            }
        }
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        Lives -= 1;
        lostLifeIndex = Lives; // animate the leftmost remaining heart
        livesHitTimer = 0f;
        livesRecentlyHit = true;
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0)
        {
            StartGameOver();
        }
    }

    void StartGameOver()
    {
        sessionRunning = false;
        isGameOver = true;
        gameOverTimer = 0f;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
        // Log is deferred to let RunStateService/ProgressionService receive the event first
        int currentWave = FindObjectOfType<WaveManager>()?.currentWave ?? 0;
        GameLogger.PlayerGameOver(Score, currentWave, SessionTime);
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
        // XP/Score intentionally not shown — players see level-up popups instead.
        // LevelService accumulates XP from score events and fires OnLevelUp.

        float lifeWidth = lifeIconSize.x;
        float lifeHeight = lifeIconSize.y;
        float spacing = 5f;

        // draw remaining lives
        for (int i = 0; i < Lives; i++)
        {
            Rect r = new Rect(Screen.width - lifeWidth * (i + 1) - spacing * i - 20, 20, lifeWidth, lifeHeight);
            GUI.DrawTexture(r, lifeIcon);
        }

        // animate the lost life icon on the left
        if (livesRecentlyHit && lostLifeIndex >= 0)
        {
            Rect rect = new Rect(Screen.width - lifeWidth * (lostLifeIndex + 1) - spacing * lostLifeIndex - 20, 20, lifeWidth, lifeHeight);
            float t = livesHitTimer;

            Matrix4x4 prevMatrix = GUI.matrix;
            Color prevColor = GUI.color;

            // blinking alpha
            float blink = Mathf.PingPong(t * 6f, 1f) > 0.5f ? 1f : 0.2f;
            GUI.color = new Color(1f, 1f, 1f, blink);

            // scale animation during first second
            float scale = 1f;
            if (t < 1f)
            {
                scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
            }

            Vector2 center = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
            GUI.matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(scale, scale, 1f)) * Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one);
            GUI.DrawTexture(rect, lifeIcon);

            GUI.matrix = prevMatrix;
            GUI.color = prevColor;
        }

        if (showingCountdown)
        {
            int number = Mathf.CeilToInt(countdown);
            float alpha = countdown - (number - 1);
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            DrawDigit(number, new Rect(Screen.width / 2 - digitSize.x / 2, Screen.height / 2 - digitSize.y / 2, digitSize.x, digitSize.y));
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
            
            // Show appropriate controls based on input device
            string controlsText = "R - Restart    Enter/Esc - Menu";
            if (inputManager != null && inputManager.IsUsingGamepad())
            {
                controlsText = "Y - Restart    A/B - Menu";
            }
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 40, 300, 30), controlsText, centerStyle);
        }
    }

    void DrawDigit(int digit, Rect rect)
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        digit = Mathf.Clamp(digit, 0, digitSprites.Length - 1);
        Texture2D tex = digitSprites[digit];
        if (tex != null)
        {
            GUI.DrawTexture(rect, tex);
        }
    }

    void DrawNumber(Vector2 position, int number)
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        string s = Mathf.Max(0, number).ToString();
        for (int i = 0; i < s.Length; i++)
        {
            int d = s[i] - '0';
            Rect r = new Rect(position.x + i * digitSize.x, position.y, digitSize.x, digitSize.y);
            DrawDigit(d, r);
        }
    }
}
