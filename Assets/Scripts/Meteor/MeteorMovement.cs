using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    public float minSpeed = 3f; // Minimum speed at which the meteor moves
    public float maxSpeed = 7f; // Maximum speed at which the meteor moves
    private Vector3 direction; // Direction in which the meteor moves
    private float speed; // Speed of the meteor
    private float rotationSpeed; // Speed and direction of rotation

    // Ellipse parameters
    private float ellipseWidth;
    private float ellipseHeight;
    private Vector3 ellipseCenter;

    // New variables
    private bool hasEnteredPlayArea = false;
    private float destroyThreshold = 1.2f; // Slightly larger than the spawn ellipse

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        rotationSpeed = Random.Range(-180f, 180f);

        // Get ellipse parameters from MeteorSpawner
        MeteorSpawner spawner = FindObjectOfType<MeteorSpawner>();
        if (spawner != null)
        {
            ellipseWidth = spawner.ellipseWidthFactor * Camera.main.orthographicSize * Camera.main.aspect;
            ellipseHeight = spawner.ellipseHeightFactor * Camera.main.orthographicSize;
            ellipseCenter = Camera.main.transform.position;
        }
        else
        {
            Debug.LogError("MeteorSpawner not found in the scene!");
        }
    }

    public void SetInitialDirection(Vector3 initialDirection)
    {
        direction = initialDirection.normalized;
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
        if (!hasEnteredPlayArea)
        {
            // Check if the meteor has entered the play area
            if (IsInsideEllipse(1.0f))
            {
                hasEnteredPlayArea = true;
            }
        }
        else
        {
            // Check if the meteor has left the enlarged destroy area
            if (!IsInsideEllipse(destroyThreshold))
            {
                Destroy(gameObject);
            }
        }
    }

    private bool IsInsideEllipse(float threshold)
    {
        Vector3 localPos = transform.position - ellipseCenter;
        float xComponent = (localPos.x * localPos.x) / (ellipseWidth * ellipseWidth);
        float yComponent = (localPos.y * localPos.y) / (ellipseHeight * ellipseHeight);

        return xComponent + yComponent <= threshold;
    }
}