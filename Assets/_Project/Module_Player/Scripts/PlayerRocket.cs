using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerRocket : MonoBehaviour
    {
        [Header("Cấu hình Rocket")]
        [SerializeField] private int damage = 100;
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private LayerMask hitLayers = ~0;

        private Vector2 moveDirection = Vector2.right;
        private Rigidbody2D rb;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Init(Vector2 direction, int rocketDamage, float rocketSpeed, float rocketLifetime, float radius, GameObject explosionEffectObj = null)
        {
            moveDirection = direction.normalized;
            damage = rocketDamage;
            speed = rocketSpeed;
            lifetime = rocketLifetime;
            explosionRadius = radius;
            if (explosionEffectObj != null)
            {
                explosionPrefab = explosionEffectObj;
            }

            // Xoay đầu tên lửa theo hướng bay
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (rb != null)
            {
                rb.linearVelocity = moveDirection * speed;
            }

            Invoke(nameof(Explode), lifetime);
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
            HandleCollision(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.gameObject);
        }

        private void HandleCollision(GameObject hitObj)
        {
            if (hasExploded) return;

            // Không nổ khi chạm chính Player
            if (hitObj.CompareTag("Player") || hitObj.GetComponentInParent<PlayerController>() != null)
            {
                return;
            }

            Explode();
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            CancelInvoke(nameof(Explode));

            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                ExplosionEffect eff = exp.GetComponent<ExplosionEffect>();
                if (eff != null)
                {
                    eff.Init(damage, explosionRadius, hitLayers);
                }
            }

            Destroy(gameObject);
        }
    }
}
