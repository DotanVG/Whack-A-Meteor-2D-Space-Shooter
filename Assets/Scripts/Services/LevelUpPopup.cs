using UnityEngine;

/// <summary>
/// LevelUpPopup — shows a "LEVEL UP! → Level N" overlay via legacy OnGUI.
///
/// Triggered by LevelService.OnLevelUp.
/// Displays for 3 seconds with a 1-second fade-out at the end.
/// Positioned in the lower third of the screen so it doesn't obstruct gameplay.
/// </summary>
public class LevelUpPopup : MonoBehaviour
{
    private int   _displayLevel;
    private float _timer;
    private bool  _showing;
    private GUIStyle _style;

    private const float DisplayDuration = 3f;
    private const float FadeStartAt     = 2f; // begin fade at this many seconds in

    void OnEnable()  { LevelService.OnLevelUp += ShowPopup; }
    void OnDisable() { LevelService.OnLevelUp -= ShowPopup; }

    void ShowPopup(int newLevel)
    {
        _displayLevel = newLevel;
        _timer        = 0f;
        _showing      = true;
    }

    void Update()
    {
        if (!_showing) return;
        _timer += Time.unscaledDeltaTime; // advance even while paused
        if (_timer >= DisplayDuration) _showing = false;
    }

    void OnGUI()
    {
        if (!_showing) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 38,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        // Fade out over the last second
        float alpha = 1f;
        if (_timer > FadeStartAt)
            alpha = 1f - (_timer - FadeStartAt) / (DisplayDuration - FadeStartAt);

        Color prev = GUI.color;

        // Shadow pass (dark, slightly offset)
        GUI.color = new Color(0f, 0f, 0f, alpha * 0.6f);
        GUI.Label(PopupRect(2f), $"LEVEL UP!   Level {_displayLevel}", _style);

        // Main golden text
        GUI.color = new Color(1f, 0.88f, 0.15f, alpha);
        GUI.Label(PopupRect(0f), $"LEVEL UP!   Level {_displayLevel}", _style);

        GUI.color = prev;
    }

    static Rect PopupRect(float yOffset)
    {
        float w = 500f, h = 60f;
        return new Rect(
            Screen.width  / 2f - w / 2f,
            Screen.height * 0.67f + yOffset,
            w, h);
    }
}
