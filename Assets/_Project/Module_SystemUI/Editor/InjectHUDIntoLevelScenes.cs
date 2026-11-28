#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MetalRebels.Core.Managers;
using MetalRebels.UI;
using MetalRebels.Player;

/// <summary>
/// Editor tool: Inject GameManager + GameHUD_Canvas vào các scene Level,
/// xóa PlayerDebugHUD cũ, và thêm GameManager vào MainMenu.
/// Chạy 1 lần qua menu: MetalRebels > Inject HUD vào Level Scenes
/// </summary>
public class InjectHUDIntoLevelScenes
{
    private static readonly string[] levelScenePaths =
    {
        "Assets/_Project/Game/Scenes/Level_01.unity",
        "Assets/_Project/Game/Scenes/Level_02.unity",
        "Assets/_Project/Game/Scenes/Level_03.unity",
    };

    private const string MainMenuScenePath = "Assets/_Project/Game/Scenes/MainMenu.unity";
    private const string HUDPrefabPath = "Assets/_Project/Module_SystemUI/Prefabs/GameHUD_Canvas.prefab";

    [MenuItem("MetalRebels/Inject HUD vào Level Scenes")]
    public static void InjectAll()
    {
        string currentScenePath = SceneManager.GetActiveScene().path;

        GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDPrefabPath);
        if (hudPrefab == null)
        {
            Debug.LogError($"[InjectHUD] Không tìm thấy prefab HUD tại: {HUDPrefabPath}. Hãy chạy Menu > MetalRebels > Tạo Prefab HUD trước.");
            return;
        }

        int injectedCount = 0;

        // ========== INJECT VÀO CÁC LEVEL SCENE ==========
        foreach (string scenePath in levelScenePaths)
        {
            string fullPath = scenePath.Replace("Assets/", Application.dataPath + "/");
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogWarning($"[InjectHUD] Scene không tồn tại: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool modified = false;

            bool hasGameManager = false;
            bool hasHUD = false;
            bool hasEventSystem = false;

            // Tìm và xóa PlayerDebugHUD cũ
            foreach (GameObject rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.GetComponent<GameManager>() != null)
                    hasGameManager = true;
                if (rootObj.GetComponent<HUDController>() != null)
                    hasHUD = true;
                if (rootObj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                    hasEventSystem = true;

                // Xóa PlayerDebugHUD
                var debugHUD = rootObj.GetComponent<PlayerDebugHUD>();
                if (debugHUD != null)
                {
                    Debug.Log($"[InjectHUD] Xóa PlayerDebugHUD khỏi {scenePath}");
                    Object.DestroyImmediate(rootObj);
                    modified = true;
                }
            }

            // 1. Thêm GameManager nếu chưa có
            if (!hasGameManager)
            {
                GameObject gmObj = new GameObject("GameManager");
                GameManager gm = gmObj.AddComponent<GameManager>();

                SerializedObject so = new SerializedObject(gm);
                so.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
                var arr = so.FindProperty("missionSceneNames");
                arr.arraySize = 3;
                arr.GetArrayElementAtIndex(0).stringValue = "Level_01";
                arr.GetArrayElementAtIndex(1).stringValue = "Level_02";
                arr.GetArrayElementAtIndex(2).stringValue = "Level_03";
                so.ApplyModifiedProperties();

                SceneManager.MoveGameObjectToScene(gmObj, scene);
                modified = true;
                Debug.Log($"[InjectHUD] Đã thêm GameManager vào {scenePath}");
            }

            // 2. Thêm GameHUD_Canvas prefab nếu chưa có
            if (!hasHUD)
            {
                GameObject hudInstance = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
                SceneManager.MoveGameObjectToScene(hudInstance, scene);
                modified = true;
                Debug.Log($"[InjectHUD] Đã thêm GameHUD_Canvas vào {scenePath}");
            }

            // 3. Thêm EventSystem nếu chưa có
            if (!hasEventSystem)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                SceneManager.MoveGameObjectToScene(esObj, scene);
                modified = true;
                Debug.Log($"[InjectHUD] Đã thêm EventSystem vào {scenePath}");
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(scene);
                injectedCount++;
            }
            else
            {
                Debug.Log($"[InjectHUD] Scene {scenePath} đã có đủ, bỏ qua.");
            }
        }

        // ========== INJECT GameManager VÀO MAIN MENU ==========
        {
            string fullPath = MainMenuScenePath.Replace("Assets/", Application.dataPath + "/");
            if (System.IO.File.Exists(fullPath))
            {
                Scene menuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

                bool hasGM = false;
                foreach (GameObject rootObj in menuScene.GetRootGameObjects())
                {
                    if (rootObj.GetComponent<GameManager>() != null)
                        hasGM = true;
                }

                if (!hasGM)
                {
                    GameObject gmObj = new GameObject("GameManager");
                    GameManager gm = gmObj.AddComponent<GameManager>();

                    SerializedObject so = new SerializedObject(gm);
                    so.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
                    var arr = so.FindProperty("missionSceneNames");
                    arr.arraySize = 3;
                    arr.GetArrayElementAtIndex(0).stringValue = "Level_01";
                    arr.GetArrayElementAtIndex(1).stringValue = "Level_02";
                    arr.GetArrayElementAtIndex(2).stringValue = "Level_03";
                    so.ApplyModifiedProperties();

                    SceneManager.MoveGameObjectToScene(gmObj, menuScene);
                    EditorSceneManager.SaveScene(menuScene);
                    Debug.Log($"[InjectHUD] Đã thêm GameManager vào MainMenu scene.");
                }
            }
        }

        // Mở lại scene ban đầu
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            string fullCurrent = currentScenePath.Replace("Assets/", Application.dataPath + "/");
            if (System.IO.File.Exists(fullCurrent))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            else
                EditorSceneManager.OpenScene(levelScenePaths[0], OpenSceneMode.Single);
        }

        Debug.Log($"[InjectHUD] Hoàn tất! Đã xử lý {injectedCount}/{levelScenePaths.Length} scene Level + MainMenu.");
        EditorUtility.DisplayDialog("Inject HUD",
            $"Đã inject GameManager + HUD vào {injectedCount} scene Level.\n" +
            "• Xóa PlayerDebugHUD cũ\n" +
            "• Thêm GameManager + GameHUD_Canvas + EventSystem\n" +
            "• MainMenu đã được thay bằng bản mới từ SystemUI\n\n" +
            "Mở scene Level_01 để kiểm tra.", "OK");
    }
}
#endif
