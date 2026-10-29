using System.Collections.Generic;
using UnityEngine;

/// <summary>A sector theme and its meteor composition; five waves form a level.</summary>
[CreateAssetMenu(menuName = "WAM/Level Definition")]
public sealed class LevelDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public float activeSeconds = 25f;
    public float regroupSeconds = 3f;
    public List<int> meteorWeights = new List<int>();
    [Range(0f, 1f)] public float enemyShooterWeight = .5f;
    public Color accent = Color.cyan;
}
