using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject[] bigBrownMeteors; // Array of big brown meteor prefabs
    public GameObject[] mediumBrownMeteors; // Array of medium brown meteor prefabs
    public GameObject[] smallBrownMeteors; // Array of small brown meteor prefabs
    public GameObject[] tinyBrownMeteors; // Array of tiny brown meteor prefabs
    public GameObject[] bigGreyMeteors; // Array of big grey meteor prefabs
    public GameObject[] mediumGreyMeteors; // Array of medium grey meteor prefabs
    public GameObject[] smallGreyMeteors; // Array of small grey meteor prefabs
    public GameObject[] tinyGreyMeteors; // Array of tiny grey meteor prefabs
    public float spawnInterval = 1f; // Time between spawns
    public float spawnMargin = 1f;   // Distance outside the screen to spawn meteors

    private int meteorsLayer;
    private Camera mainCamera;

    void Start()
    {
        meteorsLayer = LayerMask.NameToLayer("Meteors");
        mainCamera = Camera.main;
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
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject meteor = SelectMeteor();

        if (meteor != null)
        {
            GameObject spawnedMeteor = Instantiate(meteor, spawnPosition, Quaternion.identity);
            spawnedMeteor.layer = meteorsLayer;

            Vector3 direction = (Vector3.zero - spawnPosition).normalized;

            MeteorMovement movement = spawnedMeteor.GetComponent<MeteorMovement>();
            if (movement != null)
            {
                movement.SetInitialDirection(direction);
            }

            Debug.Log($"Spawned {meteor.name} at {spawnPosition} moving towards {direction}");
        }
        else
        {
            Debug.LogError("Meteor selection returned null.");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 min = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (side)
        {
            case 0: // Left
                spawnPos = new Vector3(min.x - spawnMargin, Random.Range(min.y, max.y), 0);
                break;
            case 1: // Right
                spawnPos = new Vector3(max.x + spawnMargin, Random.Range(min.y, max.y), 0);
                break;
            case 2: // Top
                spawnPos = new Vector3(Random.Range(min.x, max.x), max.y + spawnMargin, 0);
                break;
            default: // Bottom
                spawnPos = new Vector3(Random.Range(min.x, max.x), min.y - spawnMargin, 0);
                break;
        }

        return spawnPos;
    }

    private GameObject SelectMeteor()
    {
        float typeChance = Random.value;
        float sizeChance = Random.value;

        if (sizeChance <= 0.7f) // 70% chance to spawn a big meteor
        {
            return typeChance <= 0.5f ? GetRandomMeteor(bigBrownMeteors) : GetRandomMeteor(bigGreyMeteors);
        }
        else if (sizeChance <= 0.9f) // 20% chance to spawn a medium meteor
        {
            return typeChance <= 0.5f ? GetRandomMeteor(mediumBrownMeteors) : GetRandomMeteor(mediumGreyMeteors);
        }
        else if (sizeChance <= 0.95f) // 5% chance to spawn a small meteor
        {
            return typeChance <= 0.5f ? GetRandomMeteor(smallBrownMeteors) : GetRandomMeteor(smallGreyMeteors);
        }
        else // 5% chance to spawn a tiny meteor
        {
            return typeChance <= 0.5f ? GetRandomMeteor(tinyBrownMeteors) : GetRandomMeteor(tinyGreyMeteors);
        }
    }

    private GameObject GetRandomMeteor(GameObject[] meteorArray)
    {
        if (meteorArray.Length == 0)
        {
            Debug.LogError("Meteor array is not set up properly!");
            return null;
        }
        return meteorArray[Random.Range(0, meteorArray.Length)];
    }


}
