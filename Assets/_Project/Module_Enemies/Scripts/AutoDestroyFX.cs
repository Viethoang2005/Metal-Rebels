using UnityEngine;

public class AutoDestroyFX : MonoBehaviour
{
    public float lifetime = 0.5f; // Khớp với độ dài animation nổ

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}