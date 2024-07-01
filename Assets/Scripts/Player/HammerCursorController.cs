using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HammerCursorController : MonoBehaviour
{
    public RectTransform hammerRectTransform; // The RectTransform of the hammer image
    public float swingAngle = 55f;
    public float swingDuration = 0.1f;

    private bool isSwinging = false;

    void Start()
    {
        // Hide the default cursor
        Cursor.visible = false;
    }

    void Update()
    {
        // Move the hammer image to the mouse position
        Vector3 mousePosition = Input.mousePosition;
        hammerRectTransform.position = mousePosition;

        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartCoroutine(SwingHammer());
        }
    }

    private IEnumerator SwingHammer()
    {
        isSwinging = true;
        float elapsedTime = 0f;

        // Swing to the target angle
        while (elapsedTime < swingDuration)
        {
            float angle = Mathf.Lerp(0, -swingAngle, elapsedTime / swingDuration);
            hammerRectTransform.rotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Check for meteor hit at the peak of the swing
        CheckMeteorHit();

        // Swing back to the original angle
        elapsedTime = 0f;
        while (elapsedTime < swingDuration)
        {
            float angle = Mathf.Lerp(-swingAngle, 0, elapsedTime / swingDuration);
            hammerRectTransform.rotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        hammerRectTransform.rotation = Quaternion.Euler(0, 0, 0);
        isSwinging = false;
    }

    private void CheckMeteorHit()
    {
        // Perform a raycast from the hammer's position
        Ray ray = Camera.main.ScreenPointToRay(hammerRectTransform.position);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            MeteorSplit meteorSplit = hit.collider.GetComponent<MeteorSplit>();
            if (meteorSplit != null)
            {
                meteorSplit.OnHammerHit();
            }
        }
    }
}