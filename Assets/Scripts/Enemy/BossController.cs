using UnityEngine;

/// <summary>Existing elite prefab with deterministic health initialization and kill-only bounty.</summary>
public class BossController : MonoBehaviour
{
    private bool configured;
    private bool rewarded;
    private int metalDrop = 20;

    /// <summary>Applies a milestone snapshot before any damage can arrive.</summary>
    public void Configure(EncounterCatalog catalogue, WaveDefinition wave)
    {
        configured = true;
        metalDrop = catalogue.eliteMetalReward + wave.metalReward;
        var health = GetComponent<EnemyHealth>();
        if (health) health.Initialize(wave.eliteHealth);
        var controller = GetComponent<EnemyController>();
        if (controller) controller.speed = catalogue.eliteSpeed * wave.speedMultiplier;
        transform.localScale = Vector3.one * catalogue.eliteScale;
    }

    private void Start()
    {
        if (configured) return;
        var health = GetComponent<EnemyHealth>();
        if (health) health.Initialize(BalanceService.Instance?.GetInt("boss.hp", 20) ?? 20);
        var controller = GetComponent<EnemyController>();
        if (controller) controller.speed = BalanceService.Instance?.GetFloat("boss.speed", 1f) ?? 1f;
        transform.localScale = Vector3.one * (BalanceService.Instance?.GetFloat("boss.scale", 2.5f) ?? 2.5f);
        metalDrop = BalanceService.Instance?.GetInt("boss.metal_drop", 20) ?? 20;
    }

    /// <summary>Only EnemyHealth death invokes this; escaping grants no bounty.</summary>
    public void OnBossDeath()
    {
        if (rewarded) return;
        rewarded = true;
        if (EconomyService.Instance) EconomyService.Instance.AddMetal(metalDrop, "Elite defeated");
        GameLogger.BossKilled(FindObjectOfType<WaveManager>()?.currentWave ?? 0, Time.timeSinceLevelLoad);
    }
}
