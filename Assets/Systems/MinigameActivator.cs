using UnityEngine;

/// <summary>
/// Base class for minigame entrance activators. Implements IInteractable with standard minigame priority.
/// </summary>
public abstract class MinigameActivator : MonoBehaviour, IInteractable
{
    [Header("Activation Settings")]
    [Tooltip("Tag required to trigger this activator (usually 'Interactive')")]
    [SerializeField] protected string requiredTag = "Interactive";
    
    [Tooltip("The range within which interaction is possible")]
    [SerializeField] protected float interactionRange = 1.5f;

    [Header("Priority")]
    [Tooltip("Interaction priority. Lower = higher priority. Dialogs=0, Minigames=5, Items=10")]
    [SerializeField] protected int interactionPriority = 5;

    protected GameObject player;
    protected bool playerInRange = false;

    protected virtual void Start()
    {
        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Player not found! Interaction will not work.");
        }

        // Ensure we have a trigger collider
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] No BoxCollider2D found! Add one as a trigger for interaction to work.");
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // Update range check
        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerInRange = distance <= interactionRange;
    }

    public abstract void Interact();

    public virtual int GetInteractionPriority()
    {
        return interactionPriority;
    }

    public virtual bool CanInteract()
    {
        // Can interact if player is in range and no other interaction is in progress
        return playerInRange && !Systems.InteractionLockManager.IsLocked;
    }

    public virtual bool ShowInteractionPrompt()
    {
        // Minigames DO show the popup icon
        return true;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
