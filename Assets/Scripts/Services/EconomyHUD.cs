using UnityEngine;

/// <summary>
/// EconomyHUD — draws the dual-currency wallet (Stardust / ScrapMetal)
/// using legacy OnGUI, consistent with the rest of the game's HUD style.
///
/// Automatically spawned as a child of GameServices at startup.
/// Invisible when GameFeatureFlags.UseEconomy is false.
///
/// Position: top-right corner, below any existing score readout.
/// </summary>
public class EconomyHUD : MonoBehaviour
{
    private int _stardust;
    private int _scrap;
    private GUIStyle _labelStyle;

    void OnEnable()
    {
        EconomyService.OnStardustChanged += HandleStardustChanged;
        EconomyService.OnScrapChanged    += HandleScrapChanged;
    }

    void OnDisable()
    {
        EconomyService.OnStardustChanged -= HandleStardustChanged;
        EconomyService.OnScrapChanged    -= HandleScrapChanged;
    }

    void HandleStardustChanged(int val) => _stardust = val;
    void HandleScrapChanged(int val)    => _scrap    = val;

    void OnGUI()
    {
        if (!GameFeatureFlags.UseEconomy) return;

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                normal    = { textColor = new Color(1f, 0.92f, 0.3f) } // gold tint for Stardust
            };
        }

        float w = 240f;
        float x = Screen.width - w - 12f;

        // Stardust row (gold)
        _labelStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(x, 10f, w, 30f), $"Stardust:  {_stardust}", _labelStyle);

        // ScrapMetal row (silver-grey)
        _labelStyle.normal.textColor = new Color(0.75f, 0.78f, 0.82f);
        GUI.Label(new Rect(x, 38f, w, 30f), $"Scrap:     {_scrap}",    _labelStyle);
    }
}
