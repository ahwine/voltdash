using UnityEngine;

public class HunterAI : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 3f;
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkDistance = 1f;
    public LayerMask groundLayer;
    
    [Header("Detection")]
    public float detectionRange = 3f;
    public float loseDetectionRange = 5f;
    public float attackRange = 1f;
    
    [Header("Combat")]
    public int maxHealth = 2;
    private int currentHealth;
    private int damage = 1;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Vector3 startPosition;
    
    private enum State { Patrol, Chase, ReturnToStart }
    private State currentState = State.Patrol;
    private bool isDead = false;
    private bool facingRight = true;
    
    private Color originalColor;
    
    // FITUR BARU: Agressive mode saat kena tembak
    private bool isAggressive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        currentHealth = maxHealth;
        originalColor = sr.color;
        facingRight = true;
    }

    void Update()
    {
        if (isDead) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distanceToPlayer <= detectionRange || isAggressive)
                {
                    currentState = State.Chase;
                }
                break;
                
            case State.Chase:
                ChasePlayer(distanceToPlayer);
                if (distanceToPlayer > loseDetectionRange && !isAggressive)
                {
                    currentState = State.ReturnToStart;
                }
                break;
                
            case State.ReturnToStart:
                ReturnToStart();
                if (Mathf.Abs(transform.position.x - startPosition.x) < 0.5f)
                {
                    currentState = State.Patrol;
                    isAggressive = false; // Reset aggressive saat kembali
                }
                if (distanceToPlayer <= detectionRange || isAggressive)
                {
                    currentState = State.Chase;
                }
                break;
        }
    }

    void Patrol()
    {
        float distanceFromStart = transform.position.x - startPosition.x;
        
        if (Mathf.Abs(distanceFromStart) >= patrolDistance)
            Flip();
        
        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * (facingRight ? 1 : -1), 0.3f, groundLayer);
        
        if (!groundAhead || wallAhead)
            Flip();
        
        rb.velocity = new Vector2(patrolSpeed * (facingRight ? 1 : -1), rb.velocity.y);
    }

    void ChasePlayer(float distanceToPlayer)
    {
        if (player.position.x > transform.position.x)
        {
            facingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            facingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void ReturnToStart()
    {
        if (startPosition.x > transform.position.x)
        {
            facingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            facingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        Vector2 direction = (startPosition - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        StartCoroutine(FlashRed());
        
        // FITUR BARU: Saat kena tembak, langsung agresif
        isAggressive = true;
        currentState = State.Chase;
        
        Debug.Log("Hunter terkena hit! Sisa HP: " + currentHealth + " | Mode Agresif AKTIF");
        
        if (currentHealth <= 0)
            Die();
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
        currentState = State.Patrol;
        sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        Debug.Log("Hunter mati permanen.");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                Debug.Log("Hunter menyentuh Volt! Damage langsung.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseDetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}