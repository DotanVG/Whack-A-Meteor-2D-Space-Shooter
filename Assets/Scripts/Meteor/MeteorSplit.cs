using UnityEngine;

public class MeteorSplit : MonoBehaviour
{
    public GameObject[] mediumBrownMeteors;
    public GameObject[] smallBrownMeteors;
    public GameObject[] tinyBrownMeteors;
    public GameObject[] mediumGreyMeteors;
    public GameObject[] smallGreyMeteors;
    public GameObject[] tinyGreyMeteors;

    public void Split()
    {
        Vector3 parentDirection = Vector3.up;
        float parentSpeed = 0f;
        MeteorMovement movement = GetComponent<MeteorMovement>();
        if (movement != null)
        {
            parentDirection = movement.GetDirection();
            parentSpeed = movement.GetSpeed();
        }

        // Determine which size and color to split into
        if (gameObject.CompareTag("BigBrownMeteor"))
        {
            SpawnMeteors(mediumBrownMeteors, "MediumBrownMeteor", 2, 3, smallBrownMeteors, parentDirection, parentSpeed);
        }
        else if (gameObject.CompareTag("BigGreyMeteor"))
        {
            SpawnMeteors(mediumGreyMeteors, "MediumGreyMeteor", 2, 3, smallGreyMeteors, parentDirection, parentSpeed);
        }
        else if (gameObject.CompareTag("MediumBrownMeteor"))
        {
            SpawnMeteors(smallBrownMeteors, "SmallBrownMeteor", 2, 3, tinyBrownMeteors, parentDirection, parentSpeed);
        }
        else if (gameObject.CompareTag("MediumGreyMeteor"))
        {
            SpawnMeteors(smallGreyMeteors, "SmallGreyMeteor", 2, 3, tinyGreyMeteors, parentDirection, parentSpeed);
        }
        else if (gameObject.CompareTag("SmallBrownMeteor"))
        {
            SpawnMeteors(tinyBrownMeteors, "TinyBrownMeteor", 2, 3, null, parentDirection, parentSpeed);
        }
        else if (gameObject.CompareTag("SmallGreyMeteor"))
        {
            SpawnMeteors(tinyGreyMeteors, "TinyGreyMeteor", 2, 3, null, parentDirection, parentSpeed);
        }

        // Destroy the current meteor
        Destroy(gameObject);
    }

    private void SpawnMeteors(GameObject[] meteorArray, string tag, int minCount, int maxCount, GameObject[] nextMeteorArray, Vector3 parentDirection, float parentSpeed)
    {
        if (meteorArray.Length == 0)
        {
            Debug.LogError("Meteor array is not set up properly!");
            return;
        }

        int count = Random.Range(minCount, maxCount + 1);
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, meteorArray.Length);
            GameObject newMeteor = Instantiate(meteorArray[randomIndex], transform.position, Quaternion.identity);
            newMeteor.tag = tag;
            Debug.Log("Spawned " + tag + ": " + meteorArray[randomIndex].name);

            MeteorMovement movement = newMeteor.GetComponent<MeteorMovement>();
            if (movement != null)
            {
                float angleOffset = Random.Range(-30f, 30f);
                Vector3 newDirection = Quaternion.Euler(0, 0, angleOffset) * parentDirection;
                movement.SetInitialDirection(newDirection);
                movement.SetInitialSpeed(parentSpeed);
            }

            if (nextMeteorArray != null)
            {
                MeteorSplit splitScript = newMeteor.AddComponent<MeteorSplit>();
                splitScript.mediumBrownMeteors = mediumBrownMeteors;
                splitScript.smallBrownMeteors = smallBrownMeteors;
                splitScript.tinyBrownMeteors = tinyBrownMeteors;
                splitScript.mediumGreyMeteors = mediumGreyMeteors;
                splitScript.smallGreyMeteors = smallGreyMeteors;
                splitScript.tinyGreyMeteors = tinyGreyMeteors;
            }
        }
    }

    // TODO: Implement method to handle projectile hit
    public void OnProjectileHit()
    {
        // Call Split() method
        Split();
        // TODO: Add score
        // TODO: Play sound effect
        // TODO: Spawn particle effect
    }

    // TODO: Implement method to handle hammer hit
    public void OnHammerHit()
    {
        // Call Split() method
        Split();
        // TODO: Add score (possibly higher than projectile hit)
        // TODO: Play hammer hit sound effect
        // TODO: Spawn hammer hit particle effect
        // TODO: Apply screen shake
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        MeteorSplit otherSplit = collision.gameObject.GetComponent<MeteorSplit>();
        if (otherSplit != null)
        {
            Split();
            otherSplit.Split();
        }
    }
}