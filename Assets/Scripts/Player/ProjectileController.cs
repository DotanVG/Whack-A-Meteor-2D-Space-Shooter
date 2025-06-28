using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 50f;  // Speed at which the projectile moves
    public float lifeTime = 2f; // Lifetime of the projectile

    void Start()
    {
        // Initialize the projectile's velocity in the direction it's facing
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;

        // Destroy the projectile after 'lifeTime' seconds if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hit an enemy
        if (other.gameObject.CompareTag("Enemy"))
        {
            // TODO: Implement logic for handling enemy hit
            // Add additional code here for what happens when the projectile hits an enemy
            Destroy(gameObject); // Destroy the projectile
        }
        // Check if the projectile hit a meteor
        else if (other.gameObject.CompareTag("BigBrownMeteor") ||
                 other.gameObject.CompareTag("BigGreyMeteor") ||
                 other.gameObject.CompareTag("MediumBrownMeteor") ||
                 other.gameObject.CompareTag("MediumGreyMeteor") ||
                 other.gameObject.CompareTag("SmallBrownMeteor") ||
                 other.gameObject.CompareTag("SmallGreyMeteor") ||
                 other.gameObject.CompareTag("TinyBrownMeteor") ||
                 other.gameObject.CompareTag("TinyGreyMeteor"))
        {
            MeteorSplit meteorSplit = other.gameObject.GetComponent<MeteorSplit>();
            if (meteorSplit != null)
            {
                meteorSplit.OnProjectileHit();
            }
            Destroy(gameObject); // Destroy the projectile
        }
    }
}