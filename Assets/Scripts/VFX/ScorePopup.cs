using UnityEngine;

/// <summary>
/// ScorePopup — displays a floating "+N" label at a world position that drifts up and fades.
/// Call ScorePopup.Spawn(worldPos, points) from any kill handler.
/// Uses legacy OnGUI so it follows the project's no-Canvas convention.
/// </summary>
public class ScorePopup : MonoBehaviour
{
    private string  _text;
    private float   _startTime;
    private float   _duration = 1f;
    private Vector3 _worldPos;
    private GUIStyle _style;

    /// <summary>Creates a floating score popup at the given world position.</summary>
    public static void Spawn(Vector2 worldPos, int points)
    {
        if (points <= 0) return;
        GameObject go = new GameObject("ScorePopup");
        ScorePopup sp = go.AddComponent<ScorePopup>();
        sp._worldPos  = worldPos;
        sp._text      = $"+{points}";
        sp._startTime = Time.time;
    }

    void OnGUI()
    {
        float t = Time.time - _startTime;
        if (t > _duration) { Destroy(gameObject); return; }

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize   = 20,
                fontStyle  = FontStyle.Bold,
                alignment  = TextAnchor.MiddleCenter
            };
        }

        // Drift upward in world space, then project to screen
        Vector3 drifted = _worldPos + Vector3.up * (t * 1.5f);
        Vector3 screen  = Camera.main.WorldToScreenPoint(drifted);
        if (screen.z < 0f) { Destroy(gameObject); return; } // behind camera
        screen.y = Screen.height - screen.y; // GUI Y is inverted

        float alpha = 1f - (t / _duration);
        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 0.2f, alpha);
        GUI.Label(new Rect(screen.x - 30f, screen.y - 12f, 60f, 24f), _text, _style);
        GUI.color = prev;
    }
}
