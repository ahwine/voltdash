using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    
    [Header("UI")]
    public TextMeshProUGUI healthText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    
    private Vector3 startPosition;
    private Vector3 currentCheckpoint;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        currentCheckpoint = startPosition;
        sr = GetComponent<SpriteRenderer>();
        
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthUI();
        StartCoroutine(FlashRed());
        
        Debug.Log("Volt terkena damage! Sisa health: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = currentCheckpoint;
        Debug.Log("Volt respawn ke checkpoint.");
    }

    void Die()
    {
        Debug.Log("VOLT MATI. GAME OVER.");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
        Debug.Log("Checkpoint tersimpan di: " + newCheckpoint);
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sr.color = originalColor;
        }
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
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
        Debug.Log("Health bertambah! Sekarang: " + currentHealth);
    }
}