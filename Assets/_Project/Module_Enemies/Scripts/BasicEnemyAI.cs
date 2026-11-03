using UnityEngine;

public class BasicEnemyAI : MonoBehaviour
{
   public enum EnemyType { RangedSoldier, MeleeOrVehicle }
    public EnemyType enemyType = EnemyType.RangedSoldier;

    [Header("Cấu hình hướng mặt ảnh gốc")]
    [Tooltip("Tích vào đây nếu bức ảnh gốc của con lính đang quay mặt sang bên TRÁI")]
    public bool spriteFacesLeft = true; 

    [Header("Thông số Di chuyển & Phát hiện")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float detectRange = 7f;
    public float attackRange = 5f;

    [Header("Thông số Tấn công")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public int contactDamage = 30;

    private Transform player;
    private Vector2 startPos;
    private int patrolDir = 1;
    private float nextFireTime;

    void Start()
    {
        startPos = transform.position;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange && enemyType == EnemyType.RangedSoldier)
        {
            FaceTarget(player.position);
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else if (dist <= detectRange)
        {
            FaceTarget(player.position);
            Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * 1.3f * Time.deltaTime);
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        transform.Translate(Vector2.right * patrolDir * moveSpeed * Time.deltaTime);
        if (Vector2.Distance(startPos, transform.position) >= patrolDistance)
        {
            patrolDir *= -1;
            FaceTarget((Vector2)transform.position + Vector2.right * patrolDir);
        }
    }

    void FaceTarget(Vector2 target)
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        bool targetIsOnRight = target.x > transform.position.x;

        if (spriteFacesLeft)
            transform.localScale = new Vector3(targetIsOnRight ? -scaleX : scaleX, transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(targetIsOnRight ? scaleX : -scaleX, transform.localScale.y, 1);
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;
        b.GetComponent<EnemyBullet>().SetDirection(dir);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemyType == EnemyType.MeleeOrVehicle && collision.gameObject.CompareTag("Player"))
        {
            var damageable = collision.gameObject.GetComponent<MetalRebels.Core.Interfaces.IDamageable>()
                          ?? collision.gameObject.GetComponentInParent<MetalRebels.Core.Interfaces.IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(contactDamage);
            }
        }
    }
}
