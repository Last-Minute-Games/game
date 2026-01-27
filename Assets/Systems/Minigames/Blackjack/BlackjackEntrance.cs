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
            Debug.LogError($"[BlackjackEntrance] {name}: BlackjackPopupController not assigned! This entrance will not work. Please assign the popup in the Inspector.");
        }
    }

    protected override void Update()
    {
        base.Update();
        // Note: Input handling now done by InteractionDetector for proper priority
    }

    public override void Interact()
    {
        // Check for popup BEFORE acquiring lock
        if (popup == null)
        {
            Debug.LogError($"[BlackjackEntrance] {name}: Cannot interact - popup is not assigned!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError($"[BlackjackEntrance] {name}: Cannot interact - player reference is null!");
            return;
        }

        // Try to acquire the interaction lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            Debug.Log($"[BlackjackEntrance] {name}: Cannot interact - lock is already held");
            return; // Another interaction is in progress
        }
        
        Debug.Log($"[BlackjackEntrance] {name}: Opening Blackjack minigame!");
        
        // Minigame pause will be set in popup.Show()
        GameFlags.SetFlag("InBlackjackMinigame");

        popup.Show();
        
        // Note: Lock will be released when popup closes in BlackjackPopupController.Hide()
    }

    public override bool CanInteract()
    {
        // Can only interact if we have a valid popup and base conditions are met
        bool baseCanInteract = base.CanInteract();
        bool hasPopup = popup != null;
        
        if (!hasPopup && baseCanInteract)
        {
            Debug.LogWarning($"[BlackjackEntrance] {name}: In range but popup is not assigned!");
        }
        
        return baseCanInteract && hasPopup;
    }
}
