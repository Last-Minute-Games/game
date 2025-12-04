using UnityEngine;

/// <summary>
/// Activates the Maze puzzle when the player is near and presses the 'E' key.
/// Replaces the old "walk-in" trigger activation.
/// </summary>
public class MazePopupActivator : MonoBehaviour
{
    [Tooltip("The range within which the player can press 'E' to interact.")]
    public float interactionRange = 1.5f;

    [Tooltip("Reference to the MazePopupController in your scene. If not assigned, will try to find it automatically.")]
    public MazePopupController mazePopupController;

    private GameObject player;
    private BoxCollider2D triggerCollider;

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

        // Find the controller instance if not assigned in inspector
        if (mazePopupController == null)
        {
            mazePopupController = FindObjectOfType<MazePopupController>();
            if (mazePopupController == null)
            {
                Debug.LogError("MazePopupController not found. Please assign it in the Inspector or ensure it exists in the scene.");
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (player == null || mazePopupController == null) return;

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
        // Try to acquire the interaction lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            return; // Another interaction is in progress
        }
        
        // Set flag
        GameFlags.SetFlag("InMinigame");

        // Pause timer
        FindObjectOfType<ClockTimer>()?.PauseTimer(true);

        Vector3 playerCurrentPosition = player.transform.position;
        Vector3 returnPosition = new Vector3(
            Mathf.Round(playerCurrentPosition.x),
            Mathf.Round(playerCurrentPosition.y),
            playerCurrentPosition.z
        );

        // Set the return point dynamically (if MazePopupController supports it)
        // Note: MazePopupController uses InitialPositionn component, so this might not be needed
        // but we'll keep it for consistency with Sokoban

        // This is the single function call that starts the minigame with transitions
        mazePopupController.StartMaze();
        
        // Note: Lock will be released when maze ends in MazePopupController
    }
}
