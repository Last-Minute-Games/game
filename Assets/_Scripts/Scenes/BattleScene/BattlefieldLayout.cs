using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public Enemy enemyPrefab;            // use Enemy (not GameObject)
    public GameObject cardPrefab;

    [Header("Enemy Spawn Settings")]
    public int enemyCount = 3;
    public float enemySpacing = 2.5f;
    public Vector3 enemyStartPos = new Vector3(6f, 0f, 0f);
    public Sprite[] enemySprites;        // assign sprite pool in Inspector

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

        player = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        player.name = "Player";

        CharacterBase playerChar = player.GetComponent<CharacterBase>();
        if (playerChar != null)
            playerChar.characterName = "Player";
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
            Enemy newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            newEnemy.characterName = $"Enemy {i + 1}";
            newEnemy.possibleSprites = enemySprites;  // optional: randomize sprite inside Enemy.Awake()

            Debug.Log($"Spawned {newEnemy.characterName} at {spawnPos}");
        }
    }
}
