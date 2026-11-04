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
            Sprite upperSprite = upperBodyRenderer != null ? upperBodyRenderer.sprite : null;
            Sprite lowerSprite = lowerBodyRenderer != null ? lowerBodyRenderer.sprite : null;
            if (usesFullBodySprite)
            {
                position = GetFullBodySpritePosition();
            }
            else if (IsShootDownUpperSprite(upperSprite) && lowerBodyRenderer != null)
            {
                // Never follow run-hip anchors for shoot-down. Keep the same pose as standing aim-down.
                position = GetShootDownLockedPosition(upperSprite);
            }
            else if (upperSprite != null
                && lowerSprite != null
                && TryGetUpperBodyAnchor(upperSprite, out Vector2 upperAnchorPixels))
            {
                Vector3 lowerAnchorLocal;
                if (TryGetLowerBodyAnchor(lowerSprite, out Vector2 lowerAnchorPixels))
                {
                    lowerAnchorLocal = (lowerAnchorPixels - lowerSprite.pivot) / lowerSprite.pixelsPerUnit;
                }
                else
                {
                    Bounds lowerBounds = lowerSprite.bounds;
                    lowerAnchorLocal = new Vector3(lowerBounds.center.x, lowerBounds.max.y, 0f);
                }

                Vector3 upperAnchorLocal = (upperAnchorPixels - upperSprite.pivot) / upperSprite.pixelsPerUnit;
                Transform positionParent = upperAnimator.transform.parent;
                Vector3 lowerAnchorWorld = lowerBodyRenderer.transform.TransformPoint(lowerAnchorLocal);
                Vector3 upperAnchorOffsetWorld = upperBodyRenderer.transform.TransformVector(upperAnchorLocal);
                Vector3 lowerAnchorPosition = positionParent != null
                    ? positionParent.InverseTransformPoint(lowerAnchorWorld)
                    : lowerAnchorWorld;
                Vector3 upperAnchorOffset = positionParent != null
                    ? positionParent.InverseTransformVector(upperAnchorOffsetWorld)
                    : upperAnchorOffsetWorld;
                position = lowerAnchorPosition - upperAnchorOffset;
                position.z = upperBodyRestPosition.z;

                if (IsRunLowerBody(lowerSprite))
                {
                    Vector3 idleUpperLocal = (IdleUpperBodyAnchorPixels - IdleUpperBodyPivotPixels)
                        / upperSprite.pixelsPerUnit;
                    Vector3 idleUpperWorld = upperBodyRenderer.transform.TransformVector(idleUpperLocal);
                    Vector3 idleUpperOffset = positionParent != null
                        ? positionParent.InverseTransformVector(idleUpperWorld)
                        : idleUpperWorld;
                    position.x = upperBodyRestPosition.x - (upperAnchorOffset.x - idleUpperOffset.x);
                }
            }

            upperAnimator.transform.localPosition = position;
        }

        private Vector3 GetFullBodySpritePosition()
        {
            Vector3 position = lowerAnimator.transform.localPosition;
            Sprite sprite = upperBodyRenderer != null ? upperBodyRenderer.sprite : null;
            if (sprite == null || sprite.rect.width < 20f)
            {
                return position;
            }

            // Idle crouch is 31px wide with a center pivot. Extra width on shoot frames is the gun/muzzle
            // extending forward, so keep that same body core instead of recentering the whole sprite.
            const float crouchBodyCoreX = 15.5f;
            Vector3 coreLocal = new Vector3((crouchBodyCoreX - sprite.pivot.x) / sprite.pixelsPerUnit, 0f, 0f);
            Transform positionParent = upperAnimator.transform.parent;
            Vector3 coreWorld = upperBodyRenderer.transform.TransformVector(coreLocal);
            Vector3 coreOffset = positionParent != null
                ? positionParent.InverseTransformVector(coreWorld)
                : coreWorld;
            position.x -= coreOffset.x;
            return position;
        }

        private Vector3 GetShootDownLockedPosition(Sprite currentSprite)
        {
            bool isShotgun = currentSprite.name == "IdleShootDown_Shotgun_0";
            Vector2 referenceSize = isShotgun ? new Vector2(36f, 43f) : new Vector2(19f, 35f);
            Vector2 referencePivot = referenceSize * 0.5f;
            Vector2 referenceAnchor = isShotgun ? new Vector2(17f, 18f) : new Vector2(11f, 15f);
            float pixelsPerUnit = currentSprite.pixelsPerUnit;

            // Stable waist: idle legs, not the bouncing run silhouette.
            Vector3 idleLowerAnchorLocal =
                (IdleLowerBodyAnchorPixels - IdleLowerBodyPivotPixels) / pixelsPerUnit;
            Vector3 referenceUpperLocal = (referenceAnchor - referencePivot) / pixelsPerUnit;
            Transform positionParent = upperAnimator.transform.parent;
            Vector3 lowerWaistWorld = lowerBodyRenderer.transform.TransformPoint(idleLowerAnchorLocal);
            Vector3 referenceUpperWorld = upperBodyRenderer.transform.TransformVector(referenceUpperLocal);
            Vector3 lowerWaist = positionParent != null
                ? positionParent.InverseTransformPoint(lowerWaistWorld)
                : lowerWaistWorld;
            Vector3 referenceUpperOffset = positionParent != null
                ? positionParent.InverseTransformVector(referenceUpperWorld)
                : referenceUpperWorld;

            Vector3 position = lowerWaist - referenceUpperOffset;

            // Keep the same visual top/left as Idle_ShootDown so taller muzzle/leg frames
            // extend downward instead of lifting the torso.
            Vector3 referenceEdgeLocal = new Vector3(
                -referencePivot.x / pixelsPerUnit,
                (referenceSize.y - referencePivot.y) / pixelsPerUnit,
                0f);
            Vector3 currentEdgeLocal = new Vector3(
                -currentSprite.pivot.x / pixelsPerUnit,
                (currentSprite.rect.height - currentSprite.pivot.y) / pixelsPerUnit,
                0f);
            Vector3 edgeDeltaLocal = referenceEdgeLocal - currentEdgeLocal;
            Vector3 edgeDeltaWorld = upperBodyRenderer.transform.TransformVector(edgeDeltaLocal);
            Vector3 edgeDelta = positionParent != null
                ? positionParent.InverseTransformVector(edgeDeltaWorld)
                : edgeDeltaWorld;
            position += edgeDelta;
            position.z = upperBodyRestPosition.z;
            return position;
        }

        private static readonly Vector2 IdleUpperBodyAnchorPixels = new Vector2(9f, 8f);
        private static readonly Vector2 IdleUpperBodyPivotPixels = new Vector2(15.5f, 15.5f);
        private static readonly Vector2 IdleLowerBodyAnchorPixels = new Vector2(11.5f, 17f);
        private static readonly Vector2 IdleLowerBodyPivotPixels = new Vector2(11.5f, 9f);

        private static bool IsRunLowerBody(Sprite sprite)
        {
            return sprite != null && sprite.name.StartsWith("LowerBody_Run_");
        }

        private static bool IsShootDownUpperSprite(Sprite sprite)
        {
            if (sprite == null) return false;

            string spriteName = sprite.name;
            return spriteName.StartsWith("ShootDown_Sungluc")
                || spriteName == "Idle_ShootDown_0"
                || spriteName == "IdleShootDown_Shotgun_0";
        }

        private static bool TryGetUpperBodyAnchor(Sprite sprite, out Vector2 anchorPixels)
        {
            // Sprite-local pixel coordinates for the waist seam, measured from each frame's lower-left.
            switch (sprite.name)
            {
                case "Idle_NoAction_0":
                    anchorPixels = new Vector2(9f, 8f);
                    return true;
                case "Idle_Shotgun_0":
                    anchorPixels = new Vector2(9f, 8f);
                    return true;
                case "Idle_ShootDown_0":
                    anchorPixels = new Vector2(11f, 15f);
                    return true;
                case "Idle_ShootUp_0":
                    anchorPixels = new Vector2(12f, 3f);
                    return true;
                case "IdleShootDown_Shotgun_0":
                    anchorPixels = new Vector2(17f, 18f);
                    return true;
                case "IdleShootUp_Shotgun_0":
                    anchorPixels = new Vector2(12f, 4f);
                    return true;
                case "Idle_Shotgun_ShootUp_0":
                    anchorPixels = new Vector2(8f, 7f);
                    return true;
                case "UpperBody_ShootForward_0":
                case "UpperBody_ShootForward_1":
                    anchorPixels = new Vector2(11f, 4f);
                    return true;
                case "UpperBody_ShootForward_2":
                    anchorPixels = new Vector2(11f, 4f);
                    return true;
                case "UpperBody_ShootForward_4":
                case "UpperBody_ShootForward_5":
                case "UpperBody_ShootForward_6":
                case "UpperBody_ShootForward_7":
                    anchorPixels = new Vector2(11f, 4f);
                    return true;
                case "UpperBody_ShootForward_8":
                case "UpperBody_ShootForward_9":
                case "UpperBody_ShootForward_10":
                    anchorPixels = new Vector2(11f, 4f);
                    return true;
                case "UpperBody_ShootForward_Shotgun_0":
                    anchorPixels = new Vector2(8f, 8f);
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
                case "ShootDown_Sungluc_0":
                    // Full-body frame (54px). Keep waist 20px from the top like Idle_ShootDown.
                    anchorPixels = new Vector2(11f, 34f);
                    return true;
                case "ShootDown_Sungluc_1":
                    anchorPixels = new Vector2(11f, 15f);
                    return true;
                case "ShootDown_Sungluc_2":
                    anchorPixels = new Vector2(11f, 36f);
                    return true;
                case "ShootDown_Sungluc_3":
                    anchorPixels = new Vector2(12f, 11f);
                    return true;
                case "ShootDown_Sungluc_4":
                    anchorPixels = new Vector2(11f, 13f);
                    return true;
                case "ShootDown_Sungluc_5":
                    anchorPixels = new Vector2(11f, 15f);
                    return true;
                case "Charge_Shotgun_0":
                case "Charge_Shotgun_1":
                case "Charge_Shotgun_2":
                case "Charge_Shotgun_3":
                case "Charge_Shotgun_4":
                case "Charge_Shotgun_5":
                    anchorPixels = new Vector2(10f, 13f);
                    return true;
                default:
                    anchorPixels = Vector2.zero;
                    return false;
            }
        }

        private static bool TryGetLowerBodyAnchor(Sprite sprite, out Vector2 anchorPixels)
        {
            // These points follow the visible top contour of the waist, not the sprite rectangle.
            switch (sprite.name)
            {
                case "LowerBody_Idle_0":
                    anchorPixels = new Vector2(11.5f, 17f);
                    return true;
                case "LowerBody_Run_0":
                    anchorPixels = new Vector2(9.5f, 21f);
                    return true;
                case "LowerBody_Run_1":
                    anchorPixels = new Vector2(14.5f, 21f);
                    return true;
                case "LowerBody_Run_2":
                    anchorPixels = new Vector2(12f, 17f);
                    return true;
                case "LowerBody_Run_3":
                    anchorPixels = new Vector2(7.5f, 21f);
                    return true;
                case "LowerBody_Run_4":
                    anchorPixels = new Vector2(6f, 21f);
                    return true;
                case "LowerBody_Run_5":
                    anchorPixels = new Vector2(7f, 21f);
                    return true;
                case "LowerBody_Run_6":
                    anchorPixels = new Vector2(8.5f, 21f);
                    return true;
                case "LowerBody_Run_7":
                    anchorPixels = new Vector2(13.5f, 21f);
                    return true;
                case "LowerBody_Run_8":
                    anchorPixels = new Vector2(11.5f, 17f);
                    return true;
                case "LowerBody_Run_9":
                    anchorPixels = new Vector2(8f, 21f);
                    return true;
                case "LowerBody_Run_10":
                    anchorPixels = new Vector2(5.5f, 21f);
                    return true;
                case "LowerBody_Run_11":
                    anchorPixels = new Vector2(7.5f, 21f);
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
                || state.IsName("Upper_Crouch_Shotgun")
                || state.IsName("Upper_Crouch_Sungluc")
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