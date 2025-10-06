using UnityEngine;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject cardPrefab;

    [Header("Positions")]
    public int handSize = 7;
    // public float cardY = -4f;       // vertical row position
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
    // Parent (optional, keeps hierarchy tidy)
    Transform parent = this.transform;

    // Center the row
    float startX = -(handSize - 1) * cardSpacing / 2f;

    // Local function: position on curve for index k
    Vector3 PosAt(int k)
    {
        float x = startX + k * cardSpacing;

        // Your curved layout (inverted V using |...|):
        float mid = (handSize - 1) / 2f;
        float y = -4f + Mathf.Abs(k - mid) * -0.2f;

        return new Vector3(x, y, 0f);
    }

    for (int i = 0; i < handSize; i++)
    {
        Vector3 pos = PosAt(i);
        GameObject card = Instantiate(cardPrefab, pos, Quaternion.identity, parent);
        
        int iPrev = Mathf.Max(0, i - 1);
        int iNext = Mathf.Min(handSize - 1, i + 1);

        Vector3 prev = PosAt(iPrev);
        Vector3 next = PosAt(iNext);

        Vector2 tangent = (next - prev);
        if (tangent.sqrMagnitude < 1e-6f) tangent = Vector2.right;

        Vector2 normal = new Vector2(-tangent.y, tangent.x).normalized;
        float angleDeg = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;;
        card.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        // Sorting order for overlap
        SpriteRenderer cardRenderer = card.GetComponent<SpriteRenderer>();
        if (cardRenderer != null)
        {
            cardRenderer.sortingOrder = i;
        }

        // Assign player/enemy refs
        CardVisual cardVisual = card.GetComponent<CardVisual>();
        if (cardVisual != null)
        {
            cardVisual.player = player.GetComponent<CharacterBase>();
            cardVisual.targetEnemy = enemy.GetComponent<CharacterBase>();
        }
    }
}
}
