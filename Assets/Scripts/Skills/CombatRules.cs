using System;
using System.Collections.Generic;

/// <summary>Deterministic combat decisions independent of scene, save and random state.</summary>
public static class CombatRules
{
    public static int Damage(int baseDamage, float criticalChance, float roll)
    {
        if (baseDamage <= 0) return 0;
        bool critical = roll >= 0f && roll < Math.Max(0f, Math.Min(1f, criticalChance));
        return critical ? (int)Math.Min(int.MaxValue, (long)baseDamage * 2) : baseDamage;
    }
}

/// <summary>A projectile spends its bounded hit budget only on a new target.</summary>
public sealed class ProjectileHits
{
    private readonly HashSet<int> targets = new HashSet<int>();
    private readonly int capacity;
    public ProjectileHits(int extraTargets) { capacity = 1 + Math.Max(0, Math.Min(8, extraTargets)); }
    public bool Exhausted => targets.Count >= capacity;
    public bool TryHit(int targetId) => !Exhausted && targets.Add(targetId);
}
