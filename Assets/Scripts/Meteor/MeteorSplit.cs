using UnityEngine;

public class MeteorSplit : MonoBehaviour
{
    public GameObject[] mediumBrownMeteors;
    public GameObject[] smallBrownMeteors;
    public GameObject[] tinyBrownMeteors;
    public GameObject[] mediumGreyMeteors;
    public GameObject[] smallGreyMeteors;
    public GameObject[] tinyGreyMeteors;

    public AudioClip projectileHitClip;
    public AudioClip hammerHitClip;
    public GameObject hitParticles;

    public float collisionSplitSpeedThreshold = 0.5f;
    public float collisionSplitEnableDelay = 0.5f;

    private bool collisionSplitEnabled = false;
    private bool _hit;
    private bool _split;
    private Durability durability = new Durability(1);
    private int bonusMetal;
    /// <summary>Remaining weapon hits; independent of delayed destruction.</summary>
    public int Health => durability.Current;

    /// <summary>Profiles only affect freshly spawned roots, not their smaller split children.</summary>
    public void Configure(MeteorProfile profile, int healthBonus)
    {
        durability = new Durability(profile.hitPoints + (profile.prefabSize < 2 ? Mathf.Max(0, healthBonus) : 0));
        bonusMetal = profile.metalReward;
    }

    private bool ApplyDamage(int amount, bool critical = false)
    {
        if (_hit || _split || amount <= 0) return false;
        CombatFeedback.Hit(gameObject, amount, critical);
        if (!durability.Apply(amount)) return false;
        _hit = true;
        return true;
    }

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
        _hit = false;
        _split = false;
        durability = new Durability(1);
        bonusMetal = 0;
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
        if (_split) return;
        _split = true;

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

    private static int GetMeteorCap()
    {
        var waves = FindObjectOfType<WaveManager>();
        if (waves && waves.catalogue) return waves.catalogue.meteorCap;
        return BalanceService.Instance != null
            ? BalanceService.Instance.GetInt("meteor.max_active_count", 80)
            : 80;
    }

    private void SpawnMeteors(GameObject[] meteorArray, string tag, int minCount, int maxCount, GameObject[] nextMeteorArray, Vector3 baseDirection, float baseSpeed)
    {
        if (meteorArray.Length == 0)
        {
            Debug.LogError("Meteor array is not set up properly!");
            return;
        }

        // Hard cap: if we're already at or near the limit, destroy without spawning children.
        // This stops cascade explosions (e.g. mass Small→Tiny collisions) from crashing the game.
        int cap = GetMeteorCap();
        if (MeteorMovement.ActiveCount >= cap)
        {
            Debug.LogWarning($"[Meteor/Cap] Split suppressed for {gameObject.tag} — " +
                             $"active:{MeteorMovement.ActiveCount} >= cap:{cap}");
            return; // parent is destroyed by SplitWithMomentum's Destroy(gameObject) call
        }

        int count = Random.Range(minCount, maxCount + 1);
        // Clamp so we don't overshoot the cap mid-loop
        count = Mathf.Min(count, cap - MeteorMovement.ActiveCount);
        if (count <= 0) return;

        GameLogger.MeteorSplitSpawned(gameObject.tag, tag, count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, meteorArray.Length);
            GameObject newMeteor = Instantiate(meteorArray[randomIndex], transform.position, Quaternion.identity);
            newMeteor.tag = tag;
            MeteorMovement move = newMeteor.GetComponent<MeteorMovement>();
            if (move != null && baseSpeed > 0f)
            {
                Vector3 offsetDir = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f)) * baseDirection;
                move.InitializeMovement(offsetDir, baseSpeed);
            }

            if (nextMeteorArray != null)
            {
                // Prefabs already carry MeteorSplit. A second copy splits and scores twice.
                MeteorSplit splitScript = newMeteor.GetComponent<MeteorSplit>();
                if (splitScript == null)
                    splitScript = newMeteor.AddComponent<MeteorSplit>();
                splitScript.mediumBrownMeteors = mediumBrownMeteors;
                splitScript.smallBrownMeteors = smallBrownMeteors;
                splitScript.tinyBrownMeteors = tinyBrownMeteors;
                splitScript.mediumGreyMeteors = mediumGreyMeteors;
                splitScript.smallGreyMeteors = smallGreyMeteors;
                splitScript.tinyGreyMeteors = tinyGreyMeteors;
            }
        }
    }

    public void OnProjectileHit(int damage = 1) => OnProjectileHit(damage, false);

    /// <summary>Projectile damage and its single label share the same critical decision.</summary>
    public void OnProjectileHit(int damage, bool critical)
    {
        if (!ApplyDamage(damage, critical)) return;

        string hitTag  = gameObject.tag;
        bool   splits  = !hitTag.StartsWith("Tiny");
        int    points  = GameConstants.GetScoreByTag(hitTag);

        MeteorMovement move = GetComponent<MeteorMovement>();
        if (move != null) SplitWithMomentum(move.CurrentDirection, move.CurrentSpeed);
        else              Split();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            GameLogger.MeteorKilledByProjectile(hitTag, points, GameManager.Instance.Score, splits);
        }
        ScorePopup.Spawn(transform.position, points);

        if (EconomyService.Instance) EconomyService.Instance.EarnFromMeteor(hitTag);
        if (EconomyService.Instance && bonusMetal > 0) EconomyService.Instance.AddMetal(bonusMetal, "Armored meteor");
        MeteorDropSpawner.Instance?.TryDrop(transform.position, hitTag);

        if (projectileHitClip != null)
            AudioSource.PlayClipAtPoint(projectileHitClip, transform.position);
        if (hitParticles != null)
            Instantiate(hitParticles, transform.position, Quaternion.identity);
    }

    public void OnHammerHit(int damage = 2)
    {
        if (!ApplyDamage(damage)) return;

        string hitTag  = gameObject.tag;
        bool   splits  = !hitTag.StartsWith("Tiny");
        int    points  = GameConstants.GetScoreByTag(hitTag) * (SkillService.Instance?.GetHammerScoreMult() ?? 2);

        MeteorMovement move = GetComponent<MeteorMovement>();
        if (move != null) SplitWithMomentum(move.CurrentDirection, move.CurrentSpeed);
        else              Split();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            GameLogger.MeteorKilledByHammer(hitTag, points, GameManager.Instance.Score, splits);
        }
        ScorePopup.Spawn(transform.position, points);

        if (EconomyService.Instance) EconomyService.Instance.EarnFromMeteor(hitTag);
        if (EconomyService.Instance && bonusMetal > 0) EconomyService.Instance.AddMetal(bonusMetal, "Armored meteor");
        MeteorDropSpawner.Instance?.TryDrop(transform.position, hitTag);

        if (hammerHitClip != null)
            AudioSource.PlayClipAtPoint(hammerHitClip, transform.position);
        if (hitParticles != null)
            Instantiate(hitParticles, transform.position, Quaternion.identity);
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_split || Health > 1) return;

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

        GameLogger.MeteorCollisionSplit(gameObject.tag, collision.gameObject.tag, relativeSpeed);

        Vector3 collisionNormal = collision.GetContact(0).normal;
        Vector3 dir = Vector3.Reflect(movement.CurrentDirection, collisionNormal);
        SplitWithMomentum(dir, movement.CurrentSpeed);
    }
}
