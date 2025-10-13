using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemies/Enemy Library")]
public class EnemyLibrary : ScriptableObject
{
    [Header("Registered Enemies")]
    public List<EnemyData> AvailableEnemies = new();

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
