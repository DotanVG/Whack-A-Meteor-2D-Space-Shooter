using UnityEngine;

/// <summary>
/// SpawnDirector — decides which enemy factions are active each wave and triggers boss waves.
/// Call OnWaveStarted(wave) from WaveManager.RunWave() at the top of each wave coroutine.
///
/// Faction ramp:
///   Wave  1–3  : no enemies (meteors only)
///   Wave  4+   : Blue + Green
///   Wave  7+   : + Red (shoots back)
///   Wave 10+   : + Black (tanky)
///   Wave 5, 10, 15, … : boss wave (regular enemies replaced by single Boss)
/// </summary>
[DefaultExecutionOrder(-50)]
public class SpawnDirector : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    /// <summary>Called by WaveManager at the start of each wave.</summary>
    public void OnWaveStarted(int wave)
    {
        if (!GameFeatureFlags.UseSpawnDirector) return;
        if (enemySpawner == null) return;

        int introWave = BalanceService.Instance?.GetInt("spawn.enemy_intro_wave", 4) ?? 4;
        if (wave < introWave) return;

        // Boss wave — skip regular spawning, spawn one boss
        if (wave % 5 == 0)
        {
            enemySpawner.StopSpawning();
            enemySpawner.SpawnBoss();
            GameLogger.SpawnDirectorWave(wave, 0, 0, 0, 0);
            return;
        }

        bool black = wave >= 10;
        bool blue  = true;
        bool green = wave >= 4;
        bool red   = wave >= 7;

        enemySpawner.SetActiveFactions(black, blue, green, red);
        enemySpawner.StartSpawning();

        GameLogger.SpawnDirectorWave(wave,
            black ? 1 : 0,
            blue  ? 1 : 0,
            green ? 1 : 0,
            red   ? 1 : 0);
    }
}
