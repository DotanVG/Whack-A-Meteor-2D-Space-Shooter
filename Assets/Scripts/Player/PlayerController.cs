using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

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

void Update()
{
    Vector3 MousePos = Input.mousePosition;
    MousePos = Camera.main.ScreenToWorldPoint(MousePos);    

    Vector2 rotation = new Vector2(MousePos.x - transform.position.x, MousePos.y - transform.position.y);

    transform.up = rotation;

    if (Input.GetKey(KeyCode.Space))
    {
        // Check if the left shift key is pressed and if enough time has passed since the last boost
        if (Input.GetKey(KeyCode.LeftShift) && Time.time - lastBoostTime >= boostCooldown)
        {
            // Apply the boost
            rb.velocity = transform.up * movementSpeed * boostMultiplier;

            // Update the last boost time and the boost end time
            lastBoostTime = Time.time;
            boostEndTime = Time.time + boostDuration;
        }
        else if (Time.time < boostEndTime)
        {
            // If the boost is still active, keep the boosted velocity
            rb.velocity = transform.up * movementSpeed * boostMultiplier;
        }
        else
        {
            rb.velocity = transform.up * movementSpeed;
        }
    }
    else
    {
        // Reduce the velocity very gradually to simulate floating
        rb.velocity *= 0.999f;
    }
}

    void Shoot()
    {
        // Instantiate a projectile at the spawn point
        Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
    }
}
