using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class TankAI : MonoBehaviour
{
    [Header("Tuần tra quanh vị trí xuất phát")]
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0f)] public float patrolDistance = 3f;

    [Header("Phát hiện player theo khoảng cách")]
    public Transform target;
    [Min(0f)] public float detectRange = 7f;
    [Tooltip("Khoảng cộng thêm trước khi mất mục tiêu, tránh đổi trạng thái liên tục ở biên.")]
    [Min(0f)] public float loseTargetMargin = 0.5f;

    [Header("Bắn pháo")]
    public MortarBullet bulletPrefab;
    public Transform firePoint;
    [Header("Hiệu ứng lóe lửa nòng pháo")]
    public GameObject muzzleEffectPrefab;
    [Tooltip("Điểm con của FirePoint. Căn vị trí, góc và scale theo hướng xe lúc chưa Play.")]
    public Transform muzzleFXPoint;
    [Tooltip("Góc nòng khi xe nhìn phải. Prefab hiệu ứng gốc hướng sang phải (+X).")]
    [Range(0f, 90f)] public float muzzleAngle = 60f;
    [Header("Animation bắn: Bool isShooting, event Shoot")]
    public Animator animator;

    [Header("Hướng hình ảnh")]
    public SpriteRenderer bodySprite;
    public bool spriteFacesLeft = true;
    public Transform smokePoint;

    private Rigidbody2D rb;
    private EnemyHealth health;
    private float startX;
    private int patrolDirection = 1;
    private bool attacking;
    private static readonly int ShootingParameter = Animator.StringToHash("isShooting");
    private bool hasShootingParameter;
    private float nextTargetSearchTime;
    private float fireOffsetX;
    private float smokeOffsetX;
    private bool facingRight = true;
    private bool muzzleReferenceFacesRight;
    private bool isApproaching;
    private float approachTargetX;
    private Vector3 muzzleReferencePosition;
    private Vector3 muzzleReferenceDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == ShootingParameter && parameter.type == AnimatorControllerParameterType.Bool)
                    hasShootingParameter = true;
            }
        }
        if (!hasShootingParameter)
            Debug.LogWarning("TankAI cần Animator có Bool isShooting và clip bắn có event Shoot.", this);
        startX = rb.position.x;
        if (bodySprite == null) bodySprite = GetComponentInChildren<SpriteRenderer>();
        if (muzzleFXPoint != null && firePoint != null && muzzleFXPoint.parent == firePoint)
        {
            muzzleReferencePosition = muzzleFXPoint.localPosition;
            muzzleReferenceDirection = firePoint.InverseTransformDirection(muzzleFXPoint.right);
            bool localRightIsWorldRight = bodySprite == null ||
                bodySprite.transform.TransformVector(Vector3.right).x >= 0f;
            bool spriteLooksLocalRight = spriteFacesLeft ? bodySprite != null && bodySprite.flipX :
                bodySprite == null || !bodySprite.flipX;
            muzzleReferenceFacesRight = spriteLooksLocalRight == localRightIsWorldRight;
        }
        if (rb.interpolation == RigidbodyInterpolation2D.None)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (firePoint != null) fireOffsetX = Mathf.Abs(firePoint.localPosition.x);
        if (smokePoint != null) smokeOffsetX = Mathf.Abs(smokePoint.localPosition.x);
        SetFacing(true);
    }

    /// <summary>
    /// Gọi sau khi spawn trong Event Lock để tank lái hẳn vào khung hình trước khi bắn/tuần tra.
    /// </summary>
    public void DriveToPosition(float targetX)
    {
        isApproaching = true;
        approachTargetX = targetX;
        if (rb != null)
        {
            patrolDirection = targetX >= rb.position.x ? 1 : -1;
        }
    }

    void FixedUpdate()
    {
        if (health.currentHP <= 0)
        {
            attacking = false;
            isApproaching = false;
            UpdateShootingAnimation();
            MoveHorizontal(0f);
            return;
        }

        // Tìm lại khi player được tạo mới hoặc hồi sinh.
        if ((target == null || !target.gameObject.activeInHierarchy) &&
            Time.time >= nextTargetSearchTime)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            target = found != null ? found.transform : null;
            nextTargetSearchTime = Time.time + 0.5f;
        }

        // Phase tiếp cận: lái vào arena trước khi tuần tra/tấn công
        if (isApproaching)
        {
            attacking = false;
            UpdateShootingAnimation();
            float dist = Mathf.Abs(rb.position.x - approachTargetX);
            if (dist <= 0.25f)
            {
                isApproaching = false;
                startX = rb.position.x; // Cập nhật tâm tuần tra = vị trí đã đến
                MoveHorizontal(0f);
            }
            else
            {
                float dir = approachTargetX > rb.position.x ? 1f : -1f;
                SetFacing(dir > 0f);
                float step = dir * moveSpeed * Time.fixedDeltaTime;
                // Không vượt quá đích
                if (Mathf.Abs(step) > dist) step = dir * dist;
                MoveHorizontal(step);
                UpdateSmoke(true);
                return;
            }
        }

        float range = Mathf.Max(0f, detectRange) +
            (attacking ? Mathf.Max(0f, loseTargetMargin) : 0f);
        attacking = target != null && target.gameObject.activeInHierarchy &&
            Vector2.Distance(rb.position, target.position) <= range;
        UpdateShootingAnimation();

        if (attacking)
        {
            MoveHorizontal(0f);
            UpdateSmoke(false);
            float difference = target.position.x - rb.position.x;
            if (Mathf.Abs(difference) > 0.2f) SetFacing(difference > 0f);

            // Đạn chỉ được tạo tại frame có Animation Event.
            return;
        }

        Patrol();
    }

    void UpdateShootingAnimation()
    {
        if (animator != null && hasShootingParameter)
            animator.SetBool(ShootingParameter, attacking);
    }

    // Đặt một event gọi hàm này tại frame nòng pháo lóe lửa.
    public void Shoot()
    {
        if (!isActiveAndEnabled || !attacking || health == null || health.currentHP <= 0)
            return;
        if (target == null || !target.gameObject.activeInHierarchy || bulletPrefab == null || firePoint == null)
            return;
        float range = Mathf.Max(0f, detectRange) + Mathf.Max(0f, loseTargetMargin);
        if (Vector2.Distance(rb.position, target.position) > range) return;

        float difference = target.position.x - rb.position.x;
        if (Mathf.Abs(difference) > 0.2f) SetFacing(difference > 0f);
        // Ngắm vị trí hiện tại: player có thể né sau khi pháo khai hỏa.
        Vector2 aimPosition = target.position;
        MortarBullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Launch(aimPosition, transform);
        PlayMuzzleEffect();
    }

    void PlayMuzzleEffect()
    {
        if (muzzleEffectPrefab == null) return;

        if (muzzleFXPoint != null)
        {
            GameObject effect = Instantiate(muzzleEffectPrefab, muzzleFXPoint.position, muzzleFXPoint.rotation);
            effect.transform.localScale = Vector3.Scale(effect.transform.localScale, muzzleFXPoint.lossyScale);
            return;
        }

        float angle = facingRight ? muzzleAngle : 180f - muzzleAngle;
        Instantiate(muzzleEffectPrefab, firePoint.position,
            Quaternion.Euler(0f, 0f, angle));
    }

    void Patrol()
    {
        UpdateSmoke(patrolDistance > 0f && moveSpeed > 0f);
        if (patrolDistance <= 0f || moveSpeed <= 0f)
        {
            MoveHorizontal(0f);
            return;
        }

        float tolerance = Mathf.Min(0.05f, patrolDistance * 0.5f);
        if (patrolDirection > 0 && rb.position.x >= startX + patrolDistance - tolerance)
            patrolDirection = -1;
        else if (patrolDirection < 0 && rb.position.x <= startX - patrolDistance + tolerance)
            patrolDirection = 1;

        float destination = startX + patrolDirection * patrolDistance;
        float nextX = Mathf.MoveTowards(rb.position.x, destination, moveSpeed * Time.fixedDeltaTime);
        float step = nextX - rb.position.x;
        if (step != 0f) SetFacing(step > 0f);
        MoveHorizontal(step);
    }

    void MoveHorizontal(float step)
    {
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(step / Time.fixedDeltaTime, rb.linearVelocity.y);
        else if (rb.bodyType == RigidbodyType2D.Kinematic)
            rb.MovePosition(rb.position + Vector2.right * step);
    }

    void SetFacing(bool lookRight)
    {
        facingRight = lookRight;
        if (bodySprite != null)
        {
            bool localRightIsWorldRight = bodySprite.transform.TransformVector(Vector3.right).x >= 0f;
            bool lookLocalRight = lookRight == localRightIsWorldRight;
            bodySprite.flipX = spriteFacesLeft ? lookLocalRight : !lookLocalRight;
        }

        PositionAttachment(firePoint, fireOffsetX, lookRight);
        PositionAttachment(smokePoint, smokeOffsetX, !lookRight);
        UpdateMuzzlePoint(lookRight);
    }

    void UpdateMuzzlePoint(bool lookRight)
    {
        if (muzzleFXPoint == null || firePoint == null || muzzleFXPoint.parent != firePoint) return;

        Vector3 offset = firePoint.TransformVector(muzzleReferencePosition);
        Vector3 direction = firePoint.TransformDirection(muzzleReferenceDirection);
        if (lookRight != muzzleReferenceFacesRight)
        {
            offset.x = -offset.x;
            direction.x = -direction.x;
        }
        muzzleFXPoint.position = firePoint.position + offset;
        muzzleFXPoint.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    void PositionAttachment(Transform attachment, float offsetX, bool onWorldRight)
    {
        if (attachment == null) return;
        bool parentRightIsWorldRight = attachment.parent == null ||
            attachment.parent.TransformVector(Vector3.right).x >= 0f;
        Vector3 position = attachment.localPosition;
        position.x = onWorldRight == parentRightIsWorldRight ? offsetX : -offsetX;
        attachment.localPosition = position;
    }

    void OnDisable()
    {
        UpdateSmoke(false);
        attacking = false;
        isApproaching = false;
        UpdateShootingAnimation();
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    void UpdateSmoke(bool isMoving)
    {
        if (smokePoint != null && smokePoint.gameObject.activeSelf != isMoving){
            smokePoint.gameObject.SetActive(isMoving);
        }
    }
}
