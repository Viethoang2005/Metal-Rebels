using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
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
        [SerializeField] private Sprite visualSprite;

        private Vector2 moveDirection = Vector2.right;
        private Rigidbody2D rb;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            EnsureVisibleRocket();
        }

        private void EnsureVisibleRocket()
        {
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (renderer.sprite == null)
            {
                renderer.sprite = visualSprite;
                renderer.color = Color.white;
            }
            else if (visualSprite != null)
            {
                renderer.sprite = visualSprite;
            }

            renderer.sortingOrder = 10;
            transform.localScale = Vector3.one;

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

            IDamageable damageable = hitObj.GetComponent<IDamageable>()
                ?? hitObj.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                Collider2D hitCollider = hitObj.GetComponent<Collider2D>();
                if (hitCollider != null && hitCollider.isTrigger)
                {
                    return;
                }
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
            else
            {
                ApplyFallbackExplosionDamage();
            }

            Destroy(gameObject);
        }

        private void ApplyFallbackExplosionDamage()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayers);
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Player") || col.GetComponentInParent<PlayerController>() != null)
                {
                    continue;
                }

                IDamageable damageable = col.GetComponent<IDamageable>()
                    ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null && damagedTargets.Add(damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }
}
