using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int offset = 0;   // Optional manual nudge if needed
    
    [Tooltip("If true, lower on screen (more negative Y) = draws on top. If false, higher Y values = draws on top.")]
    public bool invertY = true; // Most Unity projects have negative Y at bottom of screen

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr == null) return;
        
        // Lower on screen should draw on top
        // If invertY is true: more negative Y → higher sorting order
        // If invertY is false: more positive Y → higher sorting order
        float yValue = invertY ? -transform.position.y : transform.position.y;
        sr.sortingOrder = offset + Mathf.RoundToInt(yValue * 100);
    }
}
