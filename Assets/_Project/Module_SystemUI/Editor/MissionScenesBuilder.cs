#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MetalRebels.Core.Managers;
using MetalRebels.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MissionScenesBuilder
{
    [MenuItem("MetalRebels/Tạo 3 Màn Trống Test HUD & Cấu Hình Build")]
    public static void BuildTestMissionScenes()
    {
        string prefabPath = "Assets/_Project/Module_SystemUI/Prefabs/GameHUD_Canvas.prefab";
        GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (hudPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Thiếu Prefab HUD",
                "Chưa tìm thấy GameHUD_Canvas.prefab!\nHãy chạy menu 'MetalRebels -> Tạo Prefab HUD, Pause & GameOver' trước.",
                "OK"
            );
            return;
        }

        string[] sceneNames = { "Mission_1", "Mission_2", "Mission_3" };
        string[] missionTitles =
        {
            "MÀN 1: ĐỘT NHẬP CĂN CỨ RAVEN",
            "MÀN 2: VƯỢT QUA RỪNG RẬM TỬ THẦN",
            "MÀN 3: TỔNG TẤN CÔNG"
        };
        Color[] bgColors =
        {
            new Color(0.08f, 0.11f, 0.16f, 1f),
            new Color(0.06f, 0.13f, 0.11f, 1f),
            new Color(0.15f, 0.08f, 0.09f, 1f)
        };

        string scenesDir = "Assets/_Project/Module_SystemUI/Scenes";
        Directory.CreateDirectory(scenesDir);

        for (int i = 0; i < sceneNames.Length; i++)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Camera nền riêng cho từng màn dễ phân biệt khi chuyển cảnh
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = bgColors[i];
                cam.orthographic = true;
            }

            // 2. GameManager dự phòng (nếu bấm Play trực tiếp từ màn chơi mà không qua MainMenu)
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            // 3. EventSystem chuẩn Input System mới
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            // 4. Đặt Prefab GameHUD_Canvas vào Scene
            GameObject hudInstance = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hudInstance.name = "GameHUD_Canvas";

            // Cập nhật tên nhiệm vụ góc trên bên phải của HUD
            Transform missionLabelTr = hudInstance.transform.Find("TopRight_MissionBox/MissionTitleText");
            if (missionLabelTr != null)
            {
                var tmp = missionLabelTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = missionTitles[i];
            }

            // 5. Gắn bộ giả lập phím test HUDTestHarness
            GameObject testerObj = new GameObject("HUDTestHarness");
            testerObj.AddComponent<HUDTestHarness>();

            // 6. Bảng hướng dẫn phím test nổi giữa màn hình trống
            CreateOnScreenTestGuide(sceneNames[i], missionTitles[i]);

            string savePath = $"{scenesDir}/{sceneNames[i]}.unity";
            EditorSceneManager.SaveScene(scene, savePath);
        }

        // 7. Tự động đưa MainMenu + 3 Mission vào EditorBuildSettings
        ConfigureBuildSettings();

        // Mở lại scene MainMenu để sẵn sàng chạy từ đầu
        string mainMenuPath = $"{scenesDir}/MainMenu.unity";
        if (File.Exists(mainMenuPath))
        {
            EditorSceneManager.OpenScene(mainMenuPath);
        }

        AssetDatabase.Refresh();
        Debug.Log("Đã tạo xong 3 màn chơi (Mission_1, Mission_2, Mission_3) và cập nhật Build Settings!");
    }

    private static void CreateOnScreenTestGuide(string sceneName, string missionTitle)
    {
        GameObject guideCanvas = new GameObject("TestGuide_Canvas");
        Canvas c = guideCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 1;

        CanvasScaler scaler = guideCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject box = new GameObject("GuidePanel");
        box.transform.SetParent(guideCanvas.transform, false);
        RectTransform rt = box.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -40f);
        rt.sizeDelta = new Vector2(880f, 340f);
        Image img = box.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.45f);
        img.raycastTarget = false;

        GameObject txtObj = new GameObject("GuideText");
        txtObj.transform.SetParent(box.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(20f, 20f);
        trt.offsetMax = new Vector2(-20f, -20f);

        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 21;
        tmp.raycastTarget = false;
        tmp.text =
            $"<color=#FF9E2C><b>[{sceneName}] — {missionTitle}</b></color>\n\n" +
            "<color=#38BDF8>BẢNG PHÍM KIỂM THỬ GIAO DIỆN HUD & POPUP:</color>\n" +
            "• <b>ESC</b> (hoặc nút <b>II</b> góc trên phải) : Bật / Tắt <b>PAUSE MENU</b>\n" +
            "• <b>F1</b> : Trừ 25 HP (Hết HP trừ 1 mạng, hết 3 mạng bật <b>GAME OVER</b>)\n" +
            "• <b>F2</b> : Hồi đầy 100 HP     |     <b>F5</b> : Cộng +500 điểm\n" +
            "• <b>F3</b> : Đổi vũ khí (Pistol / HMG / Shotgun / Rocket)\n" +
            "• <b>F4</b> : Giảm đạn & lựu đạn\n" +
            "• <b>F6</b> : Kích hoạt ngay <b>GAME OVER</b>     |     <b>F7</b> : Kích hoạt ngay <b>VICTORY</b>";
    }

    private static void ConfigureBuildSettings()
    {
        string[] orderedPaths =
        {
            "Assets/_Project/Module_SystemUI/Scenes/MainMenu.unity",
            "Assets/_Project/Module_SystemUI/Scenes/Mission_1.unity",
            "Assets/_Project/Module_SystemUI/Scenes/Mission_2.unity",
            "Assets/_Project/Module_SystemUI/Scenes/Mission_3.unity"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        foreach (string path in orderedPaths)
        {
            if (File.Exists(path))
            {
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }
        EditorBuildSettings.scenes = buildScenes.ToArray();
    }
}
#endif