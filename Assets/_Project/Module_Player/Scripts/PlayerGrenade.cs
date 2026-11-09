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
        [SerializeField] private float explosionVisualScale = 0.22f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Sprite[] explosionSprites;
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
            Sprite visualSprite = null,
            Sprite[] visualExplosionSprites = null,
            float visualScale = 0.22f)
        {
            damage = grenadeDamage;
            fuseTime = countdown;
            explosionRadius = radius;
            explosionVisualScale = Mathf.Max(0.01f, visualScale);
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
            explosionSprites = visualExplosionSprites;

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
            if (collision.gameObject.CompareTag("Player")
                || collision.gameObject.GetComponentInParent<PlayerController>() != null)
            {
                return;
            }

            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>()
                ?? collision.gameObject.GetComponentInParent<IDamageable>();
            if (TryGetLandingPoint(collision, transform.position.y, out Vector2 landingPoint))
            {
                Explode(new Vector3(transform.position.x, landingPoint.y, transform.position.z));
            }
            else if (damageable != null)
            {
                Explode();
            }
        }

        private static bool TryGetLandingPoint(Collision2D collision, float grenadeCenterY, out Vector2 landingPoint)
        {
            landingPoint = Vector2.zero;
            int landingContacts = 0;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (Mathf.Abs(contact.normal.y) > 0.5f && contact.point.y < grenadeCenterY)
                {
                    landingPoint += contact.point;
                    landingContacts++;
                }
            }

            if (landingContacts == 0)
            {
                return false;
            }

            landingPoint /= landingContacts;
            return true;
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

        private void Explode(Vector3? explosionPosition = null)
        {
            if (hasExploded) return;
            hasExploded = true;

            StopAllCoroutines();
            Vector3 position = explosionPosition ?? transform.position;

            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, position, Quaternion.identity);
                ExplosionEffect eff = exp.GetComponent<ExplosionEffect>();
                if (eff != null)
                {
                    eff.Init(damage, explosionRadius, hitLayers, visualSprites: explosionSprites, visualScale: explosionVisualScale);
                }
            }
            else
            {
                GameObject exp = new GameObject("GrenadeExplosion_Runtime");
                exp.transform.position = position;
                SpriteRenderer effectRenderer = exp.AddComponent<SpriteRenderer>();
                effectRenderer.sortingOrder = 10;
                if (explosionSprites != null && explosionSprites.Length > 0)
                {
                    effectRenderer.sprite = explosionSprites[0];
                }

                ExplosionEffect effect = exp.AddComponent<ExplosionEffect>();
                effect.Init(damage, explosionRadius, hitLayers, visualSprites: explosionSprites, visualScale: explosionVisualScale);
            }

            Destroy(gameObject);
        }
    }
}
