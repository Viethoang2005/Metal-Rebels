using System.Collections;
using UnityEngine;

// Lõi boss 2. Phase 2: lõi đỏ lên, nhấp nháy, và có đòn bắn tia laser tỏa tròn từ lõi.
// Boss2AI gọi EnterPhase2() khi đổi phase và chờ BurstRoutine() khi chọn đòn này.
[DisallowMultipleComponent]
[RequireComponent(typeof(Boss2AI))]
public class Boss2Core : MonoBehaviour
{
    [Header("Lõi")]
    [Tooltip("SpriteRenderer của child Core (đặt Sorting Order cao hơn Visual và Cannon).")]
    public SpriteRenderer coreRenderer;
    [Tooltip("Sprite lõi đỏ dùng cho phase 2. Trống = giữ sprite cũ, chỉ nhuộm màu.")]
    public Sprite phase2Sprite;
    [Tooltip("Màu nhuộm khi nhấp nháy ở phase 2.")]
    public Color phase2Tint = new Color(1f, 0.35f, 0.3f);
    [Min(0f)] public float pulseSpeed = 6f;
    [Tooltip("Nhấp nháy nhanh hơn trong lúc nạp đòn.")]
    [Min(0f)] public float chargePulseSpeed = 25f;
    [Tooltip("Bật: phase 1 ẩn child Core (thấy lõi gốc trong sprite thân boss), sang phase 2 mới hiện lõi đỏ.")]
    public bool hideUntilPhase2 = true;

    [Header("Đòn tia tỏa tròn")]
    [Tooltip("Prefab tia: sprite hướng sang PHẢI, Collider2D (Is Trigger), Rigidbody2D (Kinematic), EnemyBullet.")]
    public EnemyBullet laserProjectilePrefab;
    [Tooltip("Điểm bắn ra. Trống = vị trí coreRenderer.")]
    public Transform firePoint;
    [Min(1)] public int raysPerVolley = 8;
    [Min(1)] public int volleys = 3;
    [Tooltip("Thời gian nạp (lõi nháy nhanh) trước mỗi loạt.")]
    [Min(0.1f)] public float chargeTime = 0.6f;
    [Min(0f)] public float volleyGap = 0.4f;
    [Tooltip("Xoay thêm mỗi loạt (độ). Bằng 0 thì loạt lẻ lệch nửa khoảng để tạo khe lách.")]
    public float rotatePerVolley = 0f;
    [Tooltip("Bỏ các tia chĩa lên trên (vô dụng vì Marco ở dưới). Chỉ bắn trong nửa dưới + ngang.")]
    public bool onlyDownward = false;
    [Min(0)] public int damage = 30;
    [Min(0.1f)] public float projectileSpeed = 7f;

    [Header("FX (tùy chọn)")]
    public GameObject chargeEffectPrefab;
    public GameObject phase2EffectPrefab;

    public bool IsPhase2 { get; private set; }
    private Color baseColor = Color.white;
    private bool charging;

    void Awake()
    {
        if (coreRenderer != null)
        {
            baseColor = coreRenderer.color;
            if (hideUntilPhase2) coreRenderer.enabled = false;
        }
        else Debug.LogWarning("Boss2Core chưa gán Core Renderer.", this);
    }

    public void EnterPhase2()
    {
        IsPhase2 = true;
        if (coreRenderer == null) return;
        coreRenderer.enabled = true;
        if (phase2Sprite != null) coreRenderer.sprite = phase2Sprite;
        if (phase2EffectPrefab != null)
        {
            GameObject fx = Instantiate(phase2EffectPrefab, coreRenderer.transform.position, Quaternion.identity);
            if (fx.GetComponent<AutoDestroyFX>() == null) Destroy(fx, 2f);
        }
    }

    void Update()
    {
        if (!IsPhase2 || coreRenderer == null) return;
        float speed = charging ? chargePulseSpeed : pulseSpeed;
        float k = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
        coreRenderer.color = Color.Lerp(baseColor, phase2Tint, k);
    }

    Vector3 Origin
    {
        get
        {
            if (firePoint != null) return firePoint.position;
            return coreRenderer != null ? coreRenderer.transform.position : transform.position;
        }
    }

    public IEnumerator BurstRoutine()
    {
        if (laserProjectilePrefab == null)
        {
            Debug.LogWarning("Boss2Core chưa gán Laser Projectile Prefab nên không bắn được.", this);
            yield break;
        }

        int rays = Mathf.Max(1, raysPerVolley);
        float step = (onlyDownward ? 180f : 360f) / (onlyDownward ? Mathf.Max(1, rays - 1) : rays);

        for (int v = 0; v < volleys; v++)
        {
            charging = true;
            if (chargeEffectPrefab != null)
            {
                GameObject fx = Instantiate(chargeEffectPrefab, Origin, Quaternion.identity, coreRenderer != null ? coreRenderer.transform : transform);
                if (fx.GetComponent<AutoDestroyFX>() == null) Destroy(fx, chargeTime);
            }
            yield return new WaitForSeconds(chargeTime);
            charging = false;

            float offset = rotatePerVolley != 0f ? v * rotatePerVolley : (v % 2 == 0 ? 0f : step * 0.5f);
            float baseAngle = onlyDownward ? 180f : 0f; // nửa dưới: từ 180° (trái) quét tới 360° (phải)
            Vector3 origin = Origin;
            for (int i = 0; i < rays; i++)
            {
                float deg = baseAngle + offset + i * step;
                if (onlyDownward && deg > 360.01f) continue; // loạt lệch: bỏ tia vượt quá phương ngang phải
                float rad = deg * Mathf.Deg2Rad;
                EnemyBullet bullet = Instantiate(laserProjectilePrefab, origin, Quaternion.identity);
                bullet.damage = damage;
                bullet.speed = projectileSpeed;
                bullet.SetDirection(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
            }
            yield return new WaitForSeconds(volleyGap);
        }
    }

    public void CancelAll()
    {
        StopAllCoroutines();
        charging = false;
    }

    void OnDisable() { charging = false; }
}
