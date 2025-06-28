using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    private bool invincible = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (invincible) return;
        if (other.CompareTag("BigBrownMeteor") || other.CompareTag("BigGreyMeteor") ||
            other.CompareTag("MediumBrownMeteor") || other.CompareTag("MediumGreyMeteor") ||
            other.CompareTag("SmallBrownMeteor") || other.CompareTag("SmallGreyMeteor") ||
            other.CompareTag("TinyBrownMeteor") || other.CompareTag("TinyGreyMeteor"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }
            StartCoroutine(Invincibility());
        }
    }

    IEnumerator Invincibility()
    {
        invincible = true;
        float elapsed = 0f;
        while (elapsed < GameConstants.InvincibilityDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
        sr.enabled = true;
        invincible = false;
    }
}
