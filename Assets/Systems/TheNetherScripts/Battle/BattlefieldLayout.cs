using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject cardPrefab;

    [Header("Positions & Scale")]
    public int handSize = 7;
    public float curveRadius = 3f;
    public float cardY = -4f;
    public float playerX = -6f;
    public float enemyX = 6f;
    public float cardScale = 0.6f;

    private GameObject enemy;

    void Start()
    {
        SpawnPlayer();
        SpawnEnemy();
        SpawnCards();
    }

    void SpawnPlayer()
    {
        Vector3 playerPos = new Vector3(playerX, 0f, 0f);
        Instantiate(playerPrefab, playerPos, Quaternion.identity);
    }

    void SpawnEnemy()
    {
        Vector3 enemyPos = new Vector3(enemyX, 0f, 0f);
        enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
    }

    void SpawnCards()
    {
        Vector3 center = new Vector3(0f, cardY, 0f);
        float totalAngle = 90f;                // wider fan
        float startAngle = -totalAngle / 2f;
        float angleStep = totalAngle / (handSize - 1);

        for (int i = 0; i < handSize; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            float x = center.x + Mathf.Sin(rad) * curveRadius;
            float y = center.y + Mathf.Cos(rad) * curveRadius * 0.35f; // flatter curve
            Vector3 pos = new Vector3(x, y, 0f);

            GameObject card = Instantiate(cardPrefab, pos, Quaternion.identity);
            card.transform.rotation = Quaternion.Euler(0, 0, -angle);
            card.transform.localScale = Vector3.one * cardScale;

            // Assign the enemy target
            CardVisual cardVisual = card.GetComponent<CardVisual>();
            if (cardVisual != null)
                cardVisual.targetEnemy = enemy;
        }
    }
}
