using UnityEngine;

/// <summary>Readable aimed-shooter drift or brief pursuit followed by a committed dive.</summary>
public class EnemyController : MonoBehaviour
{
    public float speed = 2f;
    public EnemyProfile profile;
    private Transform player;
    private Vector2 direction = Vector2.down;
    private float spawnedAt;
    private Camera view;
    /// <summary>Current world-space velocity for the existing predictive auto-aim.</summary>
    public Vector2 Velocity => direction * speed;

    /// <summary>Applies authored movement before Start; shared assets remain immutable.</summary>
    public void Configure(EnemyProfile data, float speedMultiplier)
    {
        profile = data;
        speed = data.speed * speedMultiplier;
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite) sprite.color = data.tint;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        view = Camera.main;
        spawnedAt = Time.time;
        // Legacy direct instantiation still has a sensible downward fallback.
        if (!profile)
        {
            var faction = GetComponent<EnemyFaction>();
            if (faction && BalanceService.Instance)
                speed = BalanceService.Instance.GetFloat("enemy.speed_" + faction.faction.ToString().ToLower(), speed);
        }
    }

    private void Update()
    {
        if (GameManager.Instance && !GameManager.Instance.IsSessionRunning) return;
        if (profile && profile.behaviour == EnemyBehaviour.PursuitDiver && player && Time.time - spawnedAt < profile.pursuitSeconds)
        {
            Vector2 wanted = ((Vector2)player.position - (Vector2)transform.position).normalized;
            direction = Vector3.RotateTowards(direction, wanted, profile.turnDegreesPerSecond * Mathf.Deg2Rad * Time.deltaTime, 0f);
            // Never reverse upward and circle the ship forever: this is a committed pass.
            if (direction.y > -.15f) direction = new Vector2(direction.x, -.15f).normalized;
        }
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
        if (view)
        {
            Vector3 viewport = view.WorldToViewportPoint(transform.position);
            if (viewport.y < -.25f || viewport.x < -.3f || viewport.x > 1.3f) Destroy(gameObject);
        }
        // Handles a missing/changed camera and prevents survivors accumulating indefinitely.
        if (Time.time - spawnedAt > 30f) Destroy(gameObject);
    }
    // ProjectileController is the sole projectile damage owner; no duplicate collision handler.
}
