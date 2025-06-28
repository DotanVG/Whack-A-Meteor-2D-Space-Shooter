using UnityEngine;
using UnityEngine.SceneManagement;

// Displays a simple main menu with buttons for navigating to the
// Game and Settings scenes.

public class MainMenuController : MonoBehaviour
{
    private bool confirmQuit = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            confirmQuit = true;
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
