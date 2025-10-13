using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;      // switched to GameObject for reliability

    [Header("Shield Prefab")]
    public GameObject shieldPrefab;

    [Header("Enemy Spawn Settings")]
    public int enemyCount = 3;
    public float enemySpacing = 2.5f;
    public Vector3 enemyStartPos = new Vector3(6f, 0f, 0f);
    public Sprite[] enemySprites;       // assign in Inspector

    [Header("Player Position")]
    public Vector3 playerPos = new Vector3(-6f, 0f, 0f);

    private GameObject player;

    void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("BattlefieldLayout: Missing playerPrefab!");
            return;
        }

        Vector3 adjustedPos = playerPos + new Vector3(0f, -0.0f, 0f); // ↓ move lower
        player = Instantiate(playerPrefab, adjustedPos, Quaternion.identity);
        player.name = "Player";

        CharacterBase playerChar = player.GetComponent<CharacterBase>();
        if (playerChar != null)
        {
            playerChar.characterName = "Player";

            if (shieldPrefab != null)
                playerChar.shieldPrefab = shieldPrefab;
        }
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("BattlefieldLayout: Missing enemyPrefab!");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = enemyStartPos + Vector3.left * enemySpacing * i;

            // instantiate as GameObject first
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy newEnemy = enemyObj.GetComponent<Enemy>();

            if (newEnemy == null)
            {
                Debug.LogError($"Spawned prefab {enemyPrefab.name} has no Enemy component!");
                continue;
            }

            newEnemy.characterName = $"Enemy {i + 1}";
            newEnemy.possibleSprites = enemySprites;

            Debug.Log($"Spawned {newEnemy.characterName} at {spawnPos}");
        }
    }
}
