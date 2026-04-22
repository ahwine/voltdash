using UnityEngine;

public class DataCore : MonoBehaviour
{
    public static bool isCollected = false;
    
    void Awake()
    {
        // Reset status setiap scene baru dimulai
        isCollected = false;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            Destroy(gameObject);
            Debug.Log("Data Core dikoleksi! Pintu sekarang bisa dibuka.");
        }
    }
}