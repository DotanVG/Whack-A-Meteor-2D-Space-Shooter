using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 2.0f;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Projectile")
        {
            Destroy(other.gameObject); // Destroy the projectile
            Destroy(gameObject); // Destroy the enemy
        }
    }
}
