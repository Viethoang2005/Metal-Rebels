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
        [SerializeField, Min(0f)] private float moveSpeed = 3.2f;
        [SerializeField, Min(0f)] private float jumpForce = 7.2f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("Điều chỉnh cung nhảy")]
        [SerializeField, Range(0.1f, 1f)] private float fallingGravityMultiplier = 0.65f;
        [SerializeField, Min(0.1f)] private float maxFallSpeed = 8f;

        [Header("Chỉ số Ngồi & Né tránh")]
        [SerializeField] private Transform ceilingCheck;
        [SerializeField] private float ceilingCheckRadius = 0.12f;

        [Header("Chỉ số Sinh tồn (HP & Mạng)")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float damageInvincibleTime = 1.0f;
        [SerializeField] private float respawnInvincibleTime = 2.0f;

        private Rigidbody2D rb;
        private SpriteRenderer[] characterSpriteRenderers;
        private bool[] originalSpriteRendererStates;
        private Color[] originalSpriteRendererColors;
        private BoxCollider2D boxCollider;
        private CapsuleCollider2D capsuleCollider;
        private PlayerInputHandler inputHandler;
        private PlayerAnimation playerAnimation;
        private PlayerCombat playerCombat;

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
        private bool crouchRequested;
        private bool crouchToggleActive;
        private float jumpBufferTimer;
        private float coyoteTimer;
        private const float JumpBufferDuration = 0.12f;
        private const float CoyoteDuration = 0.12f;
        private readonly Collider2D[] overlapResults = new Collider2D[8];

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
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (groundLayer.value == 0)
            {
                int ground = LayerMask.NameToLayer("Ground");
                int mask = 1;
                if (ground >= 0) mask |= 1 << ground;
                groundLayer = mask;
            }
            characterSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            originalSpriteRendererStates = new bool[characterSpriteRenderers.Length];
            originalSpriteRendererColors = new Color[characterSpriteRenderers.Length];
            for (int i = 0; i < characterSpriteRenderers.Length; i++)
            {
                originalSpriteRendererStates[i] = characterSpriteRenderers[i].enabled;
                originalSpriteRendererColors[i] = characterSpriteRenderers[i].color;
            }
            inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();
            }

            playerAnimation = GetComponent<PlayerAnimation>();
            playerCombat = GetComponent<PlayerCombat>();

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
            UpdateJumpTimers();
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
            Vector2 checkPosition = groundCheck != null
                ? groundCheck.position
                : GetColliderBounds().center + Vector3.down * (GetColliderBounds().extents.y + 0.02f);
            isGrounded = rb.linearVelocity.y <= 0.15f
                && HasValidOverlap(checkPosition, groundCheckRadius, groundLayer);
        }

        private void HandleCrouch()
        {
            bool isRocketEquipped = playerCombat != null
                && playerCombat.IsRocketLauncherEquipped;
            if (isRocketEquipped)
            {
                if (isCrouching)
                {
                    crouchRequested = false;
                    crouchToggleActive = false;
                    isCrouching = false;
                    ApplyColliderSize(false);
                }
                return;
            }

            if (inputHandler.CrouchPressed && (isGrounded || isCrouching))
            {
                crouchToggleActive = !isCrouching;
                crouchRequested = crouchToggleActive;
            }
            else if (!crouchToggleActive)
            {
                crouchRequested = false;
            }

            if (crouchRequested && isGrounded && !isCrouching)
            {
                isCrouching = true;
                ApplyColliderSize(true);
            }

            if (isCrouching
                && !crouchToggleActive
                && !crouchRequested)
            {
                TryStandUp();
            }
        }

        private bool TryStandUp()
        {
            if (HasCeilingObstruction()) return false;
            isCrouching = false;
            crouchRequested = false;
            ApplyColliderSize(false);
            return true;
        }

        private bool HasCeilingObstruction()
        {
            if (ceilingCheck != null)
            {
                return HasValidOverlap(ceilingCheck.position, ceilingCheckRadius, groundLayer);
            }

            Bounds bounds = GetColliderBounds();
            Vector2 position = new Vector2(bounds.center.x, bounds.max.y + ceilingCheckRadius);
            return HasValidOverlap(position, ceilingCheckRadius, groundLayer);
        }

        private bool HasValidOverlap(Vector2 position, float radius, LayerMask layerMask)
        {
            int count = Physics2D.OverlapCircle(position, radius, CreateSolidFilter(layerMask), overlapResults);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = overlapResults[i];
                overlapResults[i] = null;
                if (hit != null && !hit.isTrigger && hit.attachedRigidbody != rb)
                {
                    return true;
                }
            }
            return false;
        }

        private static ContactFilter2D CreateSolidFilter(LayerMask layerMask)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            filter.useTriggers = false;
            return filter;
        }

        private Bounds GetColliderBounds()
        {
            if (boxCollider != null) return boxCollider.bounds;
            if (capsuleCollider != null) return capsuleCollider.bounds;
            return GetComponent<Collider2D>().bounds;
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

        private void UpdateJumpTimers()
        {
            jumpBufferTimer = inputHandler.JumpPressed
                ? JumpBufferDuration
                : Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
            coyoteTimer = isGrounded
                ? CoyoteDuration
                : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        }

        private void HandleJump()
        {
            if (jumpBufferTimer <= 0f || coyoteTimer <= 0f) return;

            if (playerCombat != null
                && playerCombat.IsRocketLauncherEquipped
                && isCrouching)
            {
                crouchRequested = false;
                crouchToggleActive = false;
                isCrouching = false;
                ApplyColliderSize(false);
            }

            if (isCrouching)
            {
                if (!TryStandUp()) return;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
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
            crouchRequested = false;
            crouchToggleActive = false;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
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
                RestoreCharacterSpriteRendererStates();
            }
            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            isInvincible = true;
            float timer = 0f;
            float flashInterval = 0.1f;
            bool characterVisible = true;

            while (timer < duration)
            {
                characterVisible = !characterVisible;
                for (int i = 0; i < characterSpriteRenderers.Length; i++)
                {
                    SpriteRenderer renderer = characterSpriteRenderers[i];
                    if (renderer == null) continue;
                    renderer.enabled = originalSpriteRendererStates[i];
                    Color flashColor = originalSpriteRendererColors[i];
                    flashColor.a = characterVisible ? 1f : 0.35f;
                    renderer.color = flashColor;
                }
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            RestoreCharacterSpriteRendererStates();
            isInvincible = false;
            invincibilityCoroutine = null;
        }

        private void RestoreCharacterSpriteRendererStates()
        {
            for (int i = 0; i < characterSpriteRenderers.Length; i++)
            {
                if (characterSpriteRenderers[i] != null)
                {
                    characterSpriteRenderers[i].enabled = originalSpriteRendererStates[i];
                    characterSpriteRenderers[i].color = originalSpriteRendererColors[i];
                }
            }
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
