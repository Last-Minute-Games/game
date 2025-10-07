using UnityEngine;

public class CardVisual : MonoBehaviour
{
    public CharacterBase player;         // The card user (can be assigned later)
    public CharacterBase targetEnemy;    // Target enemy
    public float doubleClickTime = 0.3f; // Time window for detecting double-click
    private float lastClickTime = -1f;

    private CardBase cardBase;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color glowColor = Color.yellow;

    void Awake()
    {
        cardBase = GetComponent<CardBase>();
        Debug.Log($"Card '{name}' energy cost: {cardBase.energy}");
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // Detect double-click within allowed time
        if (timeSinceLastClick <= doubleClickTime)
        {
            if (cardBase != null && EnergySystem.Instance.UseEnergy(cardBase.energy))
            {
                // Use card logic (apply effects, damage, etc.)
                cardBase.Use(player, targetEnemy);

                // Hide or destroy card after using
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Not enough energy to play this card!");
            }
        }
        else
        {
            // Single-click → highlight the card
            if (spriteRenderer != null)
                spriteRenderer.color = glowColor;
        }

        lastClickTime = Time.time;
    }

    void Update()
    {
        // Reset highlight if the click timer expires
        if (spriteRenderer != null && Time.time - lastClickTime > doubleClickTime)
            spriteRenderer.color = originalColor;
    }
}
