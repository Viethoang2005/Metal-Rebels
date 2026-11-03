using UnityEngine;

namespace MetalRebels.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("Tham chiếu 2 Animator Thân & Chân")]
        [SerializeField] private Animator upperAnimator; // Animator của UpperBody
        [SerializeField] private Animator lowerAnimator; // Animator của LowerBody

        private PlayerController playerController;
        private PlayerInputHandler inputHandler;
        private PlayerCombat playerCombat;
        private SpriteRenderer upperBodyRenderer;
        private SpriteRenderer lowerBodyRenderer;
        private Vector3 upperBodyRestPosition;
        private bool hasUpperBodyRestPosition;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            inputHandler = GetComponent<PlayerInputHandler>();
            playerCombat = GetComponent<PlayerCombat>();

            // Tự động tìm Animator ở con nếu chưa kéo trong Inspector
            if (upperAnimator == null)
            {
                Transform upperTransform = transform.Find("Visuals/UpperBody");
                if (upperTransform != null) upperAnimator = upperTransform.GetComponent<Animator>();
            }

            if (lowerAnimator == null)
            {
                Transform lowerTransform = transform.Find("Visuals/LowerBody");
                if (lowerTransform != null) lowerAnimator = lowerTransform.GetComponent<Animator>();
            }

            if (lowerAnimator != null)
            {
                lowerBodyRenderer = lowerAnimator.GetComponent<SpriteRenderer>();
            }

            if (upperAnimator != null)
            {
                upperBodyRenderer = upperAnimator.GetComponent<SpriteRenderer>();
                upperBodyRestPosition = upperAnimator.transform.localPosition;
                hasUpperBodyRestPosition = true;
            }
        }

        private void LateUpdate()
        {
            if (playerController == null) return;

            UpdateUpperBodyAnimation();
            UpdateLowerBodyAnimation();
            UpdateFullBodyPosition();
            UpdateLowerBodyVisibility();
        }

        /// <summary>
        /// Cập nhật các Parameter cho Thân trên (UpperBody)
        /// </summary>
        private void UpdateUpperBodyAnimation()
        {
            if (upperAnimator == null) return;

            if (inputHandler != null)
            {
                // Cập nhật hướng ngắm lên/xuống cho thân trên
                SetBoolIfAvailable(upperAnimator, "IsAimingUp", inputHandler.IsAimingUp);
                SetBoolIfAvailable(upperAnimator, "IsAimingDown", inputHandler.IsAimingDown);
            }

            SetBoolIfAvailable(upperAnimator, "IsCrouching", playerController.IsCrouching);
            SetBoolIfAvailable(upperAnimator, "IsGrounded", playerController.IsGrounded);
            SetBoolIfAvailable(upperAnimator, "IsShotgun", playerCombat != null && playerCombat.IsShotgunEquipped);
        }

        /// <summary>
        /// Cập nhật các Parameter cho Chân (LowerBody)
        /// </summary>
        private void UpdateLowerBodyAnimation()
        {
            if (lowerAnimator == null) return;

            // Truyền trạng thái chạm đất & ngồi
            SetBoolIfAvailable(lowerAnimator, "IsGrounded", playerController.IsGrounded);
            SetBoolIfAvailable(lowerAnimator, "IsCrouching", playerController.IsCrouching);
        }

        private void UpdateLowerBodyVisibility()
        {
            if (lowerBodyRenderer == null) return;

            lowerBodyRenderer.enabled = !UsesFullBodySprite();
        }

        private void UpdateFullBodyPosition()
        {
            if (upperAnimator == null || lowerAnimator == null || !hasUpperBodyRestPosition) return;

            bool usesFullBodySprite = UsesFullBodySprite();
            Vector3 position = upperBodyRestPosition;
            if (usesFullBodySprite)
            {
                position.y = lowerAnimator.transform.localPosition.y;
                if (UsesFullBodyShotgunAim())
                {
                    position.x = lowerAnimator.transform.localPosition.x;
                }
            }
            else if (upperBodyRenderer != null
                && lowerBodyRenderer != null
                && upperBodyRenderer.sprite != null
                && lowerBodyRenderer.sprite != null
                && TryGetUpperBodyAnchor(upperBodyRenderer.sprite, out Vector2 upperAnchorPixels))
            {
                Bounds lowerBounds = lowerBodyRenderer.sprite.bounds;
                Vector3 lowerAnchor = lowerAnimator.transform.localPosition
                    + new Vector3(
                        lowerBounds.center.x,
                        lowerBounds.max.y,
                        0f);
                Vector2 upperAnchor = (upperAnchorPixels - upperBodyRenderer.sprite.pivot)
                    / upperBodyRenderer.sprite.pixelsPerUnit;
                position = lowerAnchor - new Vector3(upperAnchor.x, upperAnchor.y, 0f);
            }

            upperAnimator.transform.localPosition = position;
        }

        private bool UsesFullBodyShotgunAim()
        {
            if (upperAnimator == null) return false;

            if (IsFullBodyShotgunAim(upperAnimator.GetCurrentAnimatorStateInfo(0))) return true;
            return upperAnimator.IsInTransition(0)
                && IsFullBodyShotgunAim(upperAnimator.GetNextAnimatorStateInfo(0));
        }

        private static bool IsFullBodyShotgunAim(AnimatorStateInfo state)
        {
            return state.IsName("Upper_Idle_Shotgun_ShootUp");
        }

        private static bool TryGetUpperBodyAnchor(Sprite sprite, out Vector2 anchorPixels)
        {
            // Sprite-local pixel coordinates for the waist seam, measured from each frame's lower-left.
            switch (sprite.name)
            {
                case "Idle_ShootDown_0":
                    anchorPixels = new Vector2(11f, 15f);
                    return true;
                case "Idle_ShootUp_0":
                    anchorPixels = new Vector2(12f, 3f);
                    return true;
                case "UpperBody_ShootForward_0":
                case "UpperBody_ShootForward_1":
                    anchorPixels = new Vector2(14f, 3f);
                    return true;
                case "UpperBody_ShootForward_2":
                    anchorPixels = new Vector2(14f, 3f);
                    return true;
                case "UpperBody_ShootForward_4":
                case "UpperBody_ShootForward_5":
                case "UpperBody_ShootForward_6":
                case "UpperBody_ShootForward_7":
                    anchorPixels = new Vector2(14f, 3f);
                    return true;
                case "UpperBody_ShootForward_8":
                case "UpperBody_ShootForward_9":
                case "UpperBody_ShootForward_10":
                    anchorPixels = new Vector2(13f, 4f);
                    return true;
                case "UpperBody_ShootForward_Shotgun_0":
                    anchorPixels = new Vector2(18f, 6f);
                    return true;
                case "UpperBody_ShootUp_0":
                    anchorPixels = new Vector2(12f, 3f);
                    return true;
                case "UpperBody_ShootUp_3":
                case "UpperBody_ShootUp_4":
                case "UpperBody_ShootUp_5":
                case "UpperBody_ShootUp_6":
                    anchorPixels = new Vector2(12f, 3f);
                    return true;
                case "UpperBody_Jump_0":
                case "UpperBody_Jump_1":
                case "UpperBody_Jump_2":
                case "UpperBody_Jump_3":
                case "UpperBody_Jump_4":
                case "UpperBody_Jump_5":
                    anchorPixels = new Vector2(17f, 0f);
                    return true;
                default:
                    anchorPixels = Vector2.zero;
                    return false;
            }
        }

        private bool UsesFullBodySprite()
        {
            bool usesFullBodySprite = playerController.IsCrouching;
            if (upperAnimator != null)
            {
                usesFullBodySprite |= IsFullBodyState(upperAnimator.GetCurrentAnimatorStateInfo(0));
                if (upperAnimator.IsInTransition(0))
                {
                    usesFullBodySprite |= IsFullBodyState(upperAnimator.GetNextAnimatorStateInfo(0));
                }
            }

            return usesFullBodySprite;
        }

        private static bool IsFullBodyState(AnimatorStateInfo state)
        {
            return state.IsName("Upper_Crouch")
                || state.IsName("Upper_Crouch_Shoot")
                || state.IsName("Upper_Crouch_Grenade")
                || state.IsName("Upper_Idle_Shotgun_ShootUp")
                || state.IsName("Upper_Victory");
        }

        private static void SetBoolIfAvailable(Animator animator, string parameterName, bool value)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameterName, value);
                    return;
                }
            }
        }

        private static bool HasTriggerParameter(Animator animator, string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Hàm được gọi từ PlayerController để cập nhật tốc độ di chuyển cho chân
        /// </summary>
        public void SetMovementSpeed(float speed)
        {
            if (lowerAnimator != null)
            {
                lowerAnimator.SetFloat("Speed", speed);
            }
        }

        /// <summary>
        /// Hàm được gọi từ PlayerCombat khi người chơi bấm nút Bắn (J)
        /// </summary>
        public void PlayShootAnimation()
        {
            if (upperAnimator != null && HasTriggerParameter(upperAnimator, "Shoot"))
            {
                upperAnimator.SetTrigger("Shoot");
            }
        }

        /// <summary>
        /// Hàm được gọi từ PlayerCombat khi người chơi bấm nút Ném lựu đạn (G)
        /// </summary>
        public void PlayGrenadeAnimation()
        {
            if (playerController != null
                && playerController.IsCrouching
                && upperAnimator != null
                && HasTriggerParameter(upperAnimator, "Grenade"))
            {
                upperAnimator.SetTrigger("Grenade");
            }
        }

        public void PlayChargeAnimation()
        {
            if (playerCombat != null
                && playerCombat.IsShotgunEquipped
                && upperAnimator != null
                && HasTriggerParameter(upperAnimator, "ChargeShotgun"))
            {
                upperAnimator.SetTrigger("ChargeShotgun");
            }
        }

        public void PlayVictoryAnimation()
        {
            if (upperAnimator != null && HasTriggerParameter(upperAnimator, "Victory"))
            {
                upperAnimator.SetTrigger("Victory");
            }
        }

        /// <summary>
        /// Hàm được gọi khi nhân vật bị trúng đạn
        /// </summary>
        public void PlayHurtAnimation()
        {
            if (upperAnimator != null && HasTriggerParameter(upperAnimator, "Hurt")) upperAnimator.SetTrigger("Hurt");
            if (lowerAnimator != null && HasTriggerParameter(lowerAnimator, "Hurt")) lowerAnimator.SetTrigger("Hurt");
        }
    }
}