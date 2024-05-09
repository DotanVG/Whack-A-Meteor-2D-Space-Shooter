using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
    private Vector2 screenBounds;

    void Start()
    {
        // Calculate screen bounds using the camera's viewport
        Camera mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.LogError("ScreenWrapper requires a main camera tagged as 'MainCamera'");
            return;
        }

        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
    }

    void Update()
    {
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
}
