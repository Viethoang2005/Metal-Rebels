using System.Collections;
using MetalRebels.Core.Events;
using MetalRebels.Core.Interfaces;
using UnityEngine;

namespace MetalRebels.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Chỉ số di chuyển")]
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float jumpForce = 7.5f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.08f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Điều chỉnh cung nhảy")]
        [SerializeField, Range(0.1f, 1f)] private float fallingGravityMultiplier = 0.65f;
        [SerializeField, Min(0.1f)] private float maxFallSpeed = 8f;

        [Header("Chỉ số Ngồi & Né tránh")]
        [SerializeField] private Transform ceilingCheck;
        [SerializeField] private float ceilingCheckRadius = 0.2f;

        [Header("Chỉ số Sinh tồn (HP & Mạng)")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float damageInvincibleTime = 1.0f;
        [SerializeField] private float respawnInvincibleTime = 2.0f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;
        private CapsuleCollider2D capsuleCollider;
        private PlayerInputHandler inputHandler;
        private PlayerAnimation playerAnimation;

        private int currentHP;
        private int currentLives;
        private bool isGrounded;
        private bool isCrouching;
        private bool isInvincible;
        private bool isAlive = true;
        private float facingDirection = 1f;
        private Vector3 respawnPosition;
        private Coroutine invincibilityCoroutine;
        private float defaultGravityScale;

        // Lưu kích thước collider ban đầu để thu nhỏ khi ngồi
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;

        // Public properties
        public bool IsCrouching => isCrouching;
        public bool IsGrounded => isGrounded;
        public bool IsInvincible => isInvincible;
        public bool IsAlive => isAlive;
        public float FacingDirection => facingDirection;
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public int CurrentLives => currentLives;
        public bool CanShoot => isAlive && !isInvincible;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultGravityScale = rb.gravityScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();
            }

            playerAnimation = GetComponent<PlayerAnimation>();

            boxCollider = GetComponent<BoxCollider2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();

            if (boxCollider != null)
            {
                originalColliderSize = boxCollider.size;
                originalColliderOffset = boxCollider.offset;
            }
            else if (capsuleCollider != null)
            {
                originalColliderSize = capsuleCollider.size;
                originalColliderOffset = capsuleCollider.offset;
            }
        }

        private void Start()
        {
            currentHP = maxHP;
            currentLives = maxLives;
            respawnPosition = transform.position;
            isAlive = true;

            PlayerEvents.OnHealthChanged?.Invoke(currentHP, maxHP);
            PlayerEvents.OnLivesChanged?.Invoke(currentLives);
        }

        private void Update()
        {
            if (!isAlive) return;

            CheckGrounded();
            HandleCrouch();
            HandleJump();
        }

        private void FixedUpdate()
        {
            if (!isAlive) return;

            ApplyFallGravity();
            HandleMovement();
        }

        private void ApplyFallGravity()
        {
            if (rb.linearVelocity.y < 0f)
            {
                rb.gravityScale = defaultGravityScale * fallingGravityMultiplier;
                if (rb.linearVelocity.y < -maxFallSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
                }
            }
            else
            {
                rb.gravityScale = defaultGravityScale;
            }
        }

        private void CheckGrounded()
        {
            Vector2 checkPosition;
            bool hasGroundCheck = false;

            if (boxCollider != null)
            {
                Bounds bounds = boxCollider.bounds;
                checkPosition = new Vector2(bounds.center.x, bounds.min.y + groundCheckRadius * 0.75f);
                hasGroundCheck = true;
            }
            else if (capsuleCollider != null)
            {
                Bounds bounds = capsuleCollider.bounds;
                checkPosition = new Vector2(bounds.center.x, bounds.min.y + groundCheckRadius * 0.75f);
                hasGroundCheck = true;
            }
            else if (groundCheck != null)
            {
                checkPosition = groundCheck.position;
                hasGroundCheck = true;
            }
            else
            {
                checkPosition = (Vector2)transform.position + Vector2.down * 0.9f;
                isGrounded = rb.linearVelocity.y <= 0.01f
                    && Physics2D.Raycast(checkPosition, Vector2.down, 0.2f, groundLayer);
                return;
            }

            isGrounded = rb.linearVelocity.y <= 0.01f
                && hasGroundCheck
                && Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);
        }

        private void HandleCrouch()
        {
            bool wantsToCrouch = isGrounded && inputHandler.CrouchHeld;

            if (wantsToCrouch && !isCrouching)
            {
                // Bắt đầu ngồi xuống: giảm chiều cao collider đi một nửa để né đạn tầm cao
                isCrouching = true;
                ApplyColliderSize(isCrouching);
            }
            else if (!wantsToCrouch && isCrouching)
            {
                // Kiểm tra trần nhà trước khi đứng lên
                bool hasCeiling = false;
                if (ceilingCheck != null)
                {
                    hasCeiling = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
                }

                if (!hasCeiling)
                {
                    isCrouching = false;
                    ApplyColliderSize(isCrouching);
                }
            }
        }

        private void ApplyColliderSize(bool crouching)
        {
            if (boxCollider != null)
            {
                if (crouching)
                {
                    boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
                    boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y * 0.25f));
                }
                else
                {
                    boxCollider.size = originalColliderSize;
                    boxCollider.offset = originalColliderOffset;
                }
            }
            else if (capsuleCollider != null)
            {
                if (crouching)
                {
                    capsuleCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
                    capsuleCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y * 0.25f));
                }
                else
                {
                    capsuleCollider.size = originalColliderSize;
                    capsuleCollider.offset = originalColliderOffset;
                }
            }
        }

        private void HandleJump()
        {
            if (inputHandler.JumpPressed && isGrounded && !isCrouching)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
            }
        }

        private void HandleMovement()
        {
            float horizontal = inputHandler.MoveInput.x;

            // Khi ngồi không thể di chuyển
            if (isCrouching)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                playerAnimation?.SetMovementSpeed(0f);
                return;
            }

            // Xoay hướng mặt
            if (Mathf.Abs(horizontal) > 0.01f)
            {
                facingDirection = Mathf.Sign(horizontal);
                transform.localScale = new Vector3(facingDirection, 1f, 1f);
            }

            // Vận tốc ngang
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
            playerAnimation?.SetMovementSpeed(Mathf.Abs(horizontal));
        }

        public void TakeDamage(int damageAmount)
        {
            if (!isAlive || isInvincible || damageAmount <= 0) return;

            currentHP = Mathf.Max(0, currentHP - damageAmount);
            PlayerEvents.OnHealthChanged?.Invoke(currentHP, maxHP);
            playerAnimation?.PlayHurtAnimation();

            if (currentHP <= 0)
            {
                HandleDeath();
            }
            else
            {
                StartInvincibility(damageInvincibleTime);
            }
        }

        private void HandleDeath()
        {
            currentLives--;
            PlayerEvents.OnLivesChanged?.Invoke(currentLives);

            if (currentLives > 0)
            {
                Respawn();
            }
            else
            {
                isAlive = false;
                rb.linearVelocity = Vector2.zero;
                PlayerEvents.OnPlayerDied?.Invoke();
                GameEvents.OnGameOver?.Invoke(false);
                gameObject.SetActive(false);
            }
        }

        public void Respawn()
        {
            transform.position = respawnPosition;
            currentHP = maxHP;
            rb.linearVelocity = Vector2.zero;
            isAlive = true;
            isCrouching = false;
            ApplyColliderSize(false);

            PlayerEvents.OnHealthChanged?.Invoke(currentHP, maxHP);
            PlayerEvents.OnPlayerRespawned?.Invoke();
            StartInvincibility(respawnInvincibleTime);
        }

        public void SetCheckpoint(Vector3 newCheckpoint)
        {
            respawnPosition = newCheckpoint;
        }

        public bool TryHeal(int amount)
        {
            if (!isAlive || currentHP >= maxHP) return false;

            currentHP = Mathf.Min(currentHP + amount, maxHP);
            PlayerEvents.OnHealthChanged?.Invoke(currentHP, maxHP);
            return true;
        }

        private void StartInvincibility(float duration)
        {
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
            }
            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            isInvincible = true;
            float timer = 0f;
            float flashInterval = 0.1f;

            while (timer < duration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            isInvincible = false;
            invincibilityCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D groundCollider = boxCollider;
            if (groundCollider == null) groundCollider = capsuleCollider;
            if (groundCollider == null) groundCollider = GetComponent<Collider2D>();
            if (groundCollider != null)
            {
                Bounds bounds = groundCollider.bounds;
                Vector3 position = new Vector3(bounds.center.x, bounds.min.y + groundCheckRadius * 0.75f, bounds.center.z);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, groundCheckRadius);
            }
            else if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
            if (ceilingCheck != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
            }
        }
    }
}
