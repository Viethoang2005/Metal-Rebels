using UnityEngine;
using MetalRebels.Core.Interfaces;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Đại diện cho một vật phẩm tiếp tế rơi ra từ thùng gỗ, con tin hoặc bố trí trên màn chơi.
    /// Tương tác với nhân vật Marco qua giao diện IPickupCollector.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Loại & Số lượng")]
        [SerializeField] private PickupType pickupType = PickupType.Health;
        [SerializeField] private int amount = 30;

        [Header("Hiệu ứng chuyển động (Bobbing)")]
        [SerializeField] private bool enableBobbing = true;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 3f;

        [Header("Hiệu ứng khi nhặt")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private Vector3 startPosition;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;
            // Đảm bảo Collider là Trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (enableBobbing && !isCollected)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            // Kiểm tra xem đối tượng va chạm có hỗ trợ nhặt vật phẩm không
            IPickupCollector collector = other.GetComponent<IPickupCollector>();
            if (collector == null)
            {
                // Thử tìm trên đối tượng cha nếu collider nằm ở child
                collector = other.GetComponentInParent<IPickupCollector>();
            }

            if (collector != null)
            {
                // Chỉ thu hồi và hủy vật phẩm nếu Player xác nhận tiếp nhận thành công
                bool success = collector.TryCollect(pickupType, amount);
                if (success)
                {
                    OnCollectSuccess();
                }
            }
        }

        private void OnCollectSuccess()
        {
            isCollected = true;

            // Tạo hiệu ứng nhặt (nếu có)
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Phát âm thanh nhặt (nếu có)
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Khởi tạo thông số cho vật phẩm khi được sinh ra tự động từ thùng hoặc con tin.
        /// </summary>
        public void Setup(PickupType type, int itemAmount)
        {
            pickupType = type;
            amount = itemAmount;
        }
    }
}

