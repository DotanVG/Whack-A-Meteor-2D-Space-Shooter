using UnityEngine;

/// <summary>
/// EconomyHUD — draws the dual-currency wallet (Stardust / Metal)
/// at the top-left corner using legacy OnGUI.
///
/// Stardust : gold text  — earned from meteor kills
/// Metal    : silver text — earned from enemy ship kills (future)
///
/// Auto-spawned as a child of GameServices at startup.
/// Invisible when GameFeatureFlags.UseEconomy is false.
/// </summary>
public class EconomyHUD : MonoBehaviour
{
    private int _stardust;
    private int _metal;
    private GUIStyle _labelStyle;

    void OnEnable()
    {
        EconomyService.OnStardustChanged += HandleStardustChanged;
        EconomyService.OnMetalChanged    += HandleMetalChanged;
    }

    void OnDisable()
    {
        EconomyService.OnStardustChanged -= HandleStardustChanged;
        EconomyService.OnMetalChanged    -= HandleMetalChanged;
    }

    void HandleStardustChanged(int val) => _stardust = val;
    void HandleMetalChanged(int val)    => _metal    = val;

    void OnGUI()
    {
        if (!GameFeatureFlags.UseEconomy) return;

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
            };
        }

        // Stardust — gold
        _labelStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(12f, 10f, 200f, 28f), $"Stardust: {_stardust}", _labelStyle);

        // Metal — silver
        _labelStyle.normal.textColor = new Color(0.75f, 0.78f, 0.82f);
        GUI.Label(new Rect(12f, 36f, 200f, 28f), $"Metal:    {_metal}", _labelStyle);
    }
}
