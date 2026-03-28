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

    private InputManager inputManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        inputManager = InputManager.GetOrCreateInstance();

        // Apply skill-tree multipliers (null-safe)
        movementSpeed *= SkillService.Instance?.GetMoveSpeedMultiplier()       ?? 1f;
        boostDuration *= SkillService.Instance?.GetBoostDurationMultiplier()   ?? 1f;
    }

    private float shootInterval = 0.2f;
    private float lastShootTime = 0.0f;

    void Update()
    {
        // Use InputManager if available, otherwise fallback to old Input system
        if (inputManager == null)
        {
            // Fallback to old Input system for keyboard/mouse
            UpdateWithOldInput();
            return;
        }

        // Rotation control - support analog input from gamepad
        float rotateLeft = inputManager.GetRotateLeft();
        float rotateRight = inputManager.GetRotateRight();
        
        if (rotateLeft > 0f)
        {
            transform.Rotate(0, 0, Time.deltaTime * rotationSpeed * rotateLeft); // Scale by input magnitude for analog support
        }
        if (rotateRight > 0f)
        {
            transform.Rotate(0, 0, -Time.deltaTime * rotationSpeed * rotateRight); // Scale by input magnitude for analog support
        }

        // Movement control - support analog input from gamepad
        float moveForward = inputManager.GetMoveForward();
        float moveBackward = inputManager.GetMoveBackward();

        if (moveForward > 0f)
        {
            rb.velocity = transform.up * movementSpeed * moveForward; // Scale by input magnitude for analog support
        }

        // Rapid deceleration
        if (moveBackward > 0f)
        {
            rb.velocity *= 0.5f; // Adjust deceleration factor as needed for more responsiveness
        }

        // Maintain speed with Boost
        if (inputManager.GetBoostDown() && Time.time - lastBoostTime >= boostCooldown)
        {
            rb.velocity = transform.up * movementSpeed * boostMultiplier;
            lastBoostTime = Time.time;
            boostEndTime = Time.time + boostDuration;
            GameLogger.PlayerBoost(boostCooldown, boostMultiplier);
        }
        else if (Time.time < boostEndTime)
        {
            // Continue boosted velocity
            rb.velocity = transform.up * movementSpeed * boostMultiplier;
        }
        else if (moveForward <= 0f && moveBackward <= 0f)
        {
            // Gradual velocity reduction when no input
            rb.velocity *= 0.999f;
        }

        // Manual shooting disabled — AutoShooter handles firing automatically.
        // Re-enable this block when spacebar shot is added back as an unlockable skill.
        // if (inputManager.GetShoot() && Time.time - lastShootTime >= shootInterval)
        // {
        //     Shoot();
        //     lastShootTime = Time.time;
        // }
    }

    // Fallback method using old Input system
    private void UpdateWithOldInput()
    {
        // Rotation control
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, 0, Time.deltaTime * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, -Time.deltaTime * rotationSpeed);
        }

        // Movement control
        if (Input.GetKey(KeyCode.W))
        {
            rb.velocity = transform.up * movementSpeed;
        }

        // Rapid deceleration with 'S'
        if (Input.GetKey(KeyCode.S))
        {
            rb.velocity *= 0.5f;
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

        // Manual shooting disabled — AutoShooter handles firing automatically.
        // Re-enable when spacebar shot is added back as an unlockable skill.
        // if (Input.GetKey(KeyCode.Space) && Time.time - lastShootTime >= shootInterval)
        // {
        //     Shoot();
        //     lastShootTime = Time.time;
        // }
    }

    void Shoot()
    {
        // Instantiate a projectile at the spawn point with the player's rotation
        Instantiate(projectilePrefab, projectileSpawnPoint.position, transform.rotation);
    }
}
