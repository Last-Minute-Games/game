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

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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
        
        // Lower on screen should draw on top
        // If invertY is true: more negative Y → higher sorting order
        // If invertY is false: more positive Y → higher sorting order
        float yValue = invertY ? -sortingY : sortingY;
        sr.sortingOrder = offset + Mathf.RoundToInt(yValue * 100);
    }
}
