using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
using UnityEngine;

// Gắn lên prefab tia laser. Mỗi người chơi chỉ nhận sát thương một lần mỗi tia.
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Boss2LaserBeam : MonoBehaviour
{
    [Min(0)] public int damage = 45;

    [Header("Collider tự khớp theo sprite đang hiển thị")]
    public bool autoFitCollider = true;
    [Tooltip("Sprite thấp hơn mức này (unit) thì collider coi như chưa có, tránh trúng đòn ở frame vệt mảnh đầu clip.")]
    [Min(0f)] public float minDamageHeight = 0.8f;

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
    private BoxCollider2D box;
    private SpriteRenderer sprite;
    private Sprite lastSprite;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        box = col as BoxCollider2D;
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (autoFitCollider && box == null)
            Debug.LogWarning("Boss2LaserBeam: autoFitCollider cần Box Collider 2D.", this);
    }

    // Animator đổi sprite trong Update, nên khớp lại ở LateUpdate.
    void LateUpdate()
    {
        if (!autoFitCollider || box == null || sprite == null || sprite.sprite == lastSprite) return;
        lastSprite = sprite.sprite;
        if (lastSprite == null) { box.enabled = false; return; }

        // bounds đã tính theo pivot, nên Top-pivot cho offset.y âm đúng một nửa chiều cao.
        Bounds b = lastSprite.bounds;
        box.enabled = b.size.y >= minDamageHeight;
        box.size = b.size;
        box.offset = b.center;
    }

    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other); }

    void TryHit(Collider2D other)
    {
        bool isPlayer = false;
        for (Transform t = other.transform; t != null; t = t.parent)
            isPlayer |= t.CompareTag("Player");
        if (!isPlayer) return;

        IDamageable receiver = other.GetComponentInParent<IDamageable>();
        if (receiver == null || !alreadyHit.Add(receiver)) return;
        receiver.TakeDamage(Mathf.Max(0, damage));
    }
}
