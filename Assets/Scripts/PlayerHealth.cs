using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Revive Settings")]
    public int maxRevives = 2;
    public int currentRevives;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI reviveText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Hit Feedback")]
    public float flashDuration = 0.15f;
    public float knockbackForceX = 4f;
    public float knockbackForceY = 2.5f;

    [Header("Death Animation")]
    public float deathAnimDuration = 0.45f;

    private Vector3 startPosition;
    private Vector3 currentCheckpoint;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement playerMovement;

    private Color originalColor;
    private Coroutine flashRoutine;
    private bool isDeathSequence = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentRevives = maxRevives;

        startPosition = transform.position;
        currentCheckpoint = startPosition;

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (sr != null)
            originalColor = sr.color;

        UpdateUI();
    }

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (isDeathSequence) return;

        ApplyDamage(damage, hitSource, false);
    }

    public void TakeHazardDamage(int damage)
    {
        if (isDeathSequence) return;

        ApplyDamage(damage, transform.position, true);
    }

    void ApplyDamage(int damage, Vector2 hitSource, bool forceRespawnToCheckpoint)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
            return;
        }

        PlayHitAnimation();
        PlayFlash();

        if (!forceRespawnToCheckpoint)
            ApplyKnockback(hitSource);

        Debug.Log("Volt terkena damage! Sisa HP: " + currentHealth);

        if (forceRespawnToCheckpoint)
        {
            RespawnToCheckpoint();
        }
    }

    void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Death");
            animator.SetTrigger("Hit");
        }
    }

    System.Collections.IEnumerator DeathSequence()
    {
        isDeathSequence = true;

        Debug.Log("Volt mati. Memainkan animasi death.");

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (rb != null)
            rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Shoot");
            animator.ResetTrigger("Reload");
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Death");
        }

        if (sr != null)
            sr.color = originalColor;

        yield return new WaitForSeconds(deathAnimDuration);

        if (currentRevives > 0)
        {
            currentRevives--;
            currentHealth = maxHealth;
            UpdateUI();

            Debug.Log("Volt revive. Sisa revive: " + currentRevives);

            RespawnToCheckpoint();

            if (animator != null)
                animator.Play("Volt_Idle", 0, 0f);

            if (playerMovement != null)
                playerMovement.enabled = true;

            isDeathSequence = false;
        }
        else
        {
            Debug.Log("VOLT MATI. GAME OVER.");

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    void RespawnToCheckpoint()
    {
        transform.position = currentCheckpoint;

        if (rb != null)
            rb.velocity = Vector2.zero;

        if (sr != null)
            sr.color = originalColor;

        Debug.Log("Volt respawn ke checkpoint.");
    }

    void ApplyKnockback(Vector2 hitSource)
    {
        if (rb == null) return;

        Vector2 direction = ((Vector2)transform.position - hitSource);

        if (direction.sqrMagnitude < 0.001f)
        {
            float fallbackDir = transform.localScale.x >= 0 ? 1f : -1f;
            direction = new Vector2(fallbackDir, 0f);
        }

        direction.Normalize();

        float horizontal = direction.x >= 0 ? knockbackForceX : -knockbackForceX;
        Vector2 knockback = new Vector2(horizontal, knockbackForceY);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.AddForce(knockback, ForceMode2D.Impulse);
    }

    void PlayFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRed());
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }

        flashRoutine = null;
    }

    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
        Debug.Log("Checkpoint tersimpan di: " + newCheckpoint);
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;

        if (reviveText != null)
            reviveText.text = "Revive: " + currentRevives;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDeathSequence) return;

        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1, other.transform.position);
            Destroy(other.gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (isDeathSequence) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
        Debug.Log("Health bertambah! Sekarang: " + currentHealth);
    }
}