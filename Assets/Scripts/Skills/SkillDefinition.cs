using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The existing shop's three columns, in display order.</summary>
public enum SkillBranch { AutoShooter, Hammer, Ship }

/// <summary>Describes the mechanic unlocked or upgraded by a definition.</summary>
public enum SkillEffect { Stat, TargetPriority, SlamWave, Shield, Lightning }

/// <summary>Gameplay values supplied by the skill tree.</summary>
public enum SkillStat
{
    FireRate, Spread, ProjectileSpeed, HammerRadius, HammerScore, MoveSpeed,
    BoostDuration, ShieldCharges, Invincibility, LightningBounces,
    LightningBonusDamage, TargetPriority, SlamWave
}

/// <summary>Set stores absolute level values, avoiding cumulative upgrade rounding.</summary>
public enum SkillOperation { Set, Add, Multiply }

/// <summary>A designer-authored stat operation.</summary>
[Serializable]
public class SkillModifier
{
    public SkillStat stat;
    public SkillOperation operation;
    public float value;
}

/// <summary>One shop row. LegacyId is migration metadata only, never an array index.</summary>
[Serializable]
public class SkillLevel
{
    public int legacyId;
    public string label;
    public int costStardust;
    public int costMetal;
    public List<SkillModifier> modifiers = new List<SkillModifier>();
}

/// <summary>Immutable authored skill data; ownership is kept separately in SkillProgress.</summary>
[CreateAssetMenu(menuName = "WAM/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public SkillBranch branch;
    public int tier;
    public List<string> prerequisiteIds = new List<string>();
    public SkillEffect effect;
    public List<SkillLevel> levels = new List<SkillLevel>();

    /// <summary>The number of purchasable levels authored for this skill.</summary>
    public int MaxLevel => levels.Count;
}
