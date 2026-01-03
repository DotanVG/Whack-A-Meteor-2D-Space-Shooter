using UnityEngine;
using UnityEngine.SceneManagement;

// Displays a simple main menu with buttons for navigating to the
// Game and Settings scenes.

public class MainMenuController : MonoBehaviour
{
    private bool confirmQuit = false;
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
            if (inputManager.GetCancel())
            {
                if (confirmQuit)
                {
                    confirmQuit = false;
                }
                else
                {
                    confirmQuit = true;
                }
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
                    selectedButtonIndex = Mathf.Min(confirmQuit ? 1 : 2, selectedButtonIndex + 1);
                }
                lastNavigateTime = Time.time;
            }

            // Handle submit (A button on gamepad, Enter on keyboard)
            if (inputManager.GetSubmit())
            {
                if (confirmQuit)
                {
                    if (selectedButtonIndex == 0) // Yes
                    {
                        Application.Quit();
                    }
                    else // No
                    {
                        confirmQuit = false;
                        selectedButtonIndex = 0;
                    }
                }
                else
                {
                    switch (selectedButtonIndex)
                    {
                        case 0:
                            StartGame();
                            break;
                        case 1:
                            OpenSettings();
                            break;
                        case 2:
                            confirmQuit = true;
                            selectedButtonIndex = 0; // Reset to "Yes" in quit dialog
                            break;
                    }
                }
            }
        }
        else
        {
            // Fallback to old input system
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                confirmQuit = true;
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 70, 200, 140));
        if (!confirmQuit)
        {
            if (GUILayout.Button("New Game"))
            {
                StartGame();
            }
            if (GUILayout.Button("Settings"))
            {
                OpenSettings();
            }
            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }
        else
        {
            GUILayout.Label("Are you sure you want to quit Wack-A-Meteor?");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                Application.Quit();
            }
            if (GUILayout.Button("No"))
            {
                confirmQuit = false;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
