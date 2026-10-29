using UnityEngine;

/// <summary>Enemy motion/attack choices independent of faction cosmetics.</summary>
public enum EnemyBehaviour { AimedShooter, PursuitDiver }

/// <summary>Stats applied before a newly spawned enemy can receive damage.</summary>
[CreateAssetMenu(menuName = "WAM/Enemy Profile")]
public sealed class EnemyProfile : ScriptableObject
{
    public string id;
    public EnemyBehaviour behaviour;
    public int hitPoints = 3;
    public float speed = 2.5f;
    public float fireInterval = 2.5f;
    public float turnDegreesPerSecond = 50f;
    public float pursuitSeconds = 2.5f;
    public Color tint = Color.white;
}
