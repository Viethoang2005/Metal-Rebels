using MetalRebels.Core.Events;
using MetalRebels.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MetalRebels.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Thông số nhân vật (Góc trên trái)")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private GameObject infinityAmmoIcon;
        [SerializeField] private TextMeshProUGUI grenadeText;

        [Header("HUD Nhiệm vụ (Góc trên phải)")]
        [SerializeField] private TextMeshProUGUI missionTitleText;

        [Header("Các bảng Popup trong màn chơi")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryTitleText;
        [SerializeField] private TextMeshProUGUI victorySubtitleText;
        [SerializeField] private GameObject nextMissionButton;

        private void OnEnable()
        {
            PlayerEvents.OnHealthChanged += UpdateHealthUI;
            PlayerEvents.OnLivesChanged += UpdateLivesUI;
            PlayerEvents.OnWeaponChanged += UpdateWeaponUI;
            PlayerEvents.OnAmmoChanged += UpdateAmmoUI;
            PlayerEvents.OnGrenadeChanged += UpdateGrenadeUI;
        }

        private void OnDisable()
        {
            PlayerEvents.OnHealthChanged -= UpdateHealthUI;
            PlayerEvents.OnLivesChanged -= UpdateLivesUI;
            PlayerEvents.OnWeaponChanged -= UpdateWeaponUI;
            PlayerEvents.OnAmmoChanged -= UpdateAmmoUI;
            PlayerEvents.OnGrenadeChanged -= UpdateGrenadeUI;
        }

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Tự động đặt tên nhiệm vụ theo scene hiện tại
            if (missionTitleText != null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string title = sceneName switch
                {
                    "Level_01" => "MÀN 1: ĐỘT NHẬP CĂN CỨ RAVEN",
                    "Level_02" => "MÀN 2: VƯỢT RỪNG SÂU",
                    "Level_03" => "MÀN 3: TỔNG TẤN CÔNG",
                    "Mission_1" => "MÀN 1: ĐỘT NHẬP CĂN CỨ RAVEN",
                    "Mission_2" => "MÀN 2: VƯỢT RỪNG SÂU",
                    "Mission_3" => "MÀN 3: TỔNG TẤN CÔNG",
                    _ => sceneName
                };
                missionTitleText.text = title;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (gameOverPanel != null && gameOverPanel.activeSelf) return;
                if (victoryPanel != null && victoryPanel.activeSelf) return;
                TogglePauseMenu();
            }
        }

        public void TogglePauseMenu()
        {
            if (GameManager.Instance == null || pausePanel == null) return;

            bool willPause = !pausePanel.activeSelf;
            pausePanel.SetActive(willPause);
            GameManager.Instance.SetPaused(willPause);
        }

        public void OnResumeClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartCurrentMission();
        }

        public void OnNextMissionClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.LoadNextMission();
        }

        public void OnMainMenuClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        }

        public void ShowGameOverPanel()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        public void ShowVictoryPanel()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) 
            {
                bool isFinalMission = SceneManager.GetActiveScene().name == "Mission_3" 
                    || SceneManager.GetActiveScene().name == "Level_03";

                if (victoryTitleText != null)
                    victoryTitleText.text = isFinalMission ? "HOÀN THÀNH NHIỆM VỤ" : "CHIẾN THẮNG";

                if (victorySubtitleText != null)
                    victorySubtitleText.text = isFinalMission
                        ? "TOÀN BỘ CHIẾN DỊCH ĐÃ THẮNG LỢI — MỤC TIÊU BỊ TIÊU DIỆT!"
                        : "KHU VỰC ĐÃ ĐƯỢC KIỂM SOÁT — SẴN SÀNG CHO CHẶNG TIẾP THEO";

                if (nextMissionButton != null)
                    nextMissionButton.SetActive(!isFinalMission);

                victoryPanel.SetActive(true);
            }
        }

        private void UpdateHealthUI(int currentHp, int maxHp)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.value = currentHp;
            }
            if (hpText != null) hpText.text = $"{currentHp}/{maxHp}";
        }

        private void UpdateLivesUI(int lives)
        {
            if (livesText != null) livesText.text = $"x{lives}";
        }

        private bool isCurrentWeaponPistol = true;
        private void UpdateWeaponUI(string weaponName, int weaponIndex)
        {
            if (weaponNameText != null) weaponNameText.text = weaponName.ToUpper();
            isCurrentWeaponPistol = weaponName.Trim().Equals("PISTOL", System.StringComparison.OrdinalIgnoreCase);

            if (isCurrentWeaponPistol)
            {
                if (infinityAmmoIcon != null) infinityAmmoIcon.SetActive(true);
                if (ammoText != null) ammoText.gameObject.SetActive(false);
            }
        }

        private void UpdateAmmoUI(int currentAmmo, int reserveAmmo)
        {
            bool isInfinite = isCurrentWeaponPistol || reserveAmmo < 0;
            if (infinityAmmoIcon != null) infinityAmmoIcon.SetActive(isInfinite);
            if (ammoText != null)
            {
                ammoText.gameObject.SetActive(!isInfinite);
                if (!isInfinite) ammoText.text = $"{currentAmmo}/{reserveAmmo}";
            }
        }

        private void UpdateGrenadeUI(int currentGrenades, int maxGrenades)
        {
            if (grenadeText != null)
                grenadeText.text = maxGrenades > 0 ? $"{currentGrenades}/{maxGrenades}" : $"{currentGrenades}";
        }
    }
}