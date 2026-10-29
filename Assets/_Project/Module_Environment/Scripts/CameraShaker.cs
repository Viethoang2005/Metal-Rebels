using System.Collections;
using UnityEngine;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Xử lý hiệu ứng rung màn hình (Screen Shake) khi có vụ nổ lớn,
    /// trúng đòn nặng hoặc Boss ra chiêu.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance { get; private set; }

        private Vector3 originalLocalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            originalLocalPos = transform.localPosition;
        }

        /// <summary>
        /// Kích hoạt rung màn hình.
        /// </summary>
        /// <param name="duration">Thời gian rung (giây)</param>
        /// <param name="intensity">Cường độ rung (độ lệch tọa độ)</param>
        public void Shake(float duration = 0.25f, float intensity = 0.3f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(DoShake(duration, intensity));
        }

        /// <summary>
        /// Phương thức tĩnh tiện lợi để gọi từ bất kỳ đâu.
        /// </summary>
        public static void ShakeCurrent(float duration = 0.25f, float intensity = 0.3f)
        {
            if (Instance != null)
            {
                Instance.Shake(duration, intensity);
            }
        }

        private IEnumerator DoShake(float duration, float intensity)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);
                float xOffset = Random.Range(-1f, 1f) * strength;
                float yOffset = Random.Range(-1f, 1f) * strength;

                transform.localPosition = new Vector3(originalLocalPos.x + xOffset, originalLocalPos.y + yOffset, originalLocalPos.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPos;
            shakeCoroutine = null;
        }
    }
}

