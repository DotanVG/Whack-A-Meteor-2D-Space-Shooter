using UnityEngine;
using UnityEngine.SceneManagement;

// Displays a simple main menu with buttons for navigating to the
// Game and Settings scenes.

public class MainMenuController : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100));
        if (GUILayout.Button("New Game"))
        {
            StartGame();
        }
        if (GUILayout.Button("Settings"))
        {
            OpenSettings();
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
