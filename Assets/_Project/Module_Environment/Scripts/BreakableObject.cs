using System.Collections;
using UnityEngine;
using MetalRebels.Core.Interfaces;

namespace MetalRebels.Module_Environment
{
    [System.Serializable]
    public class DropItemEntry
    {
        public string itemName = "Vật phẩm";
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float dropRate = 50f; // Xác suất rơi (%)
    }

    /// <summary>
    /// Đối tượng có thể bị phá hủy trong môi trường (Thùng gỗ, bao cát, rào cản).
    /// Kế thừa IDamageable để nhận sát thương từ mọi nguồn đạn/vụ nổ.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BreakableObject : MonoBehaviour, IDamageable
    {
        [Header("Chỉ số Máu")]
        [SerializeField] private int maxHealth = 20;
        private int currentHealth;

        [Header("Hiệu ứng khi nhận sát thương")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Hiệu ứng khi bị phá hủy")]
        [SerializeField] private GameObject destroyEffectPrefab;
        [SerializeField] private AudioClip destroySound;
        [SerializeField] private bool triggerCameraShake = false;

        [Header("Bảng rơi vật phẩm (Drop Table)")]
        [SerializeField] private DropItemEntry[] dropTable;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.2f, 0f);

        private bool isDestroyed = false;
        private Color originalColor;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            if (isDestroyed || damageAmount <= 0) return;

            currentHealth -= damageAmount;

            // Nhấp nháy phản hồi trúng đòn
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
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }

        private void Die()
        {
            isDestroyed = true;

            // Rơi vật phẩm dựa trên bảng xác suất
            DropItems();

            // Hiệu ứng hạt / mảnh vỡ
            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            // Âm thanh vỡ
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, transform.position);
            }

            // Rung màn hình nhẹ nếu được bật
            if (triggerCameraShake && CameraShaker.Instance != null)
            {
                CameraShaker.Instance.Shake(0.2f, 0.2f);
            }

            Destroy(gameObject);
        }

        private void DropItems()
        {
            if (dropTable == null || dropTable.Length == 0) return;

            foreach (var entry in dropTable)
            {
                if (entry.itemPrefab == null) continue;

                float roll = Random.Range(0f, 100f);
                if (roll <= entry.dropRate)
                {
                    Instantiate(entry.itemPrefab, transform.position + spawnOffset, Quaternion.identity);
                    // Sau khi rơi 1 vật phẩm thì dừng (hoặc có thể bỏ break nếu muốn rơi nhiều)
                    break;
                }
            }
        }
    }
}

