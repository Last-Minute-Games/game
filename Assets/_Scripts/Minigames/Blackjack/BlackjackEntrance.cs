using UnityEngine;

public class BlackjackEntrance : MonoBehaviour
{
    [Tooltip("Reference to the BlackjackPopupController in your Canvas.")]
    public BlackjackPopupController popup;

    [Tooltip("Maximum distance from the player to trigger the minigame.")]
    public float interactDistance = 2.5f;

    private Transform player;

    void Start()
    {
        // Find player automatically (optional)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        // Only proceed if popup is assigned and player exists
        if (popup == null || player == null)
            return;

        // Check if any interaction is already in progress
        if (Systems.InteractionLockManager.IsLocked) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);

        // If close enough and player presses E
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
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
    }
}
