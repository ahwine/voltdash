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
        if (collision.gameObject.CompareTag("Player"))
        {
            if (DataCore.isCollected)
            {
                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                    Time.timeScale = 0;
                }
                Debug.Log("LEVEL COMPLETE!");
            }
            else
            {
                Debug.Log("Pintu terkunci! Cari Data Core dulu.");
            }
        }
    }
}