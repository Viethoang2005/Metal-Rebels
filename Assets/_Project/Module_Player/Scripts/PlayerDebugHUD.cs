using MetalRebels.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetalRebels.Player
{
    public class PlayerDebugHUD : MonoBehaviour
    {
        private int currentHP = 100;
        private int maxHP = 100;
        private int currentLives = 3;
        private string weaponName = "Pistol";
        private int currentClip = -1;
        private int reserveAmmo = -1;
        private int currentGrenades = 5;
        private int maxGrenades = 5;
        private bool isReloading = false;

        private PlayerController playerController;

        private void Awake()
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        private void OnEnable()
        {
            PlayerEvents.OnHealthChanged += HandleHealthChanged;
            PlayerEvents.OnLivesChanged += HandleLivesChanged;
            PlayerEvents.OnWeaponChanged += HandleWeaponChanged;
            PlayerEvents.OnAmmoChanged += HandleAmmoChanged;
            PlayerEvents.OnGrenadeChanged += HandleGrenadeChanged;
            PlayerEvents.OnReloadingChanged += HandleReloadingChanged;
        }

        private void OnDisable()
        {
            PlayerEvents.OnHealthChanged -= HandleHealthChanged;
            PlayerEvents.OnLivesChanged -= HandleLivesChanged;
            PlayerEvents.OnWeaponChanged -= HandleWeaponChanged;
            PlayerEvents.OnAmmoChanged -= HandleAmmoChanged;
            PlayerEvents.OnGrenadeChanged -= HandleGrenadeChanged;
            PlayerEvents.OnReloadingChanged -= HandleReloadingChanged;
        }

        private void HandleHealthChanged(int current, int max)
        {
            currentHP = current;
            maxHP = max;
        }

        private void HandleLivesChanged(int lives)
        {
            currentLives = lives;
        }

        private void HandleWeaponChanged(string name, int ammo)
        {
            weaponName = name;
        }

        private void HandleAmmoChanged(int clip, int reserve)
        {
            currentClip = clip;
            reserveAmmo = reserve;
        }

        private void HandleGrenadeChanged(int current, int max)
        {
            currentGrenades = current;
            maxGrenades = max;
        }

        private void HandleReloadingChanged(bool reloading)
        {
            isReloading = reloading;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && playerController != null)
            {
                // Phím tắt kiểm thử nhanh
                if (kb.tKey.wasPressedThisFrame)
                {
                    playerController.TakeDamage(20);
                }
                if (kb.hKey.wasPressedThisFrame)
                {
                    playerController.TryHeal(30);
                }
                if (kb.kKey.wasPressedThisFrame)
                {
                    playerController.TakeDamage(999);
                }
            }
        }

        private void OnGUI()
        {
            // Panel HUD người chơi (Góc trên bên trái)
            GUI.Box(new Rect(10, 10, 310, 190), "<b>MARCO - STATUS (MODULE PLAYER)</b>");

            GUILayout.BeginArea(new Rect(20, 35, 290, 160));

            // Thanh Máu
            float hpPercent = maxHP > 0 ? (float)currentHP / maxHP : 0f;
            string hpStatus = currentHP > 0 ? $"{currentHP}/{maxHP} HP" : "DEAD";
            GUILayout.Label($"<b>HP:</b> {hpStatus}");

            Rect barRect = GUILayoutUtility.GetRect(280, 18);
            GUI.Box(barRect, "");
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * hpPercent, barRect.height);
            Color oldColor = GUI.color;
            GUI.color = currentHP > 30 ? Color.green : Color.red;
            GUI.Box(fillRect, "");
            GUI.color = oldColor;

            // Mạng sống
            GUILayout.Label($"<b>Mạng còn lại:</b> {currentLives} / 3");

            // Vũ khí hiện tại & Đạn
            string ammoText = currentClip < 0 ? "Vô hạn (∞ / ∞)" : $"{currentClip} / {reserveAmmo}";
            if (isReloading) ammoText += " <color=yellow>[RELOADING...]</color>";
            GUILayout.Label($"<b>Vũ khí:</b> {weaponName}");
            GUILayout.Label($"<b>Băng đạn:</b> {ammoText}");

            // Lựu đạn
            GUILayout.Label($"<b>Lựu đạn:</b> {currentGrenades}/{maxGrenades} quả (Phím G)");

            // Bất tử
            if (playerController != null && playerController.IsInvincible)
            {
                GUILayout.Label("<color=cyan><b>[TRẠNG THÁI BẤT TỬ TẠM THỜI]</b></color>");
            }

            GUILayout.EndArea();

            // Hướng dẫn phím điều khiển (Góc dưới bên trái)
            GUI.Box(new Rect(10, Screen.height - 210, 380, 200), "<b>HƯỚNG DẪN ĐIỀU KHIỂN & TEST PLAYER</b>");
            GUILayout.BeginArea(new Rect(20, Screen.height - 185, 360, 175));
            GUILayout.Label("• <b>A / D</b> hoặc <b>Mũi tên</b>: Di chuyển trái / phải");
            GUILayout.Label("• <b>Space</b>: Nhảy (có thể nhảy bắn mọi hướng)");
            GUILayout.Label("• <b>S / C</b>: Ngồi né đạn (hạ thấp hitbox) / Bắn thấp");
            GUILayout.Label("• <b>W</b>: Bắn hướng thẳng lên trời");
            GUILayout.Label("• <b>J / Chuột trái</b>: Bắn (Nhấp 1 phát hoặc giữ xả đạn)");
            GUILayout.Label("• <b>G</b>: Ném lựu đạn (Nổ sau 3s / bán kính 3m)");
            GUILayout.Label("• <b>R</b>: Nạp đạn thủ công | <b>1/2/3/4</b>: Đổi vũ khí");
            GUILayout.Label("• <i>Phím test nhanh:</i> <b>T</b>: Nhận 20 dmg | <b>H</b>: Hồi 30 HP | <b>K</b>: Chết");
            GUILayout.EndArea();
        }
    }
}
