using System.Collections;
using UnityEngine;

/// <summary>
/// ShieldController — absorbs incoming hits before lives are lost.
///
/// Auto-added to the player GameObject by PlayerHealth.Start() so no
/// manual scene wiring is required. Charges are set from SkillService:
///   Shield Lv1 → 1 charge   Shield Lv2 → 2 charges
///
/// When a charge is consumed the ship flashes blue and a brief window
/// prevents an immediate double-hit on the same frame.
/// </summary>
public class ShieldController : MonoBehaviour
{
    private SpriteRenderer _sr;
    private int  _charges;
    private bool _absorbing;

    void Awake()
    {
        _sr      = GetComponent<SpriteRenderer>();
        _charges = SkillService.Instance?.GetShieldCharges() ?? 0;
        if (_charges > 0)
            Debug.Log($"[Shield] {_charges} charge(s) ready.");
    }

    /// <summary>
    /// Call before applying damage. Returns true if this hit was absorbed
    /// by a shield charge (caller should skip life loss and destroy the hazard).
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (_absorbing || _charges <= 0) return false;
        _charges--;
        Debug.Log($"[Shield] Hit absorbed! Charges remaining: {_charges}");
        StartCoroutine(AbsorbFeedback());
        return true;
    }

    IEnumerator AbsorbFeedback()
    {
        _absorbing = true;
        for (int i = 0; i < 4; i++)
        {
            if (_sr) _sr.color = new Color(0.3f, 0.7f, 1f);
            yield return new WaitForSeconds(0.08f);
            if (_sr) _sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
        _absorbing = false;
    }

    public int Charges => _charges;
}
