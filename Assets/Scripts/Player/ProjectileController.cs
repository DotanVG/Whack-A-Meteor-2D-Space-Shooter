using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 20f;  // Speed at which the projectile moves


    void Start()
    {
        // Initialize the projectile's velocity in the direction it's facing
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;

        // Calculate the screen bounds using the camera's viewport
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ProjectileController requires a main camera tagged as 'MainCamera'");
            return;
        }
    }

    void Update()
    {
    }

}
