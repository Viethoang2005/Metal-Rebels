using System.Collections;
using System.Collections.Generic;
using MetalRebels.Core.Interfaces;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Boss3AI))]
public class Boss3EyeLaser : MonoBehaviour
{
    [Header("Điểm phát")]
    [Tooltip("Các điểm mắt (child của boss để lật theo hướng mặt). Boss 2 mắt thì kéo cả hai vào.")]
    public Transform[] eyePoints;
    [Tooltip("Mỗi phát chọn ngẫu nhiên một mắt. Tắt: luôn bắn từ mắt đầu tiên.")]
    public bool randomizeEye = true;

    [Header("Báo hiệu")]
    [Min(0.1f)] public float telegraphTime = 0.8f;
    [Tooltip("FX mắt chớp sáng, sinh tại eyePoint trong lúc ngắm.")]
    public GameObject eyeFlashPrefab;
    [Tooltip("Tia ngắm mảnh. Trống = tự tạo LineRenderer.")]
    public LineRenderer aimLine;
    [Min(0.01f)] public float aimWidth = 0.06f;
    public Color aimColor = new Color(1f, 0.25f, 0.2f, 0.85f);
    [Tooltip("Tia ngắm nhấp nháy bao nhiêu lần mỗi giây.")]
    [Min(0f)] public float aimBlinkRate = 10f;

    [Header("Tia laser")]
    [Tooltip("Tia chính. Trống = tự tạo LineRenderer.")]
    public LineRenderer beamLine;
    [Tooltip("Độ dày hiển thị và độ dày vùng sát thương.")]
    [Min(0.05f)] public float beamWidth = 0.45f;
    public Color beamColorStart = new Color(0.55f, 1f, 1f, 1f);
    public Color beamColorEnd = new Color(1f, 0.95f, 0.35f, 1f);
    [Min(0)] public int damage = 30;
    [Tooltip("FX tia lửa ở điểm chạm đất, chạy theo đầu tia (prefab loop).")]
    public GameObject groundImpactPrefab;

    [Header("Quét")]
    [Tooltip("Khoảng cách X tính từ boss (theo hướng mặt) tới điểm chạm xa nhất.")]
    [Min(0f)] public float farDistance = 13f;
    [Tooltip("Điểm chạm gần nhất. Đặt > 0 để tia không quét xuyên vào chân boss.")]
    [Min(0f)] public float nearDistance = 2f;
    [Min(0.1f)] public float sweepDuration = 1.4f;
    [Tooltip("Bật: quét từ xa về gần. Tắt: gần ra xa.")]
    public bool sweepFarToNear = true;
    [Tooltip("Mỗi lần bắn chọn ngẫu nhiên hướng quét.")]
    public bool randomizeDirection;
    [Tooltip("Phase 2: nghỉ giữa nhịp quét vào và quét ra.")]
    [Min(0f)] public float phase2SweepGap = 0.2f;
    [Min(0f)] public float recoveryTime = 0.4f;

    [Header("Mặt đất")]
    public LayerMask groundMask = ~0;

    public bool IsFiring { get; private set; }

    // Mắt được chọn cho phát bắn hiện tại, giữ cố định suốt tia ngắm và lúc quét.
    private Transform activeEye;
    private Animator anim;
    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly HashSet<IDamageable> hitThisSweep = new HashSet<IDamageable>();

    // Animator (tùy chọn): Bool isLaserCharging, Bool isLaserFiring
    private static readonly int ChargingParam = Animator.StringToHash("isLaserCharging");
    private static readonly int FiringParam = Animator.StringToHash("isLaserFiring");
    private bool hasCharging, hasFiring;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        hasCharging = Boss3AI.HasParam(anim, ChargingParam, AnimatorControllerParameterType.Bool);
        hasFiring = Boss3AI.HasParam(anim, FiringParam, AnimatorControllerParameterType.Bool);

