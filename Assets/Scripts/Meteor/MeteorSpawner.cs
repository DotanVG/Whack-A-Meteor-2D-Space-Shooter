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
    public float ellipseWidthFactor = 4f; // Factor to multiply with camera width for ellipse width
    public float ellipseHeightFactor = 3f; // Factor to multiply with camera height for ellipse height
    public float noiseFactor = 1f; // Factor to add randomness to spawn positions
    public int ellipseResolution = 100; // Number of points to use when drawing the ellipse
    public Color ellipseColor = Color.yellow; // Color of the ellipse in the scene view

    private int meteorsLayer;
    private Camera mainCamera;
    private float ellipseWidth;
    private float ellipseHeight;

    void Start()
    {
        meteorsLayer = LayerMask.NameToLayer("Meteors");
        mainCamera = Camera.main;
        UpdateEllipseDimensions();
        StartCoroutine(SpawnMeteors());
    }

    void Update()
    {
        UpdateEllipseDimensions();
    }

    private void UpdateEllipseDimensions()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        ellipseWidth = cameraWidth * ellipseWidthFactor;
        ellipseHeight = cameraHeight * ellipseHeightFactor;

        Debug.Log($"Camera Width: {cameraWidth}, Camera Height: {cameraHeight}");
        Debug.Log($"Ellipse Width: {ellipseWidth}, Ellipse Height: {ellipseHeight}");
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
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float x = ellipseWidth * Mathf.Cos(angle) / 2f;
        float y = ellipseHeight * Mathf.Sin(angle) / 2f;

        float noiseX = Random.Range(-1f, 1f) * noiseFactor;
        float noiseY = Random.Range(-1f, 1f) * noiseFactor;

        x += noiseX;
        y += noiseY;

        Vector3 worldSpawnPos = mainCamera.transform.position + new Vector3(x, y, 0);
        worldSpawnPos.z = 0;

        Debug.Log($"Spawn Position: {worldSpawnPos}");

        return worldSpawnPos;
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

    // This method draws the ellipse in the Scene view
    private void OnDrawGizmos()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        UpdateEllipseDimensions();
        Gizmos.color = ellipseColor;

        for (int i = 0; i <= ellipseResolution; i++)
        {
            float angle = (i / (float)ellipseResolution) * 2f * Mathf.PI;
            Vector3 point = GetEllipsePoint(angle);
            Vector3 nextPoint = GetEllipsePoint(((i + 1) / (float)ellipseResolution) * 2f * Mathf.PI);
            Gizmos.DrawLine(point, nextPoint);
        }

        // Draw noise boundary
        Gizmos.color = new Color(ellipseColor.r, ellipseColor.g, ellipseColor.b, 0.3f);
        for (int i = 0; i <= ellipseResolution; i++)
        {
            float angle = (i / (float)ellipseResolution) * 2f * Mathf.PI;
            Vector3 innerPoint = GetEllipsePoint(angle, -noiseFactor);
            Vector3 outerPoint = GetEllipsePoint(angle, noiseFactor);
            Gizmos.DrawLine(innerPoint, outerPoint);
        }
    }

    private Vector3 GetEllipsePoint(float angle, float noiseOffset = 0)
    {
        float x = (ellipseWidth / 2f + noiseOffset) * Mathf.Cos(angle);
        float y = (ellipseHeight / 2f + noiseOffset) * Mathf.Sin(angle);

        Vector3 worldPoint = mainCamera.transform.position + new Vector3(x, y, 0);
        worldPoint.z = 0;

        return worldPoint;
    }
}
