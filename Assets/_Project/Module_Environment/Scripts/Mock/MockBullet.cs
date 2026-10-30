using UnityEngine;
using MetalRebels.Core.Interfaces;

namespace MetalRebels.Module_Environment.Mock
{
    /// <summary>
    /// Viên đạn Mockup bay theo hướng người chơi,
    /// gây 20 sát thương lên bất kỳ đối tượng nào có IDamageable (trừ Player).
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class MockBullet : MonoBehaviour
    {
        [SerializeField] private float speed = 16f;
        [SerializeField] private int damage = 20;
        [SerializeField] private float lifetime = 2.5f;

        private float direction = 1f;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            rb.linearVelocity = new Vector2(direction * speed, 0f);
            Destroy(gameObject, lifetime);
        }

        public void SetDirection(float dir)
        {
            direction = Mathf.Sign(dir);
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction * speed, 0f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Bỏ qua va chạm với Player
            if (other.CompareTag("Player") || other.GetComponent<MockPlayerController>() != null) return;

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (!other.isTrigger)
            {
                // Va chạm vào địa hình / tường chắn cứng
                Destroy(gameObject);
            }
        }
    }
}
