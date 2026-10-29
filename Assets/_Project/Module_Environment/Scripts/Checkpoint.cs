using UnityEngine;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Điểm lưu vị trí hồi sinh (Checkpoint). Khi Marco đi qua,
    /// lưu lại vị trí này để làm điểm hồi sinh khi mất mạng.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        public static Vector3 LastActiveCheckpointPosition { get; private set; }
        public static bool HasActiveCheckpoint { get; private set; }

        [Header("Trạng thái trực quan")]
        [SerializeField] private SpriteRenderer flagRenderer;
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.green;

        [Header("Hiệu ứng & Âm thanh")]
        [SerializeField] private AudioClip activateSound;
        [SerializeField] private GameObject activateEffectPrefab;

        [Header("Tọa độ hồi sinh chính xác")]
        [SerializeField] private Vector3 respawnOffset = new Vector3(0f, 0.5f, 0f);

        private bool isActivated = false;

        private void Start()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (flagRenderer != null)
            {
                flagRenderer.color = inactiveColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.CompareTag("Player") || other.GetComponent<MetalRebels.Core.Interfaces.IPickupCollector>() != null)
            {
                ActivateCheckpoint();
            }
        }

        public void ActivateCheckpoint()
        {
            isActivated = true;
            LastActiveCheckpointPosition = transform.position + respawnOffset;
            HasActiveCheckpoint = true;

            // Đổi màu trực quan cờ/đèn báo
            if (flagRenderer != null)
            {
                flagRenderer.color = activeColor;
            }

            // Âm thanh kích hoạt
            if (activateSound != null)
            {
                AudioSource.PlayClipAtPoint(activateSound, transform.position);
            }

            // Hiệu ứng particle
            if (activateEffectPrefab != null)
            {
                Instantiate(activateEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Reset checkpoint tĩnh khi bắt đầu màn chơi mới.
        /// </summary>
        public static void ResetCheckpoint()
        {
            HasActiveCheckpoint = false;
            LastActiveCheckpointPosition = Vector3.zero;
        }
    }
}
