using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    private bool invincible = false;
    private SpriteRenderer    sr;
    private ShieldController  shield;
    private PlayerDamageVisual damageVisual;

    void Start()
    {
        sr          = GetComponent<SpriteRenderer>();
        shield      = GetComponent<ShieldController>() ?? gameObject.AddComponent<ShieldController>();
        damageVisual = GetComponent<PlayerDamageVisual>();
        if (GetComponent<PlayerPowerupHandler>() == null) gameObject.AddComponent<PlayerPowerupHandler>();
        RefreshDamageVisual();
    }

    void RefreshDamageVisual()
    {
        if (damageVisual == null) return;
        int lives = GameManager.Instance?.Lives ?? GameConstants.StartingLives;
        damageVisual.Refresh(lives);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleDamageSource(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDamageSource(collision.gameObject);
    }

    private void HandleDamageSource(GameObject obj)
    {
        if (invincible)
        {
            bool isThreat = obj.CompareTag("Enemy")              || obj.CompareTag("EnemyProjectile") ||
                            obj.CompareTag("BigBrownMeteor")     || obj.CompareTag("BigGreyMeteor")    ||
                            obj.CompareTag("MediumBrownMeteor")  || obj.CompareTag("MediumGreyMeteor") ||
                            obj.CompareTag("SmallBrownMeteor")   || obj.CompareTag("SmallGreyMeteor")  ||
                            obj.CompareTag("TinyBrownMeteor")    || obj.CompareTag("TinyGreyMeteor");
            if (isThreat) GameLogger.PlayerInvincibleHit(obj.tag);
            return;
        }

        if (obj.CompareTag("BigBrownMeteor") || obj.CompareTag("BigGreyMeteor")       ||
            obj.CompareTag("MediumBrownMeteor") || obj.CompareTag("MediumGreyMeteor") ||
            obj.CompareTag("SmallBrownMeteor")  || obj.CompareTag("SmallGreyMeteor")  ||
            obj.CompareTag("TinyBrownMeteor")   || obj.CompareTag("TinyGreyMeteor"))
        {
            if (shield != null && shield.TryAbsorbHit()) { Destroy(obj); return; }
            TakeDamage(obj.tag);
            Destroy(obj);
        }
        else if (obj.CompareTag("Enemy"))
        {
            if (shield != null && shield.TryAbsorbHit()) { Destroy(obj); return; }
            TakeDamage(obj.tag);
            Destroy(obj);
        }
        else if (obj.CompareTag("EnemyProjectile"))
        {
            // EnemyProjectile already destroyed itself in EnemyProjectile.OnTriggerEnter2D
            if (shield != null && shield.TryAbsorbHit()) return;
            TakeDamage(obj.tag);
        }
    }

    private void TakeDamage(string sourceTag)
    {
        if (DevMode.GodMode) return;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
            GameLogger.PlayerDamage(sourceTag, transform.position,
                                    GameManager.Instance.Lives, Time.timeSinceLevelLoad);
            if (sourceTag == "Enemy")
                GameLogger.EnemyRammedPlayer(transform.position, GameManager.Instance.Lives);
        }
        RefreshDamageVisual();
        StartCoroutine(Invincibility());
        StartCoroutine(HitPulse());
    }

    IEnumerator HitPulse()
    {
        float elapsed = 0f, dur = 0.25f;
        Vector3 orig = transform.localScale;
        while (elapsed < dur)
        {
            float s = 1f + Mathf.Sin((elapsed / dur) * Mathf.PI) * 0.3f;
            transform.localScale = orig * s;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = orig;
    }

    IEnumerator Invincibility()
    {
        invincible = true;
        float elapsed = 0f;
        float invincDuration = GameConstants.InvincibilityDuration * (SkillService.Instance?.GetInvincibilityMultiplier() ?? 1f);
        while (elapsed < invincDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
        sr.enabled = true;
        invincible = false;
    }
}
