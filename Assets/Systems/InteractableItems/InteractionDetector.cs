using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class InteractionDetector : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
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
    [Tooltip("Enable hover detection for NPCs and items (Stardew Valley style)")]
    public bool enableHoverDetection = true;
    [Tooltip("Radius around interactable to check for mouse hover (fallback for objects without precise colliders)")]
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
        IInteractable interactable = other.GetComponent<IInteractable>();
        
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            LogDebug($"Added interactable: {other.gameObject.name} (Type: {interactable.GetType().Name}, Priority: {interactable.GetInteractionPriority()})");
            UpdatePopupVisibility();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
            LogDebug($"Removed interactable: {other.gameObject.name}");
            
            // Clear hover if we're leaving the hovered interactable
            if (interactable == hoveredInteractable)
            {
                hoveredInteractable = null;
                UpdateCursor();
            }
            
            UpdatePopupVisibility();
        }
    }

    private void Update()
    {
        // Update mouse hover detection
        if (enableHoverDetection)
        {
            UpdateMouseHover();
        }

        // Handle E key (keyboard interaction)
        if (Input.GetKeyDown(KeyCode.E))
        {
            IInteractable bestInteractable = GetBestInteractable();
            LogDebug($"E key pressed! Nearby interactables: {nearbyInteractables.Count}, Best: {(bestInteractable != null ? bestInteractable.GetType().Name : "NONE")}");
            
            if (bestInteractable != null)
            {
                if (Systems.InteractionLockManager.IsLocked)
                {
                    LogDebug($"Cannot interact - lock is held");
                    return;
                }
                
                LogDebug($"Calling Interact() on {bestInteractable.GetType().Name}");
                bestInteractable.Interact();
            }
            else
            {
                LogDebug($"No valid interactable found");
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
        
        IInteractable newHovered = null;
        float closestDistance = float.MaxValue;
        
        // Check all nearby interactables
        foreach (var interactable in nearbyInteractables)
        {
            if (interactable == null) continue;
            
            // Skip if can't interact
            if (!interactable.CanInteract()) continue;
            
            // Only check hover for items that show prompts (NPCs, items, minigames)
            // Doors/teleports don't need hover (they work anywhere when in range)
            if (!interactable.ShowInteractionPrompt()) continue;
            
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            
            bool isMouseOver = false;
            float distance = float.MaxValue;
            
            // Method 1: Check if mouse is over any collider on this object
            Collider2D[] colliders = mb.GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                if (collider != null && collider.enabled && collider.OverlapPoint(mouseWorldPos))
                {
                    isMouseOver = true;
                    distance = Vector2.Distance(mouseWorldPos, mb.transform.position);
                    break;
                }
            }
            
            // Method 2: Fallback - Check distance from center (for objects without precise colliders)
            if (!isMouseOver)
            {
                float distanceToCenter = Vector2.Distance(mouseWorldPos, mb.transform.position);
                if (distanceToCenter <= hoverCheckRadius)
                {
                    isMouseOver = true;
                    distance = distanceToCenter;
                }
            }
            
            // If mouse is over this interactable, check if it's the best one
            if (isMouseOver)
            {
                // Prefer higher priority (lower number) or closer distance if same priority
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
                LogDebug($"Cursor changed - hovering over: {(hoveredInteractable as MonoBehaviour)?.gameObject.name}");
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
        LogDebug($"Right-click detected!");
        
        // Priority 1: If hovering over a specific item/NPC with cursor, interact with that
        if (enableHoverDetection && hoveredInteractable != null && hoveredInteractable.ShowInteractionPrompt())
        {
            if (Systems.InteractionLockManager.IsLocked)
            {
                LogDebug($"Cannot interact - lock is held");
                return;
            }
            
            LogDebug($"Right-click on hovered item: {hoveredInteractable.GetType().Name}");
            hoveredInteractable.Interact();
            return;
        }
        
        // Priority 2: Fallback - interact with best interactable in range
        // (Doors work this way, or if hover detection is disabled)
        IInteractable bestInteractable = GetBestInteractable();
        
        if (bestInteractable != null)
        {
            if (Systems.InteractionLockManager.IsLocked)
            {
                LogDebug($"Cannot interact - lock is held");
                return;
            }
            
            LogDebug($"Right-click interacting with: {bestInteractable.GetType().Name}");
            bestInteractable.Interact();
            return;
        }
        
        LogDebug($"Right-click found nothing to interact with");
    }

    private IInteractable GetBestInteractable()
    {
        nearbyInteractables.RemoveAll(x => x == null);

        if (nearbyInteractables.Count == 0)
            return null;

        var validInteractables = nearbyInteractables
            .Where(x => x.CanInteract())
            .OrderBy(x => x.GetInteractionPriority())
            .ToList();

        return validInteractables.FirstOrDefault();
    }

    private void UpdatePopupVisibility()
    {
        if (popupImage == null) return;

        IInteractable bestInteractable = GetBestInteractable();
        
        bool shouldShow = bestInteractable != null && bestInteractable.ShowInteractionPrompt();
        popupImage.SetActive(shouldShow);
    }

    private void OnDisable()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[InteractionDetector] {message}");
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Visualize hover check radius for nearby interactables
        if (nearbyInteractables == null || !enableHoverDetection) return;
        
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
