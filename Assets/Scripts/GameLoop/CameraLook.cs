using UnityEngine;

/// <summary>
/// CameraLook — attaches to the Camera child of the Player.
/// Handles mouse-look: vertical rotation on the camera, horizontal rotation on the player body.
///
/// SETUP:
///   - Attach to the Camera GameObject that is a CHILD of the Player.
///   - The Player parent handles yaw (left/right); this camera handles pitch (up/down).
///   - Works with the default Unity Input System (Mouse X / Mouse Y axes).
///
/// KNOWN ISSUE: CameraLook and AutoShooter both modify transform.forward.
///   If rotation feels jittery, comment out the Lerp in AutoShooter.Update() temporarily.
/// </summary>
public class CameraLook : MonoBehaviour
{
    [Tooltip("Mouse sensitivity multiplier. Higher = faster look.")]
    public float mouseSensitivity = 100.0f;

    private float xRotation = 0.0f; // Accumulated vertical (pitch) rotation — clamped to avoid flipping
    private float yRotation = 0.0f; // Accumulated horizontal (yaw) rotation — applied to parent body

    private void Start()
    {
        // Lock and hide the cursor so it doesn't drift off-screen during play
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Subtract mouseY for pitch — inverted so moving mouse up looks up (not down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent camera from flipping over

        yRotation += mouseX; // Accumulate yaw — no clamp, full 360° rotation

        // Camera handles vertical look only
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Player body handles horizontal rotation — keeps the two axes separated cleanly
        transform.parent.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
