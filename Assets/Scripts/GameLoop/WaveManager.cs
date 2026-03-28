using System.Collections;
using UnityEngine;

/// <summary>
/// WaveManager — orchestrates the game's difficulty progression.
/// Controls MeteorSpawnerGL's spawnRate and meteorSpeed as waves advance.
///
/// SETUP:
///   1. Attach to the same "GameManager" GameObject as MeteorSpawnerGL.
///   2. Assign the MeteorSpawnerGL reference in the Inspector.
///   3. Wave advances automatically after a time limit (enemy-count-based clear is a future task).
///
/// WAVE LOGIC:
///   - Each wave: faster spawn rate, faster meteors, more max meteors
///   - Difficulty curve is linear for now — swap with AnimationCurve later
///
/// PLANNED UPGRADES:
///   - Enemy-count-based wave clear (not time-based)
///   - Boss wave every 5 waves (spawn a single large meteor with high HP)
///   - Visual "Wave X" announcement UI between waves
///   - Persistent wave number across scene reloads via PlayerPrefs
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("References")]
    public MeteorSpawnerGL meteorSpawner;     // Assign in Inspector

    [Header("Wave Config")]
    public int currentWave = 1;
    public float timeBetweenWaves = 5f;     // Pause between waves (seconds)

    [Header("Difficulty Scaling (per wave)")]
    public float spawnRateDecrement = 0.15f;    // Reduce spawn interval by this each wave (faster spawning)
    public float speedIncrement = 0.5f;         // Increase meteor speed by this each wave
    public float minSpawnRate = 0.3f;           // Floor — don't go below this (prevents frame-rate chaos)
    public int maxMeteorIncrement = 2;          // Add 2 more max meteors per wave

    private bool _waveActive = false;

    private void Start()
    {
        StartCoroutine(RunWave(currentWave));
    }

    IEnumerator RunWave(int wave)
    {
        _waveActive = true;
        Debug.Log($"[WaveManager] Wave {wave} started");

        UIManager.Instance?.UpdateWave(wave);

        // Apply difficulty scaling to spawner
        if (meteorSpawner != null)
        {
            meteorSpawner.spawnRate = Mathf.Max(minSpawnRate, meteorSpawner.spawnRate - spawnRateDecrement * (wave - 1));
            meteorSpawner.meteorSpeed += speedIncrement * (wave - 1);
            meteorSpawner.maxMeteors += maxMeteorIncrement * (wave - 1);
        }

        // Wait until all enemies are cleared
        // TODO: replace with a proper wave-enemy counter once spawn tracking is added
        yield return new WaitForSeconds(timeBetweenWaves + (wave * 3f));

        _waveActive = false;
        currentWave++;
        StartCoroutine(RunWave(currentWave));
    }
}
