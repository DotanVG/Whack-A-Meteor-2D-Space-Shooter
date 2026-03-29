using UnityEngine;

/// <summary>
/// PlayerDamageVisual — overlays a damage sprite on the player ship based on remaining lives.
///
/// Assign the 3 damage overlay sprites for the current ship skin in the Inspector.
/// Call Refresh(lives) whenever lives change.
///
/// Lives mapping:
///   3 (full health) → no overlay
///   2               → damage1 overlay (light damage)
///   1               → damage2 overlay (heavy damage)
///   0               → damage3 overlay (destroyed — shown briefly before game-over)
///
/// The overlay SpriteRenderer is created as a child so it composites on top of the
/// ship sprite without replacing it.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDamageVisual : MonoBehaviour
{
    [Tooltip("Assign the 3 damage overlay sprites in order: damage1, damage2, damage3.")]
    public Sprite[] damageSprites = new Sprite[3];

    private SpriteRenderer _overlay;

    void Awake()
    {
        var child = new GameObject("DamageOverlay");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale    = Vector3.one;

        _overlay = child.AddComponent<SpriteRenderer>();
        _overlay.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        _overlay.sprite       = null;
        _overlay.color        = Color.white;
    }

    /// <summary>Update overlay based on current life count (call after every LoseLife/AddLife).</summary>
    public void Refresh(int lives)
    {
        if (_overlay == null) return;
        if (damageSprites == null || damageSprites.Length < 3) { _overlay.sprite = null; return; }

        _overlay.sprite = lives switch
        {
            2 => damageSprites[0],
            1 => damageSprites[1],
            0 => damageSprites[2],
            _ => null,
        };
    }
}
