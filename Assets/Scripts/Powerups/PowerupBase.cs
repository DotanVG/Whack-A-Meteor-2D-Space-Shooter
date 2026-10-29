using UnityEngine;

public enum PowerupType
{
    // ── Skill-gated drops (from enemies) ──
    ShieldRecharge  = 0,
    SpeedBoost      = 1,
    DoubleFire      = 2,
    ScoreMultiplier = 3,
    ExtraLife       = 4,
    // ── Tiered collectibles (from meteors) ──
    BoltTier        = 5,  // increments DoubleFire tier (bolt_bronze/silver/gold HUD)
    ShieldTier      = 6,  // increments Shield tier
    StarTier        = 7,  // increments ScoreMultiplier tier
    // ── Pills ──
    PillHealth      = 8,  // pill_red  — restore 1 life
    PillLaserBoost  = 9,  // pill_blue — brief laser speed + fire-rate boost
}

/// <summary>
/// PowerupBase — attach to every powerup prefab alongside SpriteRenderer and CircleCollider2D (IsTrigger).
/// Set the type in the Inspector per prefab. The pickup drifts downward and auto-despawns.
/// On player contact it calls PlayerPowerupHandler.ApplyPowerup().
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public class PowerupBase : MonoBehaviour
{
    public PowerupType type;

    private float _driftSpeed;
    private float _lifetime;
    private Transform _player;
    private float _magnetRadius;
    private bool _collected;

    void Start()
    {
        _driftSpeed = BalanceService.Instance?.GetFloat("powerup.drift_speed", 1.5f) ?? 1.5f;
        _lifetime   = BalanceService.Instance?.GetFloat("powerup.lifetime",    8.0f) ?? 8.0f;

        GetComponent<CircleCollider2D>().isTrigger = true;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) _player = player.transform;
        _magnetRadius = SkillService.Current.GetModifier(SkillStat.PickupMagnetRadius);
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        if (GameManager.Instance && !GameManager.Instance.IsSessionRunning) return;
        if (_player && _magnetRadius > 0f &&
            (_player.position - transform.position).sqrMagnitude <= _magnetRadius * _magnetRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, 8f * Time.deltaTime);
            if ((_player.position - transform.position).sqrMagnitude < .16f)
                Collect(_player.GetComponent<PlayerPowerupHandler>());
            return;
        }
        transform.Translate(Vector2.down * _driftSpeed * Time.deltaTime);
        // Gentle horizontal sine sway for visual appeal
        transform.Translate(Vector2.right * Mathf.Sin(Time.time * 2f + GetInstanceID()) * 0.3f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!GameFeatureFlags.UsePowerups) return;

        Collect(other.GetComponent<PlayerPowerupHandler>());
    }

    private void Collect(PlayerPowerupHandler handler)
    {
        if (_collected || !handler || !GameFeatureFlags.UsePowerups) return;
        if (GameManager.Instance && !GameManager.Instance.IsSessionRunning) return;
        _collected = true;
        handler.ApplyPowerup(type);
        GameLogger.PowerupCollected(type, transform.position);
        Destroy(gameObject);
    }
}
