using MetalRebels.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetalRebels.Player
{
    /// <summary>
    /// Bộ hỗ trợ kiểm thử Marco: camera theo nhân vật, HUD chỉ số, F1 mở toàn bộ vũ khí.
    /// </summary>
    public class PlayerPlaytestKit : MonoBehaviour
    {
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.8f, -10f);
        [SerializeField] private float cameraFollowSpeed = 10f;
        [SerializeField] private Vector2 cameraMin = new Vector2(-16f, 2.2f);
        [SerializeField] private Vector2 cameraMax = new Vector2(16f, 7f);
        [SerializeField] private bool showHud;

        private PlayerController player;
        private PlayerCombat combat;
        private int currentHp = 100;
        private int maxHp = 100;
        private int lives = 3;
        private string weaponName = "Pistol";
        private int clip = -1;
        private int reserve = -1;
        private int grenades = 5;
        private int maxGrenades = 5;
        private bool reloading;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
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

        private void Start()
        {
            if (player != null)
            {
                currentHp = player.CurrentHP;
                maxHp = player.MaxHP;
                lives = player.CurrentLives;
            }

            if (combat != null)
            {
                grenades = combat.CurrentGrenades;
                if (combat.CurrentWeapon != null)
                {
                    weaponName = combat.CurrentWeapon.weaponName;
                }
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame && combat != null)
            {
                combat.UnlockAllWeaponsForPlaytest();
            }
        }

        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 desired = transform.position + cameraOffset;
            desired.x = Mathf.Clamp(desired.x, cameraMin.x, cameraMax.x);
            desired.y = Mathf.Clamp(desired.y, cameraMin.y, cameraMax.y);
            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                desired,
                1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime));
        }

        private void OnGUI()
        {
            if (!showHud) return;

            const int width = 420;
            GUILayout.BeginArea(new Rect(12f, 12f, width, 220f), GUI.skin.box);
            GUILayout.Label("MARCO — KIỂM THỬ");
            GUILayout.Label($"HP: {currentHp}/{maxHp}   Mạng: {lives}");
            GUILayout.Label($"Vũ khí: {weaponName}   Đạn: {FormatAmmo()}");
            GUILayout.Label($"Lựu đạn: {grenades}/{maxGrenades}{(reloading ? "   ĐANG NẠP" : string.Empty)}");
            GUILayout.Space(6f);
            GUILayout.Label("A/D đi  Space nhảy  C ngồi  W/S ngắm");
            GUILayout.Label("J / chuột trái bắn  G lựu đạn  R nạp  1-4 đổi súng");
            GUILayout.Label("F1 mở Shotgun / Súng trường / Rocket để test");
            GUILayout.EndArea();
        }

        private string FormatAmmo()
        {
            if (clip < 0)
            {
                return "Infinite";
            }

            return $"{clip} / {reserve}";
        }

        private void HandleHealthChanged(int current, int max)
        {
            currentHp = current;
            maxHp = max;
        }

        private void HandleLivesChanged(int currentLives)
        {
            lives = currentLives;
        }

        private void HandleWeaponChanged(string name, int currentClip)
        {
            weaponName = name;
            clip = currentClip;
        }

        private void HandleAmmoChanged(int currentClip, int reserveAmmo)
        {
            clip = currentClip;
            reserve = reserveAmmo;
        }

        private void HandleGrenadeChanged(int current, int max)
        {
            grenades = current;
            maxGrenades = max;
        }

        private void HandleReloadingChanged(bool isReloading)
        {
            reloading = isReloading;
        }
    }
}
