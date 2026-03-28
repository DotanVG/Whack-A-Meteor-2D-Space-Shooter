using System.Collections;
using UnityEngine;

/// <summary>
/// GameServices — composition root for all service singletons.
///
/// Place ONE instance of this prefab/GameObject in the Game scene.
/// It spawns child GameObjects for each service so they appear cleanly
/// in the Hierarchy under "GameServices".
///
/// Initialization order (guaranteed by child index order):
///   1. BalanceService  — must be first; others may read balance on Start
///   2. EconomyService
///   3. RunStateService — subscribes to GameManager events
///   4. ProgressionService — subscribes to RunStateService events
///
/// All services self-register singletons in Awake(), so order-of-Awake
/// only matters within this GameObject's child list.
/// </summary>
public class GameServices : MonoBehaviour
{
    // Inspector refs — auto-populated in Awake if children already exist,
    // or created dynamically if this script is dropped in fresh.
    [Header("Auto-wired on Awake — leave blank to let this script create them")]
    public BalanceService     balanceService;
    public EconomyService     economyService;
    public RunStateService    runStateService;
    public ProgressionService progressionService;

    void Awake()
    {
        EnsureService(ref balanceService,     "BalanceService");
        EnsureService(ref economyService,     "EconomyService");
        EnsureService(ref runStateService,    "RunStateService");
        EnsureService(ref progressionService, "ProgressionService");
    }

    void Start()
    {
        // Delay one frame so child service Start() logs appear before this summary.
        StartCoroutine(LogServicesReady());
    }

    IEnumerator LogServicesReady()
    {
        yield return null; // wait one frame
        Debug.Log("========== [GameServices] All services online ==========" +
                  $"\n  BalanceService     : {(balanceService     != null ? "OK" : "MISSING")}" +
                  $"\n  EconomyService     : {(economyService     != null ? "OK" : "MISSING")}" +
                  $"\n  RunStateService    : {(runStateService    != null ? "OK" : "MISSING")}" +
                  $"\n  ProgressionService : {(progressionService != null ? "OK" : "MISSING")}" +
                  $"\n  Flags — Economy:{GameFeatureFlags.UseEconomy}" +
                  $" CSV:{GameFeatureFlags.UseCSVBalance}" +
                  $" Store:{GameFeatureFlags.UseStore}" +
                  $" SkillTree:{GameFeatureFlags.UseSkillTree}" +
                  "\n=======================================================");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void EnsureService<T>(ref T field, string goName) where T : MonoBehaviour
    {
        if (field != null) return; // already assigned in Inspector

        // Try to find existing child
        Transform existing = transform.Find(goName);
        if (existing != null)
        {
            field = existing.GetComponent<T>();
            if (field != null) return;
        }

        // Create new child
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        field = go.AddComponent<T>();
    }
}
