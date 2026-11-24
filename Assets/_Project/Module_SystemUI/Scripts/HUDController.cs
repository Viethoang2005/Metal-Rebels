using MetalRebels.Core.Events;
using MetalRebels.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        [SerializeField] private TextMeshProUGUI grenadeText;

        [Header("HUD Điểm số & Nhiệm vụ (Góc trên phải)")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI missionTitleText;

        [Header("Các bảng Popup trong màn chơi")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryScoreText;

        private void OnEnable()
        {
            PlayerEvents.OnHealthChanged += UpdateHealthUI;
            PlayerEvents.OnLivesChanged += UpdateLivesUI;
            PlayerEvents.OnWeaponChanged += UpdateWeaponUI;
            PlayerEvents.OnAmmoChanged += UpdateAmmoUI;
            PlayerEvents.OnGrenadeChanged += UpdateGrenadeUI;
            PlayerEvents.OnScoreChanged += UpdateScoreUI;
        }

        private void OnDisable()
        {
            PlayerEvents.OnHealthChanged -= UpdateHealthUI;
            PlayerEvents.OnLivesChanged -= UpdateLivesUI;
            PlayerEvents.OnWeaponChanged -= UpdateWeaponUI;
            PlayerEvents.OnAmmoChanged -= UpdateAmmoUI;
            PlayerEvents.OnGrenadeChanged -= UpdateGrenadeUI;
            PlayerEvents.OnScoreChanged -= UpdateScoreUI;
        }

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Đồng bộ điểm số hiện tại khi vừa sang màn mới
            if (scoreText != null && GameManager.Instance != null)
            {
                scoreText.text = GameManager.Instance.CurrentScore.ToString("D6");
            }
        }

        private void Update()
        {
            // Bắt phím ESC bằng Input System mới để bật/tắt Pause Menu
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

        public void ShowGameOverPanel(int finalScore)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverScoreText != null)
                {
                    gameOverScoreText.text = $"TỔNG ĐIỂM ĐẠT ĐƯỢC: <color=#FF9E2C>{finalScore:D6}</color>";
                }
            }
        }

        public void ShowVictoryPanel(int finalScore)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                if (victoryScoreText != null)
                {
                    victoryScoreText.text = $"ĐIỂM CHIẾN DỊCH: <color=#34D399>{finalScore:D6}</color>";
                }
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

        private void UpdateWeaponUI(string weaponName, int weaponIndex)
        {
            if (weaponNameText != null) weaponNameText.text = weaponName.ToUpper();
        }

        private void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (ammoText != null)
                ammoText.text = maxAmmo < 0 ? "INF" : $"{currentAmmo}/{maxAmmo}";
        }

        private void UpdateGrenadeUI(int currentGrenades, int maxGrenades)
        {
            if (grenadeText != null)
                grenadeText.text = maxGrenades > 0 ? $"{currentGrenades}/{maxGrenades}" : $"{currentGrenades}";
        }

        private void UpdateScoreUI(int pointsAdded)
        {
            if (scoreText == null) return;
            int totalScore = GameManager.Instance != null ? GameManager.Instance.CurrentScore : pointsAdded;
            scoreText.text = totalScore.ToString("D6");
        }
    }
}