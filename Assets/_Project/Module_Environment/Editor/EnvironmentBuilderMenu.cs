#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using MetalRebels.Core.Interfaces;
using MetalRebels.Module_Environment;

namespace MetalRebels.Module_Environment.Editor
{
    public static class EnvironmentBuilderMenu
    {
        private const string PrefabDir = "Assets/_Project/Module_Environment/Prefabs";
        private const string SceneDir = "Assets/_Project/Game/Scenes";

        [MenuItem("MetalRebels/Environment/Generate All Prefabs and Scenes (Levels 1-3 & MainMenu)", false, 1)]
        public static void GenerateAll()
        {
            EnsureDirectories();
            GenerateAllPrefabs();
            GenerateLevel01Scene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Metal Rebels", "Đã tạo/cập nhật đầy đủ bộ Prefab Môi trường và các Scene thành công!", "OK");
        }

        [MenuItem("MetalRebels/Environment/Generate Sandbox Scene (HuuHieu)", false, 2)]
        public static void GenerateSandbox()
        {
            EnsureDirectories();
            string scenePath = "Assets/_Project/Module_Environment/Sandbox_HuuHieu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCameraAndSystems(scene);
            SetupSampleTerrain();

            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[MetalRebels] Đã tạo Scene Sandbox tại: {scenePath}");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Module_Environment/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Module_Environment", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Game/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Game", "Scenes");
            }
        }

        public static void GenerateAllPrefabs()
        {
            // 1. Pickups
            CreatePickupPrefab("Pickup_Health", PickupType.Health, 30, Color.red);
            CreatePickupPrefab("Pickup_ShotgunAmmo", PickupType.ShotgunAmmo, 20, new Color(1f, 0.5f, 0f));
            CreatePickupPrefab("Pickup_RifleAmmo", PickupType.RifleAmmo, 30, Color.yellow);
            CreatePickupPrefab("Pickup_Rocket", PickupType.RocketWeapon, 1, new Color(0.8f, 0.2f, 1f));
            CreatePickupPrefab("Pickup_RocketAmmo", PickupType.RocketAmmo, 2, Color.magenta);
            CreatePickupPrefab("Pickup_Grenade", PickupType.Grenade, 2, new Color(0.2f, 0.8f, 0.2f));

            // 2. Breakable Crate
            CreateBreakableCratePrefab();

            // 3. Checkpoint
            CreateCheckpointPrefab();

            // 4. Death Zone
            CreateDeathZonePrefab();

            // 5. Hostage
            CreateHostagePrefab();

            // 6. Event Lock Barrier
            CreateEventLockBarrierPrefab();

            // 7. Event Lock Zone
            CreateEventLockZonePrefab();

            AssetDatabase.SaveAssets();
            Debug.Log("[MetalRebels] Tất cả 12 Prefabs Môi trường đã được khởi tạo!");
        }

        private static void CreatePickupPrefab(string prefabName, PickupType type, int amount, Color color)
        {
            GameObject go = new GameObject(prefabName);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var pickup = go.AddComponent<ItemPickup>();
            pickup.Setup(type, amount);

            SaveAndDestroy(go, $"{PrefabDir}/{prefabName}.prefab");
        }

        private static void CreateBreakableCratePrefab()
        {
            GameObject go = new GameObject("Breakable_Crate");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.6f, 0.4f, 0.2f); // Màu gỗ

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            var breakable = go.AddComponent<BreakableObject>();

            // Gán bảng rơi đồ mẫu
            var healthPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Pickup_Health.prefab");
            var ammoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Pickup_RifleAmmo.prefab");

