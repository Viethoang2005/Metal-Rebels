using MetalRebels.Core.Interfaces;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Boss1ClusterBullet : MonoBehaviour
{
    [Header("Đạn mẹ: bay lên rồi tách ở đỉnh")]
    [Min(0.1f)] public float arcHeight = 3f;
    public Boss1ClusterBullet fragmentPrefab;
    [Tooltip("Khoảng cách ngang giữa hai điểm rơi liền kề, tại độ cao mục tiêu lúc bắn.")]
    [Min(0f)] public float spreadDistance = 2f;
    public GameObject splitEffectPrefab;

    [Header("Dùng cho cả đạn mẹ và đạn con")]
    [Min(0.1f)] public float gravity = 12f;
    [Min(0.1f)] public float lifeTime = 8f;
    [Min(0)] public int damage = 20;
    public bool rotateAlongVelocity = true;
    public GameObject impactEffectPrefab;
    [Min(0.1f)] public float effectLifetime = 2f;

    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private Transform owner;
    private Vector2 velocity;
    private Vector2 aimPosition;
    private bool launched;
    private bool finished;
    private bool canSplit;
    private float acceleration;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        circle.isTrigger = true;
    }

    void Start()
    {
        Destroy(gameObject, Mathf.Max(0.1f, lifeTime));
    }

    public void Launch(Vector2 targetPosition, Transform shooter)
    {
        if (launched) return;
        aimPosition = targetPosition;
        owner = shooter;
        acceleration = Mathf.Max(0.1f, gravity);
        float apex = Mathf.Max(rb.position.y, targetPosition.y) + Mathf.Max(0.1f, arcHeight);
        float upSpeed = Mathf.Sqrt(2f * acceleration * (apex - rb.position.y));
        float duration = upSpeed / acceleration + Mathf.Sqrt(2f * (apex - targetPosition.y) / acceleration);
        velocity = new Vector2((targetPosition.x - rb.position.x) / duration, upSpeed);
        canSplit = fragmentPrefab != null;
        if (!canSplit)
            Debug.LogWarning("Boss1ClusterBullet chưa gán Fragment Prefab: đạn sẽ bay tiếp mà không tách.", this);
        launched = true;
        UpdateRotation();
    }

    // Đạn con bắt đầu rơi ngay và không bao giờ tách tiếp.
    private void LaunchFragment(Vector2 destination, Transform shooter)
    {
        owner = shooter;
        acceleration = Mathf.Max(0.1f, gravity);
        float drop = Mathf.Max(0.01f, rb.position.y - destination.y);
        float duration = Mathf.Sqrt(2f * drop / acceleration);
        velocity = new Vector2((destination.x - rb.position.x) / duration, 0f);
        canSplit = false;
        launched = true;
        UpdateRotation();
    }

    void FixedUpdate()
    {
        if (!launched || finished) return;
        float dt = Time.fixedDeltaTime;
        bool reachesApex = canSplit && velocity.y <= acceleration * dt;
        // Tính chính xác đoạn tới đỉnh, không để đạn mẹ đi quá đỉnh rồi mới tách.
        if (reachesApex) dt = Mathf.Max(0f, velocity.y / acceleration);
        Vector2 step = velocity * dt + Vector2.down * (0.5f * acceleration * dt * dt);

        Vector2 center = transform.TransformPoint(circle.offset);
        Vector3 scale = transform.lossyScale;
        float radius = circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        RaycastHit2D[] hits = Physics2D.CircleCastAll(center, radius, step.normalized, step.magnitude);
        foreach (RaycastHit2D hit in hits)
        {
            if (TryHit(hit.collider, hit.point)) return;
        }

        Vector2 nextPosition = rb.position + step;
        if (reachesApex)
        {
            Split(nextPosition);
            return;
        }
        rb.MovePosition(nextPosition);
        velocity.y -= acceleration * dt;
        UpdateRotation();
    }

    void Split(Vector2 position)
    {
        if (finished) return;
        finished = true;
        rb.simulated = false;
        SpawnEffect(splitEffectPrefab, position);
        for (int i = -1; i <= 1; i++)
        {
            Boss1ClusterBullet fragment = Instantiate(fragmentPrefab, position, Quaternion.identity);
            Vector2 destination = aimPosition + Vector2.right * (i * Mathf.Max(0f, spreadDistance));
            fragment.LaunchFragment(destination, owner);
        }
        Destroy(gameObject);
    }

    void UpdateRotation()
    {
        if (rotateAlongVelocity && velocity.sqrMagnitude > 0.0001f)
            rb.rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other, other.ClosestPoint(rb.position));
    }

    bool TryHit(Collider2D other, Vector2 position)
    {
        if (!launched || finished || other == null || other.transform.IsChildOf(transform)) return false;
        if (owner != null && other.transform.IsChildOf(owner)) return false;
        if (other.GetComponentInParent<Boss1ClusterBullet>() != null) return false;
        if (Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer) ||
            Physics2D.GetIgnoreCollision(circle, other)) return false;

        bool player = false;
        bool ground = false;
        for (Transform current = other.transform; current != null; current = current.parent)
        {
            player |= current.CompareTag("Player");
            ground |= current.CompareTag("Ground");
        }
        if (!player && !ground) return false;

        finished = true;
        rb.simulated = false;
        if (player)
        {
            IDamageable receiver = other.GetComponentInParent<IDamageable>();
            if (receiver != null) receiver.TakeDamage(Mathf.Max(0, damage));
            else Debug.LogWarning("Player cần component triển khai IDamageable để nhận sát thương.", other);
        }
        SpawnEffect(impactEffectPrefab, position);
        Destroy(gameObject);
        return true;
    }

    void SpawnEffect(GameObject prefab, Vector2 position)
    {
        if (prefab == null) return;
        GameObject effect = Instantiate(prefab, position, Quaternion.identity);
        if (effect.GetComponent<AutoDestroyFX>() == null)
            Destroy(effect, Mathf.Max(0.1f, effectLifetime));
    }
}
