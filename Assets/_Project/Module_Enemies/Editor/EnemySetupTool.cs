#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MetalRebels.Enemies.EditorTools
{
    public static class EnemySetupTool
    {
        private const string PrefabDir = "Assets/_Project/Module_Enemies/Prefabs";

        [MenuItem("MetalRebels/Module Enemies/Setup All Enemy Prefabs", priority = 20)]
        public static void SetupAll()
        {
            EnsureDirectories();

            GameObject bulletPrefab = CreateEnemyBulletPrefab();
            GameObject missilePrefab = CreateMissilePrefab();
            GameObject laserPrefab = CreateLaserPrefab();
            GameObject soldierPrefab = CreateSoldierPrefab(bulletPrefab);
            GameObject vehiclePrefab = CreateVehiclePrefab();
            GameObject bossPrefab = CreateBossViperPrefab(bulletPrefab, missilePrefab, laserPrefab, soldierPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[MetalRebels]</color> Đã tạo và cấu hình toàn bộ Prefabs cho Module_Enemies thành công!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Module_Enemies", "Prefabs");
            }
        }

        private static GameObject CreateEnemyBulletPrefab()
        {
            string path = $"{PrefabDir}/EnemyBullet.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("EnemyBullet");
            obj.tag = "Untagged";

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;

            var bullet = obj.AddComponent<EnemyBullet>();
            bullet.speed = 8f;
            bullet.damage = 20;
            bullet.lifeTime = 4f;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = Color.red;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateMissilePrefab()
        {
            string path = $"{PrefabDir}/EnemyMissile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("EnemyMissile");
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.2f);
            col.isTrigger = true;

            var bullet = obj.AddComponent<EnemyBullet>();
            bullet.speed = 6f;
            bullet.damage = 35;
            bullet.lifeTime = 5f;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.4f, 0f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateLaserPrefab()
        {
            string path = $"{PrefabDir}/EnemyLaser.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("EnemyLaser");
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 0.2f);
            col.isTrigger = true;

            var bullet = obj.AddComponent<EnemyBullet>();
            bullet.speed = 12f;
            bullet.damage = 45;
            bullet.lifeTime = 3f;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = Color.magenta;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateSoldierPrefab(GameObject bulletPrefab)
        {
            string path = $"{PrefabDir}/Enemy_Trooper.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("Enemy_Trooper");
            obj.tag = "Enemy";

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.4f);
            col.offset = new Vector2(0f, 0.7f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.3f, 0.3f);

            var health = obj.AddComponent<EnemyHealth>();
            health.maxHP = 30;

            var ai = obj.AddComponent<BasicEnemyAI>();
            ai.enemyType = BasicEnemyAI.EnemyType.RangedSoldier;
            ai.moveSpeed = 2f;
            ai.detectRange = 7f;
            ai.attackRange = 5f;
            ai.fireRate = 1.5f;

            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(obj.transform);
            fp.transform.localPosition = new Vector2(-0.5f, 0.7f);

            ai.firePoint = fp.transform;
            ai.bulletPrefab = bulletPrefab;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateVehiclePrefab()
        {
            string path = $"{PrefabDir}/Enemy_Vehicle.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("Enemy_Vehicle");
            obj.tag = "Enemy";

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.8f, 1.2f);
            col.offset = new Vector2(0f, 0.6f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.4f, 0.5f, 0.3f);

            var health = obj.AddComponent<EnemyHealth>();
            health.maxHP = 70;

            var ai = obj.AddComponent<BasicEnemyAI>();
            ai.enemyType = BasicEnemyAI.EnemyType.MeleeOrVehicle;
            ai.moveSpeed = 3f;
            ai.detectRange = 8f;
            ai.contactDamage = 30;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateBossViperPrefab(GameObject bulletPrefab, GameObject missilePrefab, GameObject laserPrefab, GameObject minionPrefab)
        {
            string path = $"{PrefabDir}/Boss_Viper01.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = new GameObject("Boss_Viper01");
            obj.tag = "Boss";

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 2f);
            col.offset = new Vector2(0f, 1f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.3f, 0.3f, 0.4f);

            var health = obj.AddComponent<EnemyHealth>();
            health.maxHP = 600;

            var boss = obj.AddComponent<BossController>();
            boss.attackInterval = 2.5f;

            GameObject mgPoint = new GameObject("MachineGunPoint");
            mgPoint.transform.SetParent(obj.transform);
            mgPoint.transform.localPosition = new Vector2(-1.5f, 0.8f);

            GameObject cannonPoint = new GameObject("CannonPoint");
            cannonPoint.transform.SetParent(obj.transform);
            cannonPoint.transform.localPosition = new Vector2(-1.5f, 1.5f);

            GameObject minionPoint = new GameObject("SpawnMinionPoint");
            minionPoint.transform.SetParent(obj.transform);
            minionPoint.transform.localPosition = new Vector2(-2.5f, 0.5f);

            boss.machineGunPoint = mgPoint.transform;
            boss.cannonPoint = cannonPoint.transform;
            boss.spawnMinionPoint = minionPoint.transform;
            boss.normalBulletPrefab = bulletPrefab;
            boss.heavyMissilePrefab = missilePrefab;
            boss.laserPrefab = laserPrefab;
            boss.minionPrefab = minionPrefab;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }
    }
}
#endif
