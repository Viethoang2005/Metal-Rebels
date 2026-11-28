using MetalRebels.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalRebels.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Các Panel con trên Main Menu")]
        [SerializeField] private GameObject missionSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject teamInfoPanel;

        [Header("Nút Chọn Màn Chơi (3 Missions)")]
        [SerializeField] private Button[] missionButtons; // Kéo thả 3 nút Mission 1, 2, 3

        [Header("Cài đặt Âm thanh (Audio)")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text volumePercentText;

        [Header("Tên các Scene trong Build Settings")]
        [SerializeField] private string[] missionScenes = { "Level_01", "Level_02", "Level_03" };

        private void Start()
        {
            Time.timeScale = 1f;
            CloseAllSubPanels();
            RefreshMissionButtons();

            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = savedVolume;
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = savedVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            UpdateVolumeLabel(savedVolume);
        }

        // 1. Nút CHIẾN DỊCH MỚI (START MÀN 1)
        public void OnNewCampaignClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewCampaign();
            }
            else if (missionScenes.Length > 0)
            {
                SceneManager.LoadScene(missionScenes[0]);
            }
        }

        // 2. Nút CHỌN MÀN CHƠI (3 MISSIONS)
        public void OnOpenMissionSelectClicked()
        {
            CloseAllSubPanels();
            RefreshMissionButtons();
            if (missionSelectPanel != null) missionSelectPanel.SetActive(true);
        }

        public void OnSelectMission(int missionIndex)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMission(missionIndex);
            }
            else if (missionIndex >= 0 && missionIndex < missionScenes.Length)
            {
                SceneManager.LoadScene(missionScenes[missionIndex]);
            }
        }

        // 3. Nút CÀI ĐẶT (AUDIO & KEYS)
        public void OnOpenSettingsClicked()
        {
            CloseAllSubPanels();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        // 4. Nút THÔNG TIN NHÓM (APEX GROUP - NHÓM 8)
        public void OnOpenTeamInfoClicked()
        {
            CloseAllSubPanels();
            if (teamInfoPanel != null) teamInfoPanel.SetActive(true);
        }

        // 5. Nút THOÁT TRÒ CHƠI (EXIT)
        public void OnQuitGameClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void CloseAllSubPanels()
        {
            if (missionSelectPanel != null) missionSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (teamInfoPanel != null) teamInfoPanel.SetActive(false);
        }

        private void RefreshMissionButtons()
        {
            if (missionButtons == null) return;
            int unlockedIndex = PlayerPrefs.GetInt("UnlockedMission", 0);

            for (int i = 0; i < missionButtons.Length; i++)
            {
                if (missionButtons[i] != null)
                {
                    missionButtons[i].interactable = (i <= unlockedIndex);
                }
            }
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
            UpdateVolumeLabel(value);
        }

        private void UpdateVolumeLabel(float value)
        {
            if (volumePercentText != null)
            {
                volumePercentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }
    }
}