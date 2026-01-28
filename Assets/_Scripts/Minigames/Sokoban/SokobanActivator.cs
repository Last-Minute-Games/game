using UnityEngine;

/// <summary>
/// Activates the Sokoban puzzle when the player is near and presses the 'E' key.
/// Replaces the old "walk-in" trigger activation.
/// </summary>
public class SokobanActivator : MinigameActivator
{
    private MinigameController minigameController;

    protected override void Start()
    {
        base.Start();

        // Find the controller instance
        minigameController = MinigameController.Instance;
        if (minigameController == null)
        {
            Debug.LogError("MinigameController not found. Cannot start puzzle.");
            enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        // Note: Input handling now done by InteractionDetector for proper priority
    }

    public override void Interact()
    {
        if (player == null || minigameController == null) return;

        // Try to acquire the interaction lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            return; // Another interaction is in progress
        }

        GameFlags.SetFlag("InMinigame");

        FindObjectOfType<ClockTimer>()?.PauseTimer(true);   // Pause

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
        
        // Note: Lock will be released when minigame ends in MinigameController
    }

    public override bool CanInteract()
    {
        // Can only interact if we have a valid minigame controller and base conditions are met
        return base.CanInteract() && minigameController != null;
    }
}
