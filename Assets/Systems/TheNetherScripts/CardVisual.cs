using UnityEngine;

public class CardVisual : MonoBehaviour
{
    public GameObject targetEnemy;          // assign in BattlefieldLayout
    public int damage = 25;                 // card damage
    public float doubleClickTime = 0.3f;    // max time between clicks for double-click
    public Color glowColor = Color.yellow;  // glow/highlight color

    private float lastClickTime = -1f;      // track last click
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnMouseDown()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickTime)
        {
            // Double-click detected → attack enemy
            if (targetEnemy != null)
            {
                Enemy enemyScript = targetEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                    enemyScript.TakeDamage(damage);

                Destroy(gameObject); // remove card after attack
            }
        }
        else
        {
            // First click → glow highlight
            if (spriteRenderer != null)
                spriteRenderer.color = glowColor;
        }

        lastClickTime = Time.time;
    }

    void Update()
    {
        // Optional: reset glow after a short time
        if (spriteRenderer != null && Time.time - lastClickTime > doubleClickTime)
            spriteRenderer.color = originalColor;
    }
}
