using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
    private Vector2 screenBounds;
    private Camera mainCamera;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start()
    {
        // Cache the main camera and calculate the initial screen bounds
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ScreenWrapper requires a main camera tagged as 'MainCamera'");
            return;
        }

        RecalculateBounds();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            RecalculateBounds();
        }

        WrapAround();
    }

    private void WrapAround()
    {
        Vector3 pos = transform.position;
        bool isWrapped = false;

        // Check horizontal wrapping
        if (pos.x > screenBounds.x)
        {
            pos.x = -screenBounds.x;
            isWrapped = true;
        }
        else if (pos.x < -screenBounds.x)
        {
            pos.x = screenBounds.x;
            isWrapped = true;
        }

        // Check vertical wrapping
        if (pos.y > screenBounds.y)
        {
            pos.y = -screenBounds.y;
            isWrapped = true;
        }
        else if (pos.y < -screenBounds.y)
        {
            pos.y = screenBounds.y;
            isWrapped = true;
        }

        // Apply new position if wrapped
        if (isWrapped)
        {
            transform.position = pos;
        }
    }

    private void RecalculateBounds()
    {
        screenBounds = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
