using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
public class AutoFitGridRow : MonoBehaviour
{
    public float cardAspect = 0.7f;     // width/height (poker-ish)
    public float minWidth = 50f;
    public float maxWidth = 120f;

    RectTransform rt;
    GridLayoutGroup grid;

    void Awake()
    {
        rt = (RectTransform)transform;
        grid = GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
    }

    void LateUpdate()
    {
        int n = Mathf.Max(1, transform.childCount);
        float avail = rt.rect.width - grid.padding.left - grid.padding.right;
        float spacing = grid.spacing.x * (n - 1);
        float w = (avail - spacing) / n;                 // width per card
        w = Mathf.Clamp(w, minWidth, maxWidth);
        float h = w / cardAspect;                        // keep aspect
        grid.cellSize = new Vector2(w, h);
    }
}
