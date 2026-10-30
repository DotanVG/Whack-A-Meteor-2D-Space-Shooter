using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : MonoBehaviour
{
    public float speed = 50f;  // Speed at which the projectile moves
    public float lifeTime = 2f; // Lifetime of the projectile
    public AudioClip enemyHitClip;
    public GameObject hitParticles;

    private bool twinSpawned;
    private ProjectileHits hits;
    private float criticalChance;
    private float speedMultiplier = 1f;
    private bool prepared;
    private Rigidbody2D body;
    private Collider2D shotCollider;
    private readonly List<RaycastHit2D> sweepHits = new List<RaycastHit2D>(16);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        shotCollider = GetComponent<Collider2D>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnEnable()
    {
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public float EffectiveSpeed => speed * speedMultiplier;
    public int DamageForRoll(float roll) => CombatRules.Damage(1, criticalChance, roll);

    /// <summary>Shared preparation boundary for spawned shots; twins cannot recursively multiply.</summary>
    public bool PrepareShot(SkillProgress skills, bool isTwin)
    {
        if (prepared) return false;
        prepared = true;
        hits = new ProjectileHits((int)skills.GetModifier(SkillStat.ProjectilePierce));
        criticalChance = skills.GetModifier(SkillStat.CriticalChance);
        speedMultiplier = skills.GetModifier(SkillStat.ProjectileSpeed);
        bool spawnTwin = !isTwin && skills.GetModifier(SkillStat.ExtraProjectiles) > 0f;
        twinSpawned = true;
        return spawnTwin;
    }

    void Start()
    {
        if (GameManager.Instance && !GameManager.Instance.IsSessionRunning) { Destroy(gameObject); return; }
        if (PrepareShot(SkillService.Current, twinSpawned))
        {
            twinSpawned = true;
            Quaternion direction = transform.rotation;
            transform.rotation = direction * Quaternion.Euler(0f, 0f, -4f);
            var twin = Instantiate(gameObject, transform.position + transform.right * .12f,
                direction * Quaternion.Euler(0f, 0f, 4f)).GetComponent<ProjectileController>();
            twin.twinSpawned = true;
            twin.prepared = false;
            twin.hits = null;
        }
        // Both manual and automatic projectiles use this single upgrade path.
        body.velocity = transform.up * EffectiveSpeed;

        // Destroy the projectile after 'lifeTime' seconds if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (hits == null || hits.Exhausted || !shotCollider ||
            (GameManager.Instance && !GameManager.Instance.IsSessionRunning)) return;
        Vector2 displacement = body.velocity * Time.fixedDeltaTime;
        if (displacement.sqrMagnitude <= 0f) return;
        // Trigger contacts can skip thin targets between steps. Sweep the actual shot
        // shape over the next physics movement, using the same hit budget as callbacks.
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask((Physics2D.GetLayerCollisionMask(gameObject.layer) |
            shotCollider.includeLayers.value) & ~shotCollider.excludeLayers.value);
        int count = shotCollider.Cast(displacement.normalized, filter, sweepHits, displacement.magnitude);
        // Keep piercing in travel order without relying on physics query ordering.
        for (int index = 1; index < count; index++)
        {
            var hit = sweepHits[index];
            int previous = index - 1;
            while (previous >= 0 && sweepHits[previous].distance > hit.distance)
            {
                sweepHits[previous + 1] = sweepHits[previous];
                previous--;
            }
            sweepHits[previous + 1] = hit;
        }
        for (int index = 0; index < count && !hits.Exhausted; index++)
        {
            var other = sweepHits[index].collider;
            if (other && !Physics2D.GetIgnoreCollision(shotCollider, other)) HandleHit(other);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleHit(other);

    private void HandleHit(Collider2D other) => ResolveHit(other, null);

    // Explicit rolls let EditMode verify the complete feedback path without random state.
    private void ResolveHit(Collider2D other, float? roll)
    {
        if (hits == null || (GameManager.Instance && !GameManager.Instance.IsSessionRunning)) return;
        var meteor = other.GetComponentInParent<MeteorSplit>();
        var enemy = other.GetComponentInParent<EnemyHealth>();
        GameObject target = meteor ? meteor.gameObject : enemy ? enemy.gameObject : null;
        if (!target || !hits.TryHit(target.GetInstanceID())) return;
        int damage = DamageForRoll(roll ?? Random.value);
        if (meteor) meteor.OnProjectileHit(damage, damage > 1);
        else enemy.TakeDamage(damage, damage > 1);
        if (enemyHitClip) AudioSource.PlayClipAtPoint(enemyHitClip, transform.position);
        if (hitParticles)
        {
            var effect = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        if (hits.Exhausted)
        {
            body.velocity = Vector2.zero;
            Destroy(gameObject);
        }
    }
}
