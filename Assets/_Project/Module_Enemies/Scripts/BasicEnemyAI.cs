using UnityEngine;

public class BasicEnemyAI : MonoBehaviour
{
    public enum EnemyType { RangedSoldier, MeleeOrVehicle }
    public EnemyType enemyType = EnemyType.RangedSoldier;

    [Header("Cấu hình hướng mặt ảnh gốc")]
    [Tooltip("Tích chọn nếu ảnh gốc của con lính đang nhìn sang bên TRÁI")]
    public bool spriteFacesLeft = true;
    [SerializeField] private SpriteRenderer sr;

    [Header("Thông số Di chuyển & Phát hiện")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float detectRange = 7f;
    public float attackRange = 5f;
    [Min(0.01f)] public float facingDeadZone = 0.2f;

    [Header("Thông số Tấn công")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Tooltip("Số giây giữa hai phát bắn. 1.5 = mỗi 1.5 giây bắn một viên.")]
    [Min(0.01f)] public float fireRate = 1.5f;
    public int contactDamage = 30;

    [Header("Hiệu ứng khói di chuyển")]
    public Transform smokePoint; // Kéo Smoke_FX vào đây
    private float smokeOffsetX;

    private Transform player;
    private Vector2 startPos;
    private int patrolDir = 1; // 1: Sang phải, -1: Sang trái
    private float nextFireTime;
    private Rigidbody2D rb;
    private float firePointOriginalX;
    private Animator anim;
    private bool hasCrashed;
    private bool shotPending;
    private bool hasShootTrigger;
    private bool isApproaching;
    private float approachTargetX;

    /// <summary>
    /// Gọi sau khi spawn trong Event Lock để địch đi vào khung hình trước khi bắn/tuần tra.
    /// </summary>
    public void DriveToPosition(float targetX)
    {
        isApproaching = true;
        approachTargetX = targetX;
        patrolDir = targetX >= transform.position.x ? 1 : -1;
    }

    void Start()
    {
        startPos = transform.position;
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.interpolation == RigidbodyInterpolation2D.None)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (firePoint != null)
        {
            firePointOriginalX = Mathf.Abs(firePoint.localPosition.x);
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (smokePoint != null)
            smokeOffsetX = Mathf.Abs(smokePoint.localPosition.x);
        SetFacing(patrolDir > 0);
        anim = GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in anim.parameters)
            {
                if (parameter.name == "Shoot" && parameter.type == AnimatorControllerParameterType.Trigger)
                    hasShootTrigger = true;
            }
        }
        if (enemyType == EnemyType.RangedSoldier && !hasShootTrigger)
            Debug.LogWarning("Animator của lính cần Trigger Shoot và state chờ Enemy_Rifle_Aim.", this);
    }

    void Update()
    {
        if (rb == null) TickAI(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (rb != null) TickAI(Time.fixedDeltaTime);
    }

    Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;

    void TickAI(float deltaTime)
    {
        if (isApproaching)
        {
            shotPending = false;
            if (anim != null && enemyType == EnemyType.RangedSoldier)
            {
                anim.SetBool("isShooting", false);
                if (hasShootTrigger) anim.ResetTrigger("Shoot");
            }

            float enterDiff = approachTargetX - Position.x;
            if (Mathf.Abs(enterDiff) <= 0.25f)
            {
                isApproaching = false;
                startPos = Position;
                MoveHorizontal(0f, deltaTime);
            }
            else
            {
                float dir = enterDiff > 0f ? 1f : -1f;
                SetFacing(dir > 0f);
                float approachSpeed = Mathf.Max(2f, moveSpeed);
                float step = dir * Mathf.Min(Mathf.Abs(enterDiff), approachSpeed * deltaTime);
                MoveHorizontal(step, deltaTime);
                return;
            }
        }

        bool isShooting = player != null && player.gameObject.activeInHierarchy && enemyType == EnemyType.RangedSoldier && Vector2.Distance(Position, player.position) <= attackRange;
        if (anim != null && enemyType == EnemyType.RangedSoldier){
            anim.SetBool("isShooting", isShooting);
        }
        if (!isShooting)
        {
            shotPending = false;
            if (anim != null && hasShootTrigger) anim.ResetTrigger("Shoot");
        }
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            Patrol(deltaTime);
            return;
        }

        float dist = Vector2.Distance(Position, player.position);

        if (isShooting)
        {
            // Trạng thái bắn: Quay mặt về phía người chơi
            FaceTarget(player.position.x);
            MoveHorizontal(0f, deltaTime);
            // Chỉ kích hoạt khi đã trở về tư thế chờ, tránh ngắt animation đang bắn.
            if (!shotPending && Time.time >= nextFireTime && bulletPrefab != null && firePoint != null &&
                anim != null && hasShootTrigger && !anim.IsInTransition(0) &&
                anim.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Rifle_Aim"))
            {
                shotPending = true;
                anim.SetTrigger("Shoot");
                nextFireTime = Time.time + Mathf.Max(0.01f, fireRate);
            }
        }
        else if (dist <= detectRange)
        {
            // Trạng thái đuổi theo
            float diff = player.position.x - Position.x;
            float distance = Mathf.Max(0f, Mathf.Abs(diff) - Mathf.Max(0.01f, facingDeadZone));
            float step = Mathf.Sign(diff) * Mathf.Min(distance, Mathf.Max(0f, moveSpeed) * 1.2f * deltaTime);
            if (step != 0f) SetFacing(step > 0f);
            MoveHorizontal(step, deltaTime);
        }
        else
        {
            // Trạng thái đi tuần
            Patrol(deltaTime);
        }
    }

    void Patrol(float deltaTime)
    {
        if (patrolDistance <= 0f)
        {
            MoveHorizontal(0f, deltaTime);
            return;
        }

        // Rigidbody có thể dừng ngay trước đích do ma sát/sai số vật lý.
        // Dùng vùng tới đích nhỏ, giới hạn theo bán kính để hai biên không chồng nhau.
        float turnTolerance = Mathf.Min(0.05f, patrolDistance * 0.5f);
        if (patrolDir == 1 && Position.x >= startPos.x + patrolDistance - turnTolerance)
        {
            patrolDir = -1;
        }
        else if (patrolDir == -1 && Position.x <= startPos.x - patrolDistance + turnTolerance)
        {
            patrolDir = 1;
        }

        SetFacing(patrolDir > 0);
        float targetX = startPos.x + patrolDir * patrolDistance;
        float nextX = Mathf.MoveTowards(Position.x, targetX, Mathf.Max(0f, moveSpeed) * deltaTime);
        MoveHorizontal(nextX - Position.x, deltaTime);
    }

    void MoveHorizontal(float step, float deltaTime)
    {
        if (rb == null)
        {
            transform.position += Vector3.right * step;
        }
        else if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            // Giữ vận tốc Y để trọng lực và va chạm với mặt đất vẫn hoạt động.
            rb.linearVelocity = new Vector2(deltaTime > 0f ? step / deltaTime : 0f, rb.linearVelocity.y);
        }
        else if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            rb.MovePosition(rb.position + Vector2.right * step);
        }
    }

    void FaceTarget(float targetX)
    {
        float diff = targetX - Position.x;
        // Giữ hướng cũ khi player nằm sát trục X của enemy.
        if (Mathf.Abs(diff) > Mathf.Max(0.01f, facingDeadZone))
        {
            SetFacing(diff > 0);
        }
    }

    void SetFacing(bool lookRight)
    {
        if (sr != null)
        {
            // Tính cả scale âm/rotation của object và parent, tránh lật hai lần.
            bool localRightIsWorldRight = sr.transform.TransformVector(Vector3.right).x >= 0f;
            bool lookLocalRight = lookRight == localRightIsWorldRight;
            sr.flipX = spriteFacesLeft ? lookLocalRight : !lookLocalRight;
        }

        // Tự động đảo vị trí đầu nòng súng (FirePoint) sang hướng đang nhìn
        if (firePoint != null)
        {
            bool parentRightIsWorldRight = firePoint.parent == null ||
                firePoint.parent.TransformVector(Vector3.right).x >= 0f;
            float targetLocalX = lookRight == parentRightIsWorldRight ? firePointOriginalX : -firePointOriginalX;
            firePoint.localPosition = new Vector3(targetLocalX, firePoint.localPosition.y, firePoint.localPosition.z);
        }
        if (smokePoint != null)
        {
        bool parentRightIsWorldRight = smokePoint.parent == null ||
            smokePoint.parent.TransformVector(Vector3.right).x >= 0f;

        bool lookLocalRight = lookRight == parentRightIsWorldRight;

        Vector3 pos = smokePoint.localPosition;
        pos.x = lookLocalRight ? -smokeOffsetX : smokeOffsetX;
        smokePoint.localPosition = pos;
        }
    }

    // Animation Event tại frame bóp cò: mỗi lượt animation chỉ tạo một viên.
    public void FireAnimationEvent()
    {
        if (!isActiveAndEnabled || !shotPending) return;
        shotPending = false;
        if (player == null || !player.gameObject.activeInHierarchy || enemyType != EnemyType.RangedSoldier) return;
        if (Vector2.Distance(Position, player.position) > attackRange) return;
        Shoot();
    }

    void OnDisable()
    {
        shotPending = false;
        if (anim != null && hasShootTrigger) anim.ResetTrigger("Shoot");
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;
        b.GetComponent<EnemyBullet>().SetDirection(dir);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCrashed || enemyType != EnemyType.MeleeOrVehicle)
            return;

        // Hỗ trợ collider Player nằm ở object con.
        Transform hit = collision.collider.transform;
        while (hit != null && !hit.CompareTag("Player"))
            hit = hit.parent;

        if (hit == null) return;

        hasCrashed = true;

        var damageable = collision.gameObject.GetComponent<MetalRebels.Core.Interfaces.IDamageable>()
                      ?? hit.GetComponentInParent<MetalRebels.Core.Interfaces.IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(contactDamage);
        }

        // Dừng AI và vật lý ngay, không tiếp tục đẩy player.
        enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Gọi xử lý chết có sẵn: tạo hiệu ứng, rơi đồ và hủy xe.
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(health.currentHP);
        else
            Destroy(gameObject);
    }
}
