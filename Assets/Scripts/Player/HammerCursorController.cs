using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the hammer cursor, AOE circle, and radar sweep effect in the Whack-A-Meteor game.
/// </summary>
public class HammerCursorController : MonoBehaviour
{
    [Header("Hammer Settings")]
    public RectTransform hammerRectTransform;  // Reference to the hammer's RectTransform
    public float swingAngle = 55f;             // Angle of the hammer swing
    public float swingDuration = 0.1f;         // Duration of the hammer swing

    [Header("AOE Settings")]
    public float aoeRadius = 0.5f;             // Radius of the AOE circle
    public float spinSpeed = 30f;              // Spin speed of the AOE circle in degrees per second
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);  // Color when AOE is activated

    [Header("Radar Sweep Settings")]
    public Color radarDotColor = new Color(0.5f, 0.8f, 1f, 0.8f); // Color of the radar dot (blueish)
    public float radarDotSize = 0.05f;         // Size of the radar dot
    public float radarRotationSpeed = 180f;    // Rotation speed of the radar dot in degrees per second
    public int trailResolution = 60;           // Number of points in the trail (higher for smoother circle)
    public float trailWidth = 0.02f;           // Width of the trail

    private bool isSwinging = false;           // Flag to prevent multiple swings
    private GameObject aoeCircle;              // GameObject for the AOE circle
    private SpriteRenderer aoeSpriteRenderer;  // SpriteRenderer for the AOE circle
    private Camera mainCamera;                 // Reference to the main camera
    private GameObject radarDot;               // GameObject for the radar dot
    private LineRenderer circularTrail;        // LineRenderer for the circular trail
    private float currentAngle = 0f;           // Current angle of the radar dot

    /// <summary>
    /// Initializes the hammer cursor, AOE circle, and radar dot.
    /// </summary>
    void Start()
    {
        // Hide the default cursor
        Cursor.visible = false;

        // Create the AOE circle and radar dot
        CreateAOECircle();
        CreateRadarDot();
        CreateCircularTrail();

        // Get reference to the main camera
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Updates the position of the hammer and AOE circle, rotates the radar dot, and checks for hammer swings.
    /// </summary>
    void Update()
    {
        UpdatePosition();
        RotateRadarDot();
        UpdateCircularTrail();

        // Check for mouse click to swing the hammer
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartCoroutine(SwingHammer());
        }
    }

    /// <summary>
    /// Updates the position of the hammer cursor and AOE circle based on mouse position.
    /// </summary>
    private void UpdatePosition()
    {
        // Convert mouse position to world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;

        // Update the hammer's UI position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        hammerRectTransform.position = screenPos;
    }

    /// <summary>
    /// Creates the AOE circle GameObject with necessary components.
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
    /// Creates a circular sprite with a fade-out effect at the edges.
    /// </summary>
    /// <returns>A Sprite representing the AOE circle or radar dot</returns>
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
    /// Creates the radar dot GameObject with necessary components.
    /// </summary>
    private void CreateRadarDot()
    {
        radarDot = new GameObject("RadarDot");
        radarDot.transform.SetParent(transform);
        radarDot.transform.localPosition = new Vector3(aoeRadius, 0, 0);

        SpriteRenderer dotRenderer = radarDot.AddComponent<SpriteRenderer>();
        dotRenderer.sprite = CreateCircleSprite();
        dotRenderer.color = radarDotColor;
        radarDot.transform.localScale = Vector3.one * radarDotSize;
    }

    /// <summary>
    /// Creates the circular trail using a LineRenderer.
    /// </summary>
    private void CreateCircularTrail()
    {
        GameObject trailObject = new GameObject("CircularTrail");
        trailObject.transform.SetParent(transform);
        trailObject.transform.localPosition = Vector3.zero;

        circularTrail = trailObject.AddComponent<LineRenderer>();
        circularTrail.positionCount = trailResolution + 1;
        circularTrail.useWorldSpace = false;
        circularTrail.startWidth = trailWidth;
        circularTrail.endWidth = trailWidth;
        circularTrail.startColor = radarDotColor;
        circularTrail.endColor = new Color(radarDotColor.r, radarDotColor.g, radarDotColor.b, 0);
        circularTrail.material = new Material(Shader.Find("Sprites/Default"));

        UpdateCircularTrail();
    }

    /// <summary>
    /// Rotates the radar dot around the AOE circle.
    /// </summary>
    private void RotateRadarDot()
    {
        currentAngle += radarRotationSpeed * Time.deltaTime;
        currentAngle %= 360f;
        float radians = currentAngle * Mathf.Deg2Rad;
        radarDot.transform.localPosition = new Vector3(Mathf.Cos(radians) * aoeRadius, Mathf.Sin(radians) * aoeRadius, 0);
    }

    /// <summary>
    /// Updates the circular trail to maintain a full 360-degree outline.
    /// </summary>
    private void UpdateCircularTrail()
    {
        for (int i = 0; i <= trailResolution; i++)
        {
            float angle = (i / (float)trailResolution) * 360f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * aoeRadius, Mathf.Sin(angle) * aoeRadius, 0);
            circularTrail.SetPosition(i, pos);

            // Calculate color based on distance from the radar dot
            float distanceFromDot = Mathf.Abs(angle - (currentAngle * Mathf.Deg2Rad));
            distanceFromDot = Mathf.Min(distanceFromDot, 2 * Mathf.PI - distanceFromDot);
            float normalizedDistance = distanceFromDot / Mathf.PI;
            Color pointColor = Color.Lerp(radarDotColor, new Color(radarDotColor.r, radarDotColor.g, radarDotColor.b, 0), normalizedDistance);
            circularTrail.SetColors(pointColor, pointColor);
        }
    }

    /// <summary>
    /// Coroutine to handle the hammer swing animation and hit detection.
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
    /// Checks for meteor hits within the AOE radius.
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
    /// Draws a wire sphere in the Scene view to visualize the AOE radius.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}