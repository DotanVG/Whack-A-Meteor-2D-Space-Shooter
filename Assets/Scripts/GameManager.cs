using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void EndGame()
    {
        // Load the Game Over scene
        SceneManager.LoadScene("GameOver");
    }

    // TODO: Implement other game management features such as keeping score here
}
