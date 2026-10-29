using System.Collections;
using UnityEngine;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Rào chắn vật lý hai đầu khu vực Event Lock, ngăn người chơi đi tiếp
    /// hoặc quay lại cho đến khi tiêu diệt sạch các đợt quái.
    /// </summary>
    public class EventLockBarrier : MonoBehaviour
    {
        [Header("Thành phần")]
        [SerializeField] private Collider2D barrierCollider;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Hiệu ứng chuyển động (Trồi lên / Hạ xuống)")]
        [SerializeField] private bool useMovementAnimation = true;
        [SerializeField] private float moveDistance = 3f;
        [SerializeField] private float moveDuration = 0.5f;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private Coroutine moveCoroutine;

        private void Awake()
        {
            if (barrierCollider == null) barrierCollider = GetComponent<Collider2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            closedPosition = transform.position;
            openPosition = closedPosition - new Vector3(0f, moveDistance, 0f);

            // Khởi đầu ở trạng thái mở
            SetOpenImmediate();
        }

        public void Close()
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            if (useMovementAnimation && gameObject.activeInHierarchy)
            {
                moveCoroutine = StartCoroutine(MoveRoutine(closedPosition, true));
            }
            else
            {
                SetClosedImmediate();
            }
        }

        public void Open()
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            if (useMovementAnimation && gameObject.activeInHierarchy)
            {
                moveCoroutine = StartCoroutine(MoveRoutine(openPosition, false));
            }
            else
            {
                SetOpenImmediate();
            }
        }

        private void SetClosedImmediate()
        {
            transform.position = closedPosition;
            if (barrierCollider != null) barrierCollider.enabled = true;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }

        private void SetOpenImmediate()
        {
            transform.position = openPosition;
            if (barrierCollider != null) barrierCollider.enabled = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }

        private IEnumerator MoveRoutine(Vector3 targetPos, bool enableColliderOnFinish)
        {
            if (enableColliderOnFinish && barrierCollider != null) barrierCollider.enabled = true;
            if (spriteRenderer != null) spriteRenderer.enabled = true;

            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            if (!enableColliderOnFinish && barrierCollider != null) barrierCollider.enabled = false;
            if (!enableColliderOnFinish && spriteRenderer != null) spriteRenderer.enabled = false;

            moveCoroutine = null;
        }
    }
}

