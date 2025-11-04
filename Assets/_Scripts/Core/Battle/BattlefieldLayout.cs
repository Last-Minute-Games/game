using UnityEngine;
using System.Collections.Generic;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs & Libraries")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private EnemyLibrary enemyLibrary;
    [SerializeField] private GameObject playerPrefab;

    private Player playerInstance;
    private readonly List<Enemy> activeEnemies = new();

    [Header("Enemy Positioning")]
    [SerializeField] private float horizontalSpacing = 2.5f;
    [SerializeField] private float rearOffsetY = 0.5f;
    [SerializeField] private float rearScale = 0.8f;

    [Header("Debug / Rounds")]
    [SerializeField] private bool autoStartNextRound = true;

    private int currentRound = 0;

    // Define your rounds here by enemyID
    private readonly List<List<string>> roundEnemyIDs = new()
    {
        new() { "sexy_eyeball" },
        new() { "sexy_eyeball", "sexy_eyeball" },
        new() { "sexy_eyeball", "sexy_eyeball", "sexy_eyeball" },
    };

    private void Start()
    {
        SpawnPlayer();
        StartNextRound();
    }

    private void Update()
    {
        // Simple round progression (auto)
        if (autoStartNextRound && activeEnemies.Count > 0)
        {
            bool allDead = true;
            foreach (Enemy e in activeEnemies)
            {
                if (e != null && !e.IsDead)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                StartNextRound();
            }
        }
    }

    public void StartNextRound()
    {
        if (currentRound >= roundEnemyIDs.Count)
        {
            Debug.Log("✅ All rounds complete!");
            return;
        }

        ClearEnemies();
        SpawnEnemiesByIDs(roundEnemyIDs[currentRound]);
        currentRound++;
    }

    private void SpawnEnemiesByIDs(List<string> enemyIDs)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Enemy Prefab!");
            return;
        }

        if (enemyLibrary == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing EnemyLibrary reference!");
            return;
        }

        activeEnemies.Clear();
        Vector3 center = Vector3.zero;
        int enemyCount = enemyIDs.Count;

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData data = enemyLibrary.GetEnemyByID(enemyIDs[i]);
            if (data == null)
            {
                Debug.LogWarning($"⚠️ Enemy ID '{enemyIDs[i]}' not found in library!");
                continue;
            }

            Vector3 spawnPos = GetSpawnPosition(i, enemyCount, center);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy newEnemy = enemyObj.GetComponent<Enemy>();

            if (newEnemy == null)
            {
                Debug.LogError("❌ Prefab is missing Enemy component!");
                continue;
            }

            newEnemy.InitializeFromData(data);

            // Perspective scaling (rear)
            if (enemyCount == 3 && i == 2)
            {
                enemyObj.transform.localScale *= rearScale;
                enemyObj.transform.position += Vector3.up * rearOffsetY;
            }

            activeEnemies.Add(newEnemy);
        }

        Debug.Log($"🌀 Spawned Round {currentRound + 1} with {activeEnemies.Count} enemies");
    }

    private void ClearEnemies()
    {
        foreach (Enemy e in activeEnemies)
        {
            if (e != null)
                Destroy(e.gameObject);
        }
        activeEnemies.Clear();
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
            return center + Vector3.back * 0.1f;
        }

        return center + Vector3.right * (index - total / 2f) * horizontalSpacing;
    }

    public IReadOnlyList<Enemy> GetEnemies() => activeEnemies;
}
