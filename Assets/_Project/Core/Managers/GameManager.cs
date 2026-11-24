using System.Collections;
using MetalRebels.Core.Events;
using MetalRebels.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalRebels.Core.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Cấu hình Màn chơi")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string[] missionSceneNames = { "Mission_1", "Mission_2", "Mission_3" };
        [SerializeField] private float victoryPanelDelay = 2.2f;
        [SerializeField] private float gameOverPanelDelay = 1.2f;

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public int CurrentScore { get; private set; }
        public int UnlockedMissionIndex => PlayerPrefs.GetInt("UnlockedMission", 0);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            PlayerEvents.OnAllLivesLost += HandleGameOver;
            PlayerEvents.OnMissionCompleted += HandleVictory;
            PlayerEvents.OnScoreChanged += AddScore;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            PlayerEvents.OnAllLivesLost -= HandleGameOver;
            PlayerEvents.OnMissionCompleted -= HandleVictory;
            PlayerEvents.OnScoreChanged -= AddScore;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Time.timeScale = 1f;
            CurrentState = (scene.name == mainMenuSceneName) ? GameState.MainMenu : GameState.Playing;
        }

        public void StartNewCampaign()
        {
            CurrentScore = 0;
            LoadMission(0);
        }

        public void LoadMission(int missionIndex)
        {
            if (missionIndex < 0 || missionIndex >= missionSceneNames.Length) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(missionSceneNames[missionIndex]);
        }

        public void LoadNextMission()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < missionSceneNames.Length; i++)
            {
                if (missionSceneNames[i] == currentScene)
                {
                    if (i + 1 < missionSceneNames.Length)
                    {
                        LoadMission(i + 1);
                    }
                    else
                    {
                        ReturnToMainMenu();
                    }
                    return;
                }
            }
        }

        public void RestartCurrentMission()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void SetPaused(bool paused)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused) return;
            CurrentState = paused ? GameState.Paused : GameState.Playing;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void AddScore(int points)
        {
            CurrentScore += points;
        }

        private void HandleVictory()
        {
            if (CurrentState == GameState.Victory || CurrentState == GameState.GameOver) return;
            CurrentState = GameState.Victory;

            // Mở khóa màn tiếp theo
            string currentScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < missionSceneNames.Length; i++)
            {
                if (missionSceneNames[i] == currentScene && i + 1 > UnlockedMissionIndex)
                {
                    PlayerPrefs.SetInt("UnlockedMission", Mathf.Min(i + 1, missionSceneNames.Length - 1));
                    PlayerPrefs.Save();
                    break;
                }
            }

            // Gọi animation chiến thắng qua SendMessage để không phụ thuộc nhánh player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerObj.SendMessage("PlayVictoryAnimation", SendMessageOptions.DontRequireReceiver);
            }

            StartCoroutine(ShowVictoryRoutine());
        }

        private IEnumerator ShowVictoryRoutine()
        {
            yield return new WaitForSeconds(victoryPanelDelay);
            HUDController hud = FindFirstObjectByType<HUDController>();
            if (hud != null)
            {
                hud.ShowVictoryPanel(CurrentScore);
            }
        }

        private void HandleGameOver()
        {
            if (CurrentState == GameState.Victory || CurrentState == GameState.GameOver) return;
            CurrentState = GameState.GameOver;
            StartCoroutine(ShowGameOverRoutine());
        }

        private IEnumerator ShowGameOverRoutine()
        {
            yield return new WaitForSeconds(gameOverPanelDelay);
            HUDController hud = FindFirstObjectByType<HUDController>();
            if (hud != null)
            {
                hud.ShowGameOverPanel(CurrentScore);
            }
        }
    }
}