using UnityEngine;

/// <summary>
/// Sets a custom cursor texture when the menu scene starts.
/// Attach this to any menu scene object and assign the cursor sprite.
/// </summary>
public class MenuCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (cursorTexture == null)
        {
            cursorTexture = Resources.Load<Texture2D>("UI/cursor");
        }

        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
            Cursor.visible = true;
        }
    }
}
