using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    // Delay before loading the main menu
    public float delay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("LoadMainMenu", delay);
    }

    // LoadMainMenu is called after the delay
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
