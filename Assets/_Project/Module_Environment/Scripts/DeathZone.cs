using UnityEngine;
using MetalRebels.Core.Interfaces;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Vực chết hoặc khu vực nguy hiểm ở đáy vực. Khi người chơi hoặc quái rơi xuống,
    /// ngay lập tức xử lý nhận sát thương chí tử để hồi sinh tại Checkpoint hoặc bị tiêu diệt.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        [Header("Sát thương rơi vực")]
        [SerializeField] private int lethalDamage = 9999;

        [Header("Âm thanh rơi vực")]
        [SerializeField] private AudioClip fallSound;

        private void Start()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                if (fallSound != null)
                {
                    AudioSource.PlayClipAtPoint(fallSound, other.transform.position);
                }

                damageable.TakeDamage(lethalDamage);
            }
            else
            {
                // Nếu là một vật thể rơi tự do không có máu thì tự hủy
                if (!other.CompareTag("MainCamera"))
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}

