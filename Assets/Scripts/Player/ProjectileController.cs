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
        GetComponent<Rigidbody2D>().velocity = transform.up * EffectiveSpeed;

        // Destroy the projectile after 'lifeTime' seconds if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hits == null || (GameManager.Instance && !GameManager.Instance.IsSessionRunning)) return;
        var meteor = other.GetComponentInParent<MeteorSplit>();
        var enemy = other.GetComponentInParent<EnemyHealth>();
        GameObject target = meteor ? meteor.gameObject : enemy ? enemy.gameObject : null;
        if (!target || !hits.TryHit(target.GetInstanceID())) return;
        int damage = DamageForRoll(Random.value);
        if (damage > 1) CombatFeedback.Critical(target.transform.position, damage);
        if (meteor) meteor.OnProjectileHit(damage);
        else enemy.TakeDamage(damage);
        if (enemyHitClip) AudioSource.PlayClipAtPoint(enemyHitClip, transform.position);
        if (hitParticles)
        {
            var effect = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        if (hits.Exhausted) Destroy(gameObject);
    }
}
