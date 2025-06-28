using UnityEngine;

public class MeteorSplit : MonoBehaviour
{
    public GameObject[] mediumBrownMeteors;
    public GameObject[] smallBrownMeteors;
    public GameObject[] tinyBrownMeteors;
    public GameObject[] mediumGreyMeteors;
    public GameObject[] smallGreyMeteors;
    public GameObject[] tinyGreyMeteors;

    public float collisionSplitSpeedThreshold = 0.5f;
    public float collisionSplitEnableDelay = 0.5f;

    private bool collisionSplitEnabled = false;

    private void Awake()
    {
        MeteorSpawner spawner = FindObjectOfType<MeteorSpawner>();
        if (spawner != null)
        {
            if (mediumBrownMeteors == null || mediumBrownMeteors.Length == 0)
                mediumBrownMeteors = spawner.mediumBrownMeteors;
            if (smallBrownMeteors == null || smallBrownMeteors.Length == 0)
                smallBrownMeteors = spawner.smallBrownMeteors;
            if (tinyBrownMeteors == null || tinyBrownMeteors.Length == 0)
                tinyBrownMeteors = spawner.tinyBrownMeteors;
            if (mediumGreyMeteors == null || mediumGreyMeteors.Length == 0)
                mediumGreyMeteors = spawner.mediumGreyMeteors;
            if (smallGreyMeteors == null || smallGreyMeteors.Length == 0)
                smallGreyMeteors = spawner.smallGreyMeteors;
            if (tinyGreyMeteors == null || tinyGreyMeteors.Length == 0)
                tinyGreyMeteors = spawner.tinyGreyMeteors;
        }
    }

    private void OnEnable()
    {
        collisionSplitEnabled = false;
        StartCoroutine(EnableCollisionSplit());
    }

    private System.Collections.IEnumerator EnableCollisionSplit()
    {
        yield return new WaitForSeconds(collisionSplitEnableDelay);
        collisionSplitEnabled = true;
    }

    public void Split()
    {
        SplitWithMomentum(Vector3.zero, 0f);
    }

    public void SplitWithMomentum(Vector3 baseDirection, float baseSpeed)
    {
        if (gameObject.CompareTag("BigBrownMeteor"))
        {
            SpawnMeteors(mediumBrownMeteors, "MediumBrownMeteor", 2, 3, smallBrownMeteors, baseDirection, baseSpeed);
        }
        else if (gameObject.CompareTag("BigGreyMeteor"))
        {
            SpawnMeteors(mediumGreyMeteors, "MediumGreyMeteor", 2, 3, smallGreyMeteors, baseDirection, baseSpeed);
        }
        else if (gameObject.CompareTag("MediumBrownMeteor"))
        {
            SpawnMeteors(smallBrownMeteors, "SmallBrownMeteor", 2, 3, tinyBrownMeteors, baseDirection, baseSpeed);
        }
        else if (gameObject.CompareTag("MediumGreyMeteor"))
        {
            SpawnMeteors(smallGreyMeteors, "SmallGreyMeteor", 2, 3, tinyGreyMeteors, baseDirection, baseSpeed);
        }
        else if (gameObject.CompareTag("SmallBrownMeteor"))
        {
            SpawnMeteors(tinyBrownMeteors, "TinyBrownMeteor", 2, 3, null, baseDirection, baseSpeed);
        }
        else if (gameObject.CompareTag("SmallGreyMeteor"))
        {
            SpawnMeteors(tinyGreyMeteors, "TinyGreyMeteor", 2, 3, null, baseDirection, baseSpeed);
        }

        Destroy(gameObject);
    }

    private void SpawnMeteors(GameObject[] meteorArray, string tag, int minCount, int maxCount, GameObject[] nextMeteorArray, Vector3 baseDirection, float baseSpeed)
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
            MeteorMovement move = newMeteor.GetComponent<MeteorMovement>();
            if (move != null)
            {
                if (baseSpeed > 0f)
                {
                    Vector3 offsetDir = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f)) * baseDirection;
                    move.InitializeMovement(offsetDir, baseSpeed);
                }
            }
            Debug.Log("Spawned " + tag + ": " + meteorArray[randomIndex].name);

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

    public void OnProjectileHit()
    {
        MeteorMovement move = GetComponent<MeteorMovement>();
        if (move != null)
        {
            SplitWithMomentum(move.CurrentDirection, move.CurrentSpeed);
        }
        else
        {
            Split();
        }
        if (GameManager.Instance != null)
        {
            int points = GameConstants.GetScoreByTag(gameObject.tag);
            GameManager.Instance.AddScore(points);
        }
        // TODO: Play sound effect
        // TODO: Spawn particle effect
    }

    public void OnHammerHit()
    {
        MeteorMovement move = GetComponent<MeteorMovement>();
        if (move != null)
        {
            SplitWithMomentum(move.CurrentDirection, move.CurrentSpeed);
        }
        else
        {
            Split();
        }
        if (GameManager.Instance != null)
        {
            int points = GameConstants.GetScoreByTag(gameObject.tag);
            GameManager.Instance.AddScore(points);
        }
        // TODO: Add score (possibly higher than projectile hit)
        // TODO: Play hammer hit sound effect
        // TODO: Spawn hammer hit particle effect
        // TODO: Apply screen shake
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collisionSplitEnabled)
        {
            return;
        }

        MeteorSplit other = collision.gameObject.GetComponent<MeteorSplit>();
        if (other == null)
        {
            return;
        }

        MeteorMovement movement = GetComponent<MeteorMovement>();
        if (movement == null)
        {
            return;
        }

        Vector3 myVelocity = movement.CurrentDirection * movement.CurrentSpeed;
        MeteorMovement otherMove = other.GetComponent<MeteorMovement>();
        Vector3 otherVelocity = otherMove != null ? otherMove.CurrentDirection * otherMove.CurrentSpeed : Vector3.zero;

        float relativeSpeed = (myVelocity - otherVelocity).magnitude;
        if (relativeSpeed < collisionSplitSpeedThreshold)
        {
            return;
        }

        Vector3 collisionNormal = collision.GetContact(0).normal;
        Vector3 dir = Vector3.Reflect(movement.CurrentDirection, collisionNormal);
        SplitWithMomentum(dir, movement.CurrentSpeed);
    }
}
