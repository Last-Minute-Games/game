using System.Collections.Generic;
using UnityEngine;

namespace GameItems
{
    /// <summary>
    /// Defines a single wave/round of enemies in a battle.
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        [Tooltip("Name/description of this wave")]
        public string waveName = "Wave 1";
        
        [Tooltip("List of enemies to spawn in this wave")]
        public List<EnemyConfig> enemies = new();
        
        [Tooltip("Optional: Delay before this wave starts (in seconds)")]
        public float delayBeforeWave = 0f;
        
        [Tooltip("Optional: Message to display when wave starts")]
        public string waveMessage = "";
    }

    /// <summary>
    /// Configuration asset that defines all waves for a battle encounter.
    /// </summary>
    [CreateAssetMenu(menuName = "Battle/Wave Configuration", fileName = "NewWaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Battle Waves")]
        [Tooltip("List of waves in this battle. Each wave triggers when the previous is defeated.")]
        public List<WaveData> waves = new();
        
        [Header("Random Wave Generation (Optional)")]
        [Tooltip("If true, ignores 'waves' list and generates random waves instead")]
        public bool useRandomWaves = false;
        
        [Tooltip("Number of random waves to generate")]
        public int numberOfRandomWaves = 3;
        
        [Tooltip("Pool of enemies to randomly select from")]
        public List<EnemyConfig> enemyPool = new();
        
        [Tooltip("Min enemies per random wave")]
        public int minEnemiesPerWave = 1;
        
        [Tooltip("Max enemies per random wave")]
        public int maxEnemiesPerWave = 3;
        
        [Tooltip("Stat multiplier increase per wave (e.g., 0.2 = +20% per wave)")]
        public float difficultyScaling = 0.2f;

        /// <summary>
        /// Get all waves, either predefined or randomly generated
        /// </summary>
        public List<WaveData> GetWaves()
        {
            if (useRandomWaves)
            {
                return GenerateRandomWaves();
            }
            return waves;
        }

        /// <summary>
        /// Generates random waves based on configuration
        /// </summary>
        private List<WaveData> GenerateRandomWaves()
        {
            List<WaveData> generatedWaves = new();
            
            if (enemyPool == null || enemyPool.Count == 0)
            {
                Debug.LogWarning("WaveConfig: Enemy pool is empty, cannot generate random waves!");
                return generatedWaves;
            }

            for (int i = 0; i < numberOfRandomWaves; i++)
            {
                WaveData wave = new WaveData
                {
                    waveName = $"Wave {i + 1}",
                    waveMessage = $"Wave {i + 1} incoming!",
                    delayBeforeWave = i == 0 ? 0f : 1f
                };

                int enemyCount = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);
                for (int j = 0; j < enemyCount; j++)
                {
                    EnemyConfig randomEnemy = enemyPool[Random.Range(0, enemyPool.Count)];
                    wave.enemies.Add(randomEnemy);
                }

                generatedWaves.Add(wave);
            }

            return generatedWaves;
        }
    }
}

