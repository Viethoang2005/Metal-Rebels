using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MetalRebels.Core.Events;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Quản lý thiết lập màn chơi của Lê Hữu Hiệu:
    /// Vị trí xuất phát ban đầu của Player, ranh giới map cho Camera, và reset Checkpoint khi bắt đầu màn.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Điểm xuất phát ban đầu")]
        [SerializeField] private Transform initialPlayerSpawnPoint;

        [Header("Camera & Ranh giới màn chơi")]
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private Vector2 levelMinBounds = new Vector2(-10f, -0.85f);
        [SerializeField] private Vector2 levelMaxBounds = new Vector2(150f, 6.15f);

        private bool showEndScreen = false;
        private bool isVictory = false;
        private Coroutine victoryDelayCoroutine;

        private void Awake()
        {
            // Reset trạng thái Checkpoint cũ khi bắt đầu một màn mới
            Checkpoint.ResetCheckpoint();
        }

        private void OnEnable()
        {
            GameEvents.OnBossKilled += HandleBossKilled;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnBossKilled -= HandleBossKilled;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void Start()
        {
            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetBounds(levelMinBounds, levelMaxBounds);
            }
        }

        private void HandleBossKilled()
        {
            if (showEndScreen || victoryDelayCoroutine != null) return;
            victoryDelayCoroutine = StartCoroutine(ShowVictoryDelayedRoutine());
        }

        private void HandleGameOver(bool victory)
        {
            if (showEndScreen) return;
            if (victoryDelayCoroutine != null)
            {
                StopCoroutine(victoryDelayCoroutine);
                victoryDelayCoroutine = null;
            }
            isVictory = victory;
            showEndScreen = true;
        }

        private IEnumerator ShowVictoryDelayedRoutine()
        {
            yield return new WaitForSeconds(2.2f);
            isVictory = true;
            showEndScreen = true;
            victoryDelayCoroutine = null;
        }

        private static string GetNextLevelSceneName(string currentSceneName)
        {
            return currentSceneName switch
            {
                "Level_01" => "Level_02",
                "Level_02" => "Level_03",
                _ => null
            };
        }

        /// <summary>
        /// Lấy tọa độ hồi sinh hiện tại: Nếu đã qua Checkpoint thì lấy Checkpoint,
        /// nếu chưa thì lấy điểm xuất phát ban đầu.
        /// </summary>
        public Vector3 GetRespawnPosition()
        {
            if (Checkpoint.HasActiveCheckpoint)
            {
                return Checkpoint.LastActiveCheckpointPosition;
            }

            return initialPlayerSpawnPoint != null ? initialPlayerSpawnPoint.position : Vector3.zero;
        }

        private void OnGUI()
        {
            if (!showEndScreen) return;

            int panelWidth = 380;
            int panelHeight = 230;
            int startX = (Screen.width - panelWidth) / 2;
            int startY = (Screen.height - panelHeight) / 2;

            GUI.Box(new Rect(startX, startY, panelWidth, panelHeight), "");
            GUILayout.BeginArea(new Rect(startX + 20, startY + 20, panelWidth - 40, panelHeight - 40));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = isVictory ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.3f, 0.3f);

            string currentScene = SceneManager.GetActiveScene().name;
            string nextScene = isVictory ? GetNextLevelSceneName(currentScene) : null;
            string title = isVictory
                ? (nextScene != null ? "MISSION COMPLETE!" : "CHIẾN THẮNG TOÀN CHIẾN DỊCH!")
                : "GAME OVER";

            GUILayout.Label(title, titleStyle);
            GUILayout.Space(18);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                fixedHeight = 38
            };

            if (nextScene != null && GUILayout.Button("▶  MÀN TIẾP THEO", btnStyle))
            {
                SceneManager.LoadScene(nextScene);
            }

            if (GUILayout.Button("↻  CHƠI LẠI MÀN NÀY", btnStyle))
            {
                SceneManager.LoadScene(currentScene);
            }

            if (GUILayout.Button("⌂  MENU CHÍNH", btnStyle))
            {
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;

            // Vẽ ranh giới màn chơi trong Editor
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((levelMinBounds.x + levelMaxBounds.x) * 0.5f, (levelMinBounds.y + levelMaxBounds.y) * 0.5f, 0f);
            Vector3 size = new Vector3(levelMaxBounds.x - levelMinBounds.x, levelMaxBounds.y - levelMinBounds.y, 1f);
            Gizmos.DrawWireCube(center, size);

            if (initialPlayerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(initialPlayerSpawnPoint.position, 0.5f);
            }
        }
    }
}

