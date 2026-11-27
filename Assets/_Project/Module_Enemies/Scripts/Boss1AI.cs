using UnityEngine;

// Phase 1: ranged attacks. Phase 2: jump closer, chase and punch.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyHealth))]
public class Boss1AI : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform target;
    [Tooltip("Tầm bắt đầu giao chiến. Sau khi phát hiện, boss tiếp tục đuổi dù player ra ngoài tầm này.")]
    [Min(0f)] public float detectRange = 12f;
    [Min(0f)] public float attackRange = 5f;
    [Min(0f)] public float rangeMargin = 0.5f;

    [Header("Di chuyển")]
    [Min(0f)] public float moveSpeed = 1.5f;
    [Min(0.01f)] public float facingDeadZone = 0.2f;
    [Tooltip("Hướng ảnh gốc khi Scale X dương và SpriteRenderer Flip X tắt.")]
    public bool spriteFacesLeft;

    [Header("Phase 2 - Punch")]
    [Min(1)] public int phase2HP = 300;
    [Min(0f)] public float phase2MoveSpeed = 2f;
    public bool IsPhase2 { get; private set; }
    private Boss1Melee melee;
    private Boss1Jump jump;
    [Min(0f)] public float phase2Pause = 0.6f;
    private float phase2ReadyTime;

    public bool IsAttacking { get; private set; }
    public bool IsFacingRight { get; private set; }
    public bool IsEngaged { get; private set; }

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private float scaleMagnitude;
    private float nextSearchTime;
    private bool parametersReady;
    private static readonly int Walking = Animator.StringToHash("isWalking");
    private static readonly int Shooting = Animator.StringToHash("isShooting");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        melee = GetComponent<Boss1Melee>();
        jump = GetComponent<Boss1Jump>();
        scaleMagnitude = Mathf.Abs(transform.localScale.x);
        if (rb.interpolation == RigidbodyInterpolation2D.None)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        bool walkingFound = false;
        bool shootingFound = false;
        if (anim.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in anim.parameters)
            {
                if (parameter.type != AnimatorControllerParameterType.Bool) continue;
                walkingFound |= parameter.nameHash == Walking;
                shootingFound |= parameter.nameHash == Shooting;
            }
        }
        parametersReady = walkingFound && shootingFound;
        if (!parametersReady)
            Debug.LogWarning("Boss1_Controller cần hai Bool: isWalking và isShooting. Thêm chúng trước khi Play.", this);
    }

    void FixedUpdate()
    {
        if (health.currentHP <= 0)
        {
            if (jump != null) jump.CancelJump();
            IsEngaged = false;
            SetState(false, false);
            Move(0f);
            return;
        }

        if (!IsPhase2 && health.currentHP < phase2HP)
        {
            IsPhase2 = true;
            phase2ReadyTime = Time.time + Mathf.Max(0f, phase2Pause);
            if (melee == null)
                Debug.LogWarning("Boss1 Phase 2 cần Boss1Melee trên cùng object và gán Punch Point.", this);
        }
        if (jump != null && jump.IsBusy)
        {
            SetState(false, false);
            jump.TickJump();
            return; // Do not overwrite the jump's horizontal velocity.
        }
        if (IsPhase2 && Time.time < phase2ReadyTime)
        {
            SetState(false, false);
            Move(0f);
            return;
        }
        if (melee != null && melee.IsBusy)
        {
            SetState(false, false);
            Move(0f);
            return;
        }

        if ((target == null || !target.gameObject.activeInHierarchy) && Time.time >= nextSearchTime)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            target = found != null ? found.transform : null;
            nextSearchTime = Time.time + 0.5f;
        }

        // Mất object mục tiêu thì tạm dừng, không truy cập Transform đã bị hủy.
        // Vẫn nhớ giao chiến để tiếp tục khi tìm lại player đang hoạt động.
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            SetState(false, false);
            Move(0f);
            return;
        }

        float distance = Vector2.Distance(rb.position, target.position);
        float margin = Mathf.Max(0f, rangeMargin);
        float detection = Mathf.Max(detectRange, attackRange);
        // Chỉ dùng tầm phát hiện để bắt đầu trận, không dùng để thoát giao chiến.
        if (!IsEngaged && distance <= detection)
            IsEngaged = true;

        if (IsPhase2)
        {
            TickMeleePhase(distance);
            return;
        }
        bool shouldAttack = IsEngaged && distance <= Mathf.Max(0f, attackRange) + (IsAttacking ? margin : 0f);

        float step = 0f;
        if (IsEngaged)
        {
            float diff = target.position.x - rb.position.x;
            if (Mathf.Abs(diff) > Mathf.Max(0.01f, facingDeadZone)) Face(diff > 0f);
            if (!shouldAttack)
            {
                float remaining = Mathf.Max(0f, Mathf.Abs(diff) - Mathf.Max(0.01f, facingDeadZone));
                step = Mathf.Sign(diff) * Mathf.Min(remaining, Mathf.Max(0f, moveSpeed) * Time.fixedDeltaTime);
            }
        }

        SetState(step != 0f, shouldAttack);
        Move(step);
    }

    void TickMeleePhase(float distance)
    {
        // Always clear shooting, including while waiting for the punch cooldown.
        SetState(false, false);
        if (!IsEngaged || melee == null || !melee.isActiveAndEnabled || melee.punchPoint == null)
        {
            Move(0f);
            return;
        }

        float diff = target.position.x - rb.position.x;
        if (Mathf.Abs(diff) > Mathf.Max(0.01f, facingDeadZone)) Face(diff > 0f);
        if (distance <= Mathf.Max(0f, melee.punchRange))
        {
            Move(0f);
            melee.TryBeginPunch(target);
            return;
        }

        if (jump != null && jump.TryBeginJump(target)) return;

        float remaining = Mathf.Max(0f, Mathf.Abs(diff) - Mathf.Max(0.01f, facingDeadZone));
        float step = Mathf.Sign(diff) * Mathf.Min(remaining, Mathf.Max(0f, phase2MoveSpeed) * Time.fixedDeltaTime);
        SetState(step != 0f, false);
        Move(step);
    }

    void Face(bool right)
    {
        IsFacingRight = right;
        bool parentRight = transform.parent == null || transform.parent.TransformVector(Vector3.right).x >= 0f;
        bool originalWorldRight = (!spriteFacesLeft) == parentRight;
        Vector3 scale = transform.localScale;
        // Lật cả thân, tay và các điểm bắn cùng nhau.
        scale.x = right == originalWorldRight ? scaleMagnitude : -scaleMagnitude;
        transform.localScale = scale;
    }

    void Move(float step)
    {
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(step / Time.fixedDeltaTime, rb.linearVelocity.y);
        else if (rb.bodyType == RigidbodyType2D.Kinematic)
            rb.MovePosition(rb.position + Vector2.right * step);
    }

    void SetState(bool walking, bool shooting)
    {
        IsAttacking = shooting;
        if (!parametersReady || anim == null) return;
        anim.SetBool(Walking, walking);
        anim.SetBool(Shooting, shooting);
    }

    public bool CanShoot()
    {
        // Check HP too: an animation event can arrive before the next FixedUpdate.
        return isActiveAndEnabled && !IsPhase2 && health.currentHP >= phase2HP &&
            IsAttacking && (melee == null || !melee.IsBusy) && health.currentHP > 0 &&
            target != null && target.gameObject.activeInHierarchy &&
            Vector2.Distance(rb.position, target.position) <= Mathf.Max(0f, attackRange) + Mathf.Max(0f, rangeMargin);
    }

    void OnDisable()
    {
        if (jump != null) jump.CancelJump();
        IsEngaged = false;
        SetState(false, false);
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
