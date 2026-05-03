using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifeTime = 0.15f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}