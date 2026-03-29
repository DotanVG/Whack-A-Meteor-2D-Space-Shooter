using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LightningChain — triggered by the hammer on hit (if Lightning skill is owned).
/// Jumps arc-lightning from the initial hit position to the N nearest targets
/// (enemies and meteors), damaging each one.
///
/// Visual: a brief white LineRenderer arc between each hop that fades out.
/// Damage: each hop deals 1 HP to enemies; meteors call OnHammerHit() as normal.
///
/// SKILL TREE:
///   Lightning Lv1 (id=15): 2 bounces
///   Lightning Lv2 (id=16): 4 bounces
///   Lightning Lv3 (id=17): 6 bounces + 2 bonus damage per hop
/// </summary>
public class LightningChain : MonoBehaviour
{
    public static LightningChain Instance { get; private set; }

    [Header("Visual")]
    public Color lightningColor = new Color(0.6f, 0.8f, 1f);
    public float arcWidth      = 0.06f;
    public float arcDuration   = 0.12f; // seconds each arc flash lasts

    [Header("Search")]
    public float searchRadius  = 6f;    // max distance each hop can jump

    private static readonly string[] TargetTags = {
        "Enemy",
        "BigBrownMeteor",    "BigGreyMeteor",
        "MediumBrownMeteor", "MediumGreyMeteor",
        "SmallBrownMeteor",  "SmallGreyMeteor",
        "TinyBrownMeteor",   "TinyGreyMeteor",
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// Trigger the chain from a world position.
    /// bounces = total extra targets to hit (already-hit set prevents revisiting).
    /// Called by HammerCursorController after primary AOE hits.
    /// </summary>
    public void Trigger(Vector3 origin, int bounces, int bonusDamage = 0)
    {
        if (bounces <= 0) return;
        StartCoroutine(ChainCoroutine(origin, bounces, bonusDamage));
    }

    IEnumerator ChainCoroutine(Vector3 origin, int bounces, int bonusDamage)
    {
        var visited = new HashSet<GameObject>();
        Vector3 current = origin;

        for (int hop = 0; hop < bounces; hop++)
        {
            GameObject next = FindNearest(current, visited);
            if (next == null) yield break;

            visited.Add(next);

            // Draw arc
            StartCoroutine(DrawArc(current, next.transform.position));

            // Deal damage
            MeteorSplit ms = next.GetComponent<MeteorSplit>();
            if (ms != null)
            {
                ms.OnHammerHit();
            }
            else
            {
                EnemyHealth eh = next.GetComponent<EnemyHealth>();
                if (eh != null)
                    eh.TakeDamage(1 + bonusDamage);
            }

            current = next.transform.position;
            yield return new WaitForSeconds(0.05f); // stagger hops visually
        }
    }

    GameObject FindNearest(Vector3 from, HashSet<GameObject> exclude)
    {
        GameObject best  = null;
        float      bestD = searchRadius * searchRadius;

        foreach (string tag in TargetTags)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (exclude.Contains(obj)) continue;
                float d = (obj.transform.position - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = obj; }
            }
        }
        return best;
    }

    IEnumerator DrawArc(Vector3 from, Vector3 to)
    {
        GameObject arco = new GameObject("LightningArc");
        LineRenderer lr = arco.AddComponent<LineRenderer>();
        lr.positionCount = 8;
        lr.startWidth    = arcWidth;
        lr.endWidth      = arcWidth * 0.3f;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = lightningColor;
        lr.endColor      = Color.white;
        lr.useWorldSpace = true;

        // Jittered path
        for (int i = 0; i < 8; i++)
        {
            float t    = i / 7f;
            Vector3 p  = Vector3.Lerp(from, to, t);
            if (i > 0 && i < 7) p += (Vector3)Random.insideUnitCircle * 0.25f;
            lr.SetPosition(i, p);
        }

        float elapsed = 0f;
        while (elapsed < arcDuration)
        {
            float alpha = 1f - elapsed / arcDuration;
            Color c     = lightningColor;
            c.a = alpha;
            lr.startColor = c;
            lr.endColor   = new Color(1f, 1f, 1f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(arco);
    }
}
