using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    private float exampleSetting = 0.5f;
    private bool  changesMade    = false;

    // Reset progression confirmation state
    private bool _confirmReset = false;

    private InputManager inputManager;
    private int   selectedButtonIndex = 0;
    private float lastNavigateTime    = 0f;
    private const float NAVIGATE_COOLDOWN = 0.3f;

    // Cached red style for the reset button
    private GUIStyle _redButtonStyle;
    private GUIStyle _warningLabelStyle;

    // Total buttons when not in confirm state: Save, Back, Reset (3)
    private const int NORMAL_BUTTON_COUNT = 2; // Save=0, Back=1 (Reset is separate)

    void Start()
    {
        inputManager = InputManager.GetOrCreateInstance();
    }

    void Update()
    {
        if (inputManager == null) return;

        if (inputManager.GetCancel())
        {
            if (_confirmReset)
                _confirmReset = false;
            else
                SceneManager.LoadScene("MainMenu");
        }

        Vector2 navigate = inputManager.GetNavigate();
        if (navigate.magnitude > 0.5f && Time.time - lastNavigateTime > NAVIGATE_COOLDOWN)
        {
            int maxIndex = _confirmReset ? 1 : 2;
            if (navigate.y > 0.5f)
                selectedButtonIndex = Mathf.Max(0, selectedButtonIndex - 1);
            else if (navigate.y < -0.5f)
                selectedButtonIndex = Mathf.Min(maxIndex, selectedButtonIndex + 1);

            if (navigate.x != 0f && !_confirmReset)
            {
                exampleSetting = Mathf.Clamp01(exampleSetting + navigate.x * 0.1f);
                changesMade = true;
            }
            lastNavigateTime = Time.time;
        }

        if (inputManager.GetSubmit())
        {
            if (_confirmReset)
            {
                if (selectedButtonIndex == 0) ExecuteReset();
                else                          _confirmReset = false;
            }
            else
            {
                switch (selectedButtonIndex)
                {
                    case 0: changesMade = false; break;             // Save
                    case 1: SceneManager.LoadScene("MainMenu"); break; // Back
                    case 2: _confirmReset = true; selectedButtonIndex = 1; break; // Reset
                }
            }
        }
    }

    void OnGUI()
    {
        EnsureStyles();

        if (_confirmReset)
        {
            DrawResetConfirmation();
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 120, Screen.height / 2 - 100, 240, 200));

        GUILayout.Label("Settings");

        // Example slider (placeholder — real audio/graphic settings go here later)
        GUILayout.Label("Example Setting");
        float newVal = GUILayout.HorizontalSlider(exampleSetting, 0f, 1f);
        if (newVal != exampleSetting) { exampleSetting = newVal; changesMade = true; }

        GUILayout.Space(8);
        if (GUILayout.Button("Save"))   changesMade = false;
        if (GUILayout.Button(changesMade ? "Cancel" : "Back")) SceneManager.LoadScene("MainMenu");

        GUILayout.Space(16);

        // Red reset button
        if (GUILayout.Button("Reset Progression", _redButtonStyle))
        {
            _confirmReset = true;
            selectedButtonIndex = 1; // default highlight "No"
        }

        GUILayout.EndArea();
    }

    void DrawResetConfirmation()
    {
        float w = 360f, h = 160f;
        float x = Screen.width  / 2f - w / 2f;
        float y = Screen.height / 2f - h / 2f;

        GUI.Box(new Rect(x - 8, y - 8, w + 16, h + 16), GUIContent.none);

        GUILayout.BeginArea(new Rect(x, y, w, h));
        GUILayout.Label("Reset ALL progression?", _warningLabelStyle);
        GUILayout.Space(6);
        GUILayout.Label("This will permanently delete:\n" +
                         "  • Stardust  • Metal  • XP  • Level",
                         GUI.skin.label);
        GUILayout.Space(12);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Yes — Reset Everything", _redButtonStyle))
            ExecuteReset();
        if (GUILayout.Button("No — Keep My Progress"))
            _confirmReset = false;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void ExecuteReset()
    {
        PlayerPrefs.DeleteKey("Economy.Stardust");
        PlayerPrefs.DeleteKey("Economy.Metal");
        PlayerPrefs.DeleteKey("Progression.TotalXP");
        PlayerPrefs.DeleteKey("Progression.Level");
        PlayerPrefs.Save();
        _confirmReset = false;
        selectedButtonIndex = 0;
        Debug.Log("[Settings] Progression reset — all persistent data cleared.");
    }

    void EnsureStyles()
    {
        if (_redButtonStyle != null) return;

        Texture2D redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, new Color(0.72f, 0.1f, 0.1f));
        redTex.Apply();

        Texture2D redHoverTex = new Texture2D(1, 1);
        redHoverTex.SetPixel(0, 0, new Color(0.85f, 0.15f, 0.15f));
        redHoverTex.Apply();

        _redButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal  = { background = redTex,      textColor = Color.white },
            hover   = { background = redHoverTex, textColor = Color.white },
            active  = { background = redTex,      textColor = Color.white },
        };

        _warningLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize  = 16,
            normal    = { textColor = new Color(1f, 0.4f, 0.2f) },
        };
    }
}
