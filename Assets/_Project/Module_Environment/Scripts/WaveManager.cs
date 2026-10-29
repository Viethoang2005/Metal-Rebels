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

        public void StartWaves()
        {
            if (isRunning) return;

            isRunning = true;
            currentWaveIndex = 0;
            activeEnemies.Clear();

            if (waves.Count == 0)
            {
                OnAllWavesCleared?.Invoke();
                return;
            }

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
