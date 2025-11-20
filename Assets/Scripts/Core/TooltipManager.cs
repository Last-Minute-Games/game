using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    private static TooltipManager instance;

    [Header("Prefab")]
    public FloatingTooltip tooltipPrefab;

    [Header("Parent Canvas (Screen Space - Overlay ONLY)")]
    public Canvas tooltipCanvas;

    private void Awake()
    {
        instance = this;
    }

    public static void SpawnTooltip(Vector3 worldPos, string message, Color color, TooltipDirection direction)
    {
        if (instance == null || instance.tooltipPrefab == null || instance.tooltipCanvas == null)
            return;

        Vector3 screenPos;

        // If this comes from a UI element (player healthbar), it's already in screen space
        if (direction == TooltipDirection.Down) // we know DOWN = player
        {
            screenPos = worldPos;
        }
        else
        {
            // Convert world → screen for enemies
            screenPos = Camera.main.WorldToScreenPoint(worldPos);
        }

        var tooltip = Instantiate(instance.tooltipPrefab, instance.tooltipCanvas.transform);

        tooltip.transform.position = screenPos;

        Vector3 moveDir = (direction == TooltipDirection.Up)
            ? Vector3.up
            : Vector3.down;

        tooltip.Play(message, color, moveDir);
    }
}

public enum TooltipDirection
{
    Up,
    Down
}
