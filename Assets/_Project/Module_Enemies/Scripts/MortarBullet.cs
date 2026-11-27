using MetalRebels.Core.Interfaces;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class MortarBullet : MonoBehaviour
{
    [Header("Quỹ đạo")]
    [Tooltip("Độ cao đỉnh cung so với điểm cao hơn giữa nòng pháo và mục tiêu.")]
    [Min(0.1f)] public float arcHeight = 3f;
    [Min(0.1f)] public float gravity = 12f;
    [Min(0.1f)] public float lifeTime = 8f;
    public bool rotateAlongVelocity = true;

    [Header("Va chạm")]
    [Min(0)] public int damage = 30;
    public GameObject impactEffectPrefab;
    [Min(0.1f)] public float impactEffectLifetime = 2f;

    private Rigidbody2D rb;
    private bool launched;
    private bool hasHit;
    private Transform owner;
    private Vector2 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    void Start()
    {
        Destroy(gameObject, Mathf.Max(0.1f, lifeTime));
    }

    // Chỉ nhận tọa độ tại thời điểm bắn, không giữ Transform để đuổi theo player.
    public void Launch(Vector2 targetPosition, Transform shooter)
    {
        if (launched) return;
        owner = shooter;
        float acceleration = Mathf.Max(0.1f, gravity);
        float apexY = Mathf.Max(rb.position.y, targetPosition.y) + Mathf.Max(0.1f, arcHeight);
        float upwardSpeed = Mathf.Sqrt(2f * acceleration * (apexY - rb.position.y));
        float flightTime = upwardSpeed / acceleration +
            Mathf.Sqrt(2f * (apexY - targetPosition.y) / acceleration);
        velocity = new Vector2((targetPosition.x - rb.position.x) / flightTime, upwardSpeed);
        launched = true;
        UpdateRotation();
    }

    void FixedUpdate()
    {
        if (!launched || hasHit) return;

        float dt = Time.fixedDeltaTime;
        Vector2 acceleration = Vector2.down * Mathf.Max(0.1f, gravity);
        Vector2 step = velocity * dt + 0.5f * acceleration * dt * dt;

        // Quét cả bước bay để giảm bỏ sót player/collider mỏng khi đạn bay nhanh.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(rb.position,
            GetWorldRadius(), step.normalized, step.magnitude);
        foreach (RaycastHit2D hit in hits)
        {
            if (TryHit(hit.collider, hit.point)) return;
        }

        rb.MovePosition(rb.position + step);
        velocity += acceleration * dt;
        UpdateRotation();
    }

    float GetWorldRadius()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        Vector3 scale = transform.lossyScale;
        return circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
    }

    void UpdateRotation()
    {
        if (rotateAlongVelocity && velocity.sqrMagnitude > 0.0001f)
            rb.rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other, rb.position);
    }

    bool TryHit(Collider2D other, Vector2 point)
    {
        if (!launched || hasHit || other == null || other.transform.IsChildOf(transform)) return false;
        if (owner != null && other.transform.IsChildOf(owner)) return false;
        if (Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)) return false;

        bool isPlayer = false;
        bool isGround = false;
        for (Transform current = other.transform; current != null; current = current.parent)
        {
            isPlayer |= current.CompareTag("Player");
            isGround |= current.CompareTag("Ground");
        }
        if (!isPlayer && !isGround) return false;

        hasHit = true;
        if (isPlayer)
        {
            IDamageable receiver = other.GetComponentInParent<IDamageable>();
            if (receiver != null) receiver.TakeDamage(damage);
            else Debug.LogWarning("Đạn cối trúng Player nhưng chưa có component triển khai IDamageable để trừ HP.", other);
        }

        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, point, Quaternion.identity);
            Destroy(effect, Mathf.Max(0.1f, impactEffectLifetime));
        }
        rb.simulated = false;
        Destroy(gameObject);
        return true;
    }
}
