/// <summary>
/// EnemyFaction — pure data component that identifies which faction an enemy belongs to.
/// Attach to each enemy prefab and set the faction in the Inspector.
/// Read by EnemyHealth and EnemyController to look up faction-specific stats from BalanceService.
/// </summary>
public enum Faction { Black, Blue, Green, Red }

public class EnemyFaction : UnityEngine.MonoBehaviour
{
    public Faction faction = Faction.Blue;
}
