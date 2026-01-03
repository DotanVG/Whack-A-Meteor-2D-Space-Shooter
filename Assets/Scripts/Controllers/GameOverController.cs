using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    private GUIStyle centerStyle;
    private InputManager inputManager;

    void Start()
    {
        centerStyle = new GUIStyle(GUI.skin.label);
        centerStyle.alignment = TextAnchor.MiddleCenter;
        centerStyle.fontSize = 20; // smaller game over menu text

        // Auto-create InputManager if it doesn't exist
        inputManager = InputManager.GetOrCreateInstance();
    }

    void Update()
    {
        if (inputManager != null)
        {
            // Handle submit or cancel to go to main menu
            if (inputManager.GetSubmit() || inputManager.GetCancel())
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width/2 - 100, Screen.height/2 - 40, 200, 80));
        GUILayout.Label($"Final Score: {GameManager.Instance?.Score ?? 0}", centerStyle);
        if (GUILayout.Button("Main Menu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        GUILayout.EndArea();
    }
}
