using MetalRebels.Core.Interfaces;
using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerBullet : MonoBehaviour
    {
        [Header("Cấu hình mặc định")]
        [SerializeField] private int damage = 20;
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private GameObject hitEffectPrefab;

        private Vector2 moveDirection = Vector2.right;
        private Rigidbody2D rb;
        private bool hasHit = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Init(Vector2 direction, int bulletDamage, float bulletSpeed, float bulletLifetime, LayerMask targetLayers = default)
        {
            moveDirection = direction.normalized;
            damage = bulletDamage;
            speed = bulletSpeed;
            lifetime = bulletLifetime;
            if (targetLayers != default)
            {
                hitLayers = targetLayers;
            }

            // Xoay góc đạn theo hướng bắn
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (rb != null)
            {
                rb.linearVelocity = moveDirection * speed;
            }

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (rb == null)
            {
                transform.position += (Vector3)(moveDirection * (speed * Time.deltaTime));
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleHit(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleHit(collision.gameObject);
        }

        private void HandleHit(GameObject hitObj)
        {
            if (hasHit) return;

            // Bỏ qua va chạm với Player
            if (hitObj.CompareTag("Player") || hitObj.GetComponentInParent<PlayerController>() != null)
            {
                return;
            }

            // Kiểm tra xem mục tiêu có nhận sát thương không (Kẻ địch, Boss, Thùng phá hủy)
            IDamageable damageable = hitObj.GetComponent<IDamageable>() ?? hitObj.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                hasHit = true;
                SpawnHitEffect();
                Destroy(gameObject);
                return;
            }

            // Nếu trúng địa hình / vật cản
            if (((1 << hitObj.layer) & hitLayers) != 0 || hitObj.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                hasHit = true;
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }

        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
