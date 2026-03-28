using UnityEngine;

/// <summary>
/// RunStateService — single source of truth for the current run's observable state.
///
/// Subscribes to GameManager's static events (OnScoreChanged, OnLivesChanged,
/// OnGameOver, OnRunStarted) and re-broadcasts them so downstream systems
/// (EconomyService, ProgressionService, UI) never need a direct GameManager reference.
///
/// AUTHORITY RULE: GameManager still owns Score and Lives mutations.
/// RunStateService is read-only observer + event relay — it never modifies run state.
/// </summary>
public class RunStateService : MonoBehaviour
{
    public static RunStateService Instance { get; private set; }

    // ── Observable state (read-only mirrors of GameManager) ──────────────────
    public int  Score      { get; private set; }
    public int  Lives      { get; private set; }
    public bool IsRunning  { get; private set; }
    public bool IsPaused   { get; private set; }
    public bool IsGameOver { get; private set; }

    // ── Re-broadcast events for other services ────────────────────────────────
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<int> OnLivesChanged;
    public static event System.Action      OnGameOverStarted;
    public static event System.Action      OnRunStarted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnScoreChanged  += HandleScoreChanged;
        GameManager.OnLivesChanged  += HandleLivesChanged;
        GameManager.OnGameOver      += HandleGameOver;
        GameManager.OnRunStarted    += HandleRunStarted;
    }

    void OnDisable()
    {
        GameManager.OnScoreChanged  -= HandleScoreChanged;
        GameManager.OnLivesChanged  -= HandleLivesChanged;
        GameManager.OnGameOver      -= HandleGameOver;
        GameManager.OnRunStarted    -= HandleRunStarted;
    }

    void Start()
    {
        IsRunning  = true;
        IsPaused   = false;
        IsGameOver = false;
        // Initial Score/Lives arrive via OnLivesChanged + OnScoreChanged fired by
        // GameManager.Start() (which runs first due to [DefaultExecutionOrder(-100)]).
        // Logging happens in HandleRunStarted once those values are set.
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    void HandleScoreChanged(int newScore)
    {
        Score = newScore;
        OnScoreChanged?.Invoke(newScore);
    }

    void HandleLivesChanged(int newLives)
    {
        Lives = newLives;
        OnLivesChanged?.Invoke(newLives);
    }

    void HandleGameOver()
    {
        IsGameOver = true;
        IsRunning  = false;
        OnGameOverStarted?.Invoke();
    }

    void HandleRunStarted()
    {
        IsRunning  = true;
        IsGameOver = false;
        // By the time this fires, OnLivesChanged + OnScoreChanged have already
        // updated our mirrors, so these values are correct.
        Debug.Log($"[RunStateService] Initialized — Score:{Score} Lives:{Lives}");
        OnRunStarted?.Invoke();
    }
}
