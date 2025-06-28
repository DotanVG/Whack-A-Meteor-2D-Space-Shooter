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
    private GUIStyle centerStyle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Lives = GameConstants.StartingLives;
        Score = 0;
        if (spawner == null)
        {
            spawner = FindObjectOfType<MeteorSpawner>();
        }
        if (player == null)
        {
            player = FindObjectOfType<PlayerHealth>();
        }
        centerStyle = new GUIStyle(GUI.skin.label);
        centerStyle.alignment = TextAnchor.MiddleCenter;
        centerStyle.fontSize = 40;
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
        else if (!isPaused && Lives <= 0)
        {
            EndGame();
        }

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused && Input.GetKeyDown(KeyCode.Q))
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
        Lives -= 1;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void EndGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 30), $"Score: {Score}");
        GUI.Label(new Rect(10, 40, 150, 30), $"Lives: {Lives}");

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
    }
}
