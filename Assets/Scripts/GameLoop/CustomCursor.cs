using UnityEngine;

/// <summary>
/// CustomCursor — draws a custom crosshair texture at the center of the screen.
/// Works by drawing a GUI texture in OnGUI, always centered regardless of resolution.
///
/// SETUP:
///   - Attach to any persistent GameObject (e.g. the Player or a UI Canvas GO).
///   - Assign a crosshair Texture2D to cursorTexture in the Inspector.
///   - The OS cursor is hidden on Start — the texture acts as the visual replacement.
///
/// NOTE: OnGUI is legacy UI and runs every frame. Fine for a single texture,
///   but if you migrate to Unity UI (Canvas), replace this with a UI Image element
///   anchored to the screen center instead.
/// </summary>
public class CustomCursor : MonoBehaviour
{
    [Tooltip("Texture to draw as the crosshair. Recommended: 32x32 or 64x64 PNG with transparency.")]
    public Texture2D cursorTexture;

    [Tooltip("Width of the drawn crosshair in pixels.")]
    public float cursorWidth = 32f;

    [Tooltip("Height of the drawn crosshair in pixels.")]
    public float cursorHeight = 32f;

    private void Start()
    {
        // Hide the system cursor — our texture replaces it visually
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Draws the crosshair texture at the exact screen center each frame.
    /// Recomputes position every frame to handle resolution changes gracefully.
    /// </summary>
    private void OnGUI()
    {
        if (cursorTexture == null) return;

        // Center the rect: offset by half the texture size so the middle lands on screen center
        float x = (Screen.width - cursorWidth) / 2;
        float y = (Screen.height - cursorHeight) / 2;
        GUI.DrawTexture(new Rect(x, y, cursorWidth, cursorHeight), cursorTexture);
    }
}
