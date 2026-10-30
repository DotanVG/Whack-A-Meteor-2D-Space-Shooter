using System.Collections;
using System.Collections.Generic;
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
    [Header("Visual — assign shield1/2/3 sprites (optional)")]
    public Sprite[] shieldFrames; // shield1, shield2, shield3 from Sprites/Effects

    private SpriteRenderer _sr;
    private int            _charges;
    private bool           _absorbing;
    private ShieldVisual   _visual;

    void Awake()
    {
        _sr      = GetComponent<SpriteRenderer>();
        _charges = SkillService.Instance?.GetShieldCharges() ?? 0;

        if (shieldFrames != null && shieldFrames.Length > 0)
            _visual = ShieldVisual.AttachTo(gameObject, shieldFrames);

        if (_charges > 0)
        {
            _visual?.Activate();
            Debug.Log($"[Shield] {_charges} charge(s) ready.");
        }
    }

    /// <summary>
    /// Call before applying damage. Returns true if this hit was absorbed
    /// by a shield charge (caller should skip life loss and destroy the hazard).
    /// </summary>
    public bool TryAbsorbHit() => TryAbsorbHazard(null);

    /// <summary>Absorbs a hit while excluding its source (including child colliders) from the pulse.</summary>
    public bool TryAbsorbHazard(GameObject hazard)
    {
        if (_absorbing) return true;
        if (_charges <= 0) return false;
        _absorbing = true;
        _charges--;
        Debug.Log($"[Shield] Hit absorbed! Charges remaining: {_charges}");
        if (_charges <= 0) _visual?.Deactivate();
        int pulseDamage = (int)SkillService.Current.GetModifier(SkillStat.ShieldPulseDamage);
        if (pulseDamage > 0)
        {
            CombatFeedback.Ring(transform.position, 3f, new Color(.25f, .8f, 1f));
            var visited = new HashSet<int>();
            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 3f))
            {
                var meteor = collider.GetComponentInParent<MeteorSplit>();
                var enemy = collider.GetComponentInParent<EnemyHealth>();
                GameObject target = meteor ? meteor.gameObject : enemy ? enemy.gameObject : null;
                if (!IsPulseTarget(target, hazard) || !visited.Add(target.GetInstanceID())) continue;
                if (meteor) meteor.OnProjectileHit(pulseDamage);
                else enemy.TakeDamage(pulseDamage);
            }
        }
        StartCoroutine(AbsorbFeedback());
        return true;
    }

    /// <summary>The caller destroys the absorbed hazard without weapon damage, loot or splitting.</summary>
    public static bool IsPulseTarget(GameObject target, GameObject hazard)
    {
        return target && (!hazard || (target != hazard &&
            !hazard.transform.IsChildOf(target.transform) && !target.transform.IsChildOf(hazard.transform)));
    }

    IEnumerator AbsorbFeedback()
    {
        _absorbing = true;
        Color shieldBlue = new Color(0.3f, 0.7f, 1f);
        for (int i = 0; i < 4; i++)
        {
            if (_sr) _sr.color = shieldBlue;
            yield return new WaitForSeconds(0.08f);
            if (_sr) _sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
        ScreenFlash.Trigger(shieldBlue, 0.4f, 2f);
        _absorbing = false;
    }

    public int Charges => _charges;

#if UNITY_EDITOR
    /// <summary>Tests can exercise the repeat-hit guard without starting timed effects.</summary>
    public void SetStateForTests(int charges, bool absorbing)
    {
        _charges = Mathf.Max(0, charges);
        _absorbing = absorbing;
    }
#endif

    /// <summary>Add n charges from a powerup pickup, capped at the skill-tree max.</summary>
    public void AddCharge(int n)
    {
        int cap = SkillService.Instance?.GetShieldCharges() ?? 0;
        _charges = Mathf.Min(_charges + n, Mathf.Max(cap, 1)); // always allow at least 1 if shield is active
        if (_charges > 0) _visual?.Activate();
        Debug.Log($"[Shield] +{n} charge from powerup. Charges: {_charges}");
    }
}
