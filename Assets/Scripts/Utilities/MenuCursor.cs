using UnityEngine;

/// <summary>
/// Sets a custom cursor texture when the menu scene starts.
/// Automatically loads cursor from Sprites/UI/cursor.png if not assigned.
/// </summary>
public class MenuCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        // If cursor texture not assigned, try to load from Resources
        if (cursorTexture == null)
        {
            cursorTexture = Resources.Load<Texture2D>("cursor");
            if (cursorTexture == null)
            {
                // Try with full path
                cursorTexture = Resources.Load<Texture2D>("Sprites/UI/cursor");
            }
        }

        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
            Cursor.visible = true;
        }
    }
}
