using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetalRebels.Module_Environment
{
    public enum HostageState
    {
        TiedUp,     // Bị trói, chờ cứu
        Rescued,    // Được cứu, nói lời cảm ơn & nhả đồ tiếp tế
        Fleeing     // Chạy thoát khỏi chiến trường
    }

    /// <summary>
    /// Quản lý cơ chế giải cứu con tin. Người chơi đến gần và bấm nút Interact (phím E)
    /// để giải cứu, nhận vật phẩm thưởng tiếp tế và tích lũy điểm.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HostageController : MonoBehaviour
    {
        [Header("Trạng thái hiện tại")]
        [SerializeField] private HostageState currentState = HostageState.TiedUp;

        [Header("Giao diện nhắc nhở (Prompt UI)")]
        [SerializeField] private GameObject promptUI; // Ví dụ: Text hoặc Icon "[E] Cứu"

        [Header("Vật phẩm thưởng khi được giải cứu")]
        [SerializeField] private GameObject[] rewardItemPrefabs;
        [SerializeField] private Vector3 rewardSpawnOffset = new Vector3(0.5f, 0.2f, 0f);

        [Header("Hiệu ứng & Âm thanh")]
        [SerializeField] private Animator animator;
        [SerializeField] private AudioClip rescueSound;
        [SerializeField] private GameObject rescueEffectPrefab;

        [Header("Hành vi chạy trốn sau khi cứu")]
        [SerializeField] private float fleeSpeed = 3f;
        [SerializeField] private float fleeDuration = 3f;
        [SerializeField] private int fleeDirection = -1; // -1: chạy sang trái, 1: chạy sang phải

        [Header("Input Tương tác")]
        [SerializeField] private InputActionReference interactAction; // Liên kết với Action "Interact" trong Input System

        private bool isPlayerNearby = false;

        private void Start()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (currentState != HostageState.TiedUp || !isPlayerNearby) return;

            // Kiểm tra bấm phím Interact: hỗ trợ cả Unity Input System lẫn fallback phím E
            bool interactPressed = false;
            if (interactAction != null && interactAction.action != null)
            {
                interactPressed = interactAction.action.WasPressedThisFrame();
            }
            else if (Keyboard.current != null)
            {
                interactPressed = Keyboard.current.eKey.wasPressedThisFrame;
            }

            if (interactPressed)
            {
                Rescue();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (currentState != HostageState.TiedUp) return;

            if (other.CompareTag("Player") || other.GetComponent<MetalRebels.Core.Interfaces.IPickupCollector>() != null)
            {
                isPlayerNearby = true;
                if (promptUI != null)
                {
                    promptUI.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<MetalRebels.Core.Interfaces.IPickupCollector>() != null)
            {
                isPlayerNearby = false;
                if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }
            }
        }

        public void Rescue()
        {
            if (currentState != HostageState.TiedUp) return;

            currentState = HostageState.Rescued;

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (animator != null)
            {
                animator.SetTrigger("Rescue");
            }

            if (rescueEffectPrefab != null)
            {
                Instantiate(rescueEffectPrefab, transform.position, Quaternion.identity);
            }

            if (rescueSound != null)
            {
                AudioSource.PlayClipAtPoint(rescueSound, transform.position);
            }

            // Sinh vật phẩm thưởng
            SpawnRewards();

            // Chuyển sang hành vi chạy trốn
            StartCoroutine(FleeRoutine());
        }

        private void SpawnRewards()
        {
            if (rewardItemPrefabs == null || rewardItemPrefabs.Length == 0) return;

            for (int i = 0; i < rewardItemPrefabs.Length; i++)
            {
                if (rewardItemPrefabs[i] != null)
                {
                    Vector3 offset = rewardSpawnOffset + new Vector3(i * 0.4f, 0f, 0f);
                    Instantiate(rewardItemPrefabs[i], transform.position + offset, Quaternion.identity);
                }
            }
        }

        private IEnumerator FleeRoutine()
        {
            yield return new WaitForSeconds(0.8f); // Chờ 1 chút để nói cảm ơn / diễn animation

            currentState = HostageState.Fleeing;
            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
            }

            // Lật mặt theo hướng chạy
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (fleeDirection > 0 ? 1 : -1);
            transform.localScale = scale;

            float elapsed = 0f;
            while (elapsed < fleeDuration)
            {
                transform.Translate(Vector3.right * (fleeDirection * fleeSpeed * Time.deltaTime));
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
