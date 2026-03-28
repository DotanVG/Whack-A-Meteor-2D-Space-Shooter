using System.Collections;
using UnityEngine;

/// <summary>
/// EnemySpawner — mirrors MeteorSpawner pattern.
/// Spawns enemy ships from the top edge of the screen.
/// SpawnDirector calls SetActiveFactions() and StartSpawning() at each wave.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab Arrays — assign in Inspector")]
    public GameObject[] blackPrefabs;
    public GameObject[] bluePrefabs;
    public GameObject[] greenPrefabs;
    public GameObject[] redPrefabs;
    public GameObject   bossPrefab;

    [Header("Spawn Config")]
    public float spawnInterval = 3f;
    public int   maxEnemies    = 12;

    public static int ActiveCount { get; private set; }

    private bool _spawning;
    private bool _spawnBlack, _spawnBlue, _spawnGreen, _spawnRed;
    private Coroutine _spawnCoroutine;

    void Start()
    {
        spawnInterval = BalanceService.Instance?.GetFloat("enemy.spawn_rate",       spawnInterval) ?? spawnInterval;
        maxEnemies    = BalanceService.Instance?.GetInt  ("enemy.max_active_count", maxEnemies)    ?? maxEnemies;
    }

    public void SetActiveFactions(bool black, bool blue, bool green, bool red)
    {
        _spawnBlack = black;
        _spawnBlue  = blue;
        _spawnGreen = green;
        _spawnRed   = red;
    }

    public void StartSpawning()
    {
        if (_spawning) return;
        _spawning = true;
        _spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        _spawning = false;
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null) return;
        // Spawn at top-center, slightly above screen
        float topY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10f)).y + 1f;
        Vector3 pos = new Vector3(0f, topY, 0f);
        Instantiate(bossPrefab, pos, Quaternion.identity);
    }

    IEnumerator SpawnLoop()
    {
        while (_spawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (ActiveCount < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null) return;

        Vector3 pos = GetSpawnPosition();
        // Face downward (transform.up = Vector2.down → rotate 180°)
        Instantiate(prefab, pos, Quaternion.Euler(0f, 0f, 180f));
    }

    /// <summary>Picks randomly from all currently active faction arrays.</summary>
    GameObject PickPrefab()
    {
        // Collect active pools
        var pools = new System.Collections.Generic.List<GameObject[]>();
        if (_spawnBlack && blackPrefabs != null && blackPrefabs.Length > 0) pools.Add(blackPrefabs);
        if (_spawnBlue  && bluePrefabs  != null && bluePrefabs.Length  > 0) pools.Add(bluePrefabs);
        if (_spawnGreen && greenPrefabs != null && greenPrefabs.Length > 0) pools.Add(greenPrefabs);
        if (_spawnRed   && redPrefabs   != null && redPrefabs.Length   > 0) pools.Add(redPrefabs);
        if (pools.Count == 0) return null;

        GameObject[] chosen = pools[Random.Range(0, pools.Count)];
        return chosen[Random.Range(0, chosen.Length)];
    }

    /// <summary>Returns a random position along the top edge of the screen.</summary>
    Vector3 GetSpawnPosition()
    {
        float spawnMargin = 1.5f;
        float topY  = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 10f)).y + spawnMargin;
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 10f)).x;
        float rightX= Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 10f)).x;
        return new Vector3(Random.Range(leftX, rightX), topY, 0f);
    }

    // Static counter helpers — called by a small tracking component on each enemy
    public static void RegisterSpawn()  => ActiveCount++;
    public static void RegisterDespawn()=> ActiveCount = Mathf.Max(0, ActiveCount - 1);
}
