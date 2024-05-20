using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject bigMeteorPrefab; // The big meteor prefab to spawn
    public float spawnInterval = 1f; // Time between spawns
    public float spawnAreaWidth = 10f; // Width of the spawn area
    public float spawnHeight = 10f; // Height at which meteors spawn

    void Start()
    {
        StartCoroutine(SpawnMeteors());
    }

    private IEnumerator SpawnMeteors()
    {
        while (true)
        {
            SpawnMeteor();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnMeteor()
    {
        float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0);
        Instantiate(bigMeteorPrefab, spawnPosition, Quaternion.identity);
    }
}
