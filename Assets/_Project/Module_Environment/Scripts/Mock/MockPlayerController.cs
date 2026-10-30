using System.Collections;
using UnityEngine;
using MetalRebels.Core.Interfaces;
using MetalRebels.Core.Events;

namespace MetalRebels.Module_Environment.Mock
{
    /// <summary>
    /// Mock Player Controller phục vụ kiểm thử độc lập cho Module_Environment.
    /// Cho phép di chuyển, nhảy, bắn đạn, nhặt vật phẩm qua IPickupCollector,
    /// nhận sát thương qua IDamageable và hồi sinh tại Checkpoint.
    /// Sẽ được thay thế bởi Marco.prefab hoàn chỉnh từ Module_Player khi Việt Hoàng bàn giao.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class MockPlayerController : MonoBehaviour, IPickupCollector, IDamageable
    {
        [Header("Chỉ số Di chuyển")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Chiến đấu & Bắn đạn")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.2f;

        [Header("Máu & Mạng sống")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int lives = 3;
        [SerializeField] private float invulnerabilityDuration = 1f;

        [Header("Kho đạn & Vật phẩm")]
        [SerializeField] private int shotgunAmmo = 0;
        [SerializeField] private int rifleAmmo = 30;
        [SerializeField] private int rocketAmmo = 0;
        [SerializeField] private int grenadeCount = 3;
        [SerializeField] private bool hasRocketLauncher = false;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer sr;
        private float facingDirection = 1f;
        private float nextFireTime = 0f;
        private bool isInvulnerable = false;
        private bool isGrounded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            tag = "Player";
        }

        private void Start()
        {
            // Báo thông số ban đầu qua PlayerEvents
            PlayerEvents.OnHealthChanged?.Invoke(currentHealth, maxHealth);
            PlayerEvents.OnLivesChanged?.Invoke(lives);
            PlayerEvents.OnAmmoChanged?.Invoke(rifleAmmo, 120);
        }

        private void Update()
        {
            HandleMovementInput();
            HandleShooting();
        }

        private void HandleMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");

            // Di chuyển
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

            // Quay mặt theo hướng di chuyển
            if (horizontal != 0)
            {
                facingDirection = Mathf.Sign(horizontal);
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
            }

            // Kiểm tra chạm đất
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, col.bounds.extents.y + 0.15f);

            // Nhảy
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        private void HandleShooting()
        {
            // Bắn đạn bằng phím J hoặc Click chuột trái
            if ((Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }

        private void Shoot()
        {
            if (bulletPrefab == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + new Vector3(facingDirection * 0.6f, 0.2f, 0f);
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            var mockBullet = bullet.GetComponent<MockBullet>();
            if (mockBullet != null)
            {
                mockBullet.SetDirection(facingDirection);
            }
        }

        #region Hợp đồng IDamageable
        public void TakeDamage(int damageAmount)
        {
            if (isInvulnerable || currentHealth <= 0) return;

            currentHealth -= damageAmount;
            if (currentHealth < 0) currentHealth = 0;

            PlayerEvents.OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));
            }
        }

        private void Die()
        {
            lives--;
            PlayerEvents.OnLivesChanged?.Invoke(lives);
            PlayerEvents.OnPlayerDied?.Invoke();

            if (lives > 0)
            {
                Respawn();
            }
            else
            {
                GameEvents.OnGameOver?.Invoke(false);
                gameObject.SetActive(false);
                Debug.Log("[MockPlayer] Hết mạng - GAME OVER!");
            }
        }

        private void Respawn()
        {
            currentHealth = maxHealth;
            Vector3 respawnPos = Checkpoint.HasActiveCheckpoint
                ? Checkpoint.LastActiveCheckpointPosition
                : Vector3.zero;

            var levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                respawnPos = levelManager.GetRespawnPosition();
            }

            transform.position = respawnPos;
            rb.linearVelocity = Vector2.zero;

            PlayerEvents.OnHealthChanged?.Invoke(currentHealth, maxHealth);
            PlayerEvents.OnPlayerRespawned?.Invoke();

            StartCoroutine(InvulnerabilityRoutine(2f));
            Debug.Log($"[MockPlayer] Đã hồi sinh tại vị trí: {respawnPos}");
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            isInvulnerable = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (sr != null)
                {
                    sr.color = new Color(1f, 1f, 1f, 0.3f);
                    yield return new WaitForSeconds(0.1f);
                    sr.color = Color.white;
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    yield return new WaitForSeconds(0.2f);
                }
                elapsed += 0.2f;
            }

            if (sr != null) sr.color = Color.white;
            isInvulnerable = false;
        }
        #endregion

        #region Hợp đồng IPickupCollector
        public bool TryCollect(PickupType pickupType, int amount)
        {
            switch (pickupType)
            {
                case PickupType.Health:
                    if (currentHealth >= maxHealth) return false;
                    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
                    PlayerEvents.OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    Debug.Log($"[MockPlayer] Nhặt HP +{amount}. Máu hiện tại: {currentHealth}/{maxHealth}");
                    return true;

                case PickupType.ShotgunAmmo:
                    shotgunAmmo += amount;
                    Debug.Log($"[MockPlayer] Nhặt Đạn Shotgun +{amount}. Tổng: {shotgunAmmo}");
                    return true;

                case PickupType.RifleAmmo:
                    rifleAmmo += amount;
                    PlayerEvents.OnAmmoChanged?.Invoke(rifleAmmo, 120);
                    Debug.Log($"[MockPlayer] Nhặt Đạn Súng trường +{amount}. Tổng: {rifleAmmo}");
                    return true;

                case PickupType.RocketWeapon:
                    hasRocketLauncher = true;
                    rocketAmmo += amount;
                    PlayerEvents.OnWeaponChanged?.Invoke("Rocket Launcher", rocketAmmo);
                    Debug.Log($"[MockPlayer] Mở khóa Súng Rocket! Đạn: {rocketAmmo}");
                    return true;

                case PickupType.RocketAmmo:
                    rocketAmmo += amount;
                    Debug.Log($"[MockPlayer] Nhặt Đạn Rocket +{amount}. Tổng: {rocketAmmo}");
                    return true;

                case PickupType.Grenade:
                    grenadeCount += amount;
                    Debug.Log($"[MockPlayer] Nhặt Lựu đạn +{amount}. Tổng: {grenadeCount}");
                    return true;

                default:
                    return false;
            }
        }
        #endregion
    }
}
