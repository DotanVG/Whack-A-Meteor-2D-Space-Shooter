using UnityEngine;

/// <summary>
/// ExplosionAnimator — cycles through fire sprite frames then destroys itself.
/// Attach to the Explosion prefab alongside a SpriteRenderer.
/// Assign the fire00–fire19 sprites in the frames array via the Inspector.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float    fps = 24f;

    private SpriteRenderer _sr;
    private int   _frame;
    private float _nextFrame;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _nextFrame = Time.time + 1f / fps;

        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject, 0.1f);
            return;
        }
        _sr.sprite = frames[0];
        Destroy(gameObject, frames.Length / fps + 0.1f);
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        if (Time.time < _nextFrame) return;

        _frame++;
        if (_frame >= frames.Length) return; // let Destroy() clean up
        _sr.sprite = frames[_frame];
        _nextFrame = Time.time + 1f / fps;
    }
}
