/// <summary>
/// GameFeatureFlags — static toggles that gate partially-implemented systems.
/// Flip a flag to true when the corresponding phase is complete and tested.
///
/// These are compile-time constants for now. Migrate to ScriptableObject if
/// you need runtime switching (e.g. QA debug menu).
/// </summary>
public static class GameFeatureFlags
{
    // Phase 1: balance table drives gameplay numbers from CSV
    public const bool UseCSVBalance = false;

    // Phase 2: dual-currency economy (Stardust / Scrap)
    public const bool UseEconomy = false;

    // Phase 3: persistent store + meta-progression
    public const bool UseStore = false;

    // Phase 4: skill tree
    public const bool UseSkillTree = false;

    // Phase 5: enemy factions + SpawnDirector
    public const bool UseSpawnDirector = false;

    // Phase 6: power-up collectible system
    public const bool UsePowerups = false;
}
