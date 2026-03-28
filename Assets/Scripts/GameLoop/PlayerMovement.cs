using UnityEngine;

/// <summary>
/// PlayerMovement — attaches to the Player ship.
/// Handles free-flight 3D movement relative to the camera's current facing direction.
///
/// SETUP:
///   - Attach to the Player root GameObject (not the Camera child).
///   - Works with Unity's legacy Input System (WASD + Space/Shift).
///   - Pair with CameraLook.cs on the Camera child for full mouse-look + WASD control.
///
/// CONTROLS:
///   W/S     — move forward/backward (relative to camera facing)
///   A/D     — strafe left/right
///   Space   — move up (world Y)
///   Shift   — move down (world Y)
///
/// PLANNED UPGRADES:
///   - Dash/boost ability on double-tap with cooldown
///   - Momentum / inertia for a more floaty space feel
///   - Speed upgrades from future UpgradeManager
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 10f;

    void Update()
    {
        // Camera-relative horizontal movement — feels natural for free-flight
        // transform.forward / transform.right respect the current yaw rotation set by CameraLook
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical   = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.forward * vertical + transform.right * horizontal;

        // Vertical thrust — world-space up/down (Space to rise, Shift to descend)
        if (Input.GetKey(KeyCode.Space))      move += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift))  move += Vector3.down;

        // Normalize only the horizontal plane component to prevent diagonal speed boost,
        // then re-add vertical so Space/Shift thrust isn't dampened by diagonal movement
        transform.position += move.normalized * moveSpeed * Time.deltaTime;
    }
}
