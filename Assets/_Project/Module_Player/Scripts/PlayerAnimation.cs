using UnityEngine;

namespace MetalRebels.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("Tham chiếu 2 Animator Thân & Chân")]
        [SerializeField] private Animator upperAnimator; // Animator của UpperBody
        [SerializeField] private Animator lowerAnimator; // Animator của LowerBody
        [SerializeField] private Transform hipSocket;
        [SerializeField] private Sprite[] rocketIdleSprites;

        [Header("Tùy chỉnh Rocket Idle")]
        [SerializeField, Min(0.05f)] private float rocketScale = 0.8f;
        [SerializeField] private Vector2 rocketPositionOffset = Vector2.zero;

        private PlayerController playerController;
        private PlayerInputHandler inputHandler;
        private PlayerCombat playerCombat;
        private SpriteRenderer upperBodyRenderer;
        private SpriteRenderer lowerBodyRenderer;
        private Vector3 upperBodyRestPosition;
        private bool hasUpperBodyRestPosition;
        private float rocketIdleFrameTime;
        private int rocketIdleFrameIndex;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            inputHandler = GetComponent<PlayerInputHandler>();
            playerCombat = GetComponent<PlayerCombat>();

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

            if (hipSocket == null)
            {
                Transform visuals = transform.Find("Visuals");
                if (visuals != null)
                {
                    hipSocket = visuals.Find("HipSocket");
                    if (hipSocket == null)
                    {
                        GameObject socket = new GameObject("HipSocket");
                        socket.transform.SetParent(visuals, false);
                        socket.transform.localPosition = new Vector3(-0.044f, 0.29f, 0f);
                        hipSocket = socket.transform;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (playerController == null) return;

            UpdateUpperBodyAnimation();
            UpdateLowerBodyAnimation();
            UpdateRocketIdleSprite();
            UpdateFullBodyPosition();
            UpdateLowerBodyVisibility();
        }

        private void UpdateRocketIdleSprite()
        {
            bool shouldUseRocketIdle =
                playerCombat != null
                && playerCombat.IsRocketLauncherEquipped
                && playerController.IsGrounded
                && !playerController.IsCrouching;

            if (!shouldUseRocketIdle || rocketIdleSprites == null || rocketIdleSprites.Length == 0)
            {
                if (upperAnimator != null) upperAnimator.enabled = true;
                return;
            }

            if (upperAnimator != null && upperAnimator.enabled)
            {
                if (IsFullBodyState(upperAnimator.GetCurrentAnimatorStateInfo(0)))
                {
                    upperAnimator.SetBool("IsCrouching", false);
                    upperAnimator.Play("Idle", 0, 0f);
                    upperAnimator.Update(0f);
                }
                upperAnimator.enabled = false;
            }

            if (upperBodyRenderer == null) return;

            if (lowerBodyRenderer != null)
            {
                upperBodyRenderer.sortingLayerID = lowerBodyRenderer.sortingLayerID;
                upperBodyRenderer.sortingOrder = lowerBodyRenderer.sortingOrder + 1;
            }

            rocketIdleFrameTime += Time.deltaTime;
            const float frameDuration = 0.12f;
            if (rocketIdleFrameTime >= frameDuration)
            {
                rocketIdleFrameTime -= frameDuration;
                rocketIdleFrameIndex = (rocketIdleFrameIndex + 1) % rocketIdleSprites.Length;
            }

            if (rocketIdleSprites[rocketIdleFrameIndex] != null)
            {
                upperBodyRenderer.sprite = rocketIdleSprites[rocketIdleFrameIndex];
            }
        }

        private void UpdateUpperBodyAnimation()
        {
            if (upperAnimator == null) return;

            if (inputHandler != null)
            {
                SetBoolIfAvailable(upperAnimator, "IsAimingUp", inputHandler.IsAimingUp);
                SetBoolIfAvailable(upperAnimator, "IsAimingDown", inputHandler.IsAimingDown);
            }

            SetBoolIfAvailable(upperAnimator, "IsCrouching", playerController.IsCrouching);
            SetBoolIfAvailable(upperAnimator, "IsGrounded", playerController.IsGrounded);
            SetBoolIfAvailable(upperAnimator, "IsShotgun", playerCombat != null && playerCombat.UsesShotgunAnimationSet);
        }

        private void UpdateLowerBodyAnimation()
        {
            if (lowerAnimator == null) return;

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

            upperAnimator.transform.localScale = Vector3.one;
            lowerAnimator.transform.localScale = Vector3.one;

            Sprite upperSprite = upperBodyRenderer != null ? upperBodyRenderer.sprite : null;
            Sprite lowerSprite = lowerBodyRenderer != null ? lowerBodyRenderer.sprite : null;

            if (IsRocketIdleSprite(upperSprite))
            {
                upperAnimator.transform.localScale = new Vector3(rocketScale, rocketScale, 1f);
            }

            if (UsesFullBodySprite())
            {
                Vector3 fullPos = GetFullBodySpritePosition();
                fullPos.z = upperBodyRestPosition.z;
                upperAnimator.transform.localPosition = fullPos;
                return;
            }

            if (upperSprite != null && lowerSprite != null
                && TryGetUpperBodyAnchor(upperSprite, out Vector2 upperAnchorPixels)
                && TryGetLowerBodyAnchor(lowerSprite, out Vector2 lowerAnchorPixels))
            {
                Vector3 lowerAnchorLocal = (lowerAnchorPixels - lowerSprite.pivot) / lowerSprite.pixelsPerUnit;
                Vector3 upperAnchorLocal = (upperAnchorPixels - upperSprite.pivot) / upperSprite.pixelsPerUnit;

                Transform positionParent = upperAnimator.transform.parent;
                Vector3 lowerWaistWorld = lowerBodyRenderer.transform.TransformPoint(lowerAnchorLocal);
                Vector3 upperAnchorWorld = upperBodyRenderer.transform.TransformVector(upperAnchorLocal);

                Vector3 alignedPosition = positionParent != null
                    ? positionParent.InverseTransformPoint(lowerWaistWorld) - positionParent.InverseTransformVector(upperAnchorWorld)
                    : lowerWaistWorld - upperAnchorWorld;

                if (IsRocketIdleSprite(upperSprite))
                {
                    float facingSign = Mathf.Sign(playerController.FacingDirection);
                    alignedPosition += new Vector3(rocketPositionOffset.x * facingSign, rocketPositionOffset.y, 0f);
                }

                alignedPosition.z = upperBodyRestPosition.z;
                upperAnimator.transform.localPosition = alignedPosition;
            }

            if (lowerBodyRenderer != null)
            {
                upperBodyRenderer.sortingLayerID = lowerBodyRenderer.sortingLayerID;
                upperBodyRenderer.sortingOrder = lowerBodyRenderer.sortingOrder + 1;
            }
        }

        private Vector3 GetFullBodySpritePosition()
        {
            Vector3 position = lowerAnimator.transform.localPosition;
            Sprite sprite = upperBodyRenderer != null ? upperBodyRenderer.sprite : null;
            if (sprite == null || sprite.rect.width < 20f)
            {
                return position;
            }

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

        private static bool IsRocketIdleSprite(Sprite sprite)
        {
            return sprite != null
                && sprite.name.StartsWith("Idle_RocketLauncher_", System.StringComparison.Ordinal);
        }

        private static bool TryGetUpperBodyAnchor(Sprite sprite, out Vector2 anchorPixels)
        {
            switch (sprite.name)
            {
                case "Idle_NoAction_0":
                    anchorPixels = new Vector2(9f, 8f);
                    return true;
                case "Idle_Shotgun_0":
                    anchorPixels = new Vector2(9f, 8f);
                    return true;
                case "Idle_RocketLauncher_0":
                case "Idle_RocketLauncher_1":
                case "Idle_RocketLauncher_2":
                case "Idle_RocketLauncher_3":
                case "Idle_RocketLauncher_4":
                case "Idle_RocketLauncher_5":
                    anchorPixels = new Vector2(32f, 4f);
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
                case "UpperBody_ShootForward_2":
                case "UpperBody_ShootForward_4":
                case "UpperBody_ShootForward_5":
                case "UpperBody_ShootForward_6":
                case "UpperBody_ShootForward_7":
                case "UpperBody_ShootForward_8":
                case "UpperBody_ShootForward_9":
                case "UpperBody_ShootForward_10":
                    anchorPixels = new Vector2(11f, 4f);
                    return true;
                case "UpperBody_ShootForward_Shotgun_0":
                    anchorPixels = new Vector2(8f, 8f);
                    return true;
                case "UpperBody_ShootUp_0":
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
            Sprite upperSprite = upperBodyRenderer != null ? upperBodyRenderer.sprite : null;
            if (IsRocketIdleSprite(upperSprite))
            {
                return false;
            }

            bool usesFullBodySprite = playerController.IsCrouching;
            if (upperAnimator != null && upperAnimator.enabled)
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

        public void SetMovementSpeed(float speed)
        {
            if (lowerAnimator != null)
            {
                lowerAnimator.SetFloat("Speed", speed);
            }
        }

        public void PlayShootAnimation()
        {
            if (upperAnimator != null && playerCombat != null && playerCombat.UsesShotgunAnimationSet)
            {
                SetBoolIfAvailable(upperAnimator, "IsShotgun", true);
                if (playerController != null && playerController.IsCrouching)
                {
                    SetBoolIfAvailable(upperAnimator, "IsCrouching", true);
                    return;
                }
            }

            if (upperAnimator != null && HasTriggerParameter(upperAnimator, "Shoot"))
            {
                upperAnimator.SetTrigger("Shoot");
            }
        }

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

        public void PlayHurtAnimation()
        {
            if (upperAnimator != null && HasTriggerParameter(upperAnimator, "Hurt")) upperAnimator.SetTrigger("Hurt");
            if (lowerAnimator != null && HasTriggerParameter(lowerAnimator, "Hurt")) lowerAnimator.SetTrigger("Hurt");
        }
    }
}