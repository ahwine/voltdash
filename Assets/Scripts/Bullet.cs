using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;
    private Rigidbody2D rb;

    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
            rb.velocity = direction * speed;
        else
            GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hancur jika kena Ground / Platform / Wall
        if (other.gameObject.layer == groundLayer)
        {
            Destroy(gameObject);
            return;
        }

        // Peluru player mengenai musuh
        if (other.CompareTag("Enemy"))
        {
            HunterAI h = other.GetComponent<HunterAI>();
            if (h != null) h.TakeDamage(damage);

            SentinelAI s = other.GetComponent<SentinelAI>();
            if (s != null) s.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // PENTING:
        // Damage ke player dari EnemyBullet TIDAK ditangani di sini.
        // Sudah ditangani oleh PlayerHealth.cs agar tidak dobel damage.
    }
}