using UnityEngine;

/// <summary>
/// EnemyActiveTracker — attach to every enemy prefab alongside EnemyController.
/// Increments/decrements EnemySpawner.ActiveCount so the spawner can enforce its cap.
/// </summary>
public class EnemyActiveTracker : MonoBehaviour
{
    void OnEnable()  => EnemySpawner.RegisterSpawn();
    void OnDisable() => EnemySpawner.RegisterDespawn();
}
