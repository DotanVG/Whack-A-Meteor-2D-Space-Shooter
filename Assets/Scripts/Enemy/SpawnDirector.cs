using UnityEngine;

/// <summary>Starts the authored enemy mix; milestone bosses replace normal enemy spawns.</summary>
[DefaultExecutionOrder(-50)]
public class SpawnDirector : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    /// <summary>WaveManager configures the spawner before announcing a wave.</summary>
    public void OnWaveStarted(int wave)
    {
        if (!GameFeatureFlags.UseSpawnDirector || !enemySpawner) return;
        enemySpawner.StopSpawning();
        var encounter = enemySpawner.Encounter;
        if (!encounter.enemiesEnabled) return;
        if (encounter.elite)
        {
            enemySpawner.SpawnBoss();
            return;
        }
        enemySpawner.SetActiveFactions(wave >= 8, true, true, true);
        enemySpawner.StartSpawning();
    }
}
