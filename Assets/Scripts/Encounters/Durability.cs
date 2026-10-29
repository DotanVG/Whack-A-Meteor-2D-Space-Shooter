using System;

/// <summary>Pure damage state: non-positive hits and repeated deaths cannot award a second kill.</summary>
public sealed class Durability
{
    public int Current { get; private set; }
    public int Maximum { get; }
    public bool IsDead => Current == 0;
    public Durability(int maximum) { Maximum = Math.Max(1, maximum); Current = Maximum; }

    /// <summary>Returns true exactly once, on the hit that crosses zero.</summary>
    public bool Apply(int damage)
    {
        if (damage <= 0 || IsDead) return false;
        Current = Math.Max(0, Current - damage);
        return IsDead;
    }
}
