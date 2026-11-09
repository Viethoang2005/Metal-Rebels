using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
using UnityEngine;

namespace MetalRebels.Player
{
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Chỉ số Vụ nổ")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private int damage = 100;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private float effectDuration = 0.5f;

        [Header("Hiệu ứng thị giác")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] explosionSprites;

        private bool damageApplied = false;
        private HashSet<IDamageable> sharedDamagedTargets = null;
        private Vector2 visualTravelDirection;
        private float visualTravelDistance;

        private void Awake()
        {
            if (sharedDamagedTargets == null)
            {
                sharedDamagedTargets = new HashSet<IDamageable>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
            }
        }

        private void Start()
        {
            float duration = Mathf.Max(effectDuration, 0.01f);
            if (visualTravelDistance > 0f)
            {
                if (explosionSprites != null && explosionSprites.Length > 0 && spriteRenderer != null)
                {
                    StartCoroutine(PlayExplosionAnimation(duration));
                }
                else
                {
                    StartCoroutine(TravelExplosion(duration));
                }
            }
            else
            {
                if (!damageApplied)
                {
                    ApplyExplosionDamage();
                }

                if (explosionSprites != null && explosionSprites.Length > 0 && spriteRenderer != null)
                {
                    StartCoroutine(PlayExplosionAnimation(duration));
                }
            }

            Destroy(gameObject, duration);
        }

        public void Init(
            int explosionDamage,
            float radius,
            LayerMask layers = default,
            HashSet<IDamageable> sharedTargets = null,
            Sprite[] visualSprites = null,
            float visualScale = 1f,
            float visualDuration = 0f,
            Vector2 visualDirection = default,
            float visualDistance = 0f)
        {
            damage = explosionDamage;
            explosionRadius = radius;
            transform.localScale *= Mathf.Max(0.01f, visualScale);
            visualTravelDirection = visualDirection.normalized;
            visualTravelDistance = visualTravelDirection == Vector2.zero ? 0f : Mathf.Max(0f, visualDistance);
            if (visualDuration > 0f)
            {
                effectDuration = visualDuration;
            }
            if (layers != default)
            {
                targetLayers = layers;
            }
            sharedDamagedTargets = sharedTargets ?? new HashSet<IDamageable>();

            if (visualSprites != null && visualSprites.Length > 0)
            {
                explosionSprites = visualSprites;
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = explosionSprites[0];
                }
            }

            if (!damageApplied)
            {
                if (visualTravelDistance <= 0f)
                {
                    ApplyExplosionDamage();
                }
            }
        }

        private void ApplyExplosionDamage()
        {
            damageApplied = true;
            ApplyExplosionDamageAt(transform.position);
        }

        private void ApplyExplosionDamageAt(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, explosionRadius, targetLayers);

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Player") || col.GetComponentInParent<PlayerController>() != null)
                {
                    continue; // Người chơi không bị sát thương bởi lựu đạn/rocket của chính mình
                }

                IDamageable damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null && !sharedDamagedTargets.Contains(damageable))
                {
                    sharedDamagedTargets.Add(damageable);
                    damageable.TakeDamage(damage);
                }
            }
        }

        private IEnumerator PlayExplosionAnimation(float duration)
        {
            float timePerFrame = duration / explosionSprites.Length;
            Vector3 initialScale = transform.localScale;
            Vector3 initialPosition = transform.position;
            Color initialColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                int frameIndex = Mathf.Min(Mathf.FloorToInt(elapsed / timePerFrame), explosionSprites.Length - 1);
                if (explosionSprites[frameIndex] != null)
                {
                    spriteRenderer.sprite = explosionSprites[frameIndex];
                }

                float normalizedTime = elapsed / duration;
                UpdateTravelPosition(initialPosition, normalizedTime);
                transform.localScale = initialScale * Mathf.Lerp(0.35f, 1.35f, normalizedTime);
                Color color = initialColor;
                color.a *= 1f - normalizedTime;
                spriteRenderer.color = color;

                if (visualTravelDistance > 0f)
                {
                    ApplyExplosionDamageAt(transform.position);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            UpdateTravelPosition(initialPosition, 1f);
            if (visualTravelDistance > 0f)
            {
                ApplyExplosionDamageAt(transform.position);
                damageApplied = true;
            }
            transform.localScale = initialScale * 1.35f;
            Color finalColor = initialColor;
            finalColor.a = 0f;
            spriteRenderer.color = finalColor;
        }

        private IEnumerator TravelExplosion(float duration)
        {
            Vector3 initialPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float normalizedTime = elapsed / duration;
                UpdateTravelPosition(initialPosition, normalizedTime);
                ApplyExplosionDamageAt(transform.position);
                elapsed += Time.deltaTime;
                yield return null;
            }

            UpdateTravelPosition(initialPosition, 1f);
            ApplyExplosionDamageAt(transform.position);
            damageApplied = true;
        }

        private void UpdateTravelPosition(Vector3 initialPosition, float normalizedTime)
        {
            transform.position = initialPosition
                + (Vector3)(visualTravelDirection * visualTravelDistance * Mathf.Clamp01(normalizedTime));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