            // Reflection hoặc serialized fields nếu cần, cơ bản BreakableObject đã có sẵn dropTable
            SaveAndDestroy(go, $"{PrefabDir}/Breakable_Crate.prefab");
        }

        private static void CreateCheckpointPrefab()
        {
            GameObject go = new GameObject("Checkpoint");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 2f);

            go.AddComponent<Checkpoint>();
            SaveAndDestroy(go, $"{PrefabDir}/Checkpoint.prefab");
        }

        private static void CreateDeathZonePrefab()
        {
            GameObject go = new GameObject("DeathZone");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(15f, 1.5f);

            go.AddComponent<DeathZone>();
            SaveAndDestroy(go, $"{PrefabDir}/DeathZone.prefab");
        }

        private static void CreateHostagePrefab()
        {
            GameObject go = new GameObject("Hostage");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.7f, 0.9f); // Màu xanh dương nhạt

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1.6f);

            go.AddComponent<HostageController>();
            SaveAndDestroy(go, $"{PrefabDir}/Hostage.prefab");
        }

        private static void CreateEventLockBarrierPrefab()
        {
            GameObject go = new GameObject("EventLockBarrier");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = Color.red;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 5f);

            go.AddComponent<EventLockBarrier>();
            SaveAndDestroy(go, $"{PrefabDir}/EventLockBarrier.prefab");
        }

        private static void CreateEventLockZonePrefab()
        {
            GameObject root = new GameObject("EventLockZone");
            var triggerCol = root.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(2f, 5f);

            var trigger = root.AddComponent<EventLockTrigger>();
            var waveManager = root.AddComponent<WaveManager>();

            // Child points
            GameObject camPoint = new GameObject("CameraLockPoint");
            camPoint.transform.SetParent(root.transform);
            camPoint.transform.localPosition = Vector3.zero;

            GameObject spawnLeft = new GameObject("SpawnPoint_Left");
            spawnLeft.transform.SetParent(root.transform);
            spawnLeft.transform.localPosition = new Vector3(-8f, 0f, 0f);

            GameObject spawnRight = new GameObject("SpawnPoint_Right");
            spawnRight.transform.SetParent(root.transform);
            spawnRight.transform.localPosition = new Vector3(8f, 0f, 0f);

            SaveAndDestroy(root, $"{PrefabDir}/EventLockZone.prefab");
        }

        private static void SaveAndDestroy(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        public static void GenerateLevel01Scene()
        {
            string scenePath = $"{SceneDir}/Level_01.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Systems
            GameObject systemsRoot = new GameObject("--- SYSTEMS ---");
            GameObject levelManagerGO = new GameObject("LevelManager");
            levelManagerGO.transform.SetParent(systemsRoot.transform);
            var lm = levelManagerGO.AddComponent<LevelManager>();

            // Camera
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(systemsRoot.transform);
            camGO.transform.position = new Vector3(0f, 1f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.18f, 0.28f);

            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<CameraFollow2D>();
            camGO.AddComponent<CameraShaker>();

            // 2. Terrain Grid
            GameObject gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(systemsRoot.transform);
            gridGO.AddComponent<Grid>();

            // 3. Environment & Interactables
            GameObject envRoot = new GameObject("--- ENVIRONMENT & INTERACTABLES ---");
            GameObject breakablesContainer = new GameObject("Breakables");
            breakablesContainer.transform.SetParent(envRoot.transform);

            // Bố trí thùng phá hủy mẫu
            var cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Breakable_Crate.prefab");
            if (cratePrefab != null)
            {
                float[] cratePositions = { 10f, 18f, 32f, 50f, 75f, 95f };
                foreach (float x in cratePositions)
                {
                    GameObject crate = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab, breakablesContainer.transform);
                    crate.transform.position = new Vector3(x, 0.5f, 0f);
                }
            }

            // Checkpoints
            GameObject checkpointsContainer = new GameObject("Checkpoints");
            checkpointsContainer.transform.SetParent(envRoot.transform);
            var cpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Checkpoint.prefab");
            if (cpPrefab != null)
            {
                GameObject cp1 = (GameObject)PrefabUtility.InstantiatePrefab(cpPrefab, checkpointsContainer.transform);
                cp1.name = "Checkpoint_BeachTrench";
                cp1.transform.position = new Vector3(25f, 1f, 0f);

                GameObject cp2 = (GameObject)PrefabUtility.InstantiatePrefab(cpPrefab, checkpointsContainer.transform);
                cp2.name = "Checkpoint_BeforeBoss";
                cp2.transform.position = new Vector3(85f, 1f, 0f);
            }

            // Con tin (Hostages)
            GameObject hostagesContainer = new GameObject("Hostages");
            hostagesContainer.transform.SetParent(envRoot.transform);
            var hostagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Hostage.prefab");
            if (hostagePrefab != null)
            {
                GameObject h1 = (GameObject)PrefabUtility.InstantiatePrefab(hostagePrefab, hostagesContainer.transform);
                h1.name = "Hostage_Beach";
                h1.transform.position = new Vector3(22f, 0.8f, 0f);

                GameObject h2 = (GameObject)PrefabUtility.InstantiatePrefab(hostagePrefab, hostagesContainer.transform);
                h2.name = "Hostage_Camp";
                h2.transform.position = new Vector3(65f, 0.8f, 0f);
            }

            // Hazards
            GameObject hazardsContainer = new GameObject("Hazards");
            hazardsContainer.transform.SetParent(envRoot.transform);
            var deathZonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/DeathZone.prefab");
            if (deathZonePrefab != null)
            {
                GameObject dz1 = (GameObject)PrefabUtility.InstantiatePrefab(deathZonePrefab, hazardsContainer.transform);
                dz1.name = "DeathZone_Pit1";
                dz1.transform.position = new Vector3(40f, -4f, 0f);
            }

            // 4. Progression & Event Lock
            GameObject progRoot = new GameObject("--- LEVEL PROGRESSION (EVENTS) ---");
            var eventLockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/EventLockZone.prefab");
            if (eventLockPrefab != null)
            {
                GameObject el = (GameObject)PrefabUtility.InstantiatePrefab(eventLockPrefab, progRoot.transform);
                el.name = "EventLock_BeachTrench";
                el.transform.position = new Vector3(55f, 2f, 0f);
            }

            // Boss Arena
            GameObject bossArena = new GameObject("BossArena_Viper01");
            bossArena.transform.SetParent(progRoot.transform);
            bossArena.transform.position = new Vector3(110f, 2f, 0f);

            // 5. Spawn Points & Containers
            GameObject spawnRoot = new GameObject("--- SPAWN POINTS & CONTAINERS ---");
            GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
            playerSpawn.transform.SetParent(spawnRoot.transform);
            playerSpawn.transform.position = new Vector3(0f, 0.5f, 0f);

            GameObject enemiesContainer = new GameObject("EnemiesContainer");
            enemiesContainer.transform.SetParent(spawnRoot.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MetalRebels] Đã tạo Scene Level 1 thành công tại: {scenePath}");
        }

        private static void SetupCameraAndSystems(UnityEngine.SceneManagement.Scene scene)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 1f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.2f, 0.25f);

            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<CameraFollow2D>();
            camGO.AddComponent<CameraShaker>();
        }

        private static void SetupSampleTerrain()
        {
            GameObject ground = new GameObject("Ground_Test");
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            var col = ground.AddComponent<BoxCollider2D>();
            col.size = new Vector2(30f, 1f);

            var sr = ground.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
        }
    }
}
#endif

