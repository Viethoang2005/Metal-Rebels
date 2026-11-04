using System.Collections;
using MetalRebels.Core.Interfaces;
using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerGrenade : MonoBehaviour
    {
        [Header("Cấu hình Lựu đạn")]
        [SerializeField] private int damage = 30;
        [SerializeField] private float fuseTime = 3f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private LayerMask hitLayers = ~0;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(
            Vector2 initialVelocity,
            int grenadeDamage = 30,
            float countdown = 3f,
            float radius = 3f,
            GameObject explosionObj = null,
            Sprite visualSprite = null)
        {
            damage = grenadeDamage;
            fuseTime = countdown;
            explosionRadius = radius;
            if (visualSprite != null)
            {
                if (spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }

                spriteRenderer.sprite = visualSprite;
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 2;
            }

            if (explosionObj != null)
            {
                explosionPrefab = explosionObj;
            }

            if (rb != null)
            {
                rb.linearVelocity = initialVelocity;
                // Thêm độ xoay ngẫu nhiên khi ném
                rb.angularVelocity = -Mathf.Sign(initialVelocity.x) * 360f;
            }

            StartCoroutine(FuseRoutine());
        }

        private IEnumerator FuseRoutine()
        {
            float elapsed = 0f;
            float flashInterval = 0.3f;

            while (elapsed < fuseTime)
            {
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;

                // Nhấp nháy nhanh dần khi sắp nổ
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = spriteRenderer.color == Color.red ? Color.white : Color.red;
                }

                if (elapsed > fuseTime * 0.7f)
                {
                    flashInterval = 0.1f;
                }
            }

            Explode();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Nếu va chạm trực tiếp với kẻ địch thì phát nổ ngay lập tức
            if (!collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponentInParent<PlayerController>() == null)
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>() ?? collision.gameObject.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    Explode();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player") && collision.GetComponentInParent<PlayerController>() == null)
            {
                IDamageable damageable = collision.GetComponent<IDamageable>() ?? collision.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            StopAllCoroutines();

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
