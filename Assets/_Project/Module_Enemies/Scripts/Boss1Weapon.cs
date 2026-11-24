using UnityEngine;

// Đặt trên cùng GameObject với Animator để nhận event Shoot.
[DisallowMultipleComponent]
[RequireComponent(typeof(Boss1AI))]
public class Boss1Weapon : MonoBehaviour
{
    [Header("Đạn mẹ tách thành ba viên ở đỉnh quỹ đạo")]
    public Boss1ClusterBullet bulletPrefab;
    public Transform firePoint;

    [Header("Lóe lửa đầu nòng (tùy chọn)")]
    public GameObject muzzleEffectPrefab;
    public Transform muzzleFXPoint;
    [Tooltip("Thời gian dọn hiệu ứng nếu prefab chưa có AutoDestroyFX.")]
    [Min(0.01f)] public float muzzleLifetime = 0.5f;

    private Boss1AI ai;
    private int lastShotFrame = -1;

    void Awake()
    {
        ai = GetComponent<Boss1AI>();
    }

    // Chỉ đặt event này trên Boss1_FireLoop tại frame khai hỏa.
    public void Shoot()
    {
        if (!isActiveAndEnabled || ai == null || !ai.CanShoot()) return;
        if (lastShotFrame == Time.frameCount) return;
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Boss1Weapon chưa gán Bullet Prefab hoặc Fire Point.", this);
            return;
        }

        lastShotFrame = Time.frameCount;
        // Chốt tọa độ hiện tại, cho player cơ hội né sau khi boss bắn.
        Vector2 aimPosition = ai.target.position;
        Boss1ClusterBullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Launch(aimPosition, transform);

        if (muzzleEffectPrefab != null)
        {
            Transform point = muzzleFXPoint != null ? muzzleFXPoint : firePoint;
            // TransformVector tính cả Scale X âm khi boss quay đầu.
            Vector3 direction = point.TransformVector(Vector3.right);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(muzzleEffectPrefab, point.position, Quaternion.Euler(0f, 0f, angle));
            Vector3 scale = point.lossyScale;
            effect.transform.localScale = Vector3.Scale(effect.transform.localScale,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            if (effect.GetComponent<AutoDestroyFX>() == null)
                Destroy(effect, Mathf.Max(0.01f, muzzleLifetime));
        }
    }
}
