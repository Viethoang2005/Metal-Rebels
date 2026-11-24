using UnityEngine;

// Boss1AI calls TickJump so only one script controls horizontal motion at a time.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyHealth))]
public class Boss1Jump : MonoBehaviour
{
    [Header("Nhảy áp sát")]
    [Min(0f)] public float minimumDistance = 4f;
    [Min(0.1f)] public float maximumDistance = 7f;
    [Min(0.1f)] public float landingDistance = 1.5f;
    [Min(0.1f)] public float jumpHeight = 2.5f;
    [Min(0.2f)] public float flightTime = 0.9f;
    [Min(0f)] public float recoveryTime = 0.7f;
    [Min(0.1f)] public float cooldown = 5f;

    [Header("Sàn có tag Ground, collider không bật Is Trigger")]
    public Collider2D bodyCollider;
    public LayerMask groundLayers = ~0;
    [Min(0.1f)] public float groundSearchHeight = 3f;
    [Min(0.1f)] public float maximumDrop = 3f;
    [Tooltip("Tùy chọn: hai mốc giới hạn vị trí đáp trong đấu trường.")]
    public Transform arenaLeft;
    public Transform arenaRight;

    [Header("Hiệu ứng tùy chọn")]
    public GameObject landingMarkerPrefab;
    public GameObject landingEffectPrefab;
    public GameObject thrusterObject;
    [Min(0.01f)] public float landingEffectLifetime = 0.7f;

