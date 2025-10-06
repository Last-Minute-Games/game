using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject cardPrefab;

    [Header("Positions")]
    public int handSize = 7;
    public float cardY = -4f;       // vertical row position
    public float cardSpacing = 1.2f; // horizontal spacing between cards
    public float playerX = -6f;
    public float enemyX = 6f;

    private GameObject enemy;
    private GameObject player;

    void Start()
    {
        SpawnPlayer();
        SpawnEnemy();
        SpawnCards();
    }

    void SpawnPlayer()
    {
        Vector3 playerPos = new Vector3(playerX, 0f, 0f);
        player = Instantiate(playerPrefab, playerPos, Quaternion.identity);
    }

    void SpawnEnemy()
    {
        Vector3 enemyPos = new Vector3(enemyX, 0f, 0f);
        enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
    }

    void SpawnCards()
    {
        // Center the row
        float startX = -(handSize - 1) * cardSpacing / 2f;

        for (int i = 0; i < handSize; i++)
        {
            Vector3 pos = new Vector3(startX + i * cardSpacing, cardY, 0f);
            GameObject card = Instantiate(cardPrefab, pos, Quaternion.identity);

            // Assign player and enemy references
            CardVisual cardVisual = card.GetComponent<CardVisual>();
            if (cardVisual != null)
            {
                cardVisual.player = player.GetComponent<CharacterBase>();
                cardVisual.targetEnemy = enemy.GetComponent<CharacterBase>();
            }
        }
    }
}
