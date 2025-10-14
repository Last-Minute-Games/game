// EnemyLibrary.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemies/Enemy Library")]
public class EnemyLibrary : ScriptableObject
{
    [Header("Registered Enemies")]
    public List<EnemyData> AvailableEnemies = new();

    [Header("Encounter Settings")]
    [Range(1, 5)] public int minEnemies = 1;
    [Range(1, 5)] public int maxEnemies = 3;

    public EnemyData GetRandomEnemy()
    {
        if (AvailableEnemies == null || AvailableEnemies.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemyLibrary is empty!");
            return null;
        }
        return AvailableEnemies[Random.Range(0, AvailableEnemies.Count)];
    }
}
