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
    public float spawnAreaWidth = 10f; // Width of the spawn area
    public float spawnHeight = 10f; // Height at which meteors spawn

    private int meteorsLayer;

    void Start()
    {
        meteorsLayer = LayerMask.NameToLayer("Meteors");
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

    // TODO: Fix the meteors spawn position and remove unnecessary comments and debug logs
    private void SpawnMeteor()
    {
        float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        // Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0);

        // Ensure arrays are not empty
        if (bigBrownMeteors.Length == 0 || mediumBrownMeteors.Length == 0 ||
            smallBrownMeteors.Length == 0 || tinyBrownMeteors.Length == 0 ||
            bigGreyMeteors.Length == 0 || mediumGreyMeteors.Length == 0 ||
            smallGreyMeteors.Length == 0 || tinyGreyMeteors.Length == 0)
        {
            Debug.LogError("Meteor arrays are not set up properly!");
            return;
        }

        // Determine which type and size of meteor to spawn
        float typeChance = Random.Range(0f, 1f);
        float sizeChance = Random.Range(0f, 1f);

        GameObject meteor = null;

        if (sizeChance <= 0.7f) // 70% chance to spawn a big meteor
        {
            if (typeChance <= 0.5f) // 50% chance to spawn a brown meteor
            {
                int randomIndex = Random.Range(0, bigBrownMeteors.Length);
                meteor = Instantiate(bigBrownMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Big Brown Meteor: " + bigBrownMeteors[randomIndex].name + " at position " + spawnPosition);
            }
            else // 50% chance to spawn a grey meteor
            {
                int randomIndex = Random.Range(0, bigGreyMeteors.Length);
                meteor = Instantiate(bigGreyMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Big Grey Meteor: " + bigGreyMeteors[randomIndex].name + " at position " + spawnPosition);
            }
        }
        else if (sizeChance <= 0.9f) // 20% chance to spawn a medium meteor
        {
            if (typeChance <= 0.5f) // 50% chance to spawn a brown meteor
            {
                int randomIndex = Random.Range(0, mediumBrownMeteors.Length);
                meteor = Instantiate(mediumBrownMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Medium Brown Meteor: " + mediumBrownMeteors[randomIndex].name + " at position " + spawnPosition);
            }
            else // 50% chance to spawn a grey meteor
            {
                int randomIndex = Random.Range(0, mediumGreyMeteors.Length);
                meteor = Instantiate(mediumGreyMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Medium Grey Meteor: " + mediumGreyMeteors[randomIndex].name + " at position " + spawnPosition);
            }
        }
        else if (sizeChance <= 0.95f) // 5% chance to spawn a small meteor
        {
            if (typeChance <= 0.5f) // 50% chance to spawn a brown meteor
            {
                int randomIndex = Random.Range(0, smallBrownMeteors.Length);
                meteor = Instantiate(smallBrownMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Small Brown Meteor: " + smallBrownMeteors[randomIndex].name + " at position " + spawnPosition);
            }
            else // 50% chance to spawn a grey meteor
            {
                int randomIndex = Random.Range(0, smallGreyMeteors.Length);
                meteor = Instantiate(smallGreyMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Small Grey Meteor: " + smallGreyMeteors[randomIndex].name + " at position " + spawnPosition);
            }
        }
        else // 5% chance to spawn a tiny meteor
        {
            if (typeChance <= 0.5f) // 50% chance to spawn a brown meteor
            {
                int randomIndex = Random.Range(0, tinyBrownMeteors.Length);
                meteor = Instantiate(tinyBrownMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Tiny Brown Meteor: " + tinyBrownMeteors[randomIndex].name + " at position " + spawnPosition);
            }
            else // 50% chance to spawn a grey meteor
            {
                int randomIndex = Random.Range(0, tinyGreyMeteors.Length);
                meteor = Instantiate(tinyGreyMeteors[randomIndex], spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Tiny Grey Meteor: " + tinyGreyMeteors[randomIndex].name + " at position " + spawnPosition);
            }
        }

        if (meteor != null)
        {
            meteor.layer = meteorsLayer;
        }
    }
}
