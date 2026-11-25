using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(Boss3AI))]
public class Boss3MissileBarrage : MonoBehaviour
{
    [Header("Bệ phóng")]
    [Tooltip("Các ống phóng trên vai/lưng. Dùng xoay vòng. Trống = vị trí boss.")]
    public Transform[] launchPoints;
    public Boss3Missile missilePrefab;

    [Header("Báo hiệu")]
    [Min(0.1f)] public float telegraphTime = 0.6f;
    [Tooltip("Khói xả nắp, sinh tại mỗi ống phóng.")]
    public GameObject hatchSmokePrefab;
    [Tooltip("Dấu cảnh báo tại điểm rơi (hiện từ lúc phóng tới khi nổ).")]
    public GameObject groundWarningPrefab;

    [Header("Số lượng & rải điểm rơi")]
    [Min(1)] public int missilesPhase1 = 3;
    [Min(1)] public int missilesPhase2 = 5;
    [Tooltip("Khoảng cách mong muốn giữa hai điểm rơi liền kề.")]
    [Min(0f)] public float spacingPhase1 = 3.5f;
    [Min(0f)] public float spacingPhase2 = 3f;
    [Tooltip("Khe trống tối thiểu giữa hai vùng nổ (≥ bề ngang Marco).")]
    [Min(0f)] public float minSafeGap = 1f;
    [Tooltip("Lệch ngẫu nhiên tối đa mỗi điểm rơi (tự thu nhỏ để không phá khe trống).")]
    [Min(0f)] public float jitter = 0.5f;

    [Header("Nhịp phóng / rơi")]
    [Min(0f)] public float launchInterval = 0.15f;
    [Tooltip("Tính từ lúc phóng quả đầu tới lúc nó bắt đầu rơi (không sớm hơn lúc lên tới trần).")]
    [Min(0f)] public float firstDropDelay = 1f;
    [Tooltip("Các quả rơi lần lượt cách nhau khoảng này (thứ tự ngẫu nhiên).")]
    [Min(0f)] public float dropInterval = 0.25f;
    [Tooltip("Khoảng nhô lên khỏi mép trên camera, nơi tên lửa quay đầu.")]
    [Min(0f)] public float ceilingMargin = 2f;
    [Min(0f)] public float recoveryTime = 0.5f;

    [Header("Sát thương")]
    [Min(0)] public int damage = 35;
    public LayerMask groundMask = ~0;

    private Animator anim;
    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly List<float> targetXs = new List<float>();

    // Animator (tùy chọn): Bool isHatchOpen
    private static readonly int HatchParam = Animator.StringToHash("isHatchOpen");
    private bool hasHatch;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        hasHatch = Boss3AI.HasParam(anim, HatchParam, AnimatorControllerParameterType.Bool);
        if (missilePrefab == null)
            Debug.LogWarning("Boss3MissileBarrage chưa gán Missile Prefab.", this);
    }

    public IEnumerator FireRoutine(Boss3AI ai, bool phase2)
    {
        float speed = ai.ActionSpeed;

        // --- Báo hiệu ---
        SetHatch(true);
        if (hatchSmokePrefab != null)
        {
            if (launchPoints != null && launchPoints.Length > 0)
            {
                foreach (Transform p in launchPoints)
                    if (p != null) Track(Boss3AI.SpawnFx(hatchSmokePrefab, p.position, p, 2f));
            }
            else Track(Boss3AI.SpawnFx(hatchSmokePrefab, transform.position, transform, 2f));
        }
        yield return new WaitForSeconds(Mathf.Max(0.1f, telegraphTime) / speed);

        // --- Ra đòn --- (khóa vị trí Marco ngay lúc phóng)
        if (missilePrefab != null && ai.HasTarget)
        {
            int count = Mathf.Max(1, phase2 ? missilesPhase2 : missilesPhase1);
            float spacing = phase2 ? spacingPhase2 : spacingPhase1;
            float ceilingY = ResolveCeilingY();
            PickTargets(ai.target.position.x, count, spacing);

            // Thứ tự rơi ngẫu nhiên để người chơi phải đọc từng dấu cảnh báo.
            List<int> order = new List<int>();
            for (int i = 0; i < count; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = order[i]; order[i] = order[j]; order[j] = tmp;
            }

            float fallbackY = ai.target.position.y;
            for (int i = 0; i < count; i++)
            {
                Vector3 origin = LaunchPosition(i);
                float x = targetXs[order[i]];
                Vector2 ground = new Vector2(x, FindGroundY(x, ceilingY, fallbackY));
                float dropDelay = firstDropDelay + i * dropInterval - i * launchInterval;

                Boss3Missile missile = Instantiate(missilePrefab, origin, Quaternion.identity);
                missile.Launch(ground, ceilingY, Mathf.Max(0f, dropDelay), damage, groundWarningPrefab, transform);
                if (launchInterval > 0f && i < count - 1) yield return new WaitForSeconds(launchInterval);
            }
        }

        // --- Hồi chiêu ---
        SetHatch(false);
        if (recoveryTime > 0f) yield return new WaitForSeconds(recoveryTime / speed);
    }

    // Các điểm rơi cách đều quanh Marco + lệch ngẫu nhiên, nhưng luôn chừa khe ≥ minSafeGap.
    void PickTargets(float centerX, int count, float spacing)
    {
        targetXs.Clear();
        float blast = missilePrefab != null ? Mathf.Max(0f, missilePrefab.blastSize.x) : 2f;
        float step = Mathf.Max(spacing, blast + minSafeGap);
        float maxJitter = Mathf.Clamp((step - blast - minSafeGap) * 0.5f, 0f, jitter);

        // Dời cả cụm nửa khoảng ngẫu nhiên: đôi khi Marco đứng ngay khe, đôi khi ngay điểm rơi.
        float shift = Random.Range(-0.5f, 0.5f) * step;
        float start = centerX + shift - step * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
            targetXs.Add(start + i * step + Random.Range(-maxJitter, maxJitter));
    }

    Vector3 LaunchPosition(int index)
    {
        if (launchPoints != null && launchPoints.Length > 0)
        {
            Transform p = launchPoints[index % launchPoints.Length];
            if (p != null) return p.position;
        }
        return transform.position;
    }

    float ResolveCeilingY()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
            return cam.transform.position.y + cam.orthographicSize + Mathf.Max(0f, ceilingMargin);
        return transform.position.y + 12f;
    }

    float FindGroundY(float x, float fromY, float fallbackY)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, fromY), Vector2.down, 80f, groundMask);
        float best = float.MaxValue;
        float y = fallbackY;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.distance < best && Boss3AI.IsTagged(hit.collider.transform, "Ground"))
            {
                best = hit.distance;
                y = hit.point.y;
            }
        }
        return y;
    }

    void SetHatch(bool open)
    {
        if (anim != null && hasHatch) anim.SetBool(HatchParam, open);
    }

    void Track(GameObject go)
    {
        if (go != null) spawned.Add(go);
    }

    // Tên lửa đã phóng vẫn rơi tiếp (người chơi đã thấy dấu cảnh báo), chỉ dọn nắp và khói.
    public void CancelAll()
    {
        SetHatch(false);
        foreach (GameObject go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }

    void OnDisable()
    {
        CancelAll();
    }
}
