using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    private GUIStyle centerStyle;

    void Start()
    {
        centerStyle = new GUIStyle(GUI.skin.label);
        centerStyle.alignment = TextAnchor.MiddleCenter;
        centerStyle.fontSize = 40;
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
