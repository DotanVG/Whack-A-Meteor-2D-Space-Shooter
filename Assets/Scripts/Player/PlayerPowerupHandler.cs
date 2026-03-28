using UnityEngine;

/// <summary>
/// PlayerPowerupHandler — auto-added to the player by PlayerHealth.Start().
/// Tracks active timed powerup effects and exposes bool properties for other scripts.
/// Draws an OnGUI countdown bar at the bottom-left of the screen.
/// </summary>
public class PlayerPowerupHandler : MonoBehaviour
{
    // ── Active state ─────────────────────────────────────────────────────────────
    public bool IsSpeedBoosted    { get; private set; }
    public bool IsDoubleFire      { get; private set; }
    public bool IsScoreMultiplied { get; private set; }

    private float _speedEndTime, _fireEndTime, _scoreEndTime;
    private GUIStyle _hudStyle;

    // ── Apply ─────────────────────────────────────────────────────────────────────

    public void ApplyPowerup(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.ShieldRecharge:
                GetComponent<ShieldController>()?.AddCharge(1);
                break;

            case PowerupType.SpeedBoost:
                float sDur = BalanceService.Instance?.GetFloat("powerup.speed_boost_duration", 5f) ?? 5f;
                _speedEndTime = Time.time + sDur;
                IsSpeedBoosted = true;
                break;

            case PowerupType.DoubleFire:
                float fDur = BalanceService.Instance?.GetFloat("powerup.double_fire_duration", 10f) ?? 10f;
                _fireEndTime = Time.time + fDur;
                IsDoubleFire = true;
                break;

            case PowerupType.ScoreMultiplier:
                float mDur = BalanceService.Instance?.GetFloat("powerup.score_mult_duration", 10f) ?? 10f;
                _scoreEndTime = Time.time + mDur;
                IsScoreMultiplied = true;
                break;

            case PowerupType.ExtraLife:
                GameManager.Instance?.AddLife();
                break;
        }

        Debug.Log($"[Powerup] {type} activated on {gameObject.name}");
    }

    // ── Expire ────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (IsSpeedBoosted    && Time.time > _speedEndTime)  IsSpeedBoosted    = false;
        if (IsDoubleFire      && Time.time > _fireEndTime)   IsDoubleFire      = false;
        if (IsScoreMultiplied && Time.time > _scoreEndTime)  IsScoreMultiplied = false;
    }

    // ── HUD ───────────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        if (!GameFeatureFlags.UsePowerups) return;
        if (!IsSpeedBoosted && !IsDoubleFire && !IsScoreMultiplied) return;

        if (_hudStyle == null)
        {
            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize   = 16,
                fontStyle  = FontStyle.Bold,
                alignment  = TextAnchor.MiddleLeft
            };
        }

        float x = 10f;
        float y = Screen.height - 80f;

        if (IsSpeedBoosted)
        {
            float rem = _speedEndTime - Time.time;
            _hudStyle.normal.textColor = new Color(0.4f, 1f, 0.4f);
            GUI.Label(new Rect(x, y, 180f, 24f), $"SPEED  {rem:F1}s", _hudStyle);
            y -= 26f;
        }
        if (IsDoubleFire)
        {
            float rem = _fireEndTime - Time.time;
            _hudStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(x, y, 180f, 24f), $"2x FIRE  {rem:F1}s", _hudStyle);
            y -= 26f;
        }
        if (IsScoreMultiplied)
        {
            float rem = _scoreEndTime - Time.time;
            _hudStyle.normal.textColor = new Color(1f, 0.5f, 1f);
            GUI.Label(new Rect(x, y, 180f, 24f), $"2x SCORE  {rem:F1}s", _hudStyle);
        }
    }
}
