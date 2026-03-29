using System.Collections;
using UnityEngine;

/// <summary>
/// EnemyCombatAI — cross-faction combat.
/// Enemies occasionally fire at the nearest enemy of a DIFFERENT faction, in
/// addition to targeting the player. This creates emergent faction warfare.
///
/// Balance key: enemy.crossfire_chance  (default 0.25 — 25% of shots go cross-faction)
/// Requires EnemyFaction and EnemyShooter siblings (or its own projectile reference).
///
/// Uses the same EnemyProjectile prefab as EnemyShooter.
/// </summary>
[RequireComponent(typeof(EnemyFaction))]
public class EnemyCombatAI : MonoBehaviour
{
    public GameObject crossFireProjectilePrefab; // assign green laser prefab

    private EnemyFaction _faction;
    private float        _nextCrossShot;
    private const string CrossFireKey = "enemy.crossfire_chance";
    private const string CrossFireRate = "enemy.crossfire_rate";

    void Start()
    {
        _faction       = GetComponent<EnemyFaction>();
        float rate     = BalanceService.Instance?.GetFloat(CrossFireRate, 4f) ?? 4f;
        _nextCrossShot = Time.time + rate * Random.Range(0.5f, 1.5f);
    }

    void Update()
    {
        if (crossFireProjectilePrefab == null) return;

        float rate = BalanceService.Instance?.GetFloat(CrossFireRate, 4f) ?? 4f;
        if (Time.time < _nextCrossShot) return;
        _nextCrossShot = Time.time + rate;

        float chance = BalanceService.Instance?.GetFloat(CrossFireKey, 0.25f) ?? 0.25f;
        if (Random.value > chance) return;

        GameObject target = FindCrossTarget();
        if (target == null) return;

        ShootAt(target.transform.position);
    }

    void ShootAt(Vector2 targetPos)
    {
        Vector2 dir   = (targetPos - (Vector2)transform.position).normalized;
        float   angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        Instantiate(crossFireProjectilePrefab, transform.position, Quaternion.Euler(0f, 0f, -angle));
    }

    GameObject FindCrossTarget()
    {
        // Find nearest enemy that belongs to a different faction
        float       best   = float.MaxValue;
        GameObject  found  = null;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (go == gameObject) continue;
            EnemyFaction ef = go.GetComponent<EnemyFaction>();
            if (ef == null || ef.faction == _faction.faction) continue;

            float d = (go.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; found = go; }
        }
        return found;
    }
}
