using System.Collections;
using UnityEngine;

/// <summary>
/// ShieldVisual — attaches a pulsing shield sprite overlay to the player ship.
/// Uses shield1/shield2/shield3 sprites to cycle while the shield has charges.
/// Auto-added to the player by ShieldController.
///
/// Shield ring fades in on charge → pulses while active → fades out when depleted.
/// </summary>
public class ShieldVisual : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Sprite[]       _frames;
    private bool           _running;

    public static ShieldVisual AttachTo(GameObject player, Sprite[] shieldFrames)
    {
        var sv = player.GetComponent<ShieldVisual>();
        if (sv == null) sv = player.AddComponent<ShieldVisual>();
        sv._frames = shieldFrames;
        return sv;
    }

    void Awake()
    {
        // Create a child GO for the shield ring so it doesn't interfere with the ship renderer
        var child = new GameObject("ShieldRing");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale    = Vector3.one * 1.4f;

        _sr = child.AddComponent<SpriteRenderer>();
        _sr.sortingOrder = 10; // above ship
        _sr.color        = new Color(1f, 1f, 1f, 0f);
    }

    /// <summary>Called by ShieldController.Awake when charges > 0.</summary>
    public void Activate()
    {
        if (_running) return;
        StartCoroutine(PulseLoop());
    }

    /// <summary>Called by ShieldController when all charges are consumed.</summary>
    public void Deactivate()
    {
        StopAllCoroutines();
        _running = false;
        if (_sr) _sr.color = new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator PulseLoop()
    {
        if (_frames == null || _frames.Length == 0) yield break;
        _running = true;

        // Fade in
        yield return Fade(0f, 0.75f, 0.2f);

        int frame = 0;
        while (_running)
        {
            if (_sr && _frames.Length > 0)
            {
                _sr.sprite = _frames[frame % _frames.Length];
                frame++;

                // Gentle alpha pulse: 0.5→0.8→0.5 over 0.9s
                float t    = Time.time % 0.9f / 0.9f;
                float a    = 0.5f + 0.3f * Mathf.Sin(t * Mathf.PI);
                Color col  = _sr.color;
                col.a      = a;
                _sr.color  = col;
            }
            yield return new WaitForSeconds(0.12f);
        }
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            if (_sr)
            {
                Color c = _sr.color;
                c.a     = Mathf.Lerp(from, to, elapsed / dur);
                _sr.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (_sr)
        {
            Color c = _sr.color;
            c.a = to;
            _sr.color = c;
        }
    }
}
