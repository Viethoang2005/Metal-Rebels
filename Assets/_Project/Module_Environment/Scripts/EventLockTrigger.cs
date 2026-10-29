using UnityEngine;
using MetalRebels.Core.Events;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Vùng kích hoạt sự kiện Khóa màn hình (Event Lock).
    /// Khi Marco bước vào: khóa camera, dựng rào chắn 2 đầu, phát GameEvents.OnEventLockStart
    /// và kích hoạt các đợt quái tràn ra. Khi diệt hết quái: mở rào và mở camera.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EventLockTrigger : MonoBehaviour
    {
        [Header("Thiết lập Camera")]
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private Transform cameraLockPoint; // Vị trí camera cố định khi khóa màn

        [Header("Rào chắn")]
        [SerializeField] private EventLockBarrier leftBarrier;
        [SerializeField] private EventLockBarrier rightBarrier;

        [Header("Bộ quản lý Đợt quái (Wave Manager)")]
        [SerializeField] private WaveManager waveManager;

        private bool isTriggered = false;

        private void Start()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
            }

            if (waveManager != null)
            {
                waveManager.OnAllWavesCleared += HandleWavesCleared;
            }
        }

        private void OnDestroy()
        {
            if (waveManager != null)
            {
                waveManager.OnAllWavesCleared -= HandleWavesCleared;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<MetalRebels.Core.Interfaces.IPickupCollector>() != null)
            {
                StartEventLock();
            }
        }

        private void StartEventLock()
        {
            isTriggered = true;

            // Khóa Camera
            if (cameraFollow != null)
            {
                Vector3 lockPos = cameraLockPoint != null ? cameraLockPoint.position : transform.position;
                cameraFollow.LockAt(lockPos);
            }

            // Đóng rào chắn
            if (leftBarrier != null) leftBarrier.Close();
            if (rightBarrier != null) rightBarrier.Close();

            // Phát thông báo toàn cục theo hợp đồng
            GameEvents.OnEventLockStart?.Invoke();

            // Kích hoạt các đợt quái
            if (waveManager != null)
            {
                waveManager.StartWaves();
            }
            else
            {
                // Nếu không có waveManager thì tự mở sau vài giây
                Invoke(nameof(HandleWavesCleared), 2f);
            }
        }

        private void HandleWavesCleared()
        {
            // Mở rào chắn
            if (leftBarrier != null) leftBarrier.Open();
            if (rightBarrier != null) rightBarrier.Open();

            // Mở khóa camera
            if (cameraFollow != null)
            {
                cameraFollow.Unlock();
            }

            // Phát thông báo hoàn tất khu vực theo hợp đồng
            GameEvents.OnEventLockCleared?.Invoke();

            // Tắt collider để không kích hoạt lại
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}

