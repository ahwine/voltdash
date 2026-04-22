using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        else
        {
            rb = GetComponent<Rigidbody2D>();
            rb.velocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HunterAI hunter = other.GetComponent<HunterAI>();
            if (hunter != null)
            {
                hunter.TakeDamage(damage);
            }
            
            SentinelAI sentinel = other.GetComponent<SentinelAI>();
            if (sentinel != null)
            {
                sentinel.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
    }
}