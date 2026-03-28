using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 2.0f;

    void Start()
    {
        EnemyFaction ef = GetComponent<EnemyFaction>();
        if (ef != null)
        {
            string key = $"enemy.speed_{ef.faction.ToString().ToLower()}";
            speed = BalanceService.Instance?.GetFloat(key, speed) ?? speed;
        }
    }

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            Destroy(other.gameObject);
            EnemyHealth health = GetComponent<EnemyHealth>();
            if (health != null) health.TakeDamage(1);
            else Destroy(gameObject);
        }
    }
}
