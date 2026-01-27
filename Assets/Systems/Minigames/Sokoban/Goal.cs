using UnityEngine;

public class Goal : MonoBehaviour
{
    // A flag to quickly check if a crate is on this goal tile
    public bool isOccupied = false;

    // NEW: Sprites for visual feedback
    [Header("Goal Sprites")]
    [Tooltip("The sprite to display when the goal is not covered by a box.")]
    public Sprite unoccupiedSprite;
    [Tooltip("The sprite to display when the goal is correctly occupied by a box.")]
    public Sprite occupiedSprite;

    private SpriteRenderer spriteRenderer;
    private WinConditionManager winManager;

    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Goal object is missing a SpriteRenderer component.", this);
            enabled = false;
            return;
        }

        // Find the WinConditionManager in the scene
        winManager = FindObjectOfType<WinConditionManager>();

        if (winManager == null)
        {
            Debug.LogError("WinConditionManager not found in scene. Win condition will not work.");
        }

        // Ensure the goal starts with the correct unoccupied visual
        UpdateVisual(isOccupied);
    }

    /// <summary>
    /// Updates the Goal's sprite based on the occupied state.
    /// </summary>
    /// <param name="occupied">True if a box is on the goal, false otherwise.</param>
    private void UpdateVisual(bool occupied)
    {
        if (spriteRenderer == null) return;

        if (occupied)
        {
            spriteRenderer.sprite = occupiedSprite;
        }
        else
        {
            // Only use the unoccupied sprite if one is assigned, otherwise keep the default
            if (unoccupiedSprite != null)
            {
                spriteRenderer.sprite = unoccupiedSprite;
            }
        }
    }

    // Called when another collider (like a box) enters the goal's trigger volume
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering is a pushable object (a box)
        if (other.CompareTag("ObjToPush"))
        {
            if (!isOccupied)
            {
                isOccupied = true;
                // Inform the manager that a goal has been met
                winManager?.GoalReached();

                // Update the visual to the occupied sprite
                UpdateVisual(true);
            }
        }
    }

    // Called when a collider (like a box) leaves the goal's trigger volume
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ObjToPush"))
        {
            if (isOccupied)
            {
                isOccupied = false;
                // Inform the manager that a goal is now empty
                winManager?.GoalUnreached();

                // Reset the visual to the unoccupied sprite
                UpdateVisual(false);
            }
        }
    }

    /// <summary>
    /// Explicitly resets the visual state to unoccupied. 
    /// Used by MinigameController when resetting the entire puzzle.
    /// </summary>
    public void ResetVisual()
    {
        isOccupied = false;
        UpdateVisual(false);
    }
}
