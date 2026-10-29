using UnityEngine;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Điều khiển Camera 2D theo dõi nhân vật chính (Marco) với độ mượt,
    /// hỗ trợ giới hạn ranh giới màn chơi và khóa vị trí khi Event Lock hoặc Boss Arena.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Mục tiêu theo dõi")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f);
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Giới hạn ranh giới (Map Bounds)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 10f);

        [Header("Trạng thái Khóa Camera")]
        [SerializeField] private bool isLocked = false;
        private Vector3 lockPosition;
        private float lockSmoothTime = 0.3f;

        private Vector3 currentVelocity;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void LateUpdate()
        {
            Vector3 destination;

            if (isLocked)
            {
                destination = new Vector3(lockPosition.x, lockPosition.y, offset.z);
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref currentVelocity, lockSmoothTime);
                return;
            }

            if (target == null)
            {
                // Thử tìm Player theo Tag nếu target bị mất hoặc vừa spawn
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    return;
                }
            }

            destination = target.position + offset;

            // Áp dụng giới hạn ranh giới màn chơi nếu được bật
            if (useBounds && cam != null)
            {
                float vertExtent = cam.orthographicSize;
                float horzExtent = vertExtent * cam.aspect;

                float clampedX = Mathf.Clamp(destination.x, minBounds.x + horzExtent, maxBounds.x - horzExtent);
                float clampedY = Mathf.Clamp(destination.y, minBounds.y + vertExtent, maxBounds.y - vertExtent);

                destination = new Vector3(clampedX, clampedY, offset.z);
            }
            else
            {
                destination.z = offset.z;
            }

            transform.position = Vector3.SmoothDamp(transform.position, destination, ref currentVelocity, smoothTime);
        }

        /// <summary>
        /// Khóa Camera tại một tọa độ cố định (Dùng cho Event Lock và Boss Arena).
        /// </summary>
        public void LockAt(Vector3 position, float customSmoothTime = 0.3f)
        {
            isLocked = true;
            lockPosition = position;
            lockSmoothTime = customSmoothTime;
        }

        /// <summary>
        /// Mở khóa Camera để tiếp tục theo dõi nhân vật Marco.
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
        }

        /// <summary>
        /// Gán mục tiêu theo dõi mới (sau khi hồi sinh hoặc chuyển đổi đối tượng).
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Cập nhật ranh giới map mới (ví dụ khi sang khu vực khác).
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}

