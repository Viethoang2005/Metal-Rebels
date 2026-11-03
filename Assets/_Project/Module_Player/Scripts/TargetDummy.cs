using System.Collections;
using MetalRebels.Core.Interfaces;
using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        [Header("Chỉ số Bia mục tiêu thử nghiệm")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private string targetName = "Target Dummy";
        [SerializeField] private bool autoRespawn = true;
        [SerializeField] private float respawnTime = 2.5f;

        private int currentHP;
        private SpriteRenderer spriteRenderer;
        private Collider2D col;
        private Color originalColor;
        private Vector3 startPosition;

        public int CurrentHP => currentHP;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
            startPosition = transform.position;
        }

        private void Start()
        {
            ResetDummy();
        }

        public void TakeDamage(int damageAmount)
        {
            if (currentHP <= 0) return;

            currentHP = Mathf.Max(0, currentHP - damageAmount);
            Debug.Log($"<color=orange>[{targetName}]</color> Nhận {damageAmount} sát thương! HP còn lại: {currentHP}/{maxHP}");

            StartCoroutine(FlashRed());

            if (currentHP <= 0)
            {
                Die();
            }
        }

        private IEnumerator FlashRed()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                if (currentHP > 0) spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            Debug.Log($"<color=red>[{targetName}]</color> Đã bị tiêu diệt!");
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (col != null) col.enabled = false;

            if (autoRespawn)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnTime);
            ResetDummy();
        }

        public void ResetDummy()
        {
            currentHP = maxHP;
            transform.position = startPosition;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }
            if (col != null) col.enabled = true;
        }

        private void OnGUI()
        {
            if (currentHP <= 0) return;

            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.2f) : Vector3.zero;
            if (screenPos.z > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 25), $"{targetName}: {currentHP}/{maxHP}", style);
            }
        }
    }
}
