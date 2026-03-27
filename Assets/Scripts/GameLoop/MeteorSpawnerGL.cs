using UnityEngine;

/// <summary>
/// MeteorSpawnerGL — 3D meteor spawner for the game-loop-foundation feature.
/// Spawns meteors at random positions around the player at a configurable rate.
/// Renamed from MeteorSpawner to avoid conflict with the existing 2D MeteorSpawner.
///
/// SETUP:
///   1. Attach to an empty "GameManager" or "Spawner" GameObject in the scene.
///   2. Assign a meteor prefab (must have MeteorMover + EnemyHealth + tag "Enemy").
///   3. Tag the Player GameObject as "Player".
///   4. Wire WaveManager.meteorSpawner to this component in the Inspector.
///
/// PLANNED UPGRADES:
///   - Multiple prefab variants (small/fast vs large/slow meteors), picked randomly
///   - Spawn in formation patterns (e.g. rings, clusters) for boss waves
/// </summary>
public class MeteorSpawnerGL : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Meteor prefab to instantiate. Must be tagged 'Enemy' and have MeteorMover + EnemyHealth.")]
    public GameObject meteorPrefab;

    [Tooltip("Seconds between each spawn. Lower = harder.")]
    public float spawnRate = 2f;

    [Tooltip("Maximum simultaneous meteors. Prevents the scene from getting unplayable.")]
    public int maxMeteors = 15;

    [Tooltip("Outer radius of the spawn ring around the player.")]
    public float spawnRadius = 30f;

    [Tooltip("Inner radius — meteors never spawn closer than this. Avoids spawning on top of the player.")]
    public float minSpawnRadius = 10f;

    [Header("Meteor Movement")]
    [Tooltip("Speed passed to MeteorMover on each spawned meteor. Increase per wave for difficulty scaling.")]
    public float meteorSpeed = 3f;

    private float _nextSpawnTime = 0f;
    private Transform _player;

    private void Start()
    {
        // Cache the player reference once — FindGameObjectWithTag every frame is expensive
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // Respect spawn cooldown
        if (Time.time < _nextSpawnTime) return;

        // Enforce the meteor cap — don't spawn if we're at max
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxMeteors) return;

        SpawnMeteor();
        _nextSpawnTime = Time.time + spawnRate;
    }

    /// <summary>
    /// Picks a random point on a sphere shell between minSpawnRadius and spawnRadius,
    /// instantiates the meteor, and hands off movement config to MeteorMover.
    /// </summary>
    void SpawnMeteor()
    {
        if (meteorPrefab == null) return;

        // Random direction * random distance in the spawn ring
        Vector3 randomDir = Random.onUnitSphere;
        float distance = Random.Range(minSpawnRadius, spawnRadius);
        Vector3 spawnPos = (_player != null ? _player.position : Vector3.zero) + randomDir * distance;

        // Random initial rotation so no two meteors look the same at spawn
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Random.rotation);

        // Configure the MeteorMover with this spawner's current difficulty values
        MeteorMover mover = meteor.GetComponent<MeteorMover>();
        if (mover != null)
        {
            mover.speed = meteorSpeed;
            mover.target = _player;
        }
    }
}
