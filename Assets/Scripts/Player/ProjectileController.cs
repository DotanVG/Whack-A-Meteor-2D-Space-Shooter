using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 50f;  // Speed at which the projectile moves
    public float lifeTime = 2f; // Lifetime of the projectile

    void Start()
    {
        // Initialize the projectile's velocity in the direction it's facing
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;

        // Destroy the projectile after 'lifeTime' seconds if it doesn't hit anything
        Destroy(gameObject, lifeTime);

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

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hit an enemy
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Destroy the projectile
            Destroy(gameObject);

            // TODO: Implement logic for handling enemy hit
            // Add additional code here for what happens when the projectile hits an enemy
        }
        // Check if the projectile hit a meteor
        else if (other.gameObject.CompareTag("Meteor"))
        {
            // Destroy the projectile
            Destroy(gameObject);

            // TODO: Implement logic for handling meteor hit
            // Add additional code here for what happens when the projectile hits a meteor
        }
    }
}
