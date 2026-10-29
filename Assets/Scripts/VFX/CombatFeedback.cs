using System.Collections.Generic;
using UnityEngine;

/// <summary>Bounded scene-local damage labels, hit flashes, rings and lightning using one owned material.</summary>
public sealed class CombatFeedback : MonoBehaviour
{
    private sealed class Label { public Vector3 position; public string text; public Color color; public float expires; }
    private sealed class Stroke { public LineRenderer line; public Vector3 origin; public float radius; public Color color; public float born; public float duration; }
    private sealed class Flash { public SpriteRenderer sprite; public Color original; public float expires; }
    private static CombatFeedback instance;
    private readonly List<Label> labels = new List<Label>();
    private readonly List<Stroke> strokes = new List<Stroke>();
    private readonly List<Flash> flashes = new List<Flash>();
    private Material material;
    private GUIStyle style;
    private Camera sceneCamera;

    private static CombatFeedback Current
    {
        get
        {
            if (!instance) instance = new GameObject("CombatFeedback").AddComponent<CombatFeedback>();
            return instance;
        }
    }

    public static void Hit(GameObject target, int damage, bool critical = false)
    {
        if (!target || damage <= 0) return;
        var hub = Current;
        hub.AddLabel(target.transform.position, critical ? "CRIT " + damage : damage.ToString(), critical ? Color.yellow : Color.white);
        var sprite = target.GetComponentInChildren<SpriteRenderer>();
        if (!sprite) return;
        foreach (var flash in hub.flashes)
            if (flash.sprite == sprite) { flash.expires = Time.time + .07f; return; }
        if (hub.flashes.Count >= 24) return;
        hub.flashes.Add(new Flash { sprite = sprite, original = sprite.color, expires = Time.time + .07f });
        sprite.color = Color.white;
    }

    public static void Critical(Vector3 position, int damage) => Current.AddLabel(position, "CRIT " + damage, Color.yellow);

    public static void Ring(Vector3 position, float radius, Color color)
    {
        var hub = Current;
        var stroke = hub.AddStroke(color, .05f, .32f);
        if (stroke == null) return;
        stroke.origin = position;
        stroke.radius = radius;
        stroke.line.positionCount = 33;
    }

    public static void Arc(Vector3 from, Vector3 to, Color color, float width, float duration)
    {
        var stroke = Current.AddStroke(color, width, duration);
        if (stroke == null) return;
        stroke.line.positionCount = 8;
        for (int index = 0; index < 8; index++)
        {
            Vector3 position = Vector3.Lerp(from, to, index / 7f);
            if (index > 0 && index < 7) position += (Vector3)Random.insideUnitCircle * .18f;
            stroke.line.SetPosition(index, position);
        }
    }

    private void AddLabel(Vector3 position, string text, Color color)
    {
        if (labels.Count >= 24) labels.RemoveAt(0);
        labels.Add(new Label { position = position, text = text, color = color, expires = Time.time + .65f });
    }

    private Stroke AddStroke(Color color, float width, float duration)
    {
        if (strokes.Count >= 16) return null;
        if (!material) material = new Material(Shader.Find("Sprites/Default"));
        var line = new GameObject("CombatStroke").AddComponent<LineRenderer>();
        line.transform.SetParent(transform, false);
        line.sharedMaterial = material;
        line.startWidth = width;
        line.endWidth = width * .5f;
        line.useWorldSpace = true;
        line.sortingOrder = 100;
        var stroke = new Stroke { line = line, color = color, born = Time.time, duration = Mathf.Max(.01f, duration) };
        strokes.Add(stroke);
        return stroke;
    }

    void Update()
    {
        for (int index = labels.Count - 1; index >= 0; index--)
            if (Time.time >= labels[index].expires) labels.RemoveAt(index);
        for (int index = flashes.Count - 1; index >= 0; index--)
        {
            var flash = flashes[index];
            if (flash.sprite && Time.time < flash.expires) continue;
            if (flash.sprite) flash.sprite.color = flash.original;
            flashes.RemoveAt(index);
        }
        for (int index = strokes.Count - 1; index >= 0; index--)
        {
            var stroke = strokes[index];
            float progress = (Time.time - stroke.born) / stroke.duration;
            if (!stroke.line || progress >= 1f)
            {
                if (stroke.line) Destroy(stroke.line.gameObject);
                strokes.RemoveAt(index);
                continue;
            }
            Color color = stroke.color;
            color.a *= 1f - progress;
            stroke.line.startColor = stroke.line.endColor = color;
            if (stroke.radius <= 0f) continue;
            float radius = stroke.radius * Mathf.Lerp(.2f, 1f, progress);
            for (int point = 0; point <= 32; point++)
            {
                float angle = point * Mathf.PI / 16f;
                stroke.line.SetPosition(point, stroke.origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }
    }

    void OnGUI()
    {
        if (!sceneCamera) sceneCamera = Camera.main;
        if (!sceneCamera || labels.Count == 0) return;
        if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        Color previous = GUI.color;
        foreach (var label in labels)
        {
            float remaining = label.expires - Time.time;
            Vector3 screen = sceneCamera.WorldToScreenPoint(label.position + Vector3.up * (.65f - remaining));
            if (screen.z < 0f) continue;
            Color color = label.color;
            color.a *= Mathf.Clamp01(remaining / .65f);
            GUI.color = color;
            GUI.Label(new Rect(screen.x - 48f, Screen.height - screen.y - 12f, 96f, 24f), label.text, style);
        }
        GUI.color = previous;
    }

    void OnDestroy()
    {
        foreach (var flash in flashes) if (flash.sprite) flash.sprite.color = flash.original;
        if (material) Destroy(material);
        if (instance == this) instance = null;
    }
}
