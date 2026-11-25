using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
using UnityEngine;


[DisallowMultipleComponent]
public class Boss3Missile : MonoBehaviour
{
    public static readonly List<Boss3Missile> Active = new List<Boss3Missile>();

    // Hướng mũi nhọn trong ảnh sprite gốc (trước khi object bị xoay).
    public enum SpriteAim { Up = 0, Right = 1, Down = 2, Left = 3 }

    [Header("Bay")]
    [Min(0.1f)] public float ascendSpeed = 16f;
    [Min(0.1f)] public float fallSpeed = 13f;
    [Tooltip("Mũi nhọn của ảnh sprite đang chỉ về đâu khi object chưa xoay. Chọn sai thì tên lửa bay ngược mũi (mũi chỉ lên trong lúc cắm xuống).")]
    public SpriteAim spriteAim = SpriteAim.Up;
    [Min(0.1f)] public float maxLifetime = 10f;

    [Header("Va chạm & Nổ")]
    [Tooltip("Bán kính thân tên lửa dùng để dò va chạm khi rơi.")]
    [Min(0.01f)] public float hitRadius = 0.25f;
    [Tooltip("Vùng nổ lan mặt đất: rộng x cao, đáy đặt tại điểm chạm.")]
    public Vector2 blastSize = new Vector2(2.2f, 1.2f);
    public GameObject explosionPrefab;
    [Tooltip("Chỉ dùng nếu prefab nổ chưa có AutoDestroyFX.")]
    [Min(0.1f)] public float explosionLifetime = 1.5f;

    private int damage;
    private Vector2 groundPoint;
    private float ceilingY;
    private float dropAt;
    private Transform owner;
    private GameObject warning;
    private Renderer[] renderers;
    private bool launched;
    private bool exploded;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    // dropDelay tính từ lúc phóng; tên lửa không rơi trước khi lên tới trần.
    public void Launch(Vector2 groundPoint, float ceilingY, float dropDelay, int damage,
        GameObject warningPrefab, Transform owner)
    {
        if (launched) return;
        launched = true;
        this.groundPoint = groundPoint;
        this.ceilingY = Mathf.Max(ceilingY, transform.position.y + 1f);
        this.damage = damage;
        this.owner = owner;
        dropAt = Time.time + Mathf.Max(0f, dropDelay);

        if (warningPrefab != null)
            warning = Instantiate(warningPrefab, groundPoint, warningPrefab.transform.rotation);

        Destroy(gameObject, Mathf.Max(0.1f, maxLifetime));
        StartCoroutine(Fly());
    }

    IEnumerator Fly()
    {
        // Pha 1: vút thẳng lên khỏi mép trên màn hình.
        SetHeading(Vector2.up);
        while (transform.position.y < ceilingY)
        {
            transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
            yield return null;
        }

        // Pha 2: ẩn ngoài màn hình, chờ tới lượt rơi.
        SetVisible(false);
        while (Time.time < dropAt) yield return null;

        // Pha 3: cắm thẳng xuống điểm đã khóa.
        transform.position = new Vector3(groundPoint.x, ceilingY, transform.position.z);
        SetHeading(Vector2.down);
        SetVisible(true);

        while (!exploded)
        {
            Vector2 pos = transform.position;
            float step = fallSpeed * Time.deltaTime;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(pos, hitRadius, Vector2.down, step);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit2D hit in hits)
            {
                Transform t = hit.collider.transform;
                if (t.IsChildOf(transform) || (owner != null && t.IsChildOf(owner))) continue;
                if (Boss3AI.IsTagged(t, "Player"))
                {
                    Explode(new Vector2(pos.x, Mathf.Max(groundPoint.y, pos.y - hit.distance - hitRadius)));
                    yield break;
                }
                if (Boss3AI.IsTagged(t, "Ground"))
                {
                    Explode(new Vector2(pos.x, hit.point.y));
                    yield break;
                }
            }

            // Lưới an toàn: tới cao độ sàn đã dò thì nổ dù không trúng collider.
            if (pos.y - step <= groundPoint.y)
            {
                Explode(groundPoint);
                yield break;
            }
            transform.position += Vector3.down * step;
            yield return null;
        }
    }

    void Explode(Vector2 point)
    {
        if (exploded) return;
        exploded = true;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, point, explosionPrefab.transform.rotation);
            if (fx.GetComponent<AutoDestroyFX>() == null) Destroy(fx, explosionLifetime);
        }

        // Nổ lan mặt đất: mỗi người chơi chỉ nhận sát thương một lần.
        Vector2 size = new Vector2(Mathf.Max(0.01f, blastSize.x), Mathf.Max(0.01f, blastSize.y));
        Vector2 center = point + Vector2.up * size.y * 0.5f;
        HashSet<IDamageable> hit = new HashSet<IDamageable>();
        foreach (Collider2D col in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            if (!Boss3AI.IsTagged(col.transform, "Player")) continue;
            IDamageable receiver = col.GetComponentInParent<IDamageable>();
            if (receiver != null && hit.Add(receiver)) receiver.TakeDamage(Mathf.Max(0, damage));
        }

        Destroy(gameObject);
    }

    void SetHeading(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - AimAngle(spriteAim));
    }

    // Góc của mũi sprite so với trục +X (độ). Trừ đi góc này để mũi luôn bám theo hướng bay.
    static float AimAngle(SpriteAim aim)
    {
        switch (aim)
        {
            case SpriteAim.Up: return 90f;
            case SpriteAim.Down: return -90f;
            case SpriteAim.Left: return 180f;
            default: return 0f; // Right
        }
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
            if (r != null) r.enabled = visible;
    }

    void OnDestroy()
    {
        if (warning != null) Destroy(warning);
    }

    // Boss chết: hủy mọi tên lửa đang bay mà không gây sát thương.
    public static void ClearAll()
    {
        foreach (Boss3Missile m in Active.ToArray())
            if (m != null) Destroy(m.gameObject);
        Active.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f);
        Vector3 p = launched ? (Vector3)groundPoint : transform.position;
        Gizmos.DrawWireCube(p + Vector3.up * blastSize.y * 0.5f, blastSize);
    }
}
