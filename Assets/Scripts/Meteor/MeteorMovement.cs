using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    public float minSpeed = 3f; // Minimum speed at which the meteor moves
    public float maxSpeed = 7f; // Maximum speed at which the meteor moves
    private Vector3 direction; // Direction in which the meteor moves
    private float speed; // Speed of the meteor
    private float rotationSpeed; // Speed and direction of rotation

    void Start()
    {
        // Assign a random direction and speed when the meteor is instantiated
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        speed = Random.Range(minSpeed, maxSpeed);
        // Assign a random rotation speed between -45 and 45 degrees per second
        // Negative for counterclockwise, positive for clockwise
        rotationSpeed = Random.Range(-180f, 180f);
        Debug.Log("Meteor moving with speed: " + speed + " in direction: " + direction + " and rotating at " + rotationSpeed + " degrees per second.");
    }

    void Update()
    {
        // Move the meteor in the assigned direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Rotate the meteor around its own axis
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Destroy the meteor if it goes off-screen
        if (transform.position.y < -Camera.main.orthographicSize - 1 ||
            transform.position.y > Camera.main.orthographicSize + 1 ||
            transform.position.x < -Camera.main.aspect * Camera.main.orthographicSize - 1 ||
            transform.position.x > Camera.main.aspect * Camera.main.orthographicSize + 1)
        {
            Destroy(gameObject);
        }
    }
}