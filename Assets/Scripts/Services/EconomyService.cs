using UnityEngine;

/// <summary>
/// EconomyService — dual-currency wallet stub for Phase 0.
///
/// Phase 0: scaffolding only. Earn/Spend do nothing while
///          GameFeatureFlags.UseEconomy == false.
///
/// Phase 2 upgrade path:
///   - Set GameFeatureFlags.UseEconomy = true
///   - Implement Wallet with earn/spend/serialize
///   - Hook EnemyHealth and MeteorSplit into OnEnemyKilled / OnMeteorDestroyed
///   - Wire HUD to display both currencies
/// </summary>
public class EconomyService : MonoBehaviour
{
    public static EconomyService Instance { get; private set; }

    // ── Currency totals (in-run) ──────────────────────────────────────────────
    public int Stardust { get; private set; }
    public int ScrapMetal { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public static event System.Action<int> OnStardustChanged;
    public static event System.Action<int> OnScrapChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Stardust  = 0;
        ScrapMetal = 0;
        Debug.Log($"[EconomyService] Initialized — Economy active: {GameFeatureFlags.UseEconomy}");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public enum CurrencyType { Stardust, ScrapMetal }

    /// <summary>Award currency. No-op until UseEconomy flag is true.</summary>
    public void Earn(CurrencyType type, int amount)
    {
        if (!GameFeatureFlags.UseEconomy) return;
        if (amount <= 0) return;

        if (type == CurrencyType.Stardust)
        {
            Stardust += amount;
            OnStardustChanged?.Invoke(Stardust);
            Debug.Log($"[EconomyService] +{amount} Stardust  (total: {Stardust})");
        }
        else
        {
            ScrapMetal += amount;
            OnScrapChanged?.Invoke(ScrapMetal);
            Debug.Log($"[EconomyService] +{amount} ScrapMetal (total: {ScrapMetal})");
        }
    }

    /// <summary>Spend currency. Returns false if insufficient funds.</summary>
    public bool Spend(CurrencyType type, int amount)
    {
        if (!GameFeatureFlags.UseEconomy) return false;
        if (amount <= 0) return false;

        if (type == CurrencyType.Stardust)
        {
            if (Stardust < amount) return false;
            Stardust -= amount;
            OnStardustChanged?.Invoke(Stardust);
            return true;
        }
        else
        {
            if (ScrapMetal < amount) return false;
            ScrapMetal -= amount;
            OnScrapChanged?.Invoke(ScrapMetal);
            return true;
        }
    }

    // TODO Phase 2: serialize wallet totals to PlayerPrefs / JSON save slot
    // TODO Phase 2: load persisted totals on Start (meta-currency persists across runs)
}
