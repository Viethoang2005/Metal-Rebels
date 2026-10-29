using UnityEngine;
using MetalRebels.Core.Events;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Quản lý thiết lập màn chơi của Lê Hữu Hiệu:
    /// Vị trí xuất phát ban đầu của Player, ranh giới map cho Camera, và reset Checkpoint khi bắt đầu màn.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Điểm xuất phát ban đầu")]
        [SerializeField] private Transform initialPlayerSpawnPoint;

        [Header("Camera & Ranh giới màn chơi")]
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private Vector2 levelMinBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 levelMaxBounds = new Vector2(150f, 15f);

        private void Awake()
        {
            // Reset trạng thái Checkpoint cũ khi bắt đầu một màn mới
            Checkpoint.ResetCheckpoint();
        }

        private void Start()
        {
            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetBounds(levelMinBounds, levelMaxBounds);
            }
        }

        /// <summary>
        /// Lấy tọa độ hồi sinh hiện tại: Nếu đã qua Checkpoint thì lấy Checkpoint,
        /// nếu chưa thì lấy điểm xuất phát ban đầu.
        /// </summary>
        public Vector3 GetRespawnPosition()
        {
            if (Checkpoint.HasActiveCheckpoint)
            {
                return Checkpoint.LastActiveCheckpointPosition;
            }

            return initialPlayerSpawnPoint != null ? initialPlayerSpawnPoint.position : Vector3.zero;
        }

        private void OnDrawGizmos()
        {
            // Vẽ ranh giới màn chơi trong Editor
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((levelMinBounds.x + levelMaxBounds.x) * 0.5f, (levelMinBounds.y + levelMaxBounds.y) * 0.5f, 0f);
            Vector3 size = new Vector3(levelMaxBounds.x - levelMinBounds.x, levelMaxBounds.y - levelMinBounds.y, 1f);
            Gizmos.DrawWireCube(center, size);

            if (initialPlayerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(initialPlayerSpawnPoint.position, 0.5f);
            }
        }
    }
}
