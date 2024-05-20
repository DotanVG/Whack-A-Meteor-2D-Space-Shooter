using UnityEngine;

public class MeteorSplit : MonoBehaviour
{
    public GameObject mediumMeteorPrefab;
    public GameObject smallMeteorPrefab;

    public void Split()
    {
        // Determine which size to split into
        if (gameObject.CompareTag("BigMeteor"))
        {
            SpawnMediumMeteors();
        }
        else if (gameObject.CompareTag("MediumMeteor"))
        {
            SpawnSmallMeteors();
        }

        // Destroy the current meteor
        Destroy(gameObject);
    }

    private void SpawnMediumMeteors()
    {
        for (int i = 0; i < 2; i++)
        {
            Instantiate(mediumMeteorPrefab, transform.position, Quaternion.identity);
        }
    }

    private void SpawnSmallMeteors()
    {
        for (int i = 0; i < 2; i++)
        {
            Instantiate(smallMeteorPrefab, transform.position, Quaternion.identity);
        }
    }
}
