using System.Collections;
using UnityEngine;

/// <summary>
/// ShieldVisual — attaches a pulsing blue shield ring + outer glow to the player ship.
/// Uses shield1/shield2/shield3 sprites (Assets/Sprites/Effects/) tinted blue.
/// Auto-added to the player by ShieldController.
///
/// Shield ring fades in → pulses + animates frames → fades out when depleted.
/// A second larger, dimmer renderer creates the fuzzy aura glow effect.
/// </summary>
public class ShieldVisual : MonoBehaviour
{
    // Blueish tint applied to the grey shield sprites
    private static readonly Color ShieldBlue     = new Color(0.35f, 0.70f, 1.00f);
    private static readonly Color GlowBlue       = new Color(0.25f, 0.55f, 1.00f);

    private SpriteRenderer _sr;      // inner ring
    private SpriteRenderer _glowSr;  // outer glow (larger, dimmer)
    private Sprite[]        _frames;
    private bool            _running;

    public static ShieldVisual AttachTo(GameObject player, Sprite[] shieldFrames)
    {
        var sv = player.GetComponent<ShieldVisual>();
        if (sv == null) sv = player.AddComponent<ShieldVisual>();
        sv._frames = shieldFrames;
        return sv;
    }

    void Awake()
    {
        // Inner ring — same scale as ship, blue-tinted
        var ring = new GameObject("ShieldRing");
        ring.transform.SetParent(transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale    = Vector3.one * 1.35f;

        _sr              = ring.AddComponent<SpriteRenderer>();
        _sr.sortingOrder = 10;
        _sr.color        = new Color(ShieldBlue.r, ShieldBlue.g, ShieldBlue.b, 0f);

        // Outer glow — bigger and more transparent, creates fuzzy aura feel
        var glow = new GameObject("ShieldGlow");
        glow.transform.SetParent(transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale    = Vector3.one * 1.85f;

        _glowSr              = glow.AddComponent<SpriteRenderer>();
        _glowSr.sortingOrder = 9; // behind inner ring
        _glowSr.color        = new Color(GlowBlue.r, GlowBlue.g, GlowBlue.b, 0f);
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
        SetAlpha(0f);
    }

    IEnumerator PulseLoop()
    {
        if (_frames == null || _frames.Length == 0) yield break;
        _running = true;

        // Sync both renderers to first frame immediately
        _sr.sprite     = _frames[0];
        _glowSr.sprite = _frames[0];

        // Fade in
        yield return Fade(0f, 0.80f, 0.25f);

        int frame = 0;
        while (_running)
        {
            if (_sr && _frames.Length > 0)
            {
                var sprite = _frames[frame % _frames.Length];
                _sr.sprite     = sprite;
                _glowSr.sprite = sprite;
                frame++;

                // Inner ring: alpha 0.55→0.85, gentle breathing pulse
                float t    = Time.time % 1.1f / 1.1f;
                float a    = 0.55f + 0.30f * Mathf.Sin(t * Mathf.PI * 2f);
                // Glow: slightly out of phase, kept at ~30% of inner alpha
                float tG   = (Time.time + 0.3f) % 1.4f / 1.4f;
                float aG   = 0.18f + 0.12f * Mathf.Sin(tG * Mathf.PI * 2f);

                SetAlpha(a, aG);
            }
            yield return new WaitForSeconds(0.10f);
        }
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            float frac = elapsed / dur;
            float a  = Mathf.Lerp(from, to, frac);
            float aG = Mathf.Lerp(from, to * 0.3f, frac);
            SetAlpha(a, aG);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(to, to * 0.3f);
    }

    void SetAlpha(float inner, float outer = 0f)
    {
        if (_sr)     _sr.color     = new Color(ShieldBlue.r, ShieldBlue.g, ShieldBlue.b, inner);
        if (_glowSr) _glowSr.color = new Color(GlowBlue.r,  GlowBlue.g,  GlowBlue.b,  outer);
    }
}
