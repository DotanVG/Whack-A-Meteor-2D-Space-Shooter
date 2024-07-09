using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float rotationSpeed = 250;

    private Rigidbody2D rb;
    private float lastBoostTime = -5.0f; // Initialize to -5 so the boost can be used immediately
    private float boostEndTime = 0.0f;
    private float boostDuration = 1.5f;
    private float boostCooldown = 5.0f;
    private float boostMultiplier = 3.0f; // Change this to control the strength of the boost

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private float shootInterval = 0.2f;
    private float lastShootTime = 0.0f;

    void Update()
    {
        // Rotation control
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, 0, Time.deltaTime * rotationSpeed); // Adjust rotation speed as needed
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, -Time.deltaTime * rotationSpeed); // Adjust rotation speed as needed
        }

        // Movement control
        if (Input.GetKey(KeyCode.W))
        {
            rb.velocity = transform.up * movementSpeed;
        }

        // Rapid deceleration with 'S'
        if (Input.GetKey(KeyCode.S))
        {
            rb.velocity *= 0.5f; // Adjust deceleration factor as needed for more responsiveness
        }

        // Maintain speed with Shift
        if (Input.GetKey(KeyCode.LeftShift) && Time.time - lastBoostTime >= boostCooldown)
        {
            rb.velocity = transform.up * movementSpeed * boostMultiplier;
            lastBoostTime = Time.time;
            boostEndTime = Time.time + boostDuration;
        }
        else if (Time.time < boostEndTime)
        {
            // Continue boosted velocity
            rb.velocity = transform.up * movementSpeed * boostMultiplier;
        }
        else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            // Gradual velocity reduction
            rb.velocity *= 0.999f;
        }

        // Shooting with Space
        if (Input.GetKey(KeyCode.Space) && Time.time - lastShootTime >= shootInterval)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        // Instantiate a projectile at the spawn point with the player's rotation
        Instantiate(projectilePrefab, projectileSpawnPoint.position, transform.rotation);
    }
}