    private enum Stage { None, Prepare, Flying, Landing }
    private Stage stage;
    public bool IsBusy => stage != Stage.None;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private Vector2 destination;
    private Vector2 groundPoint;
    private float stageStart, nextJumpTime, originalGravity, originalDamping;
    private bool leftGround, sawLandState, physicsOverridden, ready;
    private GameObject marker;
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int LandTrigger = Animator.StringToHash("Land");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        bool jump = false, land = false;
        if (anim.runtimeAnimatorController != null)
            foreach (var parameter in anim.parameters)
            {
                if (parameter.type != AnimatorControllerParameterType.Trigger) continue;
                jump |= parameter.nameHash == JumpTrigger;
                land |= parameter.nameHash == LandTrigger;
            }
        ready = jump && land;
        if (!ready) Debug.LogWarning("Boss1Jump cần Trigger Jump và Land trong Animator.", this);
        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning("Boss1Jump cần Rigidbody2D Dynamic.", this);
        SetThruster(false);
    }

    public bool TryBeginJump(Transform target)
    {
        if (!isActiveAndEnabled || !ready || IsBusy || Time.time < nextJumpTime ||
            target == null || !target.gameObject.activeInHierarchy || health.currentHP <= 0 ||
            !anim.isActiveAndEnabled || bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger ||
            rb.bodyType != RigidbodyType2D.Dynamic || !rb.simulated ||
            (rb.constraints & (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY)) != 0 ||
            Physics2D.gravity.y >= -0.01f || !IsGrounded()) return false;

        float diff = target.position.x - rb.position.x;
        if (Mathf.Abs(diff) <= minimumDistance) return false;
        float desiredX = target.position.x - Mathf.Sign(diff) * landingDistance;
        desiredX = rb.position.x + Mathf.Clamp(desiredX - rb.position.x, -maximumDistance, maximumDistance);
        if (arenaLeft != null && arenaRight != null)
        {
            float halfWidth = bodyCollider.bounds.extents.x;
            float left = Mathf.Min(arenaLeft.position.x, arenaRight.position.x) + halfWidth;
            float right = Mathf.Max(arenaLeft.position.x, arenaRight.position.x) - halfWidth;
            if (left >= right) return false;
            desiredX = Mathf.Clamp(desiredX, left, right);
        }
        if (Mathf.Abs(desiredX - rb.position.x) < 0.5f) return false;
        if (!FindLanding(desiredX))
        {
            nextJumpTime = Time.time + 0.5f;
            return false;
        }

        stage = Stage.Prepare;
        stageStart = Time.time;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.ResetTrigger(LandTrigger);
        anim.SetTrigger(JumpTrigger);
        if (landingMarkerPrefab != null)
            marker = Instantiate(landingMarkerPrefab, new Vector3(groundPoint.x, groundPoint.y, transform.position.z),
                landingMarkerPrefab.transform.rotation);
        return true;
    }

    bool IsGround(Collider2D collider)
    {
        if (collider == null || collider.isTrigger || collider.transform.IsChildOf(transform) ||
            (groundLayers.value & (1 << collider.gameObject.layer)) == 0) return false;
        for (Transform current = collider.transform; current != null; current = current.parent)
            if (current.CompareTag("Ground")) return true;
        return false;
    }

    bool IsGrounded()
    {
        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
            if (contacts[i].normal.y > 0.5f && IsGround(contacts[i].collider)) return true;
        return false;
    }

    bool FindLanding(float x)
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 offset = (Vector2)bounds.center - rb.position;
        Vector2 origin = new Vector2(x + offset.x, bounds.max.y + groundSearchHeight);
        float length = bounds.size.y + groundSearchHeight + maximumDrop;
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, Vector2.down, length, groundLayers))
        {
            if (!IsGround(hit.collider) || hit.normal.y < 0.9f) continue;
            Vector2 center = new Vector2(origin.x, hit.point.y + bounds.extents.y + 0.03f);
            Vector2 size = new Vector2(Mathf.Max(0.02f, bounds.size.x - 0.06f),
                Mathf.Max(0.02f, bounds.size.y - 0.06f));
            bool blocked = false;
            foreach (Collider2D other in Physics2D.OverlapBoxAll(center, size, 0f))
                if (!other.isTrigger && !other.transform.IsChildOf(transform)) { blocked = true; break; }
            if (blocked) return false;
            // Check support under both edges so the boss does not aim at a platform edge.
            foreach (float side in new[] { -1f, 1f })
            {
                bool supported = false;
                Vector2 edge = hit.point + new Vector2(side * bounds.extents.x * 0.9f, 0.15f);
                foreach (RaycastHit2D support in Physics2D.RaycastAll(edge, Vector2.down, 0.3f, groundLayers))
                    if (IsGround(support.collider) && support.normal.y >= 0.9f) { supported = true; break; }
                if (!supported) return false;
            }
            destination = center - offset;
            groundPoint = hit.point;
            return true;
        }
        return false;
    }

    public void TickJump()
    {
        if (!IsBusy) return;
        if (health.currentHP <= 0 || !anim.isActiveAndEnabled) { CancelJump(); return; }
        float elapsed = Time.time - stageStart;
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        if (stage == Stage.Prepare)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (state.IsName("Boss1_Jump"))
            {
                if (!IsGrounded()) { CancelJump(); return; }
                Launch();
            }
            else if (elapsed > 5f)
            {
                Debug.LogWarning("Boss1Jump: không chuyển tới Boss1_Jump. Kiểm tra transition/Speed của Animator.", this);
                CancelJump();
            }
        }
        else if (stage == Stage.Flying)
        {
            bool grounded = IsGrounded();
            if (!grounded) leftGround = true;
            if (leftGround && grounded && rb.linearVelocity.y <= 0.1f) Land();
            else if (elapsed > Mathf.Max(5f, flightTime * 3f))
            {
                // Never fake a landing in midair. Return control and restore normal gravity.
                Debug.LogWarning("Boss1Jump: chưa chạm sàn, hủy nhảy và khôi phục trọng lực.", this);
                CancelJump();
            }
        }
        else if (stage == Stage.Landing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            sawLandState |= state.IsName("Boss1_Land");
            if (elapsed >= recoveryTime && sawLandState && !state.IsName("Boss1_Land") && !anim.IsInTransition(0))
                stage = Stage.None;
            else if (elapsed > Mathf.Max(5f, recoveryTime + 2f)) CancelJump();
        }
    }

    void Launch()
    {
        float time = Mathf.Max(0.2f, flightTime);
        float rise = Mathf.Max(0.1f, jumpHeight) + Mathf.Max(0f, destination.y - rb.position.y);
        float fall = rise - (destination.y - rb.position.y);
        float acceleration = 2f * Mathf.Pow(Mathf.Sqrt(rise) + Mathf.Sqrt(fall), 2f) / (time * time);
        originalGravity = rb.gravityScale;
        originalDamping = rb.linearDamping;
        physicsOverridden = true;
        rb.gravityScale = acceleration / -Physics2D.gravity.y;
        rb.linearDamping = 0f;
        rb.linearVelocity = new Vector2((destination.x - rb.position.x) / time, Mathf.Sqrt(2f * acceleration * rise));
        stage = Stage.Flying;
        stageStart = Time.time;
        leftGround = false;
        SetThruster(true);
    }

    void Land()
    {
        RestorePhysics();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        stage = Stage.Landing;
        stageStart = Time.time;
        nextJumpTime = Time.time + cooldown;
        sawLandState = false;
        ClearEffects();
        anim.SetTrigger(LandTrigger);
        if (landingEffectPrefab != null)
        {
            Bounds bounds = bodyCollider.bounds;
            GameObject effect = Instantiate(landingEffectPrefab,
                new Vector3(bounds.center.x, bounds.min.y, transform.position.z), landingEffectPrefab.transform.rotation);
            if (effect.GetComponent<AutoDestroyFX>() == null) Destroy(effect, landingEffectLifetime);
        }
    }

    void RestorePhysics()
    {
        if (!physicsOverridden || rb == null) return;
        rb.gravityScale = originalGravity;
        rb.linearDamping = originalDamping;
        physicsOverridden = false;
    }

    void SetThruster(bool active)
    {
        // Only allow a dedicated child FX object, never disable the boss itself.
        if (thrusterObject != null && thrusterObject != gameObject && thrusterObject.transform.IsChildOf(transform))
            thrusterObject.SetActive(active);
    }

    void ClearEffects()
    {
        SetThruster(false);
        if (marker != null) Destroy(marker);
        marker = null;
    }

    public void CancelJump()
    {
        bool wasBusy = IsBusy;
        RestorePhysics();
        ClearEffects();
        stage = Stage.None;
        if (!wasBusy) return;
        nextJumpTime = Time.time + cooldown;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim != null && ready)
        {
            anim.ResetTrigger(JumpTrigger);
            anim.ResetTrigger(LandTrigger);
            if (anim.isActiveAndEnabled && anim.HasState(0, Animator.StringToHash("Base Layer.Boss1_Idle")))
                anim.Play("Base Layer.Boss1_Idle", 0, 0f);
        }
    }

    void OnDisable() { CancelJump(); }
}
