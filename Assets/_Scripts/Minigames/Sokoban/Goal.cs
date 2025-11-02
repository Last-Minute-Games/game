using UnityEngine;

public class Goal : MonoBehaviour
{
    // A flag to quickly check if a crate is on this goal tile
    public bool isOccupied = false;

    // Cache the goal count manager for quick access (will be created in Step 3)
    private WinConditionManager winManager;

    void Start()
    {
        // Find the WinConditionManager in the scene
        winManager = FindObjectOfType<WinConditionManager>();

        if (winManager == null)
        {
            Debug.LogError("WinConditionManager not found in scene. Win condition will not work.");
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

                // Optional: Change the visual of the box or goal when matched
                // other.GetComponent<SpriteRenderer>().color = Color.green;
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

                // Optional: Reset the visual
                // other.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
}