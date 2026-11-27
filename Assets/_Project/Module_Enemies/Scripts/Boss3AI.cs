using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class Boss3AI : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform target;
    [Tooltip("Tầm bắt đầu giao chiến. Đã giao chiến thì không thoát dù player ra xa.")]
    [Min(0f)] public float detectRange = 16f;
    [Tooltip("Chờ một nhịp sau khi phát hiện rồi mới ra đòn đầu tiên.")]
    [Min(0f)] public float engageDelay = 1f;

    [Header("Di chuyển (bánh xích, rất chậm)")]
    [Min(0f)] public float moveSpeed = 0.6f;
    [Tooltip("Giữ Marco trong khoảng [min, max] theo trục X: xa hơn thì nhích tới, gần hơn thì lùi.")]
    [Min(0f)] public float preferredMinDistance = 6f;
    [Min(0f)] public float preferredMaxDistance = 10f;
    [Tooltip("Chỉ trượt trong khoảng X ban đầu ± moveBounds.")]
    [Min(0f)] public float moveBounds = 3f;
    [Tooltip("Tắt nếu boss luôn đứng một phía màn hình và không bao giờ quay đầu.")]
    public bool canTurn = true;
    [Tooltip("Hướng ảnh gốc khi Scale X dương và SpriteRenderer Flip X tắt.")]
    public bool spriteFacesLeft = true;
    [Min(0.01f)] public float facingDeadZone = 0.5f;

    [Header("Nhịp tấn công")]
    [Tooltip("Khoảng nghỉ sau mỗi chiêu ở Phase 1 (cửa sổ xả đạn).")]
    [Min(0f)] public float restPhase1 = 2f;
    [Min(0f)] public float restPhase2 = 1f;
    [Tooltip("Player trong tầm này theo trục X tính từ Shock Point thì tung Xung Điện.")]
    [Min(0f)] public float antiMeleeRange = 3.5f;

    [Header("Phase 2 - Quá tải")]
    [Min(1)] public int phase2HP = 450;
    [Tooltip("Boss đứng yên, bất tử trong lúc chuyển phase.")]
    [Min(0f)] public float phaseTransitionTime = 1.5f;
    public GameObject phaseTransitionEffectPrefab;

    [Header("Sự kiện khi bị hạ")]
    [Tooltip("Tùy chọn: gọi sự kiện Mission Complete khi boss bị hạ.")]
    public UnityEvent onDefeated;
    public static event Action<Boss3AI> FinalBossDefeated;

    public bool IsEngaged { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsPhase2 { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool IsDying { get; private set; }
    public bool IsFacingRight { get; private set; }
    public int FacingSign { get { return IsFacingRight ? 1 : -1; } }
    public float ActionSpeed { get { return 1f; } }
    public bool HasTarget { get { return target != null && target.gameObject.activeInHierarchy; } }

    private enum Attack { None, Laser, Missiles, Shock }

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private Boss3EyeLaser eyeLaser;
    private Boss3MissileBarrage missiles;
    private Boss3ShockArm shock;
    private Coroutine combatRoutine;
    private Coroutine runningAttack;
    private float startX;
    private float scaleMagnitude;
    private float nextSearchTime;
    private int cycleIndex;
    private SpriteRenderer[] phaseTransitionRenderers;
    private Color[] phaseTransitionOriginalColors;

    // Animator (tất cả tùy chọn): Bool isMoving, Bool isPhase2
    private static readonly int MovingParam = Animator.StringToHash("isMoving");
    private static readonly int Phase2Param = Animator.StringToHash("isPhase2");
    private static readonly Color Phase2TransitionTint = new Color(0.72f, 0.18f, 1f, 1f);
    private const float Phase2TransitionBlinkInterval = 0.12f;
    private bool hasMoving, hasPhase2;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();
        eyeLaser = GetComponent<Boss3EyeLaser>();
        missiles = GetComponent<Boss3MissileBarrage>();
        shock = GetComponent<Boss3ShockArm>();

        startX = rb.position.x;
        scaleMagnitude = Mathf.Abs(transform.localScale.x);
        IsFacingRight = (transform.TransformVector(Vector3.right).x >= 0f) != spriteFacesLeft;
        if (rb.interpolation == RigidbodyInterpolation2D.None)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (anim != null)
        {
            hasMoving = HasParam(anim, MovingParam, AnimatorControllerParameterType.Bool);
            hasPhase2 = HasParam(anim, Phase2Param, AnimatorControllerParameterType.Bool);
        }

        if (eyeLaser == null && missiles == null)
            Debug.LogWarning("Boss3AI cần ít nhất Boss3EyeLaser hoặc Boss3MissileBarrage trên cùng object.", this);
        if (shock == null)
            Debug.LogWarning("Boss3AI không có Boss3ShockArm: player đứng sát sẽ không bị đẩy lùi.", this);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        RestorePhaseTransitionColors();
        CancelWeapons();
        combatRoutine = null;
        runningAttack = null;
        IsEngaged = false;
        IsAttacking = false;
        IsTransitioning = false;
        SetMoving(false);
    }

    void FixedUpdate()
    {
        if (IsDying || health.currentHP <= 0)
        {
            Move(0f);
            SetMoving(false);
            return;
        }

        if (!IsPhase2 && health.currentHP <= phase2HP)
            StartCoroutine(EnterPhase2());

        if (!HasTarget && Time.time >= nextSearchTime)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            target = found != null ? found.transform : null;
            nextSearchTime = Time.time + 0.5f;
        }
        if (!HasTarget)
        {
            Move(0f);
            SetMoving(false);
            return;
        }

        if (!IsEngaged && Vector2.Distance(rb.position, target.position) <= Mathf.Max(0f, detectRange))
        {
            IsEngaged = true;
            combatRoutine = StartCoroutine(CombatLoop(engageDelay));
        }

        // Khóa hướng và vị trí trong lúc ra đòn để báo hiệu luôn khớp với đòn thật.
        if (!IsEngaged || IsAttacking || IsTransitioning)
        {
            Move(0f);
            SetMoving(false);
            return;
        }

        float dx = target.position.x - rb.position.x;
        if (canTurn && Mathf.Abs(dx) > Mathf.Max(0.01f, facingDeadZone)) Face(dx > 0f);
        TickTreads(dx);
    }

    // Nhích tới/lui để giữ Marco trong tầm bắn ưa thích, không ra khỏi vùng canh giữ.
    void TickTreads(float dx)
    {
        float distance = Mathf.Abs(dx);
        float dir = 0f;
        if (distance > Mathf.Max(preferredMinDistance, preferredMaxDistance)) dir = Mathf.Sign(dx);
        else if (distance < preferredMinDistance) dir = -Mathf.Sign(dx);

        float bounds = Mathf.Max(0f, moveBounds);
        float nextX = Mathf.Clamp(rb.position.x + dir * Mathf.Max(0f, moveSpeed) * Time.fixedDeltaTime,
            startX - bounds, startX + bounds);
        float step = nextX - rb.position.x;
        SetMoving(Mathf.Abs(step) > 0.0001f);
        Move(step);
    }

    IEnumerator CombatLoop(float initialDelay)
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        while (!IsDying && health.currentHP > 0)
        {
            if (!HasTarget) { yield return null; continue; }

            Attack pick;
            if (ShouldShock()) pick = Attack.Shock; // Ngắt chu kỳ, không tăng cycleIndex.
            else
            {
                pick = NextCycleAttack();
                if (pick == Attack.None) { yield return null; continue; }
                cycleIndex++;
            }

            yield return RunAttack(pick);
            yield return Rest();
        }
        combatRoutine = null;
    }

    // Chu kỳ cố định: Laser -> Tên lửa -> Laser ... Thiếu component thì dùng đòn còn lại.
    Attack NextCycleAttack()
    {
        bool laserFirst = cycleIndex % 2 == 0;
        Attack first = laserFirst ? Attack.Laser : Attack.Missiles;
        Attack second = laserFirst ? Attack.Missiles : Attack.Laser;
        if (IsAvailable(first)) return first;
        if (IsAvailable(second)) return second;
        return Attack.None;
    }

    bool IsAvailable(Attack attack)
    {
        switch (attack)
        {
            case Attack.Laser: return eyeLaser != null && eyeLaser.isActiveAndEnabled;
            case Attack.Missiles: return missiles != null && missiles.isActiveAndEnabled;
            case Attack.Shock: return shock != null && shock.isActiveAndEnabled && shock.IsReady;
        }
        return false;
    }

    bool ShouldShock()
    {
        if (!HasTarget || !IsAvailable(Attack.Shock)) return false;

        float originX = shock.armPoint != null ? shock.armPoint.position.x : rb.position.x;
        return Mathf.Abs(target.position.x - originX) <= Mathf.Max(0f, antiMeleeRange);
    }

    IEnumerator RunAttack(Attack attack)
    {
        IsAttacking = true;
        SetMoving(false);
        Move(0f);

        // Quay về phía Marco trước khi khóa hướng cho đòn.
        if (canTurn && HasTarget)
        {
            float dx = target.position.x - rb.position.x;
            if (Mathf.Abs(dx) > 0.01f) Face(dx > 0f);
        }

        IEnumerator routine = null;
        if (attack == Attack.Laser) routine = eyeLaser.FireRoutine(this, IsPhase2);
        else if (attack == Attack.Missiles) routine = missiles.FireRoutine(this, IsPhase2);
        else if (attack == Attack.Shock) routine = shock.FireRoutine(this);

        if (routine != null)
        {
            runningAttack = StartCoroutine(routine);
            yield return runningAttack;
            runningAttack = null;
        }
        IsAttacking = false;
    }

    // Khoảng nghỉ: cửa sổ để người chơi xả đạn. Bị ngắt ngay nếu Marco áp sát.
    IEnumerator Rest()
    {
        float end = Time.time + (IsPhase2 ? restPhase2 : restPhase1);
        while (Time.time < end)
        {
            if (ShouldShock()) yield break;
            yield return null;
        }
    }

    IEnumerator EnterPhase2()
    {
        IsPhase2 = true;
        IsTransitioning = true;
        StopCombat();

        if (anim != null)
        {
            if (hasPhase2) anim.SetBool(Phase2Param, true);
        }

        float duration = Mathf.Max(0f, phaseTransitionTime);
        CapturePhaseTransitionColors();
        SpawnFx(phaseTransitionEffectPrefab, transform.position, null, Mathf.Max(0.1f, duration));

        float elapsed = 0f;
        while (elapsed < duration)
        {
            bool purple = Mathf.FloorToInt(elapsed / Phase2TransitionBlinkInterval) % 2 == 0;
            SetPhaseTransitionColor(purple);
            yield return null;
            elapsed += Time.deltaTime;
        }

        RestorePhaseTransitionColors();

        IsTransitioning = false;
        if (!IsDying && health.currentHP > 0 && IsEngaged)
            combatRoutine = StartCoroutine(CombatLoop(0f));
    }

    // EnemyHealth gọi trước khi tạo death FX/wreck và xóa boss.
    public void HandleDefeated()
    {
        if (IsDying) return;
        IsDying = true;
        StopAllCoroutines();
        RestorePhaseTransitionColors();
        CancelWeapons();
        combatRoutine = null;
        runningAttack = null;
        IsAttacking = false;
        IsTransitioning = false;
        SetMoving(false);
        Move(0f);

        // Tên lửa còn bay sau khi boss chết thì hủy, tránh giết Marco sau khi đã thắng.
        Boss3Missile.ClearAll();
        onDefeated?.Invoke();
        FinalBossDefeated?.Invoke(this);
    }

    void CapturePhaseTransitionColors()
    {
        health.StopDamageFlash();
        phaseTransitionRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        phaseTransitionOriginalColors = new Color[phaseTransitionRenderers.Length];
        for (int i = 0; i < phaseTransitionRenderers.Length; i++)
            if (phaseTransitionRenderers[i] != null)
                phaseTransitionOriginalColors[i] = phaseTransitionRenderers[i].color;
    }

    void SetPhaseTransitionColor(bool purple)
    {
        if (phaseTransitionRenderers == null || phaseTransitionOriginalColors == null) return;

        for (int i = 0; i < phaseTransitionRenderers.Length; i++)
        {
            SpriteRenderer renderer = phaseTransitionRenderers[i];
            if (renderer == null) continue;

            Color original = phaseTransitionOriginalColors[i];
            Color tint = new Color(Phase2TransitionTint.r, Phase2TransitionTint.g,
                Phase2TransitionTint.b, original.a);
            renderer.color = purple ? Color.Lerp(original, tint, 0.85f) : original;
        }
    }

    void RestorePhaseTransitionColors()
    {
        SetPhaseTransitionColor(false);
        phaseTransitionRenderers = null;
        phaseTransitionOriginalColors = null;
    }

    void StopCombat()
    {
        if (combatRoutine != null) StopCoroutine(combatRoutine);
        if (runningAttack != null) StopCoroutine(runningAttack);
        combatRoutine = null;
        runningAttack = null;
        CancelWeapons();
        IsAttacking = false;
        SetMoving(false);
        Move(0f);
    }

    void CancelWeapons()
    {
        if (eyeLaser != null) eyeLaser.CancelAll();
        if (missiles != null) missiles.CancelAll();
        if (shock != null) shock.CancelAll();
    }

    // EnemyHealth gọi: bất tử trong lúc chuyển phase và chuỗi nổ chết.
    public bool CanTakeDamage()
    {
        return !IsTransitioning && !IsDying;
    }

    void Face(bool right)
    {
        if (IsFacingRight == right) return;
        IsFacingRight = right;
        bool parentRight = transform.parent == null || transform.parent.TransformVector(Vector3.right).x >= 0f;
        bool originalWorldRight = (!spriteFacesLeft) == parentRight;
        Vector3 scale = transform.localScale;
        scale.x = right == originalWorldRight ? scaleMagnitude : -scaleMagnitude;
        transform.localScale = scale;
    }

    void Move(float step)
    {
        if (rb == null) return;
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(step / Time.fixedDeltaTime, rb.linearVelocity.y);
        else if (rb.bodyType == RigidbodyType2D.Kinematic && step != 0f)
            rb.MovePosition(rb.position + Vector2.right * step);
    }

    void SetMoving(bool moving)
    {
        if (anim != null && hasMoving) anim.SetBool(MovingParam, moving);
    }

    // lifetime <= 0: không tự hủy (dùng cho FX loop như khói).
    public static GameObject SpawnFx(GameObject prefab, Vector3 position, Transform parent, float lifetime)
    {
        if (prefab == null) return null;
        GameObject fx = Instantiate(prefab, position, prefab.transform.rotation, parent);
        if (lifetime > 0f && fx.GetComponent<AutoDestroyFX>() == null) Destroy(fx, lifetime);
        return fx;
    }

    public static bool HasParam(Animator animator, int hash, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash && p.type == type) return true;
        return false;
    }

    public static bool IsTagged(Transform t, string tag)
    {
        for (; t != null; t = t.parent)
            if (t.CompareTag(tag)) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, detectRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p + Vector3.left * antiMeleeRange, p + Vector3.right * antiMeleeRange);
        Gizmos.color = Color.green;
        float x = Application.isPlaying ? startX : p.x;
        Gizmos.DrawLine(new Vector3(x - moveBounds, p.y), new Vector3(x + moveBounds, p.y));
    }
}
