using System.Collections;
using UnityEngine;

/// <summary>Survival encounters with authored themes, milestone elites and explicit regroup rewards.</summary>
public class WaveManager : MonoBehaviour
{
    public MeteorSpawner meteorSpawner;
    public SpawnDirector spawnDirector;
    public int currentWave = 1;
    public EncounterCatalog catalogue;
    public string StatusText { get; private set; } = "Prepare for launch";
    public WaveDefinition Current { get; private set; }
    private EnemySpawner enemies;

    private IEnumerator Start()
    {
        if (!meteorSpawner) meteorSpawner = FindObjectOfType<MeteorSpawner>();
        if (!spawnDirector) spawnDirector = FindObjectOfType<SpawnDirector>();
        enemies = spawnDirector ? spawnDirector.enemySpawner : FindObjectOfType<EnemySpawner>();
        if (!catalogue) catalogue = Resources.Load<EncounterCatalog>("Encounters/Catalogue");
        if (!catalogue || catalogue.Validate().Count != 0)
        {
            Debug.LogError("[WaveManager] Missing or invalid encounter catalogue");
            yield break;
        }
        // GameManager owns the launch countdown. No enemy, reward or wave clock starts early.
        while (!GameManager.Instance || !GameManager.Instance.IsSessionRunning) yield return null;
        while (GameManager.Instance && GameManager.Instance.Lives > 0)
        {
            Current = catalogue.Evaluate(currentWave);
            if (meteorSpawner) meteorSpawner.Configure(catalogue, Current);
            if (enemies) enemies.Configure(catalogue, Current);
            if (meteorSpawner) meteorSpawner.StartSpawning();
            if (spawnDirector) spawnDirector.OnWaveStarted(currentWave);
            UIManager.Instance?.UpdateWave(currentWave);
            GameLogger.WaveStarted(currentWave, Current.meteorInterval, Current.speedMultiplier, catalogue.meteorCap);
            float remaining = Current.level.activeSeconds;
            while (remaining > 0f)
            {
                StatusText = $"{Current.level.displayName}  |  Wave {currentWave}{(Current.elite ? " ELITE" : "")}  |  {Mathf.CeilToInt(remaining)}s";
                if (GameManager.Instance && GameManager.Instance.IsSessionRunning) remaining -= Time.deltaTime;
                yield return null;
            }
            if (meteorSpawner) meteorSpawner.StopSpawning();
            if (enemies) enemies.StopSpawning();
            // Survival bonus is earned once; boss bounty remains kill-only in EnemyHealth.
            if (GameManager.Instance && GameManager.Instance.Lives > 0 && EconomyService.Instance)
            {
                EconomyService.Instance.AddStardust(Current.stardustReward, "Wave survived");
                EconomyService.Instance.AddMetal(Current.metalReward, "Wave survived");
                PlayerPrefs.Save();
            }
            remaining = Current.level.regroupSeconds;
            while (remaining > 0f)
            {
                StatusText = $"Wave {currentWave} survived: +{Current.stardustReward} Stardust / +{Current.metalReward} Metal  |  Regroup {Mathf.CeilToInt(remaining)}s";
                if (GameManager.Instance && GameManager.Instance.IsSessionRunning) remaining -= Time.deltaTime;
                yield return null;
            }
            currentWave++;
        }
    }

    private void OnDisable()
    {
        if (meteorSpawner) meteorSpawner.StopSpawning();
        if (enemies) enemies.StopSpawning();
    }
}
