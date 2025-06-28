using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    private bool changesMade;
    private float exampleSetting = 0.5f;

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
