using UnityEngine;

/// <summary>
/// Activates the Maze puzzle when the player is near and presses the 'E' key.
/// Replaces the old "walk-in" trigger activation.
/// </summary>
public class MazePopupActivator : MinigameActivator
{
    [Tooltip("Reference to the MazePopupController in your scene. If not assigned, will try to find it automatically.")]
    public MazePopupController mazePopupController;

    protected override void Start()
    {
        base.Start();

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

    protected override void Update()
    {
        base.Update();
        // Note: Input handling now done by InteractionDetector for proper priority
    }

    public override void Interact()
    {
        if (player == null || mazePopupController == null) return;

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

    public override bool CanInteract()
    {
        // Can only interact if we have a valid popup controller and base conditions are met
        return base.CanInteract() && mazePopupController != null;
    }
}
