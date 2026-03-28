using UnityEngine;

/// <summary>
/// MeteorMover — attaches to each meteor prefab.
/// Drifts the meteor toward a target (usually the player) and adds a random tumble rotation.
///
/// SETUP:
///   - Attach to your meteor prefab alongside EnemyHealth.
///   - MeteorSpawnerGL sets the target and speed at spawn time — no manual wiring needed.
///
/// PLANNED UPGRADES:
///   - Acceleration over time (meteors get faster the longer they live)
///   - Zig-zag / sine-wave movement for harder-to-hit variants
///   - Speed tiers driven by WaveManager difficulty curve
/// </summary>
public class MeteorMover : MonoBehaviour
{
    [Tooltip("The transform to move toward. Assigned automatically by MeteorSpawnerGL.")]
    public Transform target;

    [Tooltip("Movement speed in units/second. Set by MeteorSpawnerGL based on current difficulty.")]
    public float speed = 3f;

    [Tooltip("Degrees per second for the random tumble rotation. Purely visual.")]
    public float rotationSpeed = 30f;

    private Vector3 _rotationAxis; // Random axis chosen at spawn for unique tumble per meteor

    private void Start()
    {
        // Each meteor tumbles on a unique random axis — avoids all meteors spinning identically
        _rotationAxis = Random.onUnitSphere;

        // Fallback: if MeteorSpawnerGL didn't assign a target, find the Player by tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            // Move directly toward the target — straightforward homing
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }

        // Cosmetic tumble — makes meteors feel weightier and less like floating boxes
        transform.Rotate(_rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
