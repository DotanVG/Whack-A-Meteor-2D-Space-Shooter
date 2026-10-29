using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Authored encounter palette and absolute-wave difficulty curve.</summary>
[CreateAssetMenu(menuName = "WAM/Encounter Catalogue")]
public sealed class EncounterCatalog : ScriptableObject
{
    public List<LevelDefinition> levels = new List<LevelDefinition>();
    public List<MeteorProfile> meteors = new List<MeteorProfile>();
    public List<EnemyProfile> enemies = new List<EnemyProfile>();
    public int wavesPerLevel = 5;
    public int enemyIntroWave = 2;
    public float baseMeteorInterval = 1.5f;
    public float minimumMeteorInterval = .45f;
    public float baseEnemyInterval = 4f;
    public float minimumEnemyInterval = 1.5f;
    public float intervalRamp = .08f;
    public float speedRamp = .06f;
    public float maximumSpeedMultiplier = 2.2f;
    public int healthStepWaves = 6;
    public int maximumHealthBonus = 4;
    public int baseStardustReward = 40;
    public int stardustRewardPerWave = 12;
    public int baseMetalReward = 6;
    public int metalRewardPerWave = 2;
    public int maximumRewardWave = 100;
    public int baseEliteHealth = 20;
    public int eliteHealthPerMilestone = 5;
    public int eliteMetalReward = 30;
    public float eliteSpeed = 1f;
    public float eliteScale = 2.5f;
    public int meteorCap = 60;
    public int enemyCap = 10;

    /// <summary>Computes bounded tuning directly from wave number; never compounds mutable state.</summary>
    public WaveDefinition Evaluate(int wave)
    {
        int number = Math.Max(1, wave);
        int step = number - 1;
        int bounded = Math.Min(number, maximumRewardWave);
        return new WaveDefinition {
            number = number,
            level = levels[(step / wavesPerLevel) % levels.Count],
            meteorInterval = Math.Max(minimumMeteorInterval, baseMeteorInterval / (1f + intervalRamp * step)),
            enemyInterval = Math.Max(minimumEnemyInterval, baseEnemyInterval / (1f + intervalRamp * step)),
            speedMultiplier = Math.Min(maximumSpeedMultiplier, 1f + speedRamp * step),
            healthBonus = Math.Min(maximumHealthBonus, step / healthStepWaves),
            stardustReward = baseStardustReward + stardustRewardPerWave * bounded,
            metalReward = baseMetalReward + metalRewardPerWave * bounded,
            elite = number % wavesPerLevel == 0,
            eliteHealth = baseEliteHealth + eliteHealthPerMilestone * Math.Min(number / wavesPerLevel, maximumRewardWave),
            enemiesEnabled = number >= enemyIntroWave
        };
    }

    /// <summary>Checks committed authoring data before gameplay or tests consume it.</summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (levels.Count < 3 || meteors.Count < 3 || enemies.Count < 2) errors.Add("Missing encounter variety");
        if (wavesPerLevel < 1 || healthStepWaves < 1 || enemyIntroWave < 1 || maximumRewardWave < 1)
            errors.Add("Invalid progression steps");
        if (!Positive(baseMeteorInterval) || !Positive(minimumMeteorInterval) || minimumMeteorInterval > baseMeteorInterval ||
            !Positive(baseEnemyInterval) || !Positive(minimumEnemyInterval) || minimumEnemyInterval > baseEnemyInterval ||
            !NonNegative(intervalRamp) || !NonNegative(speedRamp) || !Positive(maximumSpeedMultiplier) || maximumSpeedMultiplier < 1f)
            errors.Add("Invalid difficulty curve");
        if (maximumHealthBonus < 0 || baseStardustReward < 0 || stardustRewardPerWave < 0 || baseMetalReward < 0 ||
            metalRewardPerWave < 0 || baseEliteHealth < 1 || eliteHealthPerMilestone < 0 || eliteMetalReward < 0 ||
            !Positive(eliteSpeed) || !Positive(eliteScale) || meteorCap < 1 || enemyCap < 1)
            errors.Add("Invalid reward/health/cap");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var level in levels)
        {
            if (!level) { errors.Add("Missing level"); continue; }
            if (string.IsNullOrEmpty(level.id) || !ids.Add(level.id)) errors.Add("Duplicate/empty level id");
            if (!Positive(level.activeSeconds) || !Positive(level.regroupSeconds) || level.meteorWeights.Count != meteors.Count)
                errors.Add("Invalid level timing/weights");
            if (!NonNegative(level.enemyShooterWeight) || level.enemyShooterWeight > 1f) errors.Add("Invalid enemy mix");
            int sum = 0;
            foreach (int weight in level.meteorWeights) { if (weight < 0) errors.Add("Negative meteor weight"); sum += weight; }
            if (sum <= 0) errors.Add("Empty meteor mix");
        }
        ids.Clear();
        foreach (var meteor in meteors)
            if (!meteor || string.IsNullOrEmpty(meteor.id) || !ids.Add(meteor.id) || meteor.hitPoints < 1 ||
                !Positive(meteor.speed) || meteor.metalReward < 0 || meteor.prefabSize < 0 || meteor.prefabSize > 3)
                errors.Add("Invalid meteor profile");
        ids.Clear();
        var behaviours = new HashSet<EnemyBehaviour>();
        foreach (var enemy in enemies)
        {
            if (!enemy || string.IsNullOrEmpty(enemy.id) || !ids.Add(enemy.id) || enemy.hitPoints < 1 ||
                !Positive(enemy.speed) || !Positive(enemy.fireInterval) || !NonNegative(enemy.turnDegreesPerSecond) || !Positive(enemy.pursuitSeconds) ||
                !Enum.IsDefined(typeof(EnemyBehaviour), enemy.behaviour)) errors.Add("Invalid enemy profile");
            else behaviours.Add(enemy.behaviour);
        }
        if (behaviours.Count < 2) errors.Add("Missing enemy behaviours");
        return errors;
    }

    private static bool Positive(float value) => NonNegative(value) && value > 0f;
    private static bool NonNegative(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
}

/// <summary>One evaluated encounter, kept separate from its immutable assets.</summary>
public struct WaveDefinition
{
    public int number;
    public LevelDefinition level;
    public float meteorInterval, enemyInterval, speedMultiplier;
    public int healthBonus, stardustReward, metalReward, eliteHealth;
    public bool elite, enemiesEnabled;
}
