using UnityEngine;

/// <summary>
/// LevelService — manages persistent player level and cumulative XP.
///
/// XP is earned from meteor kills in real-time (via GameManager.OnScoreChanged delta).
/// Level-ups fire during gameplay so the popup appears immediately on threshold.
///
/// XP curve: XP needed for level N → N+1  =  xp_base * xp_growth^(N-1)
///   Default base=300, growth=1.35 gives:
///     Lv 1→2 :   300 XP   (cumulative:     300)
///     Lv 2→3 :   405 XP   (cumulative:     705)
///     Lv 3→4 :   547 XP   (cumulative:   1 252)
///     Lv 4→5 :   739 XP   (cumulative:   1 991)
///     Lv 5→6 :   998 XP   (cumulative:   2 989)
///     Lv10→11: 3 315 XP   (cumulative: ~11 925)
///
/// Both values are tunable via balance_master.csv without code changes.
/// </summary>
public class LevelService : MonoBehaviour
{
    public static LevelService Instance { get; private set; }

    public int Level         { get; private set; } = 1;
    public int TotalXP       { get; private set; } = 0;
    public int CurrentLevelXP { get; private set; } = 0;  // XP within current level
    public int XPToNextLevel  { get; private set; } = 0;  // XP cost of current level

    /// <summary>Fired when the player gains a level. Payload = new level number.</summary>
    public static event System.Action<int> OnLevelUp;

    private const string PrefKeyXP    = "Progression.TotalXP";
    private const string PrefKeyLevel = "Progression.Level";
    private const int    MaxLevel     = 50;

    private int _prevRunScore = 0; // tracks score delta within the current run

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnScoreChanged += HandleScoreChanged;
        GameManager.OnRunStarted   += HandleRunStarted;
    }

    void OnDisable()
    {
        GameManager.OnScoreChanged -= HandleScoreChanged;
        GameManager.OnRunStarted   -= HandleRunStarted;
    }

    void Start()
    {
        TotalXP = Mathf.Max(0, PlayerPrefs.GetInt(PrefKeyXP, 0));
        RefreshLevelFromXP();
        Debug.Log($"[LevelService] Initialized — Level:{Level}  TotalXP:{TotalXP}  " +
                  $"CurrentLevelXP:{CurrentLevelXP}/{XPToNextLevel}");
    }

    // ── Public helpers ────────────────────────────────────────────────────────

    /// <summary>XP cost to advance from level <paramref name="level"/> to level+1.</summary>
    public int XPForLevel(int level)
    {
        if (level >= MaxLevel) return int.MaxValue;
        float xpBase   = Get("progression.xp_base",   5000f);
        float xpGrowth = Get("progression.xp_growth",   1.5f);
        return Mathf.RoundToInt(xpBase * Mathf.Pow(xpGrowth, level - 1));
    }

    /// <summary>Total cumulative XP required to reach <paramref name="level"/>.</summary>
    public int CumulativeXPForLevel(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++)
            total += XPForLevel(i);
        return total;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    void HandleRunStarted()
    {
        _prevRunScore = 0; // reset delta tracking so new run starts clean
    }

    void HandleScoreChanged(int newScore)
    {
        int gained = newScore - _prevRunScore;
        _prevRunScore = newScore;
        if (gained > 0) AddXP(gained);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    void AddXP(int amount)
    {
        TotalXP += amount;

        int prevLevel = Level;
        RefreshLevelFromXP();
        SaveProgression();

        if (Level > prevLevel)
        {
            for (int lv = prevLevel + 1; lv <= Level; lv++)
            {
                Debug.Log($"[LevelService] LEVEL UP! {lv - 1} → {lv}  (TotalXP:{TotalXP})");
                OnLevelUp?.Invoke(lv);
            }
        }
    }

    void RefreshLevelFromXP()
    {
        // Derive Level from TotalXP — always authoritative
        int computed = 1;
        while (computed < MaxLevel && TotalXP >= CumulativeXPForLevel(computed + 1))
            computed++;
        Level = computed;

        int floor      = CumulativeXPForLevel(Level);
        CurrentLevelXP = TotalXP - floor;
        XPToNextLevel  = Level < MaxLevel ? XPForLevel(Level) : 0;
    }

    void SaveProgression()
    {
        PlayerPrefs.SetInt(PrefKeyXP,    TotalXP);
        PlayerPrefs.SetInt(PrefKeyLevel, Level);
        PlayerPrefs.Save();
    }

    static float Get(string key, float def)
        => BalanceService.Instance != null
            ? BalanceService.Instance.GetFloat(key, def)
            : def;
}
