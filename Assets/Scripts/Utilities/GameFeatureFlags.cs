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
    public const bool UseCSVBalance = true;

    // Phase 2: dual-currency economy (Stardust / Scrap)
    public const bool UseEconomy = true;

    // Phase 3: persistent store + meta-progression
    public const bool UseStore = true;

    // Phase 3: skill tree (functional purchase + gameplay effects)
    public const bool UseSkillTree = true;

    // Phase 5: enemy factions + SpawnDirector
    public const bool UseSpawnDirector = true;

    // Phase 6: power-up collectible system
    public const bool UsePowerups = true;
}
