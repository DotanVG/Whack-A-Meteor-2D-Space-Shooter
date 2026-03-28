using UnityEngine;

/// <summary>
/// EconomyService — dual-currency in-run wallet (Stardust / ScrapMetal).
///
/// Stardust  : earned from projectile meteor kills + enemy kills.
/// ScrapMetal: earned from hammer meteor kills + enemy kills.
///
/// Both reset to 0 at the start of each run.
/// Drop amounts are tunable via balance_master.csv (economy.stardust_*/scrap_*).
///
/// Phase 3 will persist totals to a save slot so meta-currency survives
/// across runs. For now everything resets on run start.
/// </summary>
public class EconomyService : MonoBehaviour
{
    public static EconomyService Instance { get; private set; }

    public int Stardust   { get; private set; }
    public int ScrapMetal { get; private set; }

    public static event System.Action<int> OnStardustChanged;
    public static event System.Action<int> OnScrapChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()  { RunStateService.OnRunStarted += ResetWallet; }
    void OnDisable() { RunStateService.OnRunStarted -= ResetWallet; }

    void Start()
    {
        // ResetWallet fires from OnRunStarted, but call it here too so the wallet
        // starts clean even if OnRunStarted already fired before we subscribed.
        ResetWallet();
        Debug.Log($"[EconomyService] Initialized — Economy:{GameFeatureFlags.UseEconomy}");
    }

    // ── Public earn methods — called by kill handlers ─────────────────────────

    /// <summary>
    /// Award Stardust from a projectile meteor kill.
    /// Amount depends on meteor size (read from BalanceService).
    /// </summary>
    public void EarnFromMeteorProjectile(string meteorTag)
    {
        if (!GameFeatureFlags.UseEconomy) return;
        AddStardust(DropAmount("stardust", meteorTag), meteorTag);
    }

    /// <summary>
    /// Award ScrapMetal from a hammer meteor kill.
    /// Amount depends on meteor size (read from BalanceService).
    /// </summary>
    public void EarnFromMeteorHammer(string meteorTag)
    {
        if (!GameFeatureFlags.UseEconomy) return;
        AddScrap(DropAmount("scrap", meteorTag), meteorTag);
    }

    /// <summary>Award Stardust + ScrapMetal from an enemy ship kill.</summary>
    public void EarnFromEnemyKill()
    {
        if (!GameFeatureFlags.UseEconomy) return;
        AddStardust(GetInt("economy.stardust_enemy", 5), "Enemy");
        AddScrap   (GetInt("economy.scrap_enemy",    2), "Enemy");
    }

    // ── Spend (Phase 3 Store will use this) ───────────────────────────────────

    public enum CurrencyType { Stardust, ScrapMetal }

    /// <summary>Deduct currency. Returns false if insufficient funds.</summary>
    public bool Spend(CurrencyType type, int amount)
    {
        if (!GameFeatureFlags.UseEconomy) return false;
        if (amount <= 0) return false;

        if (type == CurrencyType.Stardust)
        {
            if (Stardust < amount) return false;
            Stardust -= amount;
            OnStardustChanged?.Invoke(Stardust);
            Debug.Log($"[Economy] -{amount} Stardust (total: {Stardust})");
            return true;
        }
        else
        {
            if (ScrapMetal < amount) return false;
            ScrapMetal -= amount;
            OnScrapChanged?.Invoke(ScrapMetal);
            Debug.Log($"[Economy] -{amount} ScrapMetal (total: {ScrapMetal})");
            return true;
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    void ResetWallet()
    {
        Stardust   = 0;
        ScrapMetal = 0;
        OnStardustChanged?.Invoke(Stardust);
        OnScrapChanged?.Invoke(ScrapMetal);
    }

    void AddStardust(int amount, string source)
    {
        if (amount <= 0) return;
        Stardust += amount;
        OnStardustChanged?.Invoke(Stardust);
        Debug.Log($"[Economy] +{amount} Stardust from {source}  (total: {Stardust})");
    }

    void AddScrap(int amount, string source)
    {
        if (amount <= 0) return;
        ScrapMetal += amount;
        OnScrapChanged?.Invoke(ScrapMetal);
        Debug.Log($"[Economy] +{amount} ScrapMetal from {source}  (total: {ScrapMetal})");
    }

    /// <summary>
    /// Returns the base drop amount for a given currency and meteor tag.
    /// Key format: "economy.{currency}_{size}" e.g. "economy.stardust_big"
    /// </summary>
    int DropAmount(string currency, string meteorTag)
    {
        string size = meteorTag.StartsWith("Big")    ? "big"
                    : meteorTag.StartsWith("Medium")  ? "medium"
                    : meteorTag.StartsWith("Small")   ? "small"
                    : "tiny";
        return GetInt($"economy.{currency}_{size}", 1);
    }

    static int GetInt(string key, int defaultVal)
        => BalanceService.Instance != null
            ? BalanceService.Instance.GetInt(key, defaultVal)
            : defaultVal;
}
