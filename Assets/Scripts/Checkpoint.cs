using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;
            sr.color = Color.blue;
            
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.SetCheckpoint(transform.position);
            }
        }
    }
}