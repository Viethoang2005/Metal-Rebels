using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalRebels.Module_Environment
{
    /// <summary>
    /// Điều khiển Menu khởi động chính (MainMenu) độc lập cho Lê Hữu Hiệu,
    /// cho phép chọn trực tiếp bất kỳ màn chơi nào (Level 1, 2, 3) để test hoặc chơi game hoàn chỉnh.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Tùy chọn hiển thị")]
        [SerializeField] private string gameTitle = "METAL REBELS";
        [SerializeField] private string subTitle = "2D Side-Scrolling Run-and-Gun Action";

        private void OnGUI()
        {
            // Panel trung tâm Menu phong cách Arcade Metal Slug
            int panelWidth = 520;
            int panelHeight = 440;
            int startX = (Screen.width - panelWidth) / 2;
            int startY = (Screen.height - panelHeight) / 2;

            GUI.Box(new Rect(startX, startY, panelWidth, panelHeight), "");

            GUILayout.BeginArea(new Rect(startX + 20, startY + 20, panelWidth - 40, panelHeight - 40));

            // Tiêu đề Game
            GUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f); // Màu vàng đồng Metal
            GUILayout.Label(gameTitle, titleStyle);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Italic
            };
            subStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f);
            GUILayout.Label(subTitle, subStyle);

            GUILayout.Space(25);

            // Nút bấm chọn các màn chơi
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                fixedHeight = 42
            };

            GUI.color = new Color(0.4f, 0.9f, 0.5f);
            if (GUILayout.Button("▶  CHIẾN DỊCH: MÀN 1 (BÃI BIỂN RAVEN)", btnStyle))
            {
                SceneManager.LoadScene("Level_01");
            }

            GUILayout.Space(10);
            GUI.color = new Color(0.3f, 0.8f, 1f);
            if (GUILayout.Button("▶  CHIẾN DỊCH: MÀN 2 (RỪNG RẬM TỬ THẦN)", btnStyle))
            {
                SceneManager.LoadScene("Level_02");
            }

            GUILayout.Space(10);
            GUI.color = new Color(1f, 0.5f, 0.4f);
            if (GUILayout.Button("▶  CHIẾN DỊCH: MÀN 3 (CĂN CỨ PROJECT TITAN)", btnStyle))
            {
                SceneManager.LoadScene("Level_03");
            }

            GUILayout.Space(15);
            GUI.color = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("✕  THOÁT GAME", btnStyle))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            GUI.color = Color.white;
            GUILayout.Space(20);

            // Hướng dẫn phím cơ bản
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            hintStyle.normal.textColor = Color.gray;
            GUILayout.Label("Phím di chuyển: [A/D] Chạy | [Space] Nhảy | [S] Cúi né\nChiến đấu: [J] Bắn | [G] Lựu đạn | [E] Cứu con tin | [1/2/3/4] Đổi súng", hintStyle);

            GUILayout.EndArea();
        }
    }
}
