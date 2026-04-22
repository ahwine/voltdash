using UnityEngine;

public class SentinelAI : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Detection")]
    public float detectionRadius = 8f;
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootCooldown = 1.5f;
    private float shootTimer;
    public float bulletSpeed = 8f;
    
    [Header("Hover Effect")]
    public bool enableHover = false;
    public float hoverAmplitude = 0.05f;
    public float hoverFrequency = 2f;
    private Vector3 startPosition;
    
    private Transform player;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isDead = false;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("SENTINEL: Player tidak ditemukan!");
        
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
        
        currentHealth = maxHealth;
        startPosition = transform.position;
        shootTimer = 0f;
        
        if (firePoint == null)
            Debug.LogError("SENTINEL: FirePoint BELUM di-assign!");
        
        Debug.Log("Sentinel siap. Health: " + currentHealth);
    }
    
    void Update()
    {
        if (isDead) return;
        if (player == null) return;
        
        if (enableHover)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRadius)
        {
            if (player.position.x > transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
            
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("SENTINEL: Bullet Prefab BELUM di-assign!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("SENTINEL: Fire Point BELUM di-assign!");
            return;
        }
        
        if (player == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            rb.velocity = direction * bulletSpeed;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        bullet.tag = "EnemyBullet";
        
        // NO FRIENDLY FIRE: Abaikan collision dengan Sentinel dan Hunter
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        Collider2D sentinelCollider = GetComponent<Collider2D>();
        if (bulletCollider != null && sentinelCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, sentinelCollider);
        }
        
        // Abaikan juga collision dengan semua objek bertag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
            }
        }
        
        Debug.Log("SENTINEL: Tembak ke arah player!");
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (sr != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRed());
        }
        
        Debug.Log("Sentinel terkena hit! Sisa HP: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }
    
    void Die()
    {
        isDead = true;
        sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Debug.Log("SENTINEL MATI!");
        Destroy(gameObject, 0.1f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}