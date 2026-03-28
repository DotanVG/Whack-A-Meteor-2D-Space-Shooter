using UnityEngine;

/// <summary>
/// ScreenFlash — full-screen color overlay that fades to transparent.
/// Call ScreenFlash.Trigger(color, alpha, decayRate) from any feedback source
/// (e.g. ShieldController.AbsorbFeedback, player damage hit).
/// Auto-creates itself if not present in the scene.
/// </summary>
public class ScreenFlash : MonoBehaviour
{
    private static ScreenFlash _instance;

    private float _alpha  = 0f;
    private float _decay  = 3f;
    private Color _color  = Color.white;

    /// <summary>
    /// Triggers a full-screen flash.
    /// </summary>
    /// <param name="color">Flash color (alpha component ignored — use the alpha parameter).</param>
    /// <param name="alpha">Starting opacity (0–1).</param>
    /// <param name="decayRate">Alpha units per second decay. Higher = faster fade.</param>
    public static void Trigger(Color color, float alpha, float decayRate)
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("ScreenFlash");
            _instance = go.AddComponent<ScreenFlash>();
        }
        _instance._color  = color;
        _instance._alpha  = alpha;
        _instance._decay  = decayRate;
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void Update()
    {
        _alpha = Mathf.Max(0f, _alpha - _decay * Time.unscaledDeltaTime);
    }

    void OnGUI()
    {
        if (_alpha <= 0.01f) return;
        Color prev = GUI.color;
        GUI.color = new Color(_color.r, _color.g, _color.b, _alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }
}
