using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    [Header("Wave Identity")]
    public string waveName = "Wave 1";
    public string waveMessage = "";
    public float delayBeforeWave = 0f;

    [Header("Guaranteed Enemies (Always Spawned First)")]
    public List<EnemyConfig> guaranteedEnemies = new();

    [Header("Random Enemy Rules")]
    public List<EnemyConfig> randomEnemyPool = new();
    public int minEnemiesPerWave = 1;
    public int maxEnemiesPerWave = 3;

    [Header("Scaling")]
    [Tooltip("Stat multiplier applied on top of previous waves (e.g., 0.2 = +20%)")]
    public float statMultiplierIncrease = 0f;

    /// <summary>
    /// Generate the list of EnemyConfigs that should appear in this wave.
    /// Portable & self-contained.
    /// </summary>
    public List<EnemyConfig> GenerateEnemiesForWave()
    {
        List<EnemyConfig> output = new();

        if ((randomEnemyPool == null || randomEnemyPool.Count == 0) && (guaranteedEnemies == null || guaranteedEnemies.Count == 0))
        {
            Debug.LogWarning("WaveConfig: Enemy, and guaranteed Enemy pool are empty, cannot generate random waves!");
            return output;
        }

        //1️⃣ Add guaranteed enemies
        foreach (var e in guaranteedEnemies)
            if (e != null)
                output.Add(e);

        //2️⃣ Random enemies
        if (randomEnemyPool.Count > 0)
        {
            int remaining = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

            for (int i = 0; i < remaining; i++)
            {
                var r = randomEnemyPool[Random.Range(0, randomEnemyPool.Count)];
                output.Add(r);
            }
        }

        return output;
    }
}
