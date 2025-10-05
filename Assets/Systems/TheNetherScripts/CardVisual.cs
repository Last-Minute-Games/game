using UnityEngine;

public class CardVisual : MonoBehaviour
{
    public CharacterBase player;         // the card user
    public CharacterBase targetEnemy;    // target enemy
    public float doubleClickTime = 0.3f;
    private float lastClickTime = -1f;

    private CardBase cardBase;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color glowColor = Color.yellow;

    void Awake()
    {
        cardBase = GetComponent<CardBase>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickTime)
        {
            // Double-click → use card
            if (cardBase != null && player.energy >= cardBase.energy)
            {
                cardBase.Use(player, targetEnemy);

                // Hide / remove the card after using
                gameObject.SetActive(false); // hide
                // OR Destroy(gameObject); // permanently remove
            }
            else
            {
                Debug.Log("Not enough energy to play this card!");
            }
        }
        else
        {
            // First click → glow
            if (spriteRenderer != null)
                spriteRenderer.color = glowColor;
        }

        lastClickTime = Time.time;
    }

    void Update()
    {
        // Reset glow if double-click timer expired
        if (spriteRenderer != null && Time.time - lastClickTime > doubleClickTime)
            spriteRenderer.color = originalColor;
    }
}