        if (aimLine == null) aimLine = CreateLine("EyeLaser_Aim", 40);
        if (beamLine == null) beamLine = CreateLine("EyeLaser_Beam", 41);
        aimLine.enabled = false;
        beamLine.enabled = false;
        activeEye = FirstEye();
        if (activeEye == null)
            Debug.LogWarning("Boss3EyeLaser chưa gán Eye Points: tia sẽ phát từ gốc boss.", this);
    }

    public IEnumerator FireRoutine(Boss3AI ai, bool phase2)
    {
        activeEye = PickEye();
        float speed = ai.ActionSpeed;
        int dir = ai.FacingSign;
        float baseX = transform.position.x;
        float groundY = FindGroundY(ai);

        bool farToNear = randomizeDirection ? Random.value < 0.5f : sweepFarToNear;
        float far = baseX + dir * Mathf.Max(farDistance, nearDistance);
        float near = baseX + dir * Mathf.Min(farDistance, nearDistance);
        float fromX = farToNear ? far : near;
        float toX = farToNear ? near : far;

        // --- Báo hiệu ---
        SetAnim(hasCharging, ChargingParam, true);
        GameObject flash = Boss3AI.SpawnFx(eyeFlashPrefab, EyePosition(), activeEye, 0f);
        if (flash != null) spawned.Add(flash);

        aimLine.enabled = true;
        float telegraph = Mathf.Max(0.1f, telegraphTime) / speed;
        for (float t = 0f; t < telegraph; t += Time.deltaTime)
        {
            Vector2 eye = EyePosition();
            DrawLine(aimLine, eye, ResolveEnd(eye, new Vector2(fromX, groundY)), aimWidth);
            aimLine.enabled = aimBlinkRate <= 0f || Mathf.Repeat(t * aimBlinkRate, 1f) < 0.65f;
            yield return null;
        }
        aimLine.enabled = false;
        if (flash != null) { spawned.Remove(flash); Destroy(flash); }
        SetAnim(hasCharging, ChargingParam, false);

        // --- Ra đòn ---
        IsFiring = true;
        SetAnim(hasFiring, FiringParam, true);
        GameObject impact = Boss3AI.SpawnFx(groundImpactPrefab, new Vector3(fromX, groundY), null, 0f);
        if (impact != null) spawned.Add(impact);

        float sweep = Mathf.Max(0.1f, sweepDuration) / speed;
        yield return Sweep(fromX, toX, groundY, sweep, impact);
        if (phase2)
        {
            // Nhịp 2: quét ngược lại, ép người chơi nhảy thêm lần nữa.
            if (phase2SweepGap > 0f) yield return new WaitForSeconds(phase2SweepGap / speed);
            yield return Sweep(toX, fromX, groundY, sweep, impact);
        }

        // --- Hồi chiêu ---
        StopVisuals();
        if (recoveryTime > 0f) yield return new WaitForSeconds(recoveryTime / speed);
    }

    IEnumerator Sweep(float fromX, float toX, float groundY, float duration, GameObject impact)
    {
        hitThisSweep.Clear();
        beamLine.enabled = true;
        for (float t = 0f; ; t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / duration);
            Vector2 eye = EyePosition();
            Vector2 end = ResolveEnd(eye, new Vector2(Mathf.Lerp(fromX, toX, k), groundY));

            // Rung nhẹ độ dày để tia trông "sống".
            float width = beamWidth * (1f + 0.12f * Mathf.Sin(Time.time * 60f));
            DrawLine(beamLine, eye, end, width);
            if (impact != null) impact.transform.position = new Vector3(end.x, end.y, impact.transform.position.z);
            DamageAlong(eye, end);

            if (k >= 1f) break;
            yield return null;
        }
    }

    void DamageAlong(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        float length = delta.magnitude;
        if (length < 0.01f) return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(from, beamWidth * 0.5f, delta / length, length);
        foreach (RaycastHit2D hit in hits)
        {
            if (!Boss3AI.IsTagged(hit.collider.transform, "Player")) continue;
            IDamageable receiver = hit.collider.GetComponentInParent<IDamageable>();
            if (receiver == null || !hitThisSweep.Add(receiver)) continue;
            receiver.TakeDamage(Mathf.Max(0, damage));
        }
    }

    // Dừng tia ở vật cản Ground đầu tiên trên đường đi (bục, dốc...).
    Vector2 ResolveEnd(Vector2 eye, Vector2 desired)
    {
        Vector2 delta = desired - eye;
        float length = delta.magnitude;
        if (length < 0.01f) return desired;

        RaycastHit2D[] hits = Physics2D.RaycastAll(eye, delta / length, length, groundMask);
        float best = length;
        Vector2 end = desired;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.distance < best && hit.distance > 0.05f && Boss3AI.IsTagged(hit.collider.transform, "Ground"))
            {
                best = hit.distance;
                end = hit.point;
            }
        }
        return end;
    }

    float FindGroundY(Boss3AI ai)
    {
        Vector2 origin = EyePosition();
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 50f, groundMask);
        float bestDistance = float.MaxValue;
        float y = float.NaN;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.distance < bestDistance && Boss3AI.IsTagged(hit.collider.transform, "Ground"))
            {
                bestDistance = hit.distance;
                y = hit.point.y;
            }
        }
        if (!float.IsNaN(y)) return y;
        // Không thấy Ground: lấy cao độ chân Marco, cuối cùng là gốc boss.
        return ai.HasTarget ? ai.target.position.y : transform.position.y;
    }

    Vector2 EyePosition()
    {
        return activeEye != null ? (Vector2)activeEye.position : (Vector2)transform.position;
    }

    Transform FirstEye()
    {
        if (eyePoints == null) return null;
        foreach (Transform eye in eyePoints)
            if (eye != null) return eye;
        return null;
    }

    Transform PickEye()
    {
        if (eyePoints == null || eyePoints.Length == 0) return null;
        if (!randomizeEye) return FirstEye();
        for (int attempt = 0; attempt < 4; attempt++)
        {
            Transform eye = eyePoints[Random.Range(0, eyePoints.Length)];
            if (eye != null) return eye;
        }
        return FirstEye();
    }

    void DrawLine(LineRenderer line, Vector2 from, Vector2 to, float width)
    {
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(from.x, from.y, 0f));
        line.SetPosition(1, new Vector3(to.x, to.y, 0f));
        line.startWidth = width;
        line.endWidth = width;
    }

    LineRenderer CreateLine(string objectName, int sortingOrder)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        LineRenderer line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        line.sortingOrder = sortingOrder;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader != null) line.material = new Material(shader);

        bool isAim = objectName.EndsWith("Aim");
        line.startColor = isAim ? aimColor : beamColorStart;
        line.endColor = isAim ? aimColor : beamColorEnd;
        return line;
    }

    void SetAnim(bool has, int hash, bool value)
    {
        if (anim != null && has) anim.SetBool(hash, value);
    }

    void StopVisuals()
    {
        IsFiring = false;
        if (aimLine != null) aimLine.enabled = false;
        if (beamLine != null) beamLine.enabled = false;
        SetAnim(hasCharging, ChargingParam, false);
        SetAnim(hasFiring, FiringParam, false);
        foreach (GameObject go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }

    // Boss3AI gọi khi chuyển phase / chết để tắt tia đang bắn.
    public void CancelAll()
    {
        StopVisuals();
        hitThisSweep.Clear();
    }

    void OnDisable()
    {
        StopVisuals();
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;
        Boss3AI ai = GetComponent<Boss3AI>();
        float dir = ai != null && Application.isPlaying ? ai.FacingSign
            : ((transform.TransformVector(Vector3.right).x >= 0f) != (ai != null && ai.spriteFacesLeft) ? 1f : -1f);
        Gizmos.color = new Color(0.4f, 1f, 1f);
        bool drewEye = false;
        if (eyePoints != null)
        {
            foreach (Transform eyeTransform in eyePoints)
            {
                if (eyeTransform == null) continue;
                drewEye = true;
                Vector3 eye = eyeTransform.position;
                Gizmos.DrawLine(eye, new Vector3(p.x + dir * farDistance, p.y));
                Gizmos.DrawLine(eye, new Vector3(p.x + dir * nearDistance, p.y));
            }
        }
        if (!drewEye)
        {
            Gizmos.DrawLine(p, new Vector3(p.x + dir * farDistance, p.y));
            Gizmos.DrawLine(p, new Vector3(p.x + dir * nearDistance, p.y));
        }
    }
}
