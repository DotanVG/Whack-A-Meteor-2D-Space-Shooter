using UnityEngine;

/// <summary>
/// EconomyService — dual-currency persistent wallet (Stardust / Metal).
///
/// Stardust : earned from meteor kills (any weapon). Basic currency for
///            skills, upgrades, and most purchases.
/// Metal    : earned from enemy ship kills (future enemy types). Used for
///            special attacks, ship unlocks, and high-tier upgrades.
///
/// Wallet persists across runs via PlayerPrefs — players carry their
/// earnings into the store after death. Phase 3 will migrate to a
/// proper save slot (JSON).
///
/// Drop amounts are tunable via balance_master.csv (economy.stardust_*).
/// </summary>
public class EconomyService : MonoBehaviour
{
    public static EconomyService Instance { get; private set; }

    public int Stardust { get; private set; }
    public int Metal    { get; private set; }

    public static event System.Action<int> OnStardustChanged;
    public static event System.Action<int> OnMetalChanged;

    private const string PrefKeyStardust = "Economy.Stardust";
    private const string PrefKeyMetal    = "Economy.Metal";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadWallet();
        Debug.Log($"[EconomyService] Initialized — Economy:{GameFeatureFlags.UseEconomy} " +
                  $"| Stardust:{Stardust}  Metal:{Metal}");
    }

    // ── Public earn methods — called by kill handlers ─────────────────────────

    /// <summary>
    /// Award Stardust from any meteor kill (projectile or hammer).
    /// Amount depends on meteor size (tuned via BalanceService).
    /// </summary>
    public void EarnFromMeteor(string meteorTag)
    {
        if (!GameFeatureFlags.UseEconomy) return;
        AddStardust(StardustDropAmount(meteorTag), meteorTag);
    }

    /// <summary>
    /// Award Metal from an enemy ship kill.
    /// Pass the faction for faction-specific drop amounts; omit for the generic fallback.
    /// </summary>
    public void EarnMetalFromEnemy(Faction? faction = null)
    {
        if (!GameFeatureFlags.UseEconomy) return;
        int amount;
        string source;
        if (faction.HasValue)
        {
            string key = $"economy.metal_enemy_{faction.Value.ToString().ToLower()}";
            amount = GetInt(key, GetInt("economy.metal_enemy", 2));
            source = faction.Value.ToString();
        }
        else
        {
            amount = GetInt("economy.metal_enemy", 2);
            source = "Enemy";
        }
        AddMetal(amount, source);
    }

    // ── Spend (Phase 3 Store will use this) ───────────────────────────────────

    public enum CurrencyType { Stardust, Metal }

    /// <summary>Deduct currency. Returns false if insufficient funds.</summary>
    public bool Spend(CurrencyType type, int amount)
    {
        if (!GameFeatureFlags.UseEconomy) return false;
        if (amount <= 0) return false;
        if (DevMode.InfiniteCurrency) return true;

        if (type == CurrencyType.Stardust)
        {
            if (Stardust < amount) return false;
            Stardust -= amount;
            OnStardustChanged?.Invoke(Stardust);
            SaveWallet();
            Debug.Log($"[Economy] -{amount} Stardust (total: {Stardust})");
            return true;
        }
        else
        {
            if (Metal < amount) return false;
            Metal -= amount;
            OnMetalChanged?.Invoke(Metal);
            SaveWallet();
            Debug.Log($"[Economy] -{amount} Metal (total: {Metal})");
            return true;
        }
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    void LoadWallet()
    {
        Stardust = PlayerPrefs.GetInt(PrefKeyStardust, 0);
        Metal    = PlayerPrefs.GetInt(PrefKeyMetal,    0);
        OnStardustChanged?.Invoke(Stardust);
        OnMetalChanged?.Invoke(Metal);
    }

    void SaveWallet()
    {
        PlayerPrefs.SetInt(PrefKeyStardust, Stardust);
        PlayerPrefs.SetInt(PrefKeyMetal,    Metal);
        PlayerPrefs.Save();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    void AddStardust(int amount, string source)
    {
        if (amount <= 0) return;
        Stardust += amount;
        OnStardustChanged?.Invoke(Stardust);
        SaveWallet();
        Debug.Log($"[Economy] +{amount} Stardust from {source}  (total: {Stardust})");
    }

    public void AddStardust(int amount, string source = "Direct")
    {
        if (amount <= 0) return;
        Stardust += amount;
        OnStardustChanged?.Invoke(Stardust);
        SaveWallet();
        Debug.Log($"[Economy] +{amount} Stardust from {source}  (total: {Stardust})");
    }

    public void AddMetal(int amount, string source = "Direct")
    {
        if (amount <= 0) return;
        Metal += amount;
        OnMetalChanged?.Invoke(Metal);
        SaveWallet();
        Debug.Log($"[Economy] +{amount} Metal from {source}  (total: {Metal})");
    }

    int StardustDropAmount(string meteorTag)
    {
        string size = meteorTag.StartsWith("Big")    ? "big"
                    : meteorTag.StartsWith("Medium")  ? "medium"
                    : meteorTag.StartsWith("Small")   ? "small"
                    : "tiny";
        return GetInt($"economy.stardust_{size}", 1);
    }

    static int GetInt(string key, int defaultVal)
        => BalanceService.Instance != null
            ? BalanceService.Instance.GetInt(key, defaultVal)
            : defaultVal;
}
