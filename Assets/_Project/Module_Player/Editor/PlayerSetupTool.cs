#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MetalRebels.Core.Data;
using MetalRebels.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MetalRebels.Player.EditorTools
{
    public static class PlayerSetupTool
    {
        private const string WeaponDataDir = "Assets/_Project/Core/Data";
        private const string PrefabDir = "Assets/_Project/Module_Player/Prefabs";
        private const string SpritesDir = "Assets/_Project/Module_Player/Sprites";
        private const string SandboxScenePath = "Assets/_Project/Module_Player/Sandbox_VietHoang.unity";

        [MenuItem("MetalRebels/Module Player/Setup All Player Assets and Data", priority = 10)]
        public static void SetupAll()
        {
            EnsureDirectories();
            ConfigureSpriteImportSettings();

            CreateWeaponDataAssets(out WeaponData pistol, out WeaponData shotgun, out WeaponData rifle, out WeaponData rocket);
            GameObject explosionPrefab = CreateExplosionPrefab();
            GameObject bulletPrefab = CreateBulletPrefab();
            GameObject rocketPrefab = CreateRocketPrefab(explosionPrefab);
            Sprite grenadeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Grenade/Grenade.png");
            if (grenadeSprite == null)
            {
                grenadeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Grenade.png");
            }
            if (grenadeSprite == null)
            {
                Debug.LogWarning($"[MetalRebels] Grenade sprite was not found at {SpritesDir}/Grenade/Grenade.png");
            }

            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Bullet/PlayerBullet.png");

            GameObject grenadePrefab = CreateGrenadePrefab(explosionPrefab, grenadeSprite);

            // Gán prefab projectile vào WeaponData
            if (pistol != null && bulletPrefab != null)
            {
                pistol.projectilePrefab = bulletPrefab;
                EditorUtility.SetDirty(pistol);
            }
            if (rifle != null && bulletPrefab != null)
            {
                rifle.projectilePrefab = bulletPrefab;
                EditorUtility.SetDirty(rifle);
            }
            if (rocket != null && rocketPrefab != null)
            {
                rocket.projectilePrefab = rocketPrefab;
                EditorUtility.SetDirty(rocket);
            }

            CreatePlayerPrefab(pistol, shotgun, rifle, rocket, grenadePrefab, explosionPrefab, grenadeSprite, bulletSprite);
            CreatePickupPrefabs(shotgun, rifle, rocket);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[MetalRebels]</color> Đã tạo và cấu hình đầy đủ toàn bộ Assets, Prefabs, Sprites và Data cho Player Marco thành công!");
        }

        [MenuItem("MetalRebels/Module Player/Create Sandbox Test Scene (VietHoang)", priority = 11)]
        public static void CreateSandboxScene()
        {
            SetupAll();

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.transform.position = new Vector3(0, 2.5f, -10);

            // URP Camera Data
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // 2. Global Light 2D
            GameObject lightObj = new GameObject("Global 2D Light");
            Light2D light2d = lightObj.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Global;
            light2d.intensity = 1f;

            // 3. Ground & Platforms
            GameObject envParent = new GameObject("Environment");

            // Mặt đất chính
            CreatePlatform(envParent.transform, "MainGround", new Vector3(0, -0.5f, 0), new Vector2(40f, 1f), new Color(0.25f, 0.3f, 0.35f));

            // Bệ nhảy bên trái & phải
            CreatePlatform(envParent.transform, "Platform_Left", new Vector3(-6f, 2.0f, 0), new Vector2(6f, 0.5f), new Color(0.35f, 0.4f, 0.45f));
            CreatePlatform(envParent.transform, "Platform_Right", new Vector3(6f, 2.0f, 0), new Vector2(6f, 0.5f), new Color(0.35f, 0.4f, 0.45f));
            CreatePlatform(envParent.transform, "Platform_High", new Vector3(0f, 4.0f, 0), new Vector2(8f, 0.5f), new Color(0.35f, 0.4f, 0.45f));

            // Hầm trần thấp để test ngồi né đạn (Crouch test tunnel)
            CreatePlatform(envParent.transform, "CrouchCeiling_Obstacle", new Vector3(12f, 1.4f, 0), new Vector2(8f, 0.4f), new Color(0.5f, 0.3f, 0.2f));

            // Bẫy gai (Hazard Spikes) gây 20 sát thương để test mất máu và bất tử
            CreateHazardSpikes(envParent.transform, new Vector3(-12f, 0.2f, 0));

            // 4. Checkpoint
            CreateCheckpoint(envParent.transform, new Vector3(4f, 0.5f, 0));

            // 5. Bia mục tiêu (Target Dummies)
            GameObject dummyParent = new GameObject("TargetDummies");
            CreateDummy(dummyParent.transform, "Dummy_Ground", new Vector3(8f, 0.7f, 0), 100);
            CreateDummy(dummyParent.transform, "Dummy_Elevated", new Vector3(0f, 5.0f, 0), 80);
            CreateDummy(dummyParent.transform, "Dummy_FarRight", new Vector3(15f, 0.7f, 0), 150);

            // 6. Vật phẩm Pickup
            GameObject pickupParent = new GameObject("Pickups");
            InstantiatePickup(pickupParent.transform, $"{PrefabDir}/Pickup_Health.prefab", new Vector3(-3f, 0.4f, 0));
            InstantiatePickup(pickupParent.transform, $"{PrefabDir}/Pickup_ShotgunAmmo.prefab", new Vector3(-1f, 0.4f, 0));
            InstantiatePickup(pickupParent.transform, $"{PrefabDir}/Pickup_RifleAmmo.prefab", new Vector3(1f, 0.4f, 0));
            InstantiatePickup(pickupParent.transform, $"{PrefabDir}/Pickup_Rocket.prefab", new Vector3(3f, 0.4f, 0));
            InstantiatePickup(pickupParent.transform, $"{PrefabDir}/Pickup_Grenade.prefab", new Vector3(5f, 0.4f, 0));

            // 7. Player Marco
            string playerPrefabPath = $"{PrefabDir}/Player_Marco.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (playerPrefab != null)
            {
                GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerInstance.transform.position = new Vector3(-8f, 0.5f, 0f);
            }

            // 8. Debug HUD
            GameObject hudObj = new GameObject("PlayerDebugHUD");
            hudObj.AddComponent<PlayerDebugHUD>();

            EditorSceneManager.SaveScene(newScene, SandboxScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[MetalRebels]</color> Đã tạo scene kiểm thử cá nhân thành công tại: {SandboxScenePath}");
        }

        private static void ConfigureSpriteImportSettings()
        {
            string[] pngFiles = Directory.GetFiles(SpritesDir, "*.png", SearchOption.AllDirectories);
            foreach (string file in pngFiles)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        changed = true;
                    }
                    if (importer.spritePixelsPerUnit != 32)
                    {
                        importer.spritePixelsPerUnit = 32;
                        changed = true;
                    }
                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(WeaponDataDir)) Directory.CreateDirectory(WeaponDataDir);
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);
            // SpritesDir (Assets/_Project/Module_Player/Sprites) is already committed in the repository
            AssetDatabase.Refresh();
        }

        public static void CreateWeaponDataAssets(out WeaponData pistol, out WeaponData shotgun, out WeaponData rifle, out WeaponData rocket)
        {
            EnsureDirectories();

            pistol = GetOrCreateWeaponAsset($"{WeaponDataDir}/Weapon_Pistol.asset", WeaponType.Pistol, "Pistol", 20, 2f, false, true, 0, 0, 0f, 0, 12f, 3f, false, 0f, 0f);
            shotgun = GetOrCreateWeaponAsset($"{WeaponDataDir}/Weapon_Shotgun.asset", WeaponType.Shotgun, "Shotgun", 50, 1f, false, false, 30, 8, 3.0f, 20, 12f, 1.5f, false, 0f, 0f);
            rifle = GetOrCreateWeaponAsset($"{WeaponDataDir}/Weapon_AssaultRifle.asset", WeaponType.AssaultRifle, "Assault Rifle", 15, 5f, true, false, 120, 30, 1.5f, 30, 14f, 3f, false, 0f, 0f);
            rocket = GetOrCreateWeaponAsset($"{WeaponDataDir}/Weapon_RocketLauncher.asset", WeaponType.RocketLauncher, "Rocket Launcher", 100, 0.5f, false, false, 5, 1, 2.0f, 2, 9f, 4f, true, 3f, 0f);
        }

        private static WeaponData GetOrCreateWeaponAsset(string path, WeaponType type, string name, int damage, float fireRate, bool isAuto, bool isInfinite, int maxAmmo, int magSize, float reloadTime, int ammoPerPickup, float speed, float lifetime, bool isExplosive, float explosionRadius, float fuseTime)
        {
            WeaponData asset = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.weaponType = type;
            asset.weaponName = name;
            asset.damage = damage;
            asset.fireRate = fireRate;
            asset.isAutomatic = isAuto;
            asset.isInfiniteAmmo = isInfinite;
            asset.maxAmmo = maxAmmo;
            asset.magazineSize = magSize;
            asset.reloadTime = reloadTime;
            asset.ammoPerPickup = ammoPerPickup;
            asset.bulletSpeed = speed;
            asset.bulletLifetime = lifetime;
            asset.isExplosive = isExplosive;
            asset.explosionRadius = explosionRadius;
            asset.fuseTime = fuseTime;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static GameObject CreateExplosionPrefab()
        {
            string path = $"{PrefabDir}/ExplosionEffect.prefab";
            List<Sprite> expSprites = LoadExplosionSprites();
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                ExplosionEffect existingEff = existing.GetComponent<ExplosionEffect>();
                SpriteRenderer existingSr = existing.GetComponent<SpriteRenderer>();
                if (existingSr != null && expSprites.Count > 0)
                {
                    existingSr.sprite = expSprites[0];
                    existingSr.color = Color.white;
                    existingSr.sortingOrder = 10;
                }
                if (existingEff != null && expSprites.Count > 0)
                {
                    SerializedObject effSO = new SerializedObject(existingEff);
                    SerializedProperty listProp = effSO.FindProperty("explosionSprites");
                    listProp.arraySize = expSprites.Count;
                    for (int i = 0; i < expSprites.Count; i++)
                    {
                        listProp.GetArrayElementAtIndex(i).objectReferenceValue = expSprites[i];
                    }
                    effSO.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(existing);
                PrefabUtility.SavePrefabAsset(existing);
                return existing;
            }

            GameObject obj = new GameObject("ExplosionEffect");
            ExplosionEffect eff = obj.AddComponent<ExplosionEffect>();
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = Color.white;
            sr.sortingOrder = 10;

            // Load các sprite nổ
            if (expSprites.Count > 0)
            {
                sr.sprite = expSprites[0];
                SerializedObject effSO = new SerializedObject(eff);
                SerializedProperty listProp = effSO.FindProperty("explosionSprites");
                listProp.arraySize = expSprites.Count;
                for (int i = 0; i < expSprites.Count; i++)
                {
                    listProp.GetArrayElementAtIndex(i).objectReferenceValue = expSprites[i];
                }
                effSO.ApplyModifiedProperties();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateBulletPrefab()
        {
            string path = $"{PrefabDir}/PlayerBullet.prefab";
            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Bullet/PlayerBullet.png");
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                SpriteRenderer existingSr = existing.GetComponent<SpriteRenderer>();
                if (existingSr != null && bulletSprite != null)
                {
                    existingSr.sprite = bulletSprite;
                    existingSr.color = Color.white;
                    existingSr.sortingOrder = 5;
                }
                PlayerBullet existingBullet = existing.GetComponent<PlayerBullet>();
                if (existingBullet != null && bulletSprite != null)
                {
                    SerializedObject bSO = new SerializedObject(existingBullet);
                    bSO.FindProperty("visualSprite").objectReferenceValue = bulletSprite;
                    bSO.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(existing);
                PrefabUtility.SavePrefabAsset(existing);
                return existing;
            }

            GameObject obj = new GameObject("PlayerBullet");
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            PlayerBullet bullet = obj.AddComponent<PlayerBullet>();
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = bulletSprite;
            sr.color = Color.white;
            sr.sortingOrder = 5;

            if (bulletSprite != null)
            {
                var bulletSO = new SerializedObject(bullet);
                bulletSO.FindProperty("visualSprite").objectReferenceValue = bulletSprite;
                bulletSO.ApplyModifiedProperties();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateRocketPrefab(GameObject explosionPrefab)
        {
            string path = $"{PrefabDir}/PlayerRocket.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("PlayerRocket");
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, 0.15f);
            col.isTrigger = true;
            PlayerRocket rocket = obj.AddComponent<PlayerRocket>();
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.2f, 0.1f, 1f);

            var serializedObj = new SerializedObject(rocket);
            serializedObj.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            serializedObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateGrenadePrefab(GameObject explosionPrefab, Sprite grenadeSprite)
        {
            string path = $"{PrefabDir}/PlayerGrenade.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                bool changed = false;
                SpriteRenderer existingRenderer = existing.GetComponent<SpriteRenderer>();
                if (existingRenderer != null && grenadeSprite != null && existingRenderer.sprite != grenadeSprite)
                {
                    existingRenderer.sprite = grenadeSprite;
                    existingRenderer.color = Color.white;
                    existingRenderer.sortingOrder = 2;
                    changed = true;
                }

                PlayerGrenade existingGrenade = existing.GetComponent<PlayerGrenade>();
                if (existingGrenade != null)
                {
                    SerializedObject grenadeSO = new SerializedObject(existingGrenade);
                    SetObjectReferenceIfChanged(grenadeSO, "explosionPrefab", explosionPrefab);
                    SerializedProperty visualScale = grenadeSO.FindProperty("explosionVisualScale");
                    if (visualScale.floatValue != 0.22f)
                    {
                        visualScale.floatValue = 0.22f;
                    }
                    changed |= grenadeSO.ApplyModifiedProperties();
                }

                Collider2D existingCollider = existing.GetComponent<Collider2D>();
                if (existingCollider != null && existingCollider.isTrigger)
                {
                    existingCollider.isTrigger = false;
                    changed = true;
                }

                Rigidbody2D existingBody = existing.GetComponent<Rigidbody2D>();
                if (existingBody != null && existingBody.gravityScale != 2.5f)
                {
                    existingBody.gravityScale = 2.5f;
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(existing);
                    PrefabUtility.SavePrefabAsset(existing);
                }

                return existing;
            }

            GameObject obj = new GameObject("PlayerGrenade");
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.18f;
            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.5f;
            PlayerGrenade grenade = obj.AddComponent<PlayerGrenade>();
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = grenadeSprite;
            sr.color = Color.white;
            sr.sortingOrder = 2;

            var serializedObj = new SerializedObject(grenade);
            serializedObj.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            serializedObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(WeaponData pistol, WeaponData shotgun, WeaponData rifle, WeaponData rocket, GameObject grenadePrefab, GameObject explosionPrefab, Sprite grenadeSprite, Sprite bulletSprite)
        {
            string path = $"{PrefabDir}/Player_Marco.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
                PlayerCombat contentsCombat = prefabContents.GetComponent<PlayerCombat>();
                if (contentsCombat != null)
                {
                    EnsureForwardFirePoints(prefabContents.transform, out Transform existingPistolFirePoint, out Transform existingShotgunFirePoint);
                    SerializedObject contentsCombatSO = new SerializedObject(contentsCombat);
                    contentsCombatSO.FindProperty("firePointForward1").objectReferenceValue = existingPistolFirePoint;
                    contentsCombatSO.FindProperty("firePointForward2").objectReferenceValue = existingShotgunFirePoint;
                    contentsCombatSO.ApplyModifiedProperties();
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                }
                PrefabUtility.UnloadPrefabContents(prefabContents);

                existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                PlayerCombat existingCombat = existing.GetComponent<PlayerCombat>();
                if (existingCombat != null)
                {
                    SerializedObject existingCombatSO = new SerializedObject(existingCombat);
                    SetObjectReferenceIfChanged(existingCombatSO, "pistolData", pistol);
                    SetObjectReferenceIfChanged(existingCombatSO, "shotgunData", shotgun);
                    SetObjectReferenceIfChanged(existingCombatSO, "assaultRifleData", rifle);
                    SetObjectReferenceIfChanged(existingCombatSO, "rocketLauncherData", rocket);
                    SetObjectReferenceIfChanged(existingCombatSO, "grenadePrefab", grenadePrefab);
                    SetObjectReferenceIfChanged(existingCombatSO, "grenadeSprite", grenadeSprite);
                    SetObjectReferenceIfChanged(existingCombatSO, "explosionPrefab", explosionPrefab);
                    SetObjectReferenceIfChanged(existingCombatSO, "bulletSprite", bulletSprite);
                    SetObjectReferenceIfChanged(existingCombatSO, "grenadeExplosionSheet", LoadGrenadeExplosionSheet());
                    SetObjectReferenceIfChanged(existingCombatSO, "shotgunExplosionSheet", LoadShotgunExplosionSheet());
                    existingCombatSO.FindProperty("grenadeThrowForceX").floatValue = 5.5f;
                    existingCombatSO.FindProperty("grenadeThrowForceY").floatValue = 5f;
                    AssignSpriteArray(existingCombatSO, "explosionSprites", LoadExplosionSprites());
                    if (existingCombatSO.ApplyModifiedProperties())
                    {
                        PrefabUtility.SavePrefabAsset(existing);
                    }
                }

                return existing;
            }

            // ── Root: Player_Marco ──
            GameObject playerObj = new GameObject("Player_Marco");
            playerObj.tag = "Player";

            // Physics
            Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.25f);
            col.offset = new Vector2(0f, 0.625f);

            // ── Visuals Parent ──
            GameObject visuals = new GameObject("Visuals");
            visuals.transform.SetParent(playerObj.transform);
            visuals.transform.localPosition = Vector3.zero;

            // ── UpperBody ──
            GameObject upperObj = new GameObject("UpperBody");
            upperObj.transform.SetParent(visuals.transform);
            upperObj.transform.localPosition = new Vector3(0.012f, 0.15f, 0f);

            SpriteRenderer upperSR = upperObj.AddComponent<SpriteRenderer>();
            upperSR.sortingOrder = 1;
            // Load idle sprite for upper body
            Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Module_Player/Sprites/Idle/Idle_Sungluc.png");
            if (idleSprite != null) upperSR.sprite = idleSprite;

            Animator upperAnimator = upperObj.AddComponent<Animator>();
            RuntimeAnimatorController upperAC = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Project/Module_Player/Animations/Marco_Upper_AC.controller");
            if (upperAC != null) upperAnimator.runtimeAnimatorController = upperAC;
            upperAnimator.updateMode = AnimatorUpdateMode.Normal;

            // ── LowerBody ──
            GameObject lowerObj = new GameObject("LowerBody");
            lowerObj.transform.SetParent(visuals.transform);
            lowerObj.transform.localPosition = new Vector3(-0.044f, 0.004f, 0f);

            SpriteRenderer lowerSR = lowerObj.AddComponent<SpriteRenderer>();
            lowerSR.sortingOrder = 0;
            // Load the idle sprite while the Animator initializes.
            Sprite lowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Module_Player/Sprites/Idle/LowerBody_Idle.png");
            if (lowerSprite != null) lowerSR.sprite = lowerSprite;

            Animator lowerAnimator = lowerObj.AddComponent<Animator>();
            RuntimeAnimatorController lowerAC = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Project/Module_Player/Animations/Marco_Lower_AC.controller");
            if (lowerAC != null) lowerAnimator.runtimeAnimatorController = lowerAC;
            lowerAnimator.updateMode = AnimatorUpdateMode.Normal;

            // ── Fire Points & Checks ──
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(playerObj.transform);
            groundCheck.transform.localPosition = new Vector2(0f, 0.02f);

            GameObject ceilingCheck = new GameObject("CeilingCheck");
            ceilingCheck.transform.SetParent(playerObj.transform);
            ceilingCheck.transform.localPosition = new Vector2(0f, 1.30f);

            GameObject firePointForward = new GameObject("Firepoint_Forward_1");
            firePointForward.transform.SetParent(playerObj.transform);
            firePointForward.transform.localPosition = new Vector2(0.55f, 0.65f);

            EnsureForwardFirePoints(playerObj.transform, out Transform pistolFirePoint, out Transform shotgunFirePoint);

            GameObject firePointUp = new GameObject("Firepoint_Up");
            firePointUp.transform.SetParent(playerObj.transform);
            firePointUp.transform.localPosition = new Vector2(0.1f, 1.35f);

            GameObject firePointDown = new GameObject("FirePointDown");
            firePointDown.transform.SetParent(playerObj.transform);
            firePointDown.transform.localPosition = new Vector2(0.1f, 0.05f);

            GameObject firePointCrouch = new GameObject("Firepoint_Crouch");
            firePointCrouch.transform.SetParent(playerObj.transform);
            firePointCrouch.transform.localPosition = new Vector2(0.55f, 0.35f);

            // ── Scripts ──
            // PlayerController
            PlayerController controller = playerObj.AddComponent<PlayerController>();
            SerializedObject ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            ctrlSO.FindProperty("ceilingCheck").objectReferenceValue = ceilingCheck.transform;
            ctrlSO.ApplyModifiedProperties();

            // PlayerInputHandler
            playerObj.AddComponent<PlayerInputHandler>();

            // PlayerAnimation — reference upper & lower animators
            PlayerAnimation anim = playerObj.AddComponent<PlayerAnimation>();
            SerializedObject animSO = new SerializedObject(anim);
            animSO.FindProperty("upperAnimator").objectReferenceValue = upperAnimator;
            animSO.FindProperty("lowerAnimator").objectReferenceValue = lowerAnimator;
            animSO.ApplyModifiedProperties();

            // PlayerCombat
            PlayerCombat combat = playerObj.AddComponent<PlayerCombat>();
            SerializedObject combatSO = new SerializedObject(combat);
            combatSO.FindProperty("firePointForward1").objectReferenceValue = pistolFirePoint;
            combatSO.FindProperty("firePointForward2").objectReferenceValue = shotgunFirePoint;
            combatSO.FindProperty("firePointUp").objectReferenceValue = firePointUp.transform;
            combatSO.FindProperty("firePointDown").objectReferenceValue = firePointDown.transform;
            combatSO.FindProperty("firePointCrouch").objectReferenceValue = firePointCrouch.transform;
            combatSO.FindProperty("pistolData").objectReferenceValue = pistol;
            combatSO.FindProperty("shotgunData").objectReferenceValue = shotgun;
            combatSO.FindProperty("assaultRifleData").objectReferenceValue = rifle;
            combatSO.FindProperty("rocketLauncherData").objectReferenceValue = rocket;
            combatSO.FindProperty("grenadePrefab").objectReferenceValue = grenadePrefab;
            combatSO.FindProperty("grenadeSprite").objectReferenceValue = grenadeSprite;
            combatSO.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            combatSO.FindProperty("bulletSprite").objectReferenceValue = bulletSprite;
            combatSO.FindProperty("grenadeExplosionSheet").objectReferenceValue = LoadGrenadeExplosionSheet();
            combatSO.FindProperty("shotgunExplosionSheet").objectReferenceValue = LoadShotgunExplosionSheet();
            combatSO.FindProperty("grenadeThrowForceX").floatValue = 5.5f;
            combatSO.FindProperty("grenadeThrowForceY").floatValue = 5f;
            AssignSpriteArray(combatSO, "explosionSprites", LoadExplosionSprites());
            combatSO.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerObj, path);
            Object.DestroyImmediate(playerObj);
            return prefab;
        }

        private static void EnsureForwardFirePoints(
            Transform playerRoot,
            out Transform pistolFirePoint,
            out Transform shotgunFirePoint)
        {
            pistolFirePoint = playerRoot.Find("Firepoint_Forward_1");
            if (pistolFirePoint == null)
            {
                pistolFirePoint = playerRoot.Find("Firepoint_Forward");
                if (pistolFirePoint == null)
                {
                    GameObject point = new GameObject("Firepoint_Forward_1");
                    pistolFirePoint = point.transform;
                    pistolFirePoint.SetParent(playerRoot, false);
                    pistolFirePoint.localPosition = new Vector3(0.55f, 0.65f, 0f);
                }
            }

            pistolFirePoint.name = "Firepoint_Forward_1";

            shotgunFirePoint = playerRoot.Find("Firepoint_Forward_2");
            if (shotgunFirePoint == null)
            {
                shotgunFirePoint = playerRoot.Find("Firepoint_Forward_Shotgun");
                if (shotgunFirePoint == null)
                {
                    GameObject point = new GameObject("Firepoint_Forward_2");
                    shotgunFirePoint = point.transform;
                    shotgunFirePoint.SetParent(playerRoot, false);
                }
            }

            shotgunFirePoint.name = "Firepoint_Forward_2";
            shotgunFirePoint.localPosition = new Vector3(
                pistolFirePoint.localPosition.x,
                0.07f,
                pistolFirePoint.localPosition.z);
        }

        private static void SetObjectReferenceIfChanged(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue != value)
            {
                property.objectReferenceValue = value;
            }
        }


        private static List<Sprite> LoadExplosionSprites()
        {
            List<Sprite> list = new List<Sprite>();
            Sprite visibleFrame = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Explosion/Explosion_3.png");
            if (visibleFrame != null)
            {
                list.Add(visibleFrame);
            }
            else
            {
                Debug.LogWarning($"[MetalRebels] Visible explosion sprite was not found at {SpritesDir}/Explosion/Explosion_3.png");
            }

            return list;
        }

        private static Texture2D LoadGrenadeExplosionSheet()
        {
            return LoadExplosionSheet($"{SpritesDir}/Grenade/ExplosionGrenade.png");
        }

        private static Texture2D LoadShotgunExplosionSheet()
        {
            return LoadExplosionSheet($"{SpritesDir}/Shoot/ShotgunExplosion.png");
        }

        private static Texture2D LoadExplosionSheet(string assetPath)
        {
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (sheet == null)
            {
                Debug.LogWarning($"[MetalRebels] Explosion sprite sheet was not found at {assetPath}");
            }

            return sheet;
        }

        private static void AssignSpriteArray(SerializedObject so, string propertyName, List<Sprite> sprites)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null) return;

            prop.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        private static void CreatePlatform(Transform parent, string name, Vector3 pos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = size;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.size = size;
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static void CreateHazardSpikes(Transform parent, Vector3 pos)
        {
            GameObject spikes = new GameObject("Hazard_Spikes");
            spikes.transform.SetParent(parent);
            spikes.transform.position = pos;

            BoxCollider2D col = spikes.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.5f);
            col.isTrigger = true;

            SpriteRenderer sr = spikes.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.8f, 0.1f, 0.1f, 0.9f);
            spikes.transform.localScale = new Vector3(2f, 0.5f, 1f);

            HazardTrigger hazard = spikes.AddComponent<HazardTrigger>();
            hazard.damage = 20;
        }

        private static void CreateCheckpoint(Transform parent, Vector3 pos)
        {
            GameObject cp = new GameObject("Checkpoint_Station");
            cp.transform.SetParent(parent);
            cp.transform.position = pos;

            BoxCollider2D col = cp.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.5f);
            col.isTrigger = true;

            SpriteRenderer sr = cp.AddComponent<SpriteRenderer>();
            sr.color = Color.white;
            cp.transform.localScale = new Vector3(0.8f, 1.5f, 1f);

            cp.AddComponent<Checkpoint>();
        }

        private static void CreateDummy(Transform parent, string name, Vector3 pos, int hp)
        {
            GameObject dummy = new GameObject(name);
            dummy.transform.SetParent(parent);
            dummy.transform.position = pos;

            BoxCollider2D col = dummy.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.6f);

            SpriteRenderer sr = dummy.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.8f, 0.4f, 0.2f);
            dummy.transform.localScale = new Vector3(0.8f, 1.6f, 1f);

            TargetDummy td = dummy.AddComponent<TargetDummy>();
            var so = new SerializedObject(td);
            so.FindProperty("maxHP").intValue = hp;
            so.FindProperty("targetName").stringValue = name;
            so.ApplyModifiedProperties();
        }

        private static void InstantiatePickup(Transform parent, string prefabPath, Vector3 pos)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(parent);
                instance.transform.position = pos;
            }
        }

        private static void CreatePickupPrefabs(WeaponData shotgun, WeaponData rifle, WeaponData rocket)
        {
            CreatePickupPrefab($"{PrefabDir}/Pickup_Health.prefab", PickupType.Health, 30, null, Color.red);
            CreatePickupPrefab($"{PrefabDir}/Pickup_ShotgunAmmo.prefab", PickupType.ShotgunAmmo, 20, shotgun, new Color(0.9f, 0.6f, 0.1f));
            CreatePickupPrefab($"{PrefabDir}/Pickup_RifleAmmo.prefab", PickupType.AssaultRifleAmmo, 30, rifle, new Color(0.3f, 0.8f, 0.3f));
            CreatePickupPrefab($"{PrefabDir}/Pickup_Rocket.prefab", PickupType.RocketWeapon, 2, rocket, new Color(0.9f, 0.2f, 0.2f));
            CreatePickupPrefab($"{PrefabDir}/Pickup_Grenade.prefab", PickupType.Grenade, 2, null, new Color(0.2f, 0.5f, 0.2f));
        }

        private static void CreatePickupPrefab(string path, PickupType type, int amount, WeaponData wpnData, Color color)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                ItemPickup existingPickup = existing.GetComponent<ItemPickup>();
                if (existingPickup != null)
                {
                    SerializedObject pickupSO = new SerializedObject(existingPickup);
                    SerializedProperty typeProperty = pickupSO.FindProperty("pickupType");
                    SerializedProperty amountProperty = pickupSO.FindProperty("amount");
                    if (typeProperty != null) typeProperty.enumValueIndex = (int)type;
                    if (amountProperty != null) amountProperty.intValue = amount;
                    SetObjectReferenceIfChanged(pickupSO, "weaponDataReference", wpnData);
                    if (pickupSO.ApplyModifiedProperties())
                    {
                        PrefabUtility.SavePrefabAsset(existing);
                    }
                }

                return;
            }

            GameObject obj = new GameObject(Path.GetFileNameWithoutExtension(path));
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.6f);
            col.isTrigger = true;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = color;

            ItemPickup pickup = obj.AddComponent<ItemPickup>();
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("pickupType").enumValueIndex = (int)type;
            so.FindProperty("amount").intValue = amount;
            if (wpnData != null)
            {
                so.FindProperty("weaponDataReference").objectReferenceValue = wpnData;
            }
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }
    }

    public class HazardTrigger : MonoBehaviour
    {
        public int damage = 20;
        private float lastDamageTime = 0f;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (Time.time - lastDamageTime < 1.0f) return;

            PlayerController player = collision.GetComponent<PlayerController>() ?? collision.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsInvincible)
            {
                lastDamageTime = Time.time;
                player.TakeDamage(damage);
            }
        }
    }
}
#endif
