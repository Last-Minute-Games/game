using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int offset = 0;   // Optional manual nudge if needed

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // In many tilemaps, "lower on the screen" = bigger Y.
        // Bigger Y → bigger sortingOrder → draws on top.
        sr.sortingOrder = offset + Mathf.RoundToInt(transform.position.y * 100);
    }
}
