using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class InteractionDetector : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupImage; // Assign your PNG UI or world-space sprite

    [Header("Cursor Settings")]
    [Tooltip("Cursor to show when hovering over interactable items/NPCs")]
    public Texture2D interactCursor;
    [Tooltip("Cursor hotspot for the interact cursor")]
    public Vector2 interactCursorHotspot = new Vector2(16, 16);
    [Tooltip("Default cursor when not hovering")]
    public Texture2D defaultCursor;
    [Tooltip("Cursor hotspot for the default cursor")]
    public Vector2 defaultCursorHotspot = Vector2.zero;
    
    [Header("Hover Detection Settings")]
    [Tooltip("Radius around interactable to check for mouse hover (for NPCs without precise colliders)")]
    public float hoverCheckRadius = 0.5f;

    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable hoveredInteractable = null;
    private Camera _mainCamera;

    private void Start()
    {
        if (popupImage != null)
            popupImage.SetActive(false);
            
        // Cache main camera reference for performance
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogWarning("[InteractionDetector] Main Camera not found! Mouse hover detection will not work.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object has an IInteractable component (no tag required)
        IInteractable interactable = other.GetComponent<IInteractable>();
        
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            Debug.Log($"[InteractionDetector] Added interactable: {other.gameObject.name} (Type: {interactable.GetType().Name}, Priority: {interactable.GetInteractionPriority()})");
            UpdatePopupVisibility();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
            Debug.Log($"[InteractionDetector] Removed interactable: {other.gameObject.name}");
            UpdatePopupVisibility();
        }
    }

    private void Update()
    {
        // Update mouse hover detection
        UpdateMouseHover();

        // Handle E key (keyboard interaction)
        if (Input.GetKeyDown(KeyCode.E))
        {
            IInteractable bestInteractable = GetBestInteractable();
            Debug.Log($"[InteractionDetector] E key pressed! Nearby interactables: {nearbyInteractables.Count}, Best: {(bestInteractable != null ? bestInteractable.GetType().Name : "NONE")}");
            
            if (bestInteractable != null)
            {
                // Check if any interaction is already in progress
                if (Systems.InteractionLockManager.IsLocked)
                {
                    Debug.Log($"[InteractionDetector] Cannot interact - lock is held");
                    return;
                }
                
                Debug.Log($"[InteractionDetector] Calling Interact() on {bestInteractable.GetType().Name}");
                // Trigger the interaction
                bestInteractable.Interact();
            }
            else
            {
                Debug.Log($"[InteractionDetector] No valid interactable found");
            }
        }

        // Handle right-click (mouse interaction)
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    private void UpdateMouseHover()
    {
        if (_mainCamera == null) return;
        
        // Get mouse position in world space
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // Check what's under the cursor
        IInteractable newHovered = null;
        float closestDistance = float.MaxValue;
        
        // Check all nearby interactables to see if mouse is over them
        foreach (var interactable in nearbyInteractables)
        {
            if (interactable == null) continue;
            
            // Skip if can't interact
            if (!interactable.CanInteract()) continue;
            
            // Only check hover for items that show prompts (not doors/teleports)
            if (!interactable.ShowInteractionPrompt()) continue;
            
            // Get the GameObject
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            
            bool isMouseOver = false;
            float distance = float.MaxValue;
            
            // Method 1: Check collider (precise)
            Collider2D collider = mb.GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(mouseWorldPos))
            {
                isMouseOver = true;
                distance = Vector2.Distance(mouseWorldPos, mb.transform.position);
                Debug.Log($"[InteractionDetector] Mouse over collider: {mb.gameObject.name}");
            }
            
            // Method 2: Fallback - Check distance from center (for NPCs without precise colliders)
            if (!isMouseOver)
            {
                float distanceToCenter = Vector2.Distance(mouseWorldPos, mb.transform.position);
                if (distanceToCenter <= hoverCheckRadius)
                {
                    isMouseOver = true;
                    distance = distanceToCenter;
                    Debug.Log($"[InteractionDetector] Mouse within radius of: {mb.gameObject.name} (distance: {distanceToCenter})");
                }
            }
            
            // If mouse is over this interactable, check if it's the closest one
            if (isMouseOver)
            {
                // Prefer higher priority (lower number) or closer distance
                if (newHovered == null || 
                    interactable.GetInteractionPriority() < newHovered.GetInteractionPriority() ||
                    (interactable.GetInteractionPriority() == newHovered.GetInteractionPriority() && distance < closestDistance))
                {
                    newHovered = interactable;
                    closestDistance = distance;
                }
            }
        }
        
        // Update cursor if hover state changed
        if (newHovered != hoveredInteractable)
        {
            if (newHovered != null)
            {
                Debug.Log($"[InteractionDetector] Now hovering over: {(newHovered as MonoBehaviour)?.gameObject.name}");
            }
            else if (hoveredInteractable != null)
            {
                Debug.Log($"[InteractionDetector] No longer hovering over: {(hoveredInteractable as MonoBehaviour)?.gameObject.name}");
            }
            
            hoveredInteractable = newHovered;
            UpdateCursor();
        }
    }

    private void UpdateCursor()
    {
        // Only change cursor for items that show prompts (not doors/teleports)
        if (hoveredInteractable != null && hoveredInteractable.ShowInteractionPrompt())
        {
            if (interactCursor != null)
            {
                Cursor.SetCursor(interactCursor, interactCursorHotspot, CursorMode.Auto);
            }
        }
        else
        {
            // Reset to default cursor
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void HandleRightClick()
    {
        Debug.Log($"[InteractionDetector] Right-click detected!");
        
        // Priority 1: If hovering over a specific item/NPC with cursor, interact with that
        if (hoveredInteractable != null && hoveredInteractable.ShowInteractionPrompt())
        {
            if (Systems.InteractionLockManager.IsLocked)
            {
                Debug.Log($"[InteractionDetector] Cannot interact - lock is held");
                return;
            }
            
            Debug.Log($"[InteractionDetector] Interacting with hovered item: {hoveredInteractable.GetType().Name}");
            hoveredInteractable.Interact();
            return;
        }
        
        // Priority 2: Check for doors/teleports (invisible interactions) in range
        // For doors, you can right-click anywhere when near them
        IInteractable bestDoor = GetBestDoorOrTeleport();
        if (bestDoor != null)
        {
            if (Systems.InteractionLockManager.IsLocked)
            {
                Debug.Log($"[InteractionDetector] Cannot interact - lock is held");
                return;
            }
            
            Debug.Log($"[InteractionDetector] Interacting with nearby door/teleport: {bestDoor.GetType().Name}");
            bestDoor.Interact();
            return;
        }
        
        Debug.Log($"[InteractionDetector] Right-click found nothing to interact with");
    }

    /// <summary>
    /// Get the highest priority interactable that can currently be interacted with
    /// </summary>
    private IInteractable GetBestInteractable()
    {
        // Clean up any null references
        nearbyInteractables.RemoveAll(x => x == null);

        if (nearbyInteractables.Count == 0)
            return null;

        // Get all valid interactables, sorted by priority (lower number = higher priority)
        var validInteractables = nearbyInteractables
            .Where(x => x.CanInteract())
            .OrderBy(x => x.GetInteractionPriority())
            .ToList();

        return validInteractables.FirstOrDefault();
    }

    private IInteractable GetBestDoorOrTeleport()
    {
        nearbyInteractables.RemoveAll(x => x == null);

        if (nearbyInteractables.Count == 0)
            return null;

        // Get doors/teleports (things that don't show prompts)
        var validDoors = nearbyInteractables
            .Where(x => x.CanInteract() && !x.ShowInteractionPrompt())
            .OrderBy(x => x.GetInteractionPriority())
            .ToList();

        return validDoors.FirstOrDefault();
    }

    private void UpdatePopupVisibility()
    {
        if (popupImage == null) return;

        // Show popup only if there's a valid interactable that wants to show the popup
        IInteractable bestInteractable = GetBestInteractable();
        
        // Only show popup if the interactable wants it shown (check ShowInteractionPrompt)
        bool shouldShow = bestInteractable != null && bestInteractable.ShowInteractionPrompt();
        popupImage.SetActive(shouldShow);
    }

    private void OnDisable()
    {
        // Reset cursor when disabled
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Visualize hover check radius for nearby interactables
        if (nearbyInteractables == null || _mainCamera == null) return;
        
        foreach (var interactable in nearbyInteractables)
        {
            if (interactable == null) continue;
            if (!interactable.ShowInteractionPrompt()) continue; // Only show for items that can be hovered
            
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            
            // Draw yellow circle around interactables that show prompts
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(mb.transform.position, hoverCheckRadius);
            
            // Draw filled circle if this is currently hovered
            if (interactable == hoveredInteractable)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                Gizmos.DrawSphere(mb.transform.position, hoverCheckRadius);
            }
        }
    }
#endif
}
