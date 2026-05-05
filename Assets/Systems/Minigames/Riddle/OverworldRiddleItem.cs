using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OverworldRiddleItem : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("Riddle Settings")]
    [Tooltip("The popup controller that shows the riddle page.")]
    [SerializeField] private RiddlePopupController riddlePopup;

    [Header("Interaction Settings")]
    [Tooltip("Range within which the player can interact")]
    [SerializeField] private float interactionRange = 1.5f;
    
    [Tooltip("Interaction priority (lower = higher priority). Teleports=0, Dialogs=1-2, Riddles=5")]
    [SerializeField] private int interactionPriority = 5;

    private GameObject _player;
    private bool _isPlayerNear = false;

    private void Reset()
    {
        // Make sure this collider behaves as a trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogWarning($"[OverworldRiddleItem] {name}: Player not found!");
        }

        // Ensure collider exists - auto-add if missing
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[OverworldRiddleItem] {name}: No Collider2D found! Auto-adding BoxCollider2D...");
            BoxCollider2D autoCollider = gameObject.AddComponent<BoxCollider2D>();

            // Try to size it based on sprite renderer if available
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                autoCollider.size = spriteRenderer.sprite.bounds.size;
                autoCollider.offset = spriteRenderer.sprite.bounds.center;
            }
            else
            {
                autoCollider.size = new Vector2(1f, 1f);
            }

            Debug.LogWarning($"[OverworldRiddleItem] {name}: Auto-added BoxCollider2D (size: {autoCollider.size})");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNear = true;
            LogDebug($"Player entered range of {name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNear = false;
            LogDebug($"Player left range of {name}");
        }
    }
    
    private void Update()
    {
        // Update player near status (fallback if trigger events don't work)
        if (_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            _isPlayerNear = distance <= interactionRange;
        }
    }

    // IInteractable Implementation
    public void Interact()
    {
        if (riddlePopup != null)
        {
            LogDebug($"Showing riddle popup for {name}");
            riddlePopup.Show();
        }
        else
        {
            Debug.LogWarning($"[OverworldRiddleItem] {name}: RiddlePopupController is not assigned!");
        }
    }

    public int GetInteractionPriority()
    {
        return interactionPriority;
    }

    public bool CanInteract()
    {
        // Can interact if player is near and no other interaction is in progress
        if (!_isPlayerNear)
        {
            LogDebug($"Cannot interact - player not in range");
            return false;
        }
        
        if (Systems.InteractionLockManager.IsLocked)
        {
            LogDebug($"Cannot interact - interaction locked");
            return false;
        }
        
        if (riddlePopup == null)
        {
            LogDebug($"Cannot interact - riddle popup not assigned");
            return false;
        }
        
        return true;
    }

    public bool ShowInteractionPrompt()
    {
        // Riddles DO show the popup icon (E to interact)
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[OverworldRiddleItem] {message}");
    }
}
