using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HammerCursorController : MonoBehaviour
{
    [Header("Hammer Settings")]
    public RectTransform hammerRectTransform;  // Reference to the hammer's RectTransform
    public float swingAngle = 55f;             // Angle of the hammer swing
    public float swingDuration = 0.1f;         // Duration of the hammer swing

    [Header("AOE Settings")]
    public float aoeRadius = 0.5f;
    public float spinSpeed = 30f;              // Spin speed of the AOE circle in degrees per second
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);  // Color when AOE is activated

    private bool isSwinging = false;           // Flag to prevent multiple swings
    private GameObject aoeCircle;              // GameObject for the AOE circle
    private SpriteRenderer aoeSpriteRenderer;  // SpriteRenderer for the AOE circle
    private Camera mainCamera;                 // Reference to the main camera

    void Start()
    {
        // Hide the default cursor
        Cursor.visible = false;

        // Create the AOE circle
        CreateAOECircle();

        // Get reference to the main camera
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Move the hammer and AOE circle to the mouse position
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;

        // Update the hammer's UI position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        hammerRectTransform.position = screenPos;

        // Rotate the AOE circle
        aoeCircle.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        // Check for mouse click to swing the hammer
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartCoroutine(SwingHammer());
        }
    }

    /// <summary>
    /// Creates the AOE circle GameObject with necessary components
    /// </summary>
    private void CreateAOECircle()
    {
        aoeCircle = new GameObject("AOECircle");
        aoeCircle.transform.SetParent(transform);
        aoeCircle.transform.localPosition = Vector3.zero;

        aoeSpriteRenderer = aoeCircle.AddComponent<SpriteRenderer>();
        aoeSpriteRenderer.sprite = CreateCircleSprite();
        aoeSpriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);  // Semi-transparent by default

        // Add a CircleCollider2D for precise collision detection
        CircleCollider2D collider = aoeCircle.AddComponent<CircleCollider2D>();
        collider.radius = aoeRadius;
        collider.isTrigger = true;  // Set to trigger so it doesn't affect physics

        aoeCircle.transform.localScale = Vector3.one * aoeRadius;  // Adjust scale to match radius
    }

    /// <summary>
    /// Creates a circular sprite with a fade-out effect at the edges
    /// </summary>
    /// <returns>A Sprite representing the AOE circle</returns>
    private Sprite CreateCircleSprite()
    {
        int resolution = 256;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] colors = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = x - resolution / 2f;
                float dy = y - resolution / 2f;
                float distanceFromCenter = Mathf.Sqrt(dx * dx + dy * dy);
                float normalizedDistance = distanceFromCenter / (resolution / 2f);

                if (normalizedDistance <= 1f)
                {
                    float alpha = normalizedDistance > 0.8f ? (1f - normalizedDistance) * 5f : 1f;
                    colors[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[y * resolution + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Coroutine to handle the hammer swing animation and hit detection
    /// </summary>
    private IEnumerator SwingHammer()
    {
        isSwinging = true;
        float elapsedTime = 0f;

        // Highlight the AOE circle
        aoeSpriteRenderer.color = highlightColor;

        // Swing animation
        while (elapsedTime < swingDuration)
        {
            float angle = Mathf.Lerp(0, -swingAngle, elapsedTime / swingDuration);
            hammerRectTransform.rotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Check for meteor hits
        CheckMeteorHits();

        // Swing back animation
        elapsedTime = 0f;
        while (elapsedTime < swingDuration)
        {
            float angle = Mathf.Lerp(-swingAngle, 0, elapsedTime / swingDuration);
            hammerRectTransform.rotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        hammerRectTransform.rotation = Quaternion.Euler(0, 0, 0);

        // Restore original AOE circle color
        aoeSpriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);

        isSwinging = false;
    }

    /// <summary>
    /// Checks for meteor hits within the AOE radius
    /// </summary>
    private void CheckMeteorHits()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            MeteorSplit meteorSplit = hitCollider.GetComponent<MeteorSplit>();
            if (meteorSplit != null)
            {
                meteorSplit.OnHammerHit();
            }
        }
    }

    /// <summary>
    /// Draws a wire sphere in the Scene view to visualize the AOE radius
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}