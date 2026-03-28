using UnityEngine;

/// <summary>
/// AutoShooter — attaches to the player ship.
/// Automatically targets the nearest enemy or meteor and fires a leading shot toward it,
/// similar to Enter the Gungeon's auto-aim. Not perfectly accurate by design — shots are
/// spread by a configurable angle so the player still feels agency and not all shots land.
///
/// DESIGN NOTES:
///   - Never rotates the player ship — aim rotation is baked into the spawned projectile.
///   - Reuses the same projectile prefab as the spacebar shot (ProjectileController handles velocity).
///   - Lead-prediction is a one-shot quadratic solve per-fire, not per-frame.
///   - Homing meteors (MeteorMover) re-aim every frame so prediction is approximate but feels good.
///
/// PLANNED UPGRADES (survivor.io / Chicken Invaders incremental skill system):
///   - Unlock side shots (left/right 90°) as an acquired skill
///   - Unlock rear shot (180°) as an acquired skill
///   - fireRate driven by upgrade level from a future UpgradeManager
///   - Spread shot, multi-point fire arrays
/// </summary>
public class AutoShooter : MonoBehaviour
{
    [Header("References — auto-populated from PlayerController if left empty")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Targeting")]
    [Tooltip("Max range to consider a target. Set to 0 for unlimited.")]
    public float detectionRange = 0f;

    [Header("Accuracy")]
    [Tooltip("Max random angular spread in degrees. 0 = pixel-perfect, 15 = noticeable imprecision.")]
    public float accuracySpread = 12f;

    [Header("Fire Rate")]
    [Tooltip("Shots per second. 1 = one shot every second.")]
    public float fireRate = 1.0f;

    [Header("Lead Calculation")]
    [Tooltip("Must match ProjectileController.speed on the projectile prefab for accurate lead aim.")]
    public float projectileSpeed = 50f;

    private static readonly string[] EnemyTags =
    {
        "Enemy",
    };
    private static readonly string[] MeteorTags =
    {
        "BigBrownMeteor",   "BigGreyMeteor",
        "MediumBrownMeteor","MediumGreyMeteor",
        "SmallBrownMeteor", "SmallGreyMeteor",
        "TinyBrownMeteor",  "TinyGreyMeteor",
    };
    // Combined fallback (no priority upgrade)
    private static readonly string[] AllTargetTags =
    {
        "Enemy",
        "BigBrownMeteor",   "BigGreyMeteor",
        "MediumBrownMeteor","MediumGreyMeteor",
        "SmallBrownMeteor", "SmallGreyMeteor",
        "TinyBrownMeteor",  "TinyGreyMeteor",
    };

    private float _nextFireTime = 0f;

    void Start()
    {
        // Auto-wire from PlayerController on the same GameObject if not manually assigned
        if (projectilePrefab == null || firePoint == null)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                if (projectilePrefab == null) projectilePrefab = pc.projectilePrefab;
                if (firePoint == null)        firePoint        = pc.projectileSpawnPoint;
            }
        }

        if (projectilePrefab == null)
            Debug.LogWarning("AutoShooter: projectilePrefab not assigned and not found on PlayerController.");
        if (firePoint == null)
            Debug.LogWarning("AutoShooter: firePoint not assigned and not found on PlayerController.");

        // Override Inspector values with BalanceService if CSV balance is active.
        // Inspector values are used as fallback if BalanceService isn't in the scene.
        ApplyBalanceValues();
    }

    /// <summary>
    /// Reads fire rate, spread, projectile speed, and range from BalanceService (level 1).
    /// Inspector fields remain visible for manual override when BalanceService is absent.
    /// Call again after purchasing an upgrade to apply the new level's values.
    /// </summary>
    public void ApplyBalanceValues(int upgradeLevel = 1)
    {
        if (BalanceService.Instance == null) return;
        fireRate        = BalanceService.Instance.GetFloat("weapon.autoshooter_fire_rate",       upgradeLevel, fireRate);
        accuracySpread  = BalanceService.Instance.GetFloat("weapon.autoshooter_accuracy_spread", upgradeLevel, accuracySpread);
        projectileSpeed = BalanceService.Instance.GetFloat("weapon.autoshooter_projectile_speed", projectileSpeed);
        detectionRange  = BalanceService.Instance.GetFloat("weapon.autoshooter_range",            detectionRange);

        // Apply skill-tree multipliers (null-safe)
        fireRate        *= SkillService.Instance?.GetFireRateMultiplier()  ?? 1f;
        accuracySpread  *= SkillService.Instance?.GetSpreadMultiplier()    ?? 1f;
        projectileSpeed *= SkillService.Instance?.GetProjSpeedMultiplier() ?? 1f;

        Debug.Log($"[AutoShooter] Balance applied (lv{upgradeLevel}) — " +
                  $"FireRate:{fireRate:F2}/s  Spread:±{accuracySpread:F1}°  " +
                  $"ProjSpeed:{projectileSpeed:F0}  Range:{(detectionRange <= 0 ? "∞" : detectionRange.ToString("F1"))}");
    }

    void Update()
    {
        if (projectilePrefab == null || firePoint == null) return;
        if (Time.time < _nextFireTime) return;

        (Transform target, Vector2 targetVel) = FindNearestTarget();
        if (target == null) return;

        float dist = Vector2.Distance(firePoint.position, target.position);
        Vector2 aimDir = ComputeLeadAim((Vector2)target.position, targetVel);
        FireProjectile(aimDir);
        GameLogger.AutoShooterFired(target.tag, dist, accuracySpread);
        _nextFireTime = Time.time + 1f / fireRate;
    }

    // ─── Target search ───────────────────────────────────────────────────────

    (Transform nearest, Vector2 velocity) FindNearestTarget()
    {
        // Target Priority upgrade: prefer enemy ships, fall back to meteors
        if (SkillService.Instance?.GetTargetPriorityEnabled() == true)
        {
            var (enemy, eVel) = FindNearestInTags(EnemyTags);
            if (enemy != null) return (enemy, eVel);
            return FindNearestInTags(MeteorTags);
        }
        return FindNearestInTags(AllTargetTags);
    }

    (Transform nearest, Vector2 velocity) FindNearestInTags(string[] tags)
    {
        Transform nearest = null;
        Vector2 nearestVel = Vector2.zero;
        float maxDistSq = detectionRange > 0f ? detectionRange * detectionRange : float.MaxValue;
        float minDistSq = maxDistSq;

        foreach (string tag in tags)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                float distSq = ((Vector2)(obj.transform.position - transform.position)).sqrMagnitude;
                if (distSq < minDistSq)
                {
                    minDistSq  = distSq;
                    nearest    = obj.transform;
                    nearestVel = GetTargetVelocity(obj);
                }
            }
        }
        return (nearest, nearestVel);
    }

    /// <summary>
    /// Reads the current velocity of a target from whichever movement component it has.
    /// Supports the three mover types present in the project:
    ///   MeteorMover     — homing; recomputes direction each frame toward player
    ///   MeteorMovement  — linear; constant world-space direction + speed
    ///   EnemyController — always moves straight down in local space
    /// </summary>
    Vector2 GetTargetVelocity(GameObject obj)
    {
        // GameLoop homing meteors: direction is recalculated every frame toward their target,
        // so we sample the instantaneous direction for the lead solve.
        MeteorMover mover = obj.GetComponent<MeteorMover>();
        if (mover != null && mover.target != null)
            return ((Vector2)(mover.target.position - obj.transform.position)).normalized * mover.speed;

        // Original 2D linear meteors: constant world-space velocity
        MeteorMovement linear = obj.GetComponent<MeteorMovement>();
        if (linear != null)
            return (Vector2)(linear.CurrentDirection * linear.CurrentSpeed);

        // Enemy ships: always drift straight down in world space
        EnemyController enemy = obj.GetComponent<EnemyController>();
        if (enemy != null)
            return Vector2.down * enemy.speed;

        return Vector2.zero;
    }

    // ─── Predictive aim ───────────────────────────────────────────────────────

    /// <summary>
    /// Solves the projectile intercept quadratic to find where to aim so that
    /// a bullet at <projectileSpeed> will meet the target at its future position.
    ///
    /// Math: |delta + targetVel*t|² = (projectileSpeed*t)²
    ///   Expands to:  a·t² + b·t + c = 0
    ///   where a = |targetVel|² - projectileSpeed²
    ///         b = 2·dot(delta, targetVel)
    ///         c = |delta|²
    /// Pick the smallest positive root t; if none, aim directly at current position.
    /// Finally, rotate the aim direction by a random spread angle for imperfect accuracy.
    /// </summary>
    Vector2 ComputeLeadAim(Vector2 targetPos, Vector2 targetVel)
    {
        Vector2 delta = targetPos - (Vector2)firePoint.position;

        float a = targetVel.sqrMagnitude - projectileSpeed * projectileSpeed;
        float b = 2f * Vector2.Dot(delta, targetVel);
        float c = delta.sqrMagnitude;

        float t = SolveSmallestPositiveRoot(a, b, c);

        Vector2 aimDir = t > 0f
            ? (delta + targetVel * t).normalized   // lead shot
            : delta.normalized;                     // fallback: direct aim

        // Add imperfect accuracy: rotate the aim vector by a random angle in [-spread, +spread]
        if (accuracySpread > 0f)
            aimDir = RotateVector(aimDir, Random.Range(-accuracySpread, accuracySpread));

        return aimDir;
    }

    static float SolveSmallestPositiveRoot(float a, float b, float c)
    {
        if (Mathf.Abs(a) < 0.001f)
        {
            // Degenerate: target speed ≈ projectile speed → linear equation b·t + c = 0
            return Mathf.Abs(b) > 0.001f ? -c / b : -1f;
        }

        float discriminant = b * b - 4f * a * c;
        if (discriminant < 0f) return -1f; // No real solution

        float sqrtD = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtD) / (2f * a);
        float t2 = (-b + sqrtD) / (2f * a);

        if (t1 > 0f && t2 > 0f) return Mathf.Min(t1, t2);
        if (t1 > 0f) return t1;
        if (t2 > 0f) return t2;
        return -1f;
    }

    // ─── Firing ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the projectile with a rotation such that transform.up points in aimDir.
    /// ProjectileController.Start() sets velocity = transform.up * speed, so this is all we need.
    /// The ship's own rotation is NOT modified.
    /// </summary>
    void FireProjectile(Vector2 aimDir)
    {
        // Convert aimDir to a Z-rotation where transform.up == aimDir
        // atan2(aimDir.x, aimDir.y) gives the angle from world-up, which maps to -Z rotation.
        float angle = Mathf.Atan2(aimDir.x, aimDir.y) * Mathf.Rad2Deg;
        Quaternion aimRotation = Quaternion.Euler(0f, 0f, -angle);
        Instantiate(projectilePrefab, firePoint.position, aimRotation);
    }

    // ─── Utility ──────────────────────────────────────────────────────────────

    static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}
