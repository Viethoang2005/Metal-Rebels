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
            Collider2D checkpointCollider = GetComponent<Collider2D>();
            if (checkpointCollider != null)
            {
                checkpointCollider.isTrigger = true;
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.color = inactiveColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryActivate(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryActivate(collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryActivate(collision.collider);
        }

        private void TryActivate(Collider2D collision)
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
