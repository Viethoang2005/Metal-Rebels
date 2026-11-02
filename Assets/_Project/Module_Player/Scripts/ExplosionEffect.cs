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

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            ApplyExplosionDamage();

            if (explosionSprites != null && explosionSprites.Length > 0 && spriteRenderer != null)
            {
                StartCoroutine(PlayExplosionAnimation());
            }

            Destroy(gameObject, effectDuration);
        }

        public void Init(int explosionDamage, float radius, LayerMask layers = default)
        {
            damage = explosionDamage;
            explosionRadius = radius;
            if (layers != default)
            {
                targetLayers = layers;
            }
        }

        private void ApplyExplosionDamage()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayers);
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Player") || col.GetComponentInParent<PlayerController>() != null)
                {
                    continue; // Người chơi không bị sát thương bởi lựu đạn/rocket của chính mình
                }

                IDamageable damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null && !damagedTargets.Contains(damageable))
                {
                    damagedTargets.Add(damageable);
                    damageable.TakeDamage(damage);
                }
            }
        }

        private IEnumerator PlayExplosionAnimation()
        {
            float timePerFrame = effectDuration / explosionSprites.Length;
            for (int i = 0; i < explosionSprites.Length; i++)
            {
                if (explosionSprites[i] != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = explosionSprites[i];
                }
                yield return new WaitForSeconds(timePerFrame);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
