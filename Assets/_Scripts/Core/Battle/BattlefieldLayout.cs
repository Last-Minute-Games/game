using UnityEngine;
using System.Collections.Generic;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs & Libraries")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab; // generic enemy prefab
    [SerializeField] private EnemyLibrary enemyLibrary;

    [Header("Positions")]
    [SerializeField] private Vector3 playerPos = new(-6f, 0f, 0f);
    [SerializeField] private Vector3 enemyStartPos = new(6f, 0f, 0f);
    [SerializeField] private float enemySpacing = 2.5f;

    [Header("Settings")]
    [SerializeField, Range(1, 5)] private int enemyCount = 3;
    [SerializeField] private bool randomizeEnemies = true;

    private GameObject player;
    private readonly List<Enemy> activeEnemies = new();

    private void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Player Prefab!");
            return;
        }

        player = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        player.name = "Player";

        CharacterBase playerChar = player.GetComponent<CharacterBase>();
        if (playerChar != null)
        {
            playerChar.characterName = "Player";
            playerChar.currentHealth = playerChar.maxHealth;
        }

        Debug.Log("✅ Player spawned.");
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

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = enemyStartPos + Vector3.left * enemySpacing * i;

            // Pick enemy data from library
            EnemyData selectedData = randomizeEnemies
                ? enemyLibrary.GetRandomEnemy()
                : enemyLibrary.AvailableEnemies[i % enemyLibrary.AvailableEnemies.Count];

            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy newEnemy = enemyObj.GetComponent<Enemy>();
            EnemyAnimator2D anim = enemyObj.GetComponent<EnemyAnimator2D>();

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
            newEnemy.strength = selectedData.baseDamage;

            // Assign animation set
            if (anim != null && selectedData.animationSet != null)
                anim.SetAnimSet(selectedData.animationSet);

            Debug.Log($"🧠 Spawned enemy: {selectedData.enemyName} ({selectedData.maxHealth} HP)");
            activeEnemies.Add(newEnemy);
        }
    }

    public IReadOnlyList<Enemy> GetEnemies() => activeEnemies;
}
