using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetalRebels.Module_Environment
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public Transform spawnPoint;
        public float delay = 0.5f; // Thời gian chờ trước khi sinh con này
    }

    [System.Serializable]
    public class EnemyWave
    {
        public string waveName = "Wave 1";
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
        public float delayBeforeWave = 1f; // Chờ trước khi wave bắt đầu
    }

    /// <summary>
    /// Quản lý các đợt quái (waves) trong khu vực Event Lock.
    /// Sinh quái theo đợt, đếm số lượng còn sống và báo hoàn tất khi toàn bộ quái bị hạ gục.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Danh sách các đợt quái")]
        [SerializeField] private List<EnemyWave> waves = new List<EnemyWave>();

        public event Action OnAllWavesCleared;

        private int currentWaveIndex = 0;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool isSpawning = false;
        private bool isRunning = false;
        private float? arenaCenterX;

        public void SetArenaCenter(float centerX)
        {
            arenaCenterX = centerX;
        }

        public bool HasValidWaves()
        {
            if (waves == null || waves.Count == 0) return false;
            foreach (var w in waves)
            {
                if (w.enemies == null) continue;
                foreach (var e in w.enemies)
                {
                    if (e.enemyPrefab != null && e.spawnPoint != null) return true;
                }
            }
            return false;
        }

        public void StartWaves()
        {
            if (isRunning) return;

            if (!HasValidWaves())
            {
                OnAllWavesCleared?.Invoke();
                return;
            }

            isRunning = true;
            currentWaveIndex = 0;
            activeEnemies.Clear();

            StartCoroutine(RunWaveRoutine());
        }

        private IEnumerator RunWaveRoutine()
        {
            while (currentWaveIndex < waves.Count)
            {
                EnemyWave wave = waves[currentWaveIndex];
                yield return new WaitForSeconds(wave.delayBeforeWave);

                isSpawning = true;
                foreach (var info in wave.enemies)
                {
                    if (info.delay > 0f)
                    {
                        yield return new WaitForSeconds(info.delay);
                    }

                    if (info.enemyPrefab != null && info.spawnPoint != null)
                    {
                        GameObject spawned = Instantiate(info.enemyPrefab, info.spawnPoint.position, info.spawnPoint.rotation);
                        EnsureEnemyEntersScreen(spawned);
                        activeEnemies.Add(spawned);
                    }
                }
                isSpawning = false;

                // Chờ cho đến khi tất cả quái trong đợt hiện tại bị tiêu diệt
                while (HasAliveEnemies())
                {
                    yield return new WaitForSeconds(0.5f);
                }

                currentWaveIndex++;
            }

            isRunning = false;
            OnAllWavesCleared?.Invoke();
        }

        private void EnsureEnemyEntersScreen(GameObject spawned)
        {
            if (spawned == null) return;

            Camera cam = Camera.main;
            float centerX = arenaCenterX ?? (cam != null ? cam.transform.position.x : transform.position.x);
            float halfWidth = (cam != null && cam.orthographic) ? cam.orthographicSize * cam.aspect : 5.33f;
            float innerHalfWidth = Mathf.Max(1.5f, halfWidth - 1.6f);
            float leftTargetX = centerX - innerHalfWidth;
            float rightTargetX = centerX + innerHalfWidth;
            float spawnX = spawned.transform.position.x;

            if (spawnX >= leftTargetX && spawnX <= rightTargetX) return;

            float enterTargetX = spawnX < centerX ? leftTargetX : rightTargetX;

            TankAI tank = spawned.GetComponent<TankAI>();
            if (tank != null)
            {
                tank.DriveToPosition(enterTargetX);
            }

            BasicEnemyAI basicEnemy = spawned.GetComponent<BasicEnemyAI>();
            if (basicEnemy != null)
            {
                basicEnemy.DriveToPosition(enterTargetX);
            }
        }

        private bool HasAliveEnemies()
        {
            // Dọn dẹp danh sách các quái đã bị Destroy
            activeEnemies.RemoveAll(e => e == null);
            return activeEnemies.Count > 0 || isSpawning;
        }

        /// <summary>
        /// Buộc dừng các wave nếu cần (ví dụ người chơi chết hoặc reset màn).
        /// </summary>
        public void StopWaves()
        {
            StopAllCoroutines();
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            activeEnemies.Clear();
            isRunning = false;
            isSpawning = false;
        }
    }
}

