using UnityEngine;
using System.Collections;

public class HunterAI : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float minPatrolDistance = 1.2f;
    public float maxPatrolDistance = 3.5f;
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 1.2f;
    [Range(0f, 1f)] public float flipChanceAfterIdle = 0.6f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform hazardCheck;
    public float checkRadius = 0.12f;
    public LayerMask groundLayer;
    public LayerMask hazardLayer;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float loseDetectionRange = 8f;

    [Header("Attack To Volt")]
    public int contactDamage = 1;
    public float attackCooldown = 0.7f;

    [Header("Health")]
    public int maxHealth = 2;

    [Header("Hit FX")]
    public float hitFlashDuration = 0.12f;
    public Color hitFlashColor = Color.red;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private int currentHealth;
    private int moveDirection = -1;

    private float targetVelocityX = 0f;
    private float idleTimer = 0f;
    private float patrolTargetDistance = 0f;
    private float patrolStartX = 0f;
    private float nextAttackTime = 0f;

    private bool isDead = false;
    private bool isIdle = false;
    private bool isChasing = false;
    private bool mustFlipAfterIdle = false;

    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;

        currentHealth = maxHealth;

        FindPlayer();

        moveDirection = Random.value < 0.5f ? -1 : 1;
        ApplyFacing();

        StartIdle(false);
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
            FindPlayer();

        HandleState();
    }

    void FixedUpdate()
    {
        if (isDead || rb == null) return;

        rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void HandleState()
    {
        if (player == null)
        {
            HandlePatrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        bool detectNow = distanceToPlayer <= detectionRange;
        bool keepChasing = isChasing && distanceToPlayer <= loseDetectionRange;

        if (detectNow || keepChasing)
        {
            isChasing = true;
            HandleChase();
            return;
        }

        if (isChasing)
        {
            isChasing = false;
            StartIdle(true);
        }

        HandlePatrol();
    }

    void HandlePatrol()
    {
        if (isIdle)
        {
            targetVelocityX = 0f;
            idleTimer -= Time.deltaTime;

            if (idleTimer <= 0f)
            {
                if (mustFlipAfterIdle || Random.value < flipChanceAfterIdle)
                    Flip();

                StartPatrol();
            }

            return;
        }

        if (!CanMoveForward())
        {
            StartIdle(true);
            return;
        }

        targetVelocityX = patrolSpeed * moveDirection;

        float traveled = Mathf.Abs(transform.position.x - patrolStartX);
        if (traveled >= patrolTargetDistance)
        {
            StartIdle(false);
        }
    }

    void HandleChase()
    {
        if (player == null)
        {
            isChasing = false;
            StartIdle(false);
            return;
        }

        float dx = player.position.x - transform.position.x;

        if (Mathf.Abs(dx) < 0.2f)
        {
            targetVelocityX = 0f;
            return;
        }

        int chaseDir = dx >= 0 ? 1 : -1;

        if (chaseDir != moveDirection)
        {
            moveDirection = chaseDir;
            ApplyFacing();
        }

        if (!CanMoveForward())
        {
            isChasing = false;
            StartIdle(true);
            return;
        }

        targetVelocityX = chaseSpeed * moveDirection;
    }

    bool CanMoveForward()
    {
        bool hasGroundAhead = true;
        bool wallAhead = false;
        bool hazardAhead = false;

        if (groundCheck != null)
            hasGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (wallCheck != null)
            wallAhead = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        if (hazardCheck != null)
            hazardAhead = Physics2D.OverlapCircle(hazardCheck.position, checkRadius, hazardLayer);

        return hasGroundAhead && !wallAhead && !hazardAhead;
    }

    void StartPatrol()
    {
        isIdle = false;
        mustFlipAfterIdle = false;

        patrolTargetDistance = Random.Range(minPatrolDistance, maxPatrolDistance);
        patrolStartX = transform.position.x;
    }

    void StartIdle(bool forceFlip)
    {
        isIdle = true;
        mustFlipAfterIdle = forceFlip;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        targetVelocityX = 0f;
    }

    void Flip()
    {
        moveDirection *= -1;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDirection;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (player != null)
        {
            float dx = player.position.x - transform.position.x;

            if (Mathf.Abs(dx) > 0.1f)
            {
                moveDirection = dx >= 0 ? 1 : -1;
                ApplyFacing();
            }

            isChasing = true;
        }

        if (sr != null)
            StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        if (sr == null) yield break;

        sr.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);

        if (sr != null)
            sr.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        targetVelocityX = 0f;

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (Time.time < nextAttackTime) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(contactDamage, (Vector2)transform.position);
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;
        if (Time.time < nextAttackTime) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(contactDamage, (Vector2)transform.position);
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }

        if (hazardCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hazardCheck.position, checkRadius);
        }
    }
}