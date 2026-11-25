using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông số HP (Lính: 30, Xe: 70, Tank: 120, Boss1: 600, Boss2: 900, Titan: 1500)")]
    public int maxHP = 30;
    public int currentHP;

    [Header("Hiệu ứng nổ & Rơi vật phẩm khi chết")]
    public GameObject deathEffectPrefab;
    public GameObject wreckPrefab;
    [Tooltip("Dành cho Boss1: bật nếu hình xác nhìn sang trái khi Scale X của root dương. Tính cả Flip X đã đặt trong prefab.")]
    public bool wreckFacesLeft;
    public GameObject[] dropItems;
    [Range(0f, 100f)] public float dropRate = 25f;

    public event Action<float> OnHealthPercentChanged;
    public event Action OnEnemyDied;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashRoutine;
    private bool bossLooksRight;
    private bool deathCompleted;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        // Boss 2 có thể kiểm tra xem có nhận sát thương không (ví dụ: chỉ khi nắp mở)
        Boss2AI boss2 = GetComponent<Boss2AI>();
        if (boss2 != null && !boss2.CanTakeDamage()) return;

        // Boss 3 bất tử trong lúc chuyển phase.
        Boss3AI boss3 = GetComponent<Boss3AI>();
        if (boss3 != null && !boss3.CanTakeDamage()) return;

        currentHP -= damage;
        OnHealthPercentChanged?.Invoke((float)currentHP / maxHP);

        if (spriteRenderer != null)
        {
            StopDamageFlash();
            flashRoutine = StartCoroutine(FlashRed());
        }

        if (currentHP <= 0) Die();
    }

#if UNITY_EDITOR
    [ContextMenu("Test/Take 100 Damage (Play Mode)")]
    private void TestDamage()
    {
        if (!CanTestDamage()) return;
        TakeDamage(100);
    }

    [ContextMenu("Test/Kill Enemy (Play Mode)")]
    private void TestDeath()
    {
        if (!CanTestDamage()) return;
        TakeDamage(currentHP);
    }

    private bool CanTestDamage()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || !gameObject.scene.IsValid())
        {
            Debug.LogWarning("Hãy bấm Play và chọn enemy đang hoạt động trong Hierarchy để thử sát thương.", this);
            return false;
        }
        if (currentHP <= 0)
        {
            Debug.LogWarning("Current HP phải lớn hơn 0. Đặt lại HP hoặc chạy lại Play trước khi thử.", this);
            return false;
        }
        return true;
    }
#endif

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    public void StopDamageFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        // Chốt hướng trước khi các callback chết thay đổi animation hoặc object.
        Boss1AI boss = GetComponent<Boss1AI>();
        bossLooksRight = boss != null &&
            ((transform.TransformVector(Vector3.right).x >= 0f) != boss.spriteFacesLeft);
        OnEnemyDied?.Invoke();

        // Let Boss 3 stop its attacks before EnemyHealth creates the death FX and wreck.
        Boss3AI boss3 = GetComponent<Boss3AI>();
        if (boss3 != null && boss3.isActiveAndEnabled)
            boss3.HandleDefeated();
        CompleteDeath();
    }

    // Nổ, để xác, rơi đồ rồi xóa enemy.
    public void CompleteDeath()
    {
        if (deathCompleted) return;
        deathCompleted = true;
        Boss1AI boss = GetComponent<Boss1AI>();

        // Tạo hiệu ứng nổ.
        if (deathEffectPrefab != null)
        {
            Instantiate(
                deathEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Giữ kích thước prefab xác, chỉ đổi hướng cho Boss1.
        if (wreckPrefab != null)
        {
            GameObject wreck = Instantiate(
                wreckPrefab,
                transform.position,
                Quaternion.identity
            );
            if (boss != null)
            {
                Vector3 scale = wreck.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (bossLooksRight != wreckFacesLeft ? 1f : -1f);
                wreck.transform.localScale = scale;
            }
        }

        // Rơi vật phẩm theo tỉ lệ.
        if (dropItems != null &&
            dropItems.Length > 0 &&
            UnityEngine.Random.Range(0f, 100f) < dropRate)
        {
            int index = UnityEngine.Random.Range(0, dropItems.Length);

            if (dropItems[index] != null)
            {
                Instantiate(
                    dropItems[index],
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        // Xóa enemy gốc, giữ các hiệu ứng vừa tạo.
        Destroy(gameObject);
    }
}
