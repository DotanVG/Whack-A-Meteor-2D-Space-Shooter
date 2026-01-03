using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    private bool changesMade;
    private float exampleSetting = 0.5f;
    private InputManager inputManager;
    private int selectedButtonIndex = 0;
    private float lastNavigateTime = 0f;
    private const float NAVIGATE_COOLDOWN = 0.3f;

    void Start()
    {
        // Auto-create InputManager if it doesn't exist
        inputManager = InputManager.GetOrCreateInstance();
    }

    void Update()
    {
        if (inputManager != null)
        {
            // Handle cancel/back
            if (inputManager.GetCancel())
            {
                SceneManager.LoadScene("MainMenu");
            }

            // Handle gamepad navigation
            Vector2 navigate = inputManager.GetNavigate();
            if (navigate.magnitude > 0.5f && Time.time - lastNavigateTime > NAVIGATE_COOLDOWN)
            {
                if (navigate.y > 0.5f)
                {
                    selectedButtonIndex = Mathf.Max(0, selectedButtonIndex - 1);
                }
                else if (navigate.y < -0.5f)
                {
                    selectedButtonIndex = Mathf.Min(1, selectedButtonIndex + 1);
                }
                lastNavigateTime = Time.time;
            }

            // Handle slider adjustment with gamepad
            if (navigate.x != 0f && Time.time - lastNavigateTime > NAVIGATE_COOLDOWN * 0.5f)
            {
                exampleSetting = Mathf.Clamp01(exampleSetting + navigate.x * 0.1f);
                changesMade = true;
                lastNavigateTime = Time.time;
            }

            // Handle submit
            if (inputManager.GetSubmit())
            {
                if (selectedButtonIndex == 0) // Save
                {
                    changesMade = false;
                }
                else // Back/Cancel
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 75, 200, 150));
        GUILayout.Label("Settings Placeholder");
        float newVal = GUILayout.HorizontalSlider(exampleSetting, 0f, 1f);
        if (newVal != exampleSetting)
        {
            exampleSetting = newVal;
            changesMade = true;
        }

        if (GUILayout.Button("Save"))
        {
            // Placeholder save action
            changesMade = false;
        }

        if (GUILayout.Button(changesMade ? "Cancel" : "Back"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        GUILayout.EndArea();
    }
}
