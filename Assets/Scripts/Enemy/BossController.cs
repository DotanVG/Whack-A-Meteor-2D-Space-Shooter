using UnityEngine;

/// <summary>
/// BossController — attach to the Boss prefab alongside EnemyHealth and EnemyController.
/// Reads boss.hp, boss.speed, and boss.scale from BalanceService on Start() and overrides
/// the sibling components so one prefab serves as the source of truth.
/// Also awards bonus Metal on death (handled in EnemyHealth.Die via EconomyService faction overload).
/// </summary>
public class BossController : MonoBehaviour
{
    void Start()
    {
        int   hp    = BalanceService.Instance?.GetInt  ("boss.hp",    20)   ?? 20;
        float spd   = BalanceService.Instance?.GetFloat("boss.speed", 1.0f) ?? 1.0f;
        float scale = BalanceService.Instance?.GetFloat("boss.scale", 2.5f) ?? 2.5f;

        EnemyHealth eh = GetComponent<EnemyHealth>();
        if (eh != null) { eh.maxHealth = hp; }

        EnemyController ec = GetComponent<EnemyController>();
        if (ec != null) { ec.speed = spd; }

        transform.localScale = Vector3.one * scale;

        Debug.Log($"[Boss] Spawned — HP:{hp}  Speed:{spd}  Scale:{scale}");
    }

    /// <summary>Called by EnemyHealth.Die() when the boss-tagged enemy dies.</summary>
    public void OnBossDeath()
    {
        int metalDrop = BalanceService.Instance?.GetInt("boss.metal_drop", 20) ?? 20;
        EconomyService.Instance?.AddMetal(metalDrop);
        GameLogger.BossKilled(
            FindObjectOfType<WaveManager>()?.currentWave ?? 0,
            Time.timeSinceLevelLoad);
    }
}
