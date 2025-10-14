// BattlefieldLayout.cs
using UnityEngine;
using System.Collections.Generic;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs & Libraries")]
    [SerializeField] private GameObject enemyPrefab; 
    [SerializeField] private EnemyLibrary enemyLibrary;
    [SerializeField] private GameObject playerPrefab;
    private Player playerInstance;

    [Header("Enemy Positioning")]
    [SerializeField] private float horizontalSpacing = 2.5f;
    [SerializeField] private float rearOffsetY = 0.5f;
    [SerializeField] private float rearScale = 0.8f;


    private readonly List<Enemy> activeEnemies = new();

    private void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Enemy Prefab!");
            return;
        }

        if (enemyLibrary == null || enemyLibrary.AvailableEnemies.Count == 0)
        {
            Debug.LogError("❌ BattlefieldLayout: No EnemyLibrary or it’s empty!");
            return;
        }

        int enemyCount = Random.Range(enemyLibrary.minEnemies, enemyLibrary.maxEnemies + 1);
        Vector3 center = Vector3.zero;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i, enemyCount, center);
            EnemyData selectedData = enemyLibrary.GetRandomEnemy();
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            Enemy newEnemy = enemyObj.GetComponent<Enemy>();
            if (newEnemy == null)
            {
                Debug.LogError($"❌ Prefab {enemyPrefab.name} is missing Enemy component!");
                continue;
            }

            // Assign data
            newEnemy.data = selectedData;
            newEnemy.characterName = selectedData.enemyName;
            newEnemy.maxHealth = selectedData.maxHealth;
            newEnemy.currentHealth = selectedData.maxHealth;

            // Apply perspective scaling if behind others
            if (enemyCount == 3 && i == 2)
            {
                enemyObj.transform.localScale *= rearScale;
                enemyObj.transform.position += Vector3.up * rearOffsetY;
            }

            activeEnemies.Add(newEnemy);
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Player Prefab!");
            return;
        }

        GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerObj.name = "Player";

        playerInstance = playerObj.GetComponent<Player>();
        if (playerInstance == null)
        {
            Debug.LogError("❌ Player prefab missing Player component!");
            return;
        }

        playerInstance.characterName = "Player";
        playerInstance.currentHealth = playerInstance.maxHealth;

        // Register player with BattleSystem
        var battleSystem = FindFirstObjectByType<BattleSystem>();
        if (battleSystem != null)
            battleSystem.RegisterPlayer(playerInstance);
    }

    private Vector3 GetSpawnPosition(int index, int total, Vector3 center)
    {
        if (total == 1)
            return center;

        if (total == 2)
        {
            return index == 0
                ? center + Vector3.left * horizontalSpacing / 2f
                : center + Vector3.right * horizontalSpacing / 2f;
        }

        if (total == 3)
        {
            if (index == 0) return center + Vector3.left * horizontalSpacing;
            if (index == 1) return center + Vector3.right * horizontalSpacing;
            return center + Vector3.back * 0.1f; // “behind” (slightly deeper in Z)
        }

        // Default fallback
        return center + Vector3.right * (index - total / 2f) * horizontalSpacing;
    }

    public IReadOnlyList<Enemy> GetEnemies() => activeEnemies;
}
