using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông số HP (Lính: 30, Xe: 70, Boss1: 600, Boss2: 900, Titan: 1500)")]
    public int maxHP = 30;
    public int currentHP;

    [Header("Hiệu ứng nổ & Rơi vật phẩm khi chết")]
    public GameObject deathEffectPrefab;
    public GameObject[] dropItems;
    [Range(0f, 100f)] public float dropRate = 25f;

    public event Action<float> OnHealthPercentChanged;
    public event Action OnEnemyDied;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
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

        currentHP -= damage;
        OnHealthPercentChanged?.Invoke((float)currentHP / maxHP);

        if (spriteRenderer != null) StartCoroutine(FlashRed());

        if (currentHP <= 0) Die();
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        OnEnemyDied?.Invoke();

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (dropItems.Length > 0 && UnityEngine.Random.Range(0f, 100f) <= dropRate)
        {
            int idx = UnityEngine.Random.Range(0, dropItems.Length);
            Instantiate(dropItems[idx], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
