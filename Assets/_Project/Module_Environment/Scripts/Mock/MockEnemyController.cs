using System.Collections;
using UnityEngine;
using MetalRebels.Core.Interfaces;

namespace MetalRebels.Module_Environment.Mock
{
    /// <summary>
    /// Kẻ địch Mockup dùng để kiểm thử WaveManager và Event Lock.
    /// Kế thừa IDamageable, có thể tuần tra qua lại hoặc đứng canh,
    /// nhấp nháy đỏ khi bị bắn trúng và tự hủy khi hết máu.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MockEnemyController : MonoBehaviour, IDamageable
    {
        [Header("Chỉ số")]
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private int currentHealth = 30;
        [SerializeField] private bool isBoss = false;

        [Header("Tuần tra")]
        [SerializeField] private bool canPatrol = true;
        [SerializeField] private float patrolDistance = 3f;
        [SerializeField] private float patrolSpeed = 2f;

        [Header("Hiệu ứng")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitColor = Color.red;

        private Vector3 startPosition;
        private int moveDirection = 1;
        private Color originalColor;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            tag = isBoss ? "Boss" : "Enemy";
            startPosition = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (!canPatrol) return;

            transform.Translate(Vector3.right * (moveDirection * patrolSpeed * Time.deltaTime));

            if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
            {
                moveDirection *= -1;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * moveDirection;
                transform.localScale = scale;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= damageAmount;

            if (spriteRenderer != null && gameObject.activeInHierarchy)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashRoutine());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }

        private void Die()
        {
            if (isBoss)
            {
                MetalRebels.Core.Events.GameEvents.OnBossKilled?.Invoke();
                MetalRebels.Core.Events.GameEvents.OnGameOver?.Invoke(true);
                Debug.Log("[MockEnemy] BOSS ĐÃ BỊ TIÊU DIỆT - LEVEL CLEAR!");
            }

            Destroy(gameObject);
        }
    }
}
