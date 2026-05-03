using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject winPanel;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
    }

    void Update()
    {
        if (DataCore.isCollected)
        {
            sr.color = Color.green;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (DataCore.isCollected)
        {
            GameObject player = collision.gameObject;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.enabled = false;
            }

            if (winPanel != null)
            {
                winPanel.SetActive(true);
                Time.timeScale = 0f;
            }

            Debug.Log("LEVEL COMPLETE!");
        }
        else
        {
            Debug.Log("Pintu terkunci! Cari Data Core dulu.");
        }
    }
}