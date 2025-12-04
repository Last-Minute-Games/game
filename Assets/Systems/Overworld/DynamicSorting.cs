using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int offset = 0;   // Optional manual nudge if needed
    
    [Tooltip("If true, lower on screen (more negative Y) = draws on top. If false, higher Y values = draws on top.")]
    public bool invertY = true; // Most Unity projects have negative Y at bottom of screen
    
    [Tooltip("Adjust sorting based on sprite bounds instead of transform position. Useful for sprites with center pivot.")]
    public bool useSpriteBounds = false;

    [Header("Player-Relative Sorting")]
    [Tooltip("If true, sorting will be relative to the player's Y position.")]
    public bool usePlayerRelativeSorting = false;
    
    [Tooltip("Y offset to adjust where the 'feet' of this object are for sorting comparison. Negative values move the sorting point down.")]
    public float feetYOffset = 0f;
    
    [Tooltip("Sorting order when this object is behind the player (object Y > player Y).")]
    public int sortingOrderBehindPlayer = -1;
    
    [Tooltip("Sorting order when this object is in front of the player (object Y < player Y).")]
    public int sortingOrderInFrontOfPlayer = 1;

    private SpriteRenderer sr;
    private Transform playerTransform;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void LateUpdate()
    {
        if (sr == null) return;
        
        // Determine the Y position to use for sorting
        float sortingY = transform.position.y;
        
        if (useSpriteBounds && sr.sprite != null)
        {
            // Use the bottom edge of the sprite for sorting
            sortingY = sr.bounds.min.y;
        }
        
        // Player-relative sorting mode
        if (usePlayerRelativeSorting && playerTransform != null)
        {
            float playerY = playerTransform.position.y;
            
            // Apply feet offset to get the actual "feet" position for comparison
            float objectFeetY = sortingY + feetYOffset;
            
            // If object is above player in Y (higher Y value), it should appear behind player
            // If object is below player in Y (lower Y value), it should appear in front of player
            if (objectFeetY > playerY)
            {
                // Object is above player → appears behind (lower sorting order)
                sr.sortingOrder = sortingOrderBehindPlayer + offset;
            }
            else
            {
                // Object is below or at same level as player → appears in front (higher sorting order)
                sr.sortingOrder = sortingOrderInFrontOfPlayer + offset;
            }
        }
        else
        {
            // Original Y-sorting behavior
            // Lower on screen should draw on top
            // If invertY is true: more negative Y → higher sorting order
            // If invertY is false: more positive Y → higher sorting order
            float yValue = invertY ? -sortingY : sortingY;
            sr.sortingOrder = offset + Mathf.RoundToInt(yValue * 100);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Calculate the sorting Y point
        float sortingY = transform.position.y;
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (useSpriteBounds && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            sortingY = spriteRenderer.bounds.min.y;
        }
        
        // Apply feet offset for player-relative sorting
        float feetY = sortingY + feetYOffset;
        
        // Draw a horizontal line at the feet Y position
        Gizmos.color = Color.green;
        Vector3 feetPosition = new Vector3(transform.position.x, feetY, transform.position.z);
        float lineWidth = 0.5f;
        
        // Draw horizontal line
        Gizmos.DrawLine(
            feetPosition + Vector3.left * lineWidth,
            feetPosition + Vector3.right * lineWidth
        );
        
        // Draw a small sphere at the center
        Gizmos.DrawWireSphere(feetPosition, 0.05f);
        
        // Draw a vertical line from transform to feet point to show the offset
        if (Mathf.Abs(feetYOffset) > 0.01f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, feetPosition);
        }
    }
#endif
}
