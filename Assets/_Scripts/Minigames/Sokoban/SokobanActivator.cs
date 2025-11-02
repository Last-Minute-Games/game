using UnityEngine;

/// <summary>
/// Activates the Sokoban puzzle when the player is near and presses the 'E' key.
/// Replaces the old "walk-in" trigger activation.
/// </summary>
public class SokobanActivator : MonoBehaviour
{
    [Tooltip("The range within which the player can press 'E' to interact.")]
    public float interactionRange = 1.5f;

    private GameObject player;
    private BoxCollider2D triggerCollider;
    private MinigameController minigameController;

    void Start()
    {
        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player");

        // Ensure the collider exists and is set as a trigger
        triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Find the controller instance
        minigameController = MinigameController.Instance;
        if (minigameController == null)
        {
            Debug.LogError("MinigameController not found. Cannot start puzzle.");
            enabled = false;
        }
    }

    void Update()
    {
        if (player == null || minigameController == null) return;

        // 1. Check if the player is within the interaction range
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool isPlayerNear = distance < interactionRange;

        // 2. If the player is near AND presses the interaction key ('E')
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Optional: You could add a check here to ensure the Overworld movement script 
            // is currently enabled before allowing the interaction.

            OnInteract();
        }

        // Optional: Add code here to display an "Press E to Play" prompt when isPlayerNear is true.
    }

    private void OnInteract()
    {
        // --- FIX: Dynamic Return Position Capture ---
        // Get the player's current position, round it to the nearest whole number for the grid, 
        // and send it to the MinigameController before starting.
        Vector3 playerCurrentPosition = player.transform.position;
        Vector3 returnPosition = new Vector3(
            Mathf.Round(playerCurrentPosition.x),
            Mathf.Round(playerCurrentPosition.y),
            playerCurrentPosition.z
        );

        // Set the return point dynamically
        minigameController.overworldExitPosition = returnPosition;

        // This is the single function call that starts the minigame
        minigameController.StartSokoban();
    }
}
