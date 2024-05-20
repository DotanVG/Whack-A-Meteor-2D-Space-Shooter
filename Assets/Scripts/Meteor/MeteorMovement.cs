using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    public float speed = 5f; // Speed at which the meteor moves

    void Update()
    {
        // Move the meteor down the screen
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // Destroy the meteor if it goes off-screen
        if (transform.position.y < -Camera.main.orthographicSize - 1)
        {
            Destroy(gameObject);
        }
    }
}
