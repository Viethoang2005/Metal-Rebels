using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Cấu hình Checkpoint")]
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.green;

        private SpriteRenderer spriteRenderer;
        private bool isActivated = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = inactiveColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isActivated) return;

            PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                isActivated = true;
                player.SetCheckpoint(transform.position);

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = activeColor;
                }

                Debug.Log($"<color=green>[Checkpoint]</color> Đã kích hoạt điểm hồi sinh mới tại: {transform.position}");
            }
        }
    }
}
