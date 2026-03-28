using UnityEngine;

/// <summary>
/// ProgressionService — meta-progression and save-slot stub for Phase 0.
///
/// Phase 0: scaffolding only. Records run completion so future phases can
///          build persistent upgrades on top. All persistence is no-op.
///
/// Phase 3 upgrade path:
///   - Implement save slots (JSON)
///   - Store purchased upgrades and their levels
///   - SkillTree + StoreManager read/write through this service
///   - Wire OnRunEnded to persist run outcome
/// </summary>
public class ProgressionService : MonoBehaviour
{
    public static ProgressionService Instance { get; private set; }

    // ── Run summary (populated at end of each run) ────────────────────────────
    public int  LastRunScore  { get; private set; }
    public int  TotalRunCount { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public static event System.Action<int> OnRunEnded; // payload = final score

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        RunStateService.OnGameOverStarted += HandleRunEnded;
    }

    void OnDisable()
    {
        RunStateService.OnGameOverStarted -= HandleRunEnded;
    }

    private const string SaveKey = "Progression.SaveSlot";

    [System.Serializable]
    private struct SaveData
    {
        public int totalRunCount;
        public int lastRunScore;
    }

    void Start()
    {
        LoadFromSave();
        Debug.Log($"[ProgressionService] Initialized — Store:{GameFeatureFlags.UseStore} SkillTree:{GameFeatureFlags.UseSkillTree} | Runs:{TotalRunCount}");
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    void SaveToSlot()
    {
        SaveData data = new SaveData { totalRunCount = TotalRunCount, lastRunScore = LastRunScore };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    void LoadFromSave()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) { TotalRunCount = 0; return; }
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        TotalRunCount = data.totalRunCount;
        LastRunScore  = data.lastRunScore;
    }

    // ── Internal handlers ────────────────────────────────────────────────────

    void HandleRunEnded()
    {
        TotalRunCount++;
        LastRunScore = RunStateService.Instance != null ? RunStateService.Instance.Score : 0;
        Debug.Log($"[ProgressionService] Run #{TotalRunCount} ended — Score: {LastRunScore}");
        OnRunEnded?.Invoke(LastRunScore);
        SaveToSlot();
    }

    // ── Upgrade stubs (Phase 3/4 will implement these) ───────────────────────

    /// <summary>Returns the purchased level of an upgrade. Always 0 until Phase 3.</summary>
    public int GetUpgradeLevel(string upgradeKey) => 0; // TODO Phase 3

    /// <summary>Attempt to purchase an upgrade. Always fails until Phase 3.</summary>
    public bool PurchaseUpgrade(string upgradeKey) => false; // TODO Phase 3
}
