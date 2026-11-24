using System.Reflection;
using MetalRebels.Core.Events;
using MetalRebels.Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetalRebels.UI
{
    /// <summary>
    /// Công cụ test nhanh HUD, Pause Menu, Game Over và Victory trên màn trống.
    /// Chỉ dùng để kiểm thử giao diện, có thể xóa object này khi ghép map thật.
    /// </summary>
    public class HUDTestHarness : MonoBehaviour
    {
        [Header("Chỉ số giả lập ban đầu")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private int maxGrenades = 10;

        private int currentHp;
        private int currentLives;
        private int currentGrenades;
        private int currentWeaponIndex;
        private int currentAmmo;
        private int testScore;

        private HUDController hud;

        private readonly string[] weapons = { "PISTOL", "HEAVY MACHINE GUN", "SHOTGUN", "ROCKET LAUNCHER" };
        private readonly int[] maxAmmos = { -1, 200, 30, 15 };

        private void Start()
        {
            hud = FindFirstObjectByType<HUDController>();

            currentHp = maxHp;
            currentLives = maxLives;
            currentGrenades = maxGrenades;
            currentWeaponIndex = 0;
            currentAmmo = maxAmmos[0];
            testScore = GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0;

            RefreshAllHUD();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // F1: Trừ 25 HP (Hết HP tự trừ 1 mạng, hết 3 mạng tự kích hoạt Game Over)
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                TakeTestDamage(25);
            }

            // F2: Hồi đầy 100 HP
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                currentHp = maxHp;
                InvokeHUD("UpdateHealthUI", currentHp, maxHp);
            }

            // F3: Đổi vũ khí & nạp đầy đạn vũ khí đó
            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
                currentAmmo = maxAmmos[currentWeaponIndex];
                InvokeHUD("UpdateWeaponUI", weapons[currentWeaponIndex], currentWeaponIndex);
                InvokeHUD("UpdateAmmoUI", currentAmmo, maxAmmos[currentWeaponIndex]);
            }

            // F4: Bắn 1 phát (giảm đạn) & ném 1 lựu đạn
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                if (currentAmmo > 0)
                {
                    currentAmmo--;
                    InvokeHUD("UpdateAmmoUI", currentAmmo, maxAmmos[currentWeaponIndex]);
                }
                if (currentGrenades > 0)
                {
                    currentGrenades--;
                    InvokeHUD("UpdateGrenadeUI", currentGrenades, maxGrenades);
                }
            }

            // F5: Cộng +500 điểm
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                testScore += 500;
                if (GameManager.Instance != null)
                {
                    var addScoreMethod = typeof(GameManager).GetMethod("AddScore", BindingFlags.Instance | BindingFlags.NonPublic);
                    addScoreMethod?.Invoke(GameManager.Instance, new object[] { 500 });
                    testScore = GameManager.Instance.CurrentScore;
                }
                InvokeHUD("UpdateScoreUI", testScore);
            }

            // F6: Kích hoạt thẳng GAME OVER
            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                currentHp = 0;
                currentLives = 0;
                InvokeHUD("UpdateHealthUI", 0, maxHp);
                InvokeHUD("UpdateLivesUI", 0);
                TriggerGameOver();
            }

            // F7: Kích hoạt thẳng VICTORY
            if (Keyboard.current.f7Key.wasPressedThisFrame)
            {
                TriggerVictory();
            }
        }

        private void RefreshAllHUD()
        {
            InvokeHUD("UpdateHealthUI", currentHp, maxHp);
            InvokeHUD("UpdateLivesUI", currentLives);
            InvokeHUD("UpdateWeaponUI", weapons[currentWeaponIndex], currentWeaponIndex);
            InvokeHUD("UpdateAmmoUI", currentAmmo, maxAmmos[currentWeaponIndex]);
            InvokeHUD("UpdateGrenadeUI", currentGrenades, maxGrenades);
            InvokeHUD("UpdateScoreUI", testScore);
        }

        private void TakeTestDamage(int dmg)
        {
            if (currentLives <= 0) return;

            currentHp -= dmg;
            if (currentHp <= 0)
            {
                currentLives--;
                InvokeHUD("UpdateLivesUI", currentLives);

                if (currentLives <= 0)
                {
                    currentHp = 0;
                    InvokeHUD("UpdateHealthUI", 0, maxHp);
                    TriggerGameOver();
                    return;
                }

                currentHp = maxHp;
            }
            InvokeHUD("UpdateHealthUI", currentHp, maxHp);
        }

        private void TriggerGameOver()
        {
            if (GameManager.Instance != null)
            {
                var goMethod = typeof(GameManager).GetMethod("HandleGameOver", BindingFlags.Instance | BindingFlags.NonPublic);
                goMethod?.Invoke(GameManager.Instance, null);
            }
            else if (hud != null)
            {
                hud.ShowGameOverPanel(testScore);
            }
        }

        private void TriggerVictory()
        {
            if (GameManager.Instance != null)
            {
                var vicMethod = typeof(GameManager).GetMethod("HandleVictory", BindingFlags.Instance | BindingFlags.NonPublic);
                vicMethod?.Invoke(GameManager.Instance, null);
            }
            else if (hud != null)
            {
                hud.ShowVictoryPanel(testScore);
            }
        }

        private void InvokeHUD(string methodName, params object[] args)
        {
            if (hud == null) hud = FindFirstObjectByType<HUDController>();
            if (hud == null) return;

            var method = typeof(HUDController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            method?.Invoke(hud, args);
        }
    }
}