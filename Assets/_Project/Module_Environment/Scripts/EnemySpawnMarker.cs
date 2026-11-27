using UnityEngine;

namespace MetalRebels.Module_Environment
{
    public enum EnemySpawnCategory
    {
        Infantry,   // Lính bộ của Black Viper (đặt đâu cũng được)
        Motorbike,  // Lính xe của Black Viper (chạy từ phải qua trái)
        Tank,       // Tank (bên phải khi mở, cả 2 phía khi khóa màn hình)
        Boss        // Boss cuối màn trong vùng khóa
    }

    /// <summary>
    /// Điểm đánh dấu vị trí xuất hiện của địch (Creep & Boss) trên bản đồ.
    /// Khi gộp nhánh Module_Enemies, chỉ cần kéo Prefab địch vào ô enemyPrefab
    /// (hoặc gắn vào WaveManager nếu thuộc vùng Event Lock).
    /// </summary>
    public class EnemySpawnMarker : MonoBehaviour
    {
        [Header("Loại địch & Hướng di chuyển")]
        [SerializeField] private EnemySpawnCategory enemyType = EnemySpawnCategory.Infantry;
        [SerializeField] private Vector2 moveDirection = Vector2.left;
        [SerializeField] private bool isInsideLockZone = false;

        [Header("Prefab địch (Kéo Prefab từ Module_Enemies vào đây khi gộp code)")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private bool autoSpawnOnStart = true;

        public EnemySpawnCategory EnemyType => enemyType;
        public Vector2 MoveDirection => moveDirection;
        public bool IsInsideLockZone => isInsideLockZone;

        private void Start()
        {
            if (autoSpawnOnStart && !isInsideLockZone && enemyPrefab != null)
            {
                Instantiate(enemyPrefab, transform.position, transform.rotation);
            }
        }

        private void OnDrawGizmos()
        {
            Color gizmoColor = enemyType switch
            {
                EnemySpawnCategory.Infantry => new Color(0.2f, 0.9f, 0.2f, 0.85f),
                EnemySpawnCategory.Motorbike => new Color(1f, 0.85f, 0.1f, 0.9f),
                EnemySpawnCategory.Tank => new Color(1f, 0.45f, 0.1f, 0.9f),
                EnemySpawnCategory.Boss => new Color(0.95f, 0.15f, 0.15f, 0.95f),
                _ => Color.white
            };

            Gizmos.color = gizmoColor;
            float radius = enemyType == EnemySpawnCategory.Boss ? 0.85f : 0.45f;
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.DrawWireCube(transform.position, Vector3.one * (radius * 2.1f));

            Vector3 dir = new Vector3(moveDirection.x, moveDirection.y, 0f).normalized;
            if (dir.sqrMagnitude > 0.01f)
            {
                Vector3 end = transform.position + dir * 1.4f;
                Gizmos.DrawLine(transform.position, end);
                Gizmos.DrawSphere(end, 0.12f);
            }

#if UNITY_EDITOR
            string lockTag = isInsideLockZone ? " [LOCK]" : "";
            string dirTag = enemyType == EnemySpawnCategory.Infantry ? "" : (moveDirection.x < 0 ? " (<-)" : " (->)");
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (radius + 0.35f),
                $"{enemyType}{dirTag}{lockTag}"
            );
#endif
        }
    }
}
