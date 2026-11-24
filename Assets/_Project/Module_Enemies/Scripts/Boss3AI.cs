using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Boss 3 - Heavy Artillery Mech ("pháo đài di động").
// Phase 1 (100% -> 50%): Laser mắt -> nghỉ 2s -> Mưa tên lửa (3) -> nghỉ 2s.
// Phase 2 (< 50%): nghỉ 1s, hoạt ảnh nhanh 1.25x, laser quét 2 nhịp, 5 tên lửa.
// Player áp sát (< antiMeleeRange) thì ngắt chu kỳ, tung Xung Điện.
// Cần trên cùng object: Boss3EyeLaser, Boss3MissileBarrage, Boss3ShockArm (thiếu cái nào thì bỏ đòn đó).
// Collider nhận đạn: chỉ đặt ở đầu + thân trên (xem Boss3 setup trong mô tả commit / README).
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
    [Tooltip("Player trong tầm này (theo X) thì tung Xung Điện.")]
    [Min(0f)] public float antiMeleeRange = 3.5f;

    [Header("Phase 2 - Quá tải")]
    [Range(0.05f, 1f)] public float phase2HPPercent = 0.5f;
    [Tooltip("Boss đứng yên, bất tử trong lúc chuyển phase.")]
    [Min(0f)] public float phaseTransitionTime = 1.5f;
    [Tooltip("Tốc độ Animator và nhịp báo hiệu ở phase 2 (1.2 - 1.3).")]
    [Range(1f, 2f)] public float phase2ActionSpeed = 1.25f;
    public GameObject transitionExplosionPrefab;
    [Min(0)] public int transitionExplosions = 3;
    [Tooltip("Nhuộm màu nếu Animator không có Bool isPhase2 để đổi sang bộ sprite Tím/Hồng.")]
    public bool tintIfNoPhase2Sprites = true;
    public Color phase2Tint = new Color(1f, 0.6f, 0.95f);
    [Tooltip("Các SpriteRenderer bị nhuộm. Trống = mọi SpriteRenderer con lúc Awake.")]
    public SpriteRenderer[] tintRenderers;

    [Header("Sắp chết (dưới criticalHPPercent)")]
    [Range(0f, 1f)] public float criticalHPPercent = 0.2f;
    [Tooltip("Khói đen bám theo thân (prefab loop, KHÔNG cần AutoDestroyFX).")]
    public GameObject smokePrefab;
    [Tooltip("Chỗ gắn khói. Trống = 1 điểm ngẫu nhiên trên thân.")]
    public Transform[] smokePoints;
    public GameObject sparkPrefab;
    [Tooltip("Khoảng thời gian ngẫu nhiên giữa hai lần lóe tia lửa điện (min, max).")]
    public Vector2 sparkInterval = new Vector2(0.5f, 1.4f);

    [Header("Tiêu diệt")]
    public GameObject deathExplosionPrefab;
    [Min(1)] public int deathExplosions = 5;
    [Tooltip("Chuỗi nổ rải đều trong khoảng thời gian này.")]
    [Min(0f)] public float deathSequenceTime = 2f;
    [Tooltip("Chờ animation sụp đổ (Trigger Die) trước khi thay bằng xác (wreckPrefab của EnemyHealth).")]
    [Min(0f)] public float collapseTime = 0.6f;
    [Tooltip("Móc màn hình Mission Complete vào đây.")]
    public UnityEvent onDefeated;
    public static event Action<Boss3AI> FinalBossDefeated;

    [Header("Điểm đặt FX trên thân (trống = ngẫu nhiên trong bounds collider)")]
    public Transform[] bodyFxPoints;
    [Tooltip("Chỉ dùng nếu prefab FX chưa có AutoDestroyFX.")]
    [Min(0.1f)] public float fxLifetime = 2f;

    public bool IsEngaged { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsPhase2 { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool IsCritical { get; private set; }
    public bool IsDying { get; private set; }
    public bool IsFacingRight { get; private set; }
    public int FacingSign { get { return IsFacingRight ? 1 : -1; } }
    // Hệ số tốc độ các component vũ khí dùng để rút ngắn báo hiệu/quét ở phase 2.
    public float ActionSpeed { get { return IsPhase2 ? Mathf.Max(1f, phase2ActionSpeed) : 1f; } }
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
    private float baseAnimSpeed = 1f;
    private int cycleIndex;
    private readonly List<GameObject> attachedFx = new List<GameObject>();

    // Animator (tất cả tùy chọn): Bool isMoving, Bool isPhase2, Trigger Die
    private static readonly int MovingParam = Animator.StringToHash("isMoving");
    private static readonly int Phase2Param = Animator.StringToHash("isPhase2");
    private static readonly int DieParam = Animator.StringToHash("Die");
    private bool hasMoving, hasPhase2, hasDie;

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
            baseAnimSpeed = anim.speed;
            hasMoving = HasParam(anim, MovingParam, AnimatorControllerParameterType.Bool);
            hasPhase2 = HasParam(anim, Phase2Param, AnimatorControllerParameterType.Bool);
            hasDie = HasParam(anim, DieParam, AnimatorControllerParameterType.Trigger);
        }
        if (tintRenderers == null || tintRenderers.Length == 0)
            tintRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (eyeLaser == null && missiles == null)
            Debug.LogWarning("Boss3AI cần ít nhất Boss3EyeLaser hoặc Boss3MissileBarrage trên cùng object.", this);
        if (shock == null)
            Debug.LogWarning("Boss3AI không có Boss3ShockArm: player đứng sát sẽ không bị đẩy lùi.", this);
    }

    void OnEnable()
    {
        if (health != null) health.OnHealthPercentChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        if (health != null) health.OnHealthPercentChanged -= HandleHealthChanged;
        StopAllCoroutines();
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
        return HasTarget && IsAvailable(Attack.Shock) &&
            Mathf.Abs(target.position.x - rb.position.x) <= Mathf.Max(0f, antiMeleeRange);
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

    void HandleHealthChanged(float percent)
    {
        if (IsDying || percent <= 0f) return;

        if (!IsPhase2 && percent <= phase2HPPercent)
            StartCoroutine(EnterPhase2());
        if (!IsCritical && percent <= criticalHPPercent)
            StartCoroutine(CriticalRoutine());
    }

    IEnumerator EnterPhase2()
    {
        IsPhase2 = true;
        IsTransitioning = true;
        StopCombat();

        if (anim != null)
        {
            anim.speed = baseAnimSpeed * Mathf.Max(1f, phase2ActionSpeed);
            if (hasPhase2) anim.SetBool(Phase2Param, true);
        }
        if (!hasPhase2 && tintIfNoPhase2Sprites) ApplyTint(phase2Tint);

        // Nổ nhỏ rải trên thân trong lúc đứng khựng.
        float duration = Mathf.Max(0f, phaseTransitionTime);
        int count = Mathf.Max(0, transitionExplosions);
        float gap = count > 0 ? duration / count : duration;
        for (int i = 0; i < count; i++)
        {
            SpawnFx(transitionExplosionPrefab, RandomBodyPoint(), null, fxLifetime);
            yield return new WaitForSeconds(gap);
        }
        if (count == 0) yield return new WaitForSeconds(duration);

        IsTransitioning = false;
        if (!IsDying && health.currentHP > 0 && IsEngaged)
            combatRoutine = StartCoroutine(CombatLoop(0f));
    }

    IEnumerator CriticalRoutine()
    {
        IsCritical = true;
        if (smokePrefab != null)
        {
            if (smokePoints != null && smokePoints.Length > 0)
            {
                foreach (Transform point in smokePoints)
                    if (point != null) attachedFx.Add(SpawnFx(smokePrefab, point.position, point, 0f));
            }
            else attachedFx.Add(SpawnFx(smokePrefab, RandomBodyPoint(), transform, 0f));
        }

        if (sparkPrefab == null) yield break;
        while (!IsDying)
        {
            float min = Mathf.Max(0.05f, Mathf.Min(sparkInterval.x, sparkInterval.y));
            float max = Mathf.Max(min, Mathf.Max(sparkInterval.x, sparkInterval.y));
            yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
            if (!IsDying) SpawnFx(sparkPrefab, RandomBodyPoint(), transform, fxLifetime);
        }
    }

    // EnemyHealth gọi khi HP về 0 thay vì Destroy ngay.
    public void BeginDeathSequence()
    {
        if (IsDying) return;
        IsDying = true;
        StopAllCoroutines();
        CancelWeapons();
        combatRoutine = null;
        runningAttack = null;
        IsAttacking = false;
        IsTransitioning = false;
        SetMoving(false);
        Move(0f);

        // Tên lửa còn bay sau khi boss chết thì hủy, tránh giết Marco sau khi đã thắng.
        Boss3Missile.ClearAll();
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        int count = Mathf.Max(1, deathExplosions);
        float gap = Mathf.Max(0f, deathSequenceTime) / count;
        for (int i = 0; i < count; i++)
        {
            SpawnFx(deathExplosionPrefab, RandomBodyPoint(), null, fxLifetime);
            yield return new WaitForSeconds(gap);
        }

        if (anim != null && hasDie)
        {
            anim.SetTrigger(DieParam);
            yield return new WaitForSeconds(Mathf.Max(0f, collapseTime));
        }

        foreach (GameObject fx in attachedFx)
            if (fx != null) Destroy(fx);
        attachedFx.Clear();

        onDefeated?.Invoke();
        FinalBossDefeated?.Invoke(this);
        health.CompleteDeath(); // Nổ lớn + xác cháy + Destroy boss.
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

    void ApplyTint(Color tint)
    {
        foreach (SpriteRenderer sr in tintRenderers)
            if (sr != null) sr.color = tint;
        health.SetBaseColor(tint);
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

    public Vector3 RandomBodyPoint()
    {
        if (bodyFxPoints != null && bodyFxPoints.Length > 0)
        {
            Transform point = bodyFxPoints[UnityEngine.Random.Range(0, bodyFxPoints.Length)];
            if (point != null) return point.position;
        }

        bool found = false;
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            if (!col.enabled) continue;
            if (!found) { bounds = col.bounds; found = true; }
            else bounds.Encapsulate(col.bounds);
        }
        if (!found)
        {
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                if (!found) { bounds = sr.bounds; found = true; }
                else bounds.Encapsulate(sr.bounds);
            }
        }
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            transform.position.z);
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
