using UnityEngine;

/// <summary>
/// MoveObject — LEGACY PROTOTYPE SCRIPT. Kept for reference only.
///
/// This was the original "whack" mechanic: clicking a meteor teleported it to a random position.
/// It has been REPLACED by the auto-shooter + spawner system in the feature/auto-shooter branch.
///
/// KEPT IN CODEBASE because:
///   - Good historical reference for what the original prototype did
///
/// DO NOT attach to new GameObjects. This script will be removed or archived
/// once the new game loop (AutoShooter + MeteorSpawnerGL + MeteorMover) is verified in-scene.
/// </summary>
public class MoveObject : MonoBehaviour
{
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            // LEGACY: teleport clicked meteor to a random position in a small local cube
            // Replaced by: MeteorMover (continuous homing movement) + MeteorSpawnerGL (spawn ring)
            float x = Random.Range(-5.0f, 5.0f);
            float y = Random.Range(-5.0f, 5.0f);
            float z = Random.Range(-5.0f, 5.0f);
            transform.position = new Vector3(x, y, z);
        }
    }
}
