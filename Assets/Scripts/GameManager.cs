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
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        Lives -= 1;
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
        GUI.Label(new Rect(Screen.width - 320, 20, 300, 50), $"LIVES: {Lives}", hudStyle);

        if (showingCountdown)
        {
            int number = Mathf.CeilToInt(countdown);
            float alpha = countdown - (number - 1);
            Color prev = GUI.color;
            GUI.color = new Color(1f,1f,1f,alpha);
            GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-25, 100,50), number.ToString(), centerStyle);
            GUI.color = prev;
        }

        if (isPaused)
        {
            float pulse = (Mathf.Sin(Time.realtimeSinceStartup * 3f) + 1f) / 2f;
            Color prev = GUI.color;
            GUI.color = new Color(1f,1f,1f,pulse);
            GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-60, 100,30), "PAUSE", centerStyle);
            GUI.color = prev;
            if (GUI.Button(new Rect(Screen.width/2-50, Screen.height/2, 100,30), "Resume"))
            {
                TogglePause();
            }
            if (GUI.Button(new Rect(Screen.width/2-50, Screen.height/2+40, 100,30), "Quit Game"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
        }

        if (isGameOver)
        {
            GUI.Label(new Rect(Screen.width/2-150, Screen.height/2-30, 300,60), "GAME OVER", gameOverStyle);
            GUI.Label(new Rect(Screen.width/2-150, Screen.height/2+40, 300,30), "R - Restart    Enter/Esc - Menu", centerStyle);
        }
    }
}
