using UnityEngine;
using UnityEngine.InputSystem;

namespace MetalRebels.Player
{
    [DefaultExecutionOrder(-100)]
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool IsAimingUp { get; private set; }
        public bool IsAimingDown { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool GrenadePressed { get; private set; }
        public bool ReloadPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public int WeaponSlotSelected { get; private set; } = -1; // 0: Pistol, 1: Shotgun, 2: Rifle, 3: Rocket
        public bool NextWeaponPressed { get; private set; }
        public bool PrevWeaponPressed { get; private set; }

        private void Update()
        {
            ReadInput();
        }

        private void ReadInput()
        {
            float horizontal = 0f;
            float vertical = 0f;
            bool jumpDown = false;
            bool jumpHold = false;
            bool crouch = false;
            bool attackDown = false;
            bool attackHold = false;
            bool grenadeDown = false;
            bool reloadDown = false;
            bool interactDown = false;
            int weaponSlot = -1;
            bool nextWpn = false;
            bool prevWpn = false;

            Keyboard kb = Keyboard.current;
            Gamepad gp = Gamepad.current;
            Mouse mouse = Mouse.current;

            // Xử lý Keyboard
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical -= 1f;

                crouch = kb.cKey.isPressed;
                jumpDown = kb.spaceKey.wasPressedThisFrame;
                jumpHold = kb.spaceKey.isPressed;

                attackDown = kb.jKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
                attackHold = kb.jKey.isPressed || kb.enterKey.isPressed;

                grenadeDown = kb.gKey.wasPressedThisFrame;
                reloadDown = kb.rKey.wasPressedThisFrame;
                interactDown = kb.eKey.wasPressedThisFrame;

                if (kb.digit1Key.wasPressedThisFrame) weaponSlot = 0;
                else if (kb.digit2Key.wasPressedThisFrame) weaponSlot = 1;
                else if (kb.digit3Key.wasPressedThisFrame) weaponSlot = 2;
                else if (kb.digit4Key.wasPressedThisFrame) weaponSlot = 3;

                if (kb.qKey.wasPressedThisFrame) prevWpn = true;
                if (kb.tabKey.wasPressedThisFrame) nextWpn = true;
            }

            // Xử lý Mouse
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) attackDown = true;
                if (mouse.leftButton.isPressed) attackHold = true;

                float scrollY = mouse.scroll.ReadValue().y;
                if (scrollY > 0) prevWpn = true;
                else if (scrollY < 0) nextWpn = true;
            }

            // Xử lý Gamepad
            if (gp != null)
            {
                Vector2 stick = gp.leftStick.ReadValue();
                if (Mathf.Abs(stick.x) > 0.2f) horizontal = Mathf.Sign(stick.x);
                if (stick.y > 0.5f) vertical = 1f;
                if (stick.y < -0.5f) vertical = -1f;

                if (gp.dpad.left.isPressed) horizontal = -1f;
                if (gp.dpad.right.isPressed) horizontal = 1f;
                if (gp.dpad.up.isPressed) vertical = 1f;
                if (gp.dpad.down.isPressed) vertical = -1f;

                if (gp.buttonSouth.wasPressedThisFrame) jumpDown = true;
                if (gp.buttonSouth.isPressed) jumpHold = true;

                if (gp.buttonEast.isPressed) crouch = true;

                if (gp.buttonWest.wasPressedThisFrame || gp.rightTrigger.wasPressedThisFrame) attackDown = true;
                if (gp.buttonWest.isPressed || gp.rightTrigger.isPressed) attackHold = true;

                if (gp.leftTrigger.wasPressedThisFrame || gp.buttonNorth.wasPressedThisFrame) grenadeDown = true;
                if (gp.leftShoulder.wasPressedThisFrame) prevWpn = true;
                if (gp.rightShoulder.wasPressedThisFrame) nextWpn = true;
            }

            MoveInput = new Vector2(Mathf.Clamp(horizontal, -1f, 1f), Mathf.Clamp(vertical, -1f, 1f));
            IsAimingUp = vertical > 0.3f;
            IsAimingDown = vertical < -0.3f;
            CrouchHeld = crouch;
            JumpPressed = jumpDown;
            JumpHeld = jumpHold;
            AttackPressed = attackDown;
            AttackHeld = attackHold;
            GrenadePressed = grenadeDown;
            ReloadPressed = reloadDown;
            InteractPressed = interactDown;
            WeaponSlotSelected = weaponSlot;
            NextWeaponPressed = nextWpn;
            PrevWeaponPressed = prevWpn;
        }
    }
}
