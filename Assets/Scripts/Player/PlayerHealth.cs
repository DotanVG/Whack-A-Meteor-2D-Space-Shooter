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
        HandleMeteorCollision(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleMeteorCollision(collision.gameObject);
    }

    private void HandleMeteorCollision(GameObject obj)
    {
        if (invincible) return;
        if (obj.CompareTag("BigBrownMeteor") || obj.CompareTag("BigGreyMeteor") ||
            obj.CompareTag("MediumBrownMeteor") || obj.CompareTag("MediumGreyMeteor") ||
            obj.CompareTag("SmallBrownMeteor") || obj.CompareTag("SmallGreyMeteor") ||
            obj.CompareTag("TinyBrownMeteor") || obj.CompareTag("TinyGreyMeteor"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }
            // destroy the meteor after losing a life and start invincibility
            Destroy(obj);
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
