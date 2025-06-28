using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    public float minSpeed = 3f; // Minimum speed at which the meteor moves
    public float maxSpeed = 7f; // Maximum speed at which the meteor moves
    private Vector3 direction; // Direction in which the meteor moves
    private float speed; // Speed of the meteor
    private float rotationSpeed; // Speed and direction of rotation

    public Vector3 CurrentDirection => direction;
    public float CurrentSpeed => speed;

    // Camera and margin
    private Camera mainCamera;
    private float spawnMargin = 1f;

    void Start()
    {
        if (speed <= 0f)
        {
            speed = Random.Range(minSpeed, maxSpeed);
        }
        rotationSpeed = Random.Range(-180f, 180f);

        MeteorSpawner spawner = FindObjectOfType<MeteorSpawner>();
        if (spawner != null)
        {
            spawnMargin = spawner.spawnMargin;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MeteorMovement requires a camera tagged as 'MainCamera'");
        }
    }

    public void SetInitialDirection(Vector3 initialDirection)
    {
        direction = initialDirection.normalized;
    }

    public void InitializeMovement(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        rotationSpeed = Random.Range(-180f, 180f);
    }

    void Update()
    {
        // Move the meteor in the assigned direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Rotate the meteor around its own axis
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Check if the meteor should be destroyed
        CheckDestroyCondition();
    }

    private void CheckDestroyCondition()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 min = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        if (transform.position.x < min.x - spawnMargin ||
            transform.position.x > max.x + spawnMargin ||
            transform.position.y < min.y - spawnMargin ||
            transform.position.y > max.y + spawnMargin)
        {
            Destroy(gameObject);
        }
    }
}