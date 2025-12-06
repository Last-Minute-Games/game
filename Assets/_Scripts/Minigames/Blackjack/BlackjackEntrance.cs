using UnityEngine;

public class BlackjackEntrance : MinigameActivator
{
    [Tooltip("Reference to the BlackjackPopupController in your Canvas.")]
    public BlackjackPopupController popup;

    protected override void Start()
    {
        base.Start();
        
        if (popup == null)
        {
            Debug.LogError("BlackjackPopupController not assigned!");
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
        if (popup == null || player == null) return;

        // Try to acquire the interaction lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            return; // Another interaction is in progress
        }
        
        // Minigame pause will be set in popup.Show()
        GameFlags.SetFlag("InBlackjackMinigame");

        popup.Show();
        
        // Note: Lock will be released when popup closes in BlackjackPopupController
    }

    public override bool CanInteract()
    {
        // Can only interact if we have a valid popup and base conditions are met
        return base.CanInteract() && popup != null;
    }
}
