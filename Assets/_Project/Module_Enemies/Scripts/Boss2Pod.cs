using System.Collections;
using MetalRebels.Core.Interfaces;
using System.Collections.Generic;
using UnityEngine;

// Pod: drone tím chui ra khỏi nắp pháo, bay đuổi Marco, tự nổ sau 4 giây hoặc khi trúng
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(EnemyHealth))]
public class Boss2Pod : MonoBehaviour
{
    [Header("Chui ra khỏi nắp pháo")]
    [Tooltip("Thời gian trượt ra khỏi cửa cannon trước khi bắt đầu đuổi.")]
    [Min(0f)] public float emergeTime = 0.45f;
    [Tooltip("Hướng và quãng đường trượt ra (tính theo local scale của Pod, để dễ lật theo boss).")]
    public Vector3 emergeOffset = new Vector3(0f, -1.2f, 0f);
    [Tooltip("Thời gian đứng yên sau khi chui ra, trước khi đuổi.")]
    [Min(0f)] public float emergePause = 0.2f;

    [Header("Di chuyển")]
    [Min(0.1f)] public float moveSpeed = 2.5f;

    [Header("Tự nổ")]
    [Min(0.1f)] public float selfDestructTime = 4f;
    public GameObject explosionEffectPrefab;
    [Min(0.1f)] public float explosionEffectLifetime = 1f;

    [Header("Va chạm")]
    [Min(0)] public int damage = 25;
    public GameObject impactEffectPrefab;

    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private EnemyHealth health;
    private Animator anim;
    private Transform target;
    private float chaseStartTime;
    private bool exploded = false;
    private bool emerging = true;
    private float nextSearchTime;

    private static readonly int EmergeParam = Animator.StringToHash("Emerge");
    private bool hasEmergeTrigger;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        health = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        circle.isTrigger = true;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in anim.parameters)
                hasEmergeTrigger |= p.nameHash == EmergeParam && p.type == AnimatorControllerParameterType.Trigger;
        }
    }

    void Start()
    {
        StartCoroutine(EmergeRoutine());
    }

    IEnumerator EmergeRoutine()
    {
        // Chưa va chạm trong lúc chui ra để không nổ ngay khi còn trong thân boss.
        circle.enabled = false;
        if (anim != null && hasEmergeTrigger) anim.SetTrigger(EmergeParam);

        Vector3 from = rb.position;
        Vector3 to = from + transform.TransformVector(Vector3.Scale(emergeOffset, transform.localScale));
        float duration = Mathf.Max(0.01f, emergeTime);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            rb.MovePosition(Vector3.Lerp(from, to, k));
            yield return null;
        }
        rb.MovePosition(to);

        yield return new WaitForSeconds(Mathf.Max(0f, emergePause));

        circle.enabled = true;
        emerging = false;
        chaseStartTime = Time.time; // Bắt đầu tính giờ tự nổ từ lúc chui ra xong.
    }

    // Danh sách Pod đang hoạt động, để Boss2AI dọn khi đổi phase mà không cần FindObjectsByType.
    public static readonly List<Boss2Pod> Active = new List<Boss2Pod>();

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    void FixedUpdate()
    {
        // Nếu chết trước, nổ sớm
        if (health.currentHP <= 0)
        {
            if (!exploded) Explode();
            return;
        }

        // Đang chui ra: không đuổi, không tính giờ nổ.
        if (emerging) return;

        // Tìm player
        if ((target == null || !target.gameObject.activeInHierarchy) && Time.time >= nextSearchTime)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            target = found != null ? found.transform : null;
            nextSearchTime = Time.time + 0.5f;
        }

        // Tự nổ sau thời gian quy định
        if (Time.time - chaseStartTime >= selfDestructTime)
        {
            Explode();
            return;
        }

        // Đuổi theo player hoặc bay quanh
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded || emerging || health.currentHP <= 0) return;

        // Chỉ tác động với player hoặc ground
        bool isPlayer = false;
        bool isGround = false;
        for (Transform current = other.transform; current != null; current = current.parent)
        {
            isPlayer |= current.CompareTag("Player");
            isGround |= current.CompareTag("Ground");
        }

        if (isPlayer || isGround)
        {
            if (isPlayer)
            {
                IDamageable receiver = other.GetComponentInParent<IDamageable>();
                if (receiver != null) receiver.TakeDamage(damage);
            }
            Explode();
        }
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, rb.position, Quaternion.identity);
            if (effect.GetComponent<AutoDestroyFX>() == null)
                Destroy(effect, Mathf.Max(0.1f, explosionEffectLifetime));
        }

        Destroy(gameObject);
    }
}
