using UnityEngine;

/// <summary>
/// DevMode — in-game developer cheat toggles for testing without grinding.
///
/// Add this MonoBehaviour to the GameManager GameObject in Game.unity.
///
/// Keyboard shortcuts (active during Play mode):
///   F1 = Toggle God Mode     — player takes no damage
///   F2 = Toggle ∞ Currency   — Stardust/Metal never deducted on spend
///   F3 = Toggle All Skills   — every skill treated as purchased
///   F4 = Currency dump       — adds 50,000 Stardust + 10,000 Metal instantly
/// </summary>
[DefaultExecutionOrder(-95)]
public class DevMode : MonoBehaviour
{
    // ── Static state (read by gameplay systems) ───────────────────────────────

    /// <summary>Player takes no damage.</summary>
    public static bool GodMode { get; private set; }

    /// <summary>Stardust/Metal are never deducted on spend.</summary>
    public static bool InfiniteCurrency { get; private set; }

    /// <summary>Every skill in SkillService.IsOwned() returns true.</summary>
    public static bool AllSkillsOwned { get; private set; }

    // ── Input ─────────────────────────────────────────────────────────────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) { GodMode          = !GodMode;          Log("God Mode",         GodMode); }
        if (Input.GetKeyDown(KeyCode.F2)) { InfiniteCurrency = !InfiniteCurrency; Log("∞ Currency",       InfiniteCurrency); }
        if (Input.GetKeyDown(KeyCode.F3)) { AllSkillsOwned   = !AllSkillsOwned;   Log("All Skills Owned", AllSkillsOwned); }
        if (Input.GetKeyDown(KeyCode.F4)) DumpCurrency();
    }

    // ── HUD overlay ───────────────────────────────────────────────────────────

    private GUIStyle _headerStyle;
    private GUIStyle _lineStyle;

    void OnGUI()
    {
        if (!GodMode && !InfiniteCurrency && !AllSkillsOwned) return;
        EnsureStyles();

        float x = Screen.width - 170f, y = 82f, w = 160f, lh = 17f;
        GUI.Label(new Rect(x, y, w, lh), "◆ DEV MODE ACTIVE", _headerStyle); y += lh + 2f;
        if (GodMode)          { GUI.Label(new Rect(x, y, w, lh), "F1  God Mode ON",    _lineStyle); y += lh; }
        if (InfiniteCurrency) { GUI.Label(new Rect(x, y, w, lh), "F2  ∞ Currency ON",  _lineStyle); y += lh; }
        if (AllSkillsOwned)   { GUI.Label(new Rect(x, y, w, lh), "F3  All Skills ON",  _lineStyle); y += lh; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void DumpCurrency()
    {
        if (EconomyService.Instance == null) { Debug.LogWarning("[DevMode] EconomyService not found."); return; }
        EconomyService.Instance.AddStardust(50000, "DevMode");
        EconomyService.Instance.AddMetal(10000,    "DevMode");
        Debug.Log("[DevMode] +50,000 Stardust  +10,000 Metal");
    }

    static void Log(string feature, bool on)
        => Debug.Log($"[DevMode] {feature}: {(on ? "ON" : "OFF")}");

    void EnsureStyles()
    {
        if (_headerStyle != null) return;
        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            normal    = { textColor = new Color(1f, 0.75f, 0.1f) },
        };
        _lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10, alignment = TextAnchor.UpperRight,
            normal    = { textColor = new Color(1f, 0.55f, 0.15f) },
        };
    }
}
