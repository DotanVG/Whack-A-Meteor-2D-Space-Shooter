using UnityEngine;

/// <summary>Spawn-only variant stats. Children retain smaller prefab durability and momentum.</summary>
[CreateAssetMenu(menuName = "WAM/Meteor Profile")]
public sealed class MeteorProfile : ScriptableObject
{
    public string id;
    public int hitPoints = 1;
    public float speed = 4f;
    public int metalReward;
    [Tooltip("0 big, 1 medium, 2 small, 3 tiny")]
    public int prefabSize;
    public bool grey;
    public Color tint = Color.white;
}
