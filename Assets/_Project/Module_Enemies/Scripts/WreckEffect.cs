using UnityEngine;

public class WreckEffect : MonoBehaviour
{
    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Bắn xác xe văng chéo lên trời ngẫu nhiên
            float randomX = Random.Range(-3f, 3f);
            float randomUp = Random.Range(5f, 8f);
            rb.linearVelocity = new Vector2(randomX, randomUp);

            // Cho xác xe xoay tròn tít mù khi bay
            rb.angularVelocity = Random.Range(300f, 600f);
        }

        // Tự biến mất hoàn toàn sau 2 giây để dọn rác bộ nhớ
        Destroy(gameObject, 2f);
    }
}