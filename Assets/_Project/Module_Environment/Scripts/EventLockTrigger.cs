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

        [Header("Chế độ Đấu trường Boss")]
        [SerializeField] private bool isBossArena = false;
        [SerializeField] private EnemySpawnMarker bossSpawnMarker;

        [Header("Thời gian khóa màn hình mẫu (khi chưa gắn quái)")]
        [SerializeField] private float fallbackLockDuration = 4f;

        private bool isTriggered = false;
        private bool isLocked = false;
        private float clearBannerTimer = 0f;
        private float fallbackTimer = 0f;

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
            if (isBossArena)
            {
                GameEvents.OnBossKilled += HandleWavesCleared;
            }
        }

        private void Update()
        {
            if (isLocked && fallbackTimer > 0f)
            {
                fallbackTimer -= Time.deltaTime;
                if (fallbackTimer <= 0f)
                {
                    HandleWavesCleared();
                }
            }
            if (clearBannerTimer > 0f)
            {
                clearBannerTimer -= Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            if (waveManager != null)
            {
                waveManager.OnAllWavesCleared -= HandleWavesCleared;
            }
            if (isBossArena)
            {
                GameEvents.OnBossKilled -= HandleWavesCleared;
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
            isLocked = true;

            // Khóa Camera
            Vector3 lockPos = cameraLockPoint != null ? cameraLockPoint.position : transform.position;
            if (cameraFollow != null)
            {
                cameraFollow.LockAt(lockPos);
            }

            // Đóng rào chắn
            if (leftBarrier != null) leftBarrier.Close();
            if (rightBarrier != null) rightBarrier.Close();

            // Phát thông báo toàn cục theo hợp đồng
            GameEvents.OnEventLockStart?.Invoke();

            // Kích hoạt các đợt quái (nếu là Boss Arena thì khóa vĩnh viễn cho đến khi hạ Boss)
            if (isBossArena)
            {
                fallbackTimer = 0f;
                PlayerEvents.TriggerBossBattleChanged(true);
                SpawnArenaBoss();
            }
            else if (waveManager != null && waveManager.HasValidWaves())
            {
                waveManager.SetArenaCenter(lockPos.x);
                waveManager.StartWaves();
            }
            else
            {
                fallbackTimer = fallbackLockDuration;
            }
        }

        private void HandleWavesCleared()
        {
            if (!isLocked) return;
            isLocked = false;
            fallbackTimer = 0f;
            clearBannerTimer = 2f;

            // Mở rào chắn
            if (leftBarrier != null) leftBarrier.Open();
            if (rightBarrier != null) rightBarrier.Open();

            // Mở khóa camera
            if (cameraFollow != null)
            {
                cameraFollow.Unlock();
            }

            // Phát thông báo hoàn tất khu vực theo hợp đồng
            if (isBossArena)
            {
                PlayerEvents.TriggerBossBattleChanged(false);
            }
            GameEvents.OnEventLockCleared?.Invoke();

            // Tắt collider để không kích hoạt lại
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        private void SpawnArenaBoss()
        {
            EnemySpawnMarker marker = bossSpawnMarker;
            if (marker == null)
            {
                foreach (var candidate in FindObjectsByType<EnemySpawnMarker>(FindObjectsSortMode.None))
                {
                    if (candidate.EnemyType == EnemySpawnCategory.Boss)
                    {
                        marker = candidate;
                        break;
                    }
                }
            }

            if (marker != null)
            {
                marker.Spawn();
            }
        }

        private void OnGUI()
        {
            if (!isLocked && clearBannerTimer <= 0f) return;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            if (isLocked)
            {
                GUI.color = new Color(1f, 0.3f, 0.2f);
                string msg = isBossArena
                    ? "⚠ BOSS ARENA LOCKED! HẠ BOSS ĐỂ THOÁT ⚠"
                    : (fallbackTimer > 0f
                        ? $"⚠ AREA LOCKED! (Mở khóa sau {Mathf.CeilToInt(fallbackTimer)}s) ⚠"
                        : "⚠ AREA LOCKED! TIÊU DIỆT HẾT ĐỊCH ĐỂ MỞ ĐƯỜNG ⚠");
                GUI.Box(new Rect((Screen.width - 480) / 2f, 20f, 480f, 40f), msg, style);
            }
            else if (clearBannerTimer > 0f)
            {
                GUI.color = new Color(0.3f, 1f, 0.4f);
                GUI.Box(new Rect((Screen.width - 320) / 2f, 20f, 320f, 40f), "✔ AREA CLEAR! TIẾN LÊN! ✔", style);
            }
            GUI.color = Color.white;
        }
    }
}

