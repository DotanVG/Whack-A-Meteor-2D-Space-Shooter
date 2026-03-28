using UnityEngine;

/// <summary>
/// PowerupSpawner — singleton placed on the GameManager GameObject.
/// Assign the 5 powerup prefabs in the Inspector (index matches PowerupType enum order).
/// Called from EnemyHealth.Die() to attempt a drop at the enemy's death position.
/// </summary>
public class PowerupSpawner : MonoBehaviour
{
    public static PowerupSpawner Instance { get; private set; }

    [Tooltip("Index 0=ShieldRecharge, 1=SpeedBoost, 2=DoubleFire, 3=ScoreMultiplier, 4=ExtraLife")]
    public GameObject[] powerupPrefabs = new GameObject[5];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// Attempts to spawn a random powerup at the given world position.
    /// Respects GameFeatureFlags.UsePowerups and balance-tuned drop chance.
    /// </summary>
    public void TryDrop(Vector2 position)
    {
        if (!GameFeatureFlags.UsePowerups) return;

        float dropChance = BalanceService.Instance?.GetFloat("powerup.drop_chance", 0.15f) ?? 0.15f;
        if (Random.value > dropChance) return;

        PowerupType chosenType = PickType();
        int idx = (int)chosenType;
        if (idx >= powerupPrefabs.Length || powerupPrefabs[idx] == null)
        {
            Debug.LogWarning($"[PowerupSpawner] Prefab missing for {chosenType} (index {idx}).");
            return;
        }

        Instantiate(powerupPrefabs[idx], position, Quaternion.identity);
    }

    PowerupType PickType()
    {
        float extraLifeChance = BalanceService.Instance?.GetFloat("powerup.extra_life_chance", 0.05f) ?? 0.05f;
        if (Random.value < extraLifeChance) return PowerupType.ExtraLife;

        // Equal chance among the 4 common types
        return (PowerupType)Random.Range(0, 4);
    }
}
