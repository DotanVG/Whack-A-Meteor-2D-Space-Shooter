using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    public float delay = 10f; // Set the delay to 10 seconds 

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
