using System.Collections;
using UnityEngine;

// Boss 2 (cầu cơ khí lơ lửng). Phase 1: thả Pod, chỉ nhận sát thương khi mở nắp pháo.
// Phase 2 (HP <= phase2HP): nhanh hơn, thêm laser dọc và tia tỏa từ lõi, lõi luôn lộ ra.
// Rigidbody2D nên để Kinematic, Gravity Scale 0.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class Boss2AI : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform target;
    [Min(0f)] public float detectRange = 12f;

    [Header("Di chuyển ngang quanh vị trí ban đầu")]
    [Min(0f)] public float moveSpeed = 1.5f;
    [Min(0f)] public float phase2SpeedMultiplier = 1.5f;
    [Min(0f)] public float moveBounds = 5f;

    [Header("Nhịp tấn công")]
    [Min(0.1f)] public float attackInterval = 2.5f;
    [Range(0.1f, 1f)] public float phase2IntervalMultiplier = 0.6f;

    [Header("Nắp pháo")]
    [Tooltip("Thời gian chờ animation mở nắp trước khi bắn.")]
    [Min(0f)] public float cannonOpenDelay = 0.5f;
    [Tooltip("Giữ nắp mở thêm sau khi bắn xong: đây là cửa sổ để người chơi đánh trả.")]
    [Min(0f)] public float cannonOpenLinger = 1.2f;
    public bool phase2AlwaysVulnerable = true;
    [Tooltip("Giữ nắp mở thêm sau khi beam laser tắt, rồi mới đóng.")]
    [Min(0f)] public float laserCloseDelay = 0.2f;

    [Header("Số lượng đòn")]
    [Min(1)] public int podsPhase1 = 2;
    [Min(1)] public int podsPhase2 = 4;

    [Header("Phase 2")]
    [Min(1)] public int phase2HP = 450;
    [Min(0f)] public float phaseTransitionTime = 1.5f;
    public GameObject phaseTransitionEffectPrefab;

    [Header("Test")]
    [Tooltip("Chỉ để thử: phase 2 chỉ bắn laser. Nhớ tắt sau khi test.")]
    public bool testForceLaser;
    [Tooltip("Chỉ để thử: phase 2 chỉ bắn tia tỏa từ lõi. Nhớ tắt sau khi test.")]
    public bool testForceCoreBurst;

    public bool IsPhase2 { get; private set; }
    public bool IsEngaged { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool HoldPosition { get; private set; }
    public bool CannonOpen { get; private set; }

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private Boss2Weapon weapon;
    private Boss2Laser laser;
    private Boss2Core core;
    private readonly System.Collections.Generic.List<int> attackPool = new System.Collections.Generic.List<int>();
    private float startX;
    private int moveDirection = 1;
    private float nextSearchTime;
    private Coroutine attackRoutine;
    private int lastAttack = -1;

    // Animator (tất cả tùy chọn, thiếu thì bỏ qua): Bool isWalking, Bool isCannonOpen, Trigger Phase2, Trigger Laser
    private static readonly int Walking = Animator.StringToHash("isWalking");
    private static readonly int CannonOpenParam = Animator.StringToHash("isCannonOpen");
    private static readonly int Phase2Param = Animator.StringToHash("Phase2");
    private static readonly int LaserParam = Animator.StringToHash("Laser");
    private bool hasWalking, hasCannon, hasPhase2, hasLaser;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        weapon = GetComponent<Boss2Weapon>();
        laser = GetComponent<Boss2Laser>();
        core = GetComponent<Boss2Core>();
        anim = GetComponentInChildren<Animator>();
        startX = rb.position.x;
        if (rb.interpolation == RigidbodyInterpolation2D.None)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in anim.parameters)
            {
                hasWalking |= p.nameHash == Walking && p.type == AnimatorControllerParameterType.Bool;
                hasCannon |= p.nameHash == CannonOpenParam && p.type == AnimatorControllerParameterType.Bool;
                hasPhase2 |= p.nameHash == Phase2Param && p.type == AnimatorControllerParameterType.Trigger;
                hasLaser |= p.nameHash == LaserParam && p.type == AnimatorControllerParameterType.Trigger;
            }
        }
        if (weapon == null) Debug.LogWarning("Boss2AI cần Boss2Weapon trên cùng object.", this);
        if (laser == null) Debug.LogWarning("Boss2AI không tìm thấy Boss2Laser trên cùng object: Phase 2 sẽ không bao giờ bắn laser.", this);
    }

    void FixedUpdate()
    {
        if (health.currentHP <= 0)
        {
            IsEngaged = false;
            SetWalking(false);
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
            SetWalking(false);
            return;
        }

        if (!IsEngaged && Vector2.Distance(rb.position, target.position) <= Mathf.Max(0f, detectRange))
        {
            IsEngaged = true;
            attackRoutine = StartCoroutine(AttackLoop());
        }

        if (IsEngaged && !IsPhase2 && health.currentHP <= phase2HP)
            StartCoroutine(EnterPhase2());

        if (!IsEngaged || IsTransitioning || HoldPosition)
        {
            SetWalking(false);
            return;
        }
        Patrol();
    }

    bool HasTarget
    {
        get { return target != null && target.gameObject.activeInHierarchy; }
    }

    void Patrol()
    {
        float speed = Mathf.Max(0f, moveSpeed) * (IsPhase2 ? phase2SpeedMultiplier : 1f);
        float bounds = Mathf.Max(0f, moveBounds);
        if (moveDirection > 0 && rb.position.x >= startX + bounds) moveDirection = -1;
        else if (moveDirection < 0 && rb.position.x <= startX - bounds) moveDirection = 1;

        float destination = startX + moveDirection * bounds;
        float nextX = Mathf.MoveTowards(rb.position.x, destination, speed * Time.fixedDeltaTime);
        SetWalking(nextX != rb.position.x);
        MoveTo(nextX);
    }

    void MoveTo(float x)
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
            rb.MovePosition(new Vector2(x, rb.position.y));
        else if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2((x - rb.position.x) / Time.fixedDeltaTime, rb.linearVelocity.y);
    }

    IEnumerator AttackLoop()
    {
        while (health.currentHP > 0)
        {
            float interval = attackInterval * (IsPhase2 ? phase2IntervalMultiplier : 1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, interval));
            if (!HasTarget || IsTransitioning) continue;

            // 0 Pod. Phase 2 thêm 1 laser dọc, 2 tia tỏa từ lõi. Không lặp liên tiếp một đòn.
            attackPool.Clear();
            attackPool.Add(0);
            if (IsPhase2 && laser != null) attackPool.Add(1);
            if (IsPhase2 && core != null) attackPool.Add(2);
            if (attackPool.Count > 1) attackPool.Remove(lastAttack);
            int pick = attackPool[Random.Range(0, attackPool.Count)];
            if (IsPhase2 && testForceLaser && laser != null) pick = 1;
            if (IsPhase2 && testForceCoreBurst && core != null) pick = 2;
            lastAttack = pick;

            IsAttacking = true;
            if (pick == 0) yield return PodAttack();
            else if (pick == 1) yield return LaserAttack();
            else yield return CoreBurstAttack();
            IsAttacking = false;
        }
    }

    // Mở nắp, thả Pod chui ra từ cửa cannon, giữ nắp mở thêm để người chơi đánh trả rồi mới đóng.
    IEnumerator PodAttack()
    {
        SetCannon(true);
        yield return new WaitForSeconds(cannonOpenDelay);
        if (weapon != null) weapon.SpawnPods(IsPhase2 ? podsPhase2 : podsPhase1);
        yield return new WaitForSeconds(cannonOpenLinger);
        SetCannon(false);
    }

    IEnumerator LaserAttack()
    {
        // Đứng yên suốt lúc nạp và bắn để tia ra đúng từ cửa cannon và vòng cảnh báo không bị lệch.
        HoldPosition = true;
        if (anim != null && hasLaser) anim.SetTrigger(LaserParam);

        // Mở nắp ngay từ lúc nạp, giữ mở suốt lúc beam bắn, đóng sau khi beam tắt.
        SetCannon(true);
        yield return laser.FireRoutine(target);
        yield return new WaitForSeconds(laserCloseDelay);
        SetCannon(false);

        HoldPosition = false;
    }

    IEnumerator CoreBurstAttack()
    {
        // Đứng yên để các tia tỏa ra đúng từ lõi.
        HoldPosition = true;
        yield return core.BurstRoutine();
        HoldPosition = false;
    }

    IEnumerator EnterPhase2()
    {
        IsPhase2 = true;
        if (core != null) core.EnterPhase2();
        IsTransitioning = true;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        IsAttacking = false;
        HoldPosition = false;
        SetCannon(false);

        // Dọn Pod đang bay, bỏ qua những lần bắn đang chờ.
        foreach (Boss2Pod pod in Boss2Pod.Active.ToArray())
            if (pod != null) pod.Explode();
        if (laser != null) laser.CancelAll();

        if (anim != null && hasPhase2) anim.SetTrigger(Phase2Param);
        if (phaseTransitionEffectPrefab != null)
        {
            GameObject fx = Instantiate(phaseTransitionEffectPrefab, transform.position, Quaternion.identity);
            if (fx.GetComponent<AutoDestroyFX>() == null) Destroy(fx, 2f);
        }

        yield return new WaitForSeconds(Mathf.Max(0f, phaseTransitionTime));
        IsTransitioning = false;
        if (health.currentHP > 0) attackRoutine = StartCoroutine(AttackLoop());
    }

    void SetCannon(bool open)
    {
        CannonOpen = open;
        if (anim != null && hasCannon) anim.SetBool(CannonOpenParam, open);
    }

    void SetWalking(bool walking)
    {
        if (anim != null && hasWalking) anim.SetBool(Walking, walking);
    }

    // EnemyHealth gọi hàm này: nắp đóng thì miễn nhiễm, đang chuyển phase thì bất tử.
    public bool CanTakeDamage()
    {
        if (IsTransitioning) return false;
        return CannonOpen || (IsPhase2 && phase2AlwaysVulnerable);
    }

    void OnDisable()
    {
        IsEngaged = false;
        IsAttacking = false;
        IsTransitioning = false;
        HoldPosition = false;
    }
}
