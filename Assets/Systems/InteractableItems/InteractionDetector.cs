using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class InteractionDetector : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("Popup Settings")]
    public GameObject popupImage; // Assign your PNG UI or world-space sprite
    [Tooltip("Optional text component used to display context-sensitive prompts (e.g. E to interact / E to play)")]
    public TMP_Text popupPromptText;
    [Tooltip("Prompt shown for regular interactions (NPCs, items, doors)")]
    public string interactPromptText = "E to interact";
    [Tooltip("Prompt shown for minigame interactions")]
    public string playPromptText = "E to play";

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
    [Tooltip("Enable lenient directional hover (Stardew Valley style - just point mouse in general direction)")]
    public bool enableDirectionalHover = true;
    [Tooltip("Max distance for directional hover to work")]
    public float directionalHoverMaxDistance = 3f;
    [Tooltip("Angle tolerance for directional hover (degrees) - higher = more forgiving")]
    public float directionalHoverAngleTolerance = 60f;
    
    [Header("Keyboard Interaction")]
    [Tooltip("Enable E key for interactions (disable to test mouse-only gameplay)")]
    public bool enableKeyboardInteraction = true;

    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable hoveredInteractable = null;
    
    // Performance: Cached camera reference
    private Camera _mainCamera;
    
    // Performance: Track last mouse position to avoid unnecessary checks
    private Vector3 _lastMousePosition;

    private void Start()
    {
        if (popupImage != null)
            popupImage.SetActive(false);

        // Auto-wire or create prompt text so legacy scenes with icon-only popup still work.
        EnsurePromptText();
        if (popupPromptText != null)
        {
            popupPromptText.text = string.Empty;
        }
            
        // Performance: Cache Camera.main
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogWarning("[InteractionDetector] Main Camera not found! Mouse hover detection will not work.");
        }
        
        _lastMousePosition = Input.mousePosition;
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
        // Update hover state every frame to ensure it handles camera/player movement
        if (enableHoverDetection)
        {
            UpdateMouseHover();
        }

        // Handle E key (keyboard interaction) - can be disabled for testing
        if (enableKeyboardInteraction && Input.GetKeyDown(KeyCode.E))
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

        // Keep prompt visibility/text in sync with changing interaction validity.
        UpdatePopupVisibility();
    }

    private void UpdateMouseHover()
    {
        // Performance: Fallback for camera if it wasn't available at Start
        if (_mainCamera == null)
            _mainCamera = Camera.main;
            
        if (_mainCamera == null) return;
        
        // Get mouse position in world space
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        
        LogDebug($"=== Hover Update === Mouse World: {mouseWorldPos}, Player: {playerPos}, Nearby: {nearbyInteractables.Count}");
        
        IInteractable newHovered = null;
        float closestDistance = float.MaxValue;
        IInteractable nearestDoor = null; // Track nearest door for cursor purposes
        float nearestDoorDistance = float.MaxValue;
        
        // Check all nearby interactables
        foreach (var interactable in nearbyInteractables)
        {
            if (interactable == null) continue;
            
            // Skip if can't interact
            if (!interactable.CanInteract()) continue;
            
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            
            bool showsPrompt = interactable.ShowInteractionPrompt();
            bool isDoor = IsDoorInteractable(interactable);
            
            LogDebug($"  Checking: {mb.gameObject.name} at {mb.transform.position} (ShowsPrompt: {showsPrompt}, IsDoor: {isDoor})");
            
            // For doors: track the nearest one for cursor changes (no hover required)
            if (isDoor)
            {
                float doorDistance = Vector2.Distance(playerPos, mb.transform.position);
                if (doorDistance < nearestDoorDistance)
                {
                    nearestDoor = interactable;
                    nearestDoorDistance = doorDistance;
                    LogDebug($"    Door found - distance: {doorDistance:F2} (tracking as nearest door)");
                }
            }
            
            bool isMouseOver = false;
            float distance = float.MaxValue;
            string detectionMethod = "NONE";
            
            // Method 1: Check if mouse is over any collider on this object
            Collider2D[] colliders = mb.GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                if (collider != null && collider.enabled && collider.OverlapPoint(mouseWorldPos))
                {
                    isMouseOver = true;
                    distance = Vector2.Distance(mouseWorldPos, mb.transform.position);
                    detectionMethod = "COLLIDER";
                    LogDebug($"    ? Method 1 (Collider): Hit! Distance: {distance:F2}");
                    break;
                }
            }
            
            // Method 2: Fallback - Check distance from center (for objects without precise colliders)
            if (!isMouseOver)
            {
                float distanceToCenter = Vector2.Distance(mouseWorldPos, mb.transform.position);
                LogDebug($"    Method 2 (Radius): Distance to center: {distanceToCenter:F2}, Threshold: {hoverCheckRadius:F2}");
                if (distanceToCenter <= hoverCheckRadius)
                {
                    isMouseOver = true;
                    distance = distanceToCenter;
                    detectionMethod = "RADIUS";
                    LogDebug($"    ? Method 2 (Radius): Hit!");
                }
            }
            
            // Method 3: Stardew Valley style - Directional hover (if enabled and player is close)
            // BUT skip directional hover for doors/teleporters - they only need collider/radius detection
            if (!isMouseOver && enableDirectionalHover && !isDoor)
            {
                Vector2 objectPos = mb.transform.position;
                float distanceToObject = Vector2.Distance(playerPos, objectPos);
                
                LogDebug($"    Method 3 (Directional): Player->Object distance: {distanceToObject:F2}, Max: {directionalHoverMaxDistance:F2}");
                
                // Only do directional check if player is reasonably close to the object
                if (distanceToObject <= directionalHoverMaxDistance)
                {
                    // Direction from player to object
                    Vector2 toObject = (objectPos - playerPos).normalized;
                    
                    // Direction from player to mouse
                    Vector2 toMouse = (mouseWorldPos - playerPos).normalized;
                    
                    // Calculate angle between the two directions
                    float angle = Vector2.Angle(toObject, toMouse);
                    
                    LogDebug($"    Method 3 (Directional): Angle: {angle:F1}�, Tolerance: {directionalHoverAngleTolerance:F1}�");
                    LogDebug($"      Player->Object vector: {toObject}, Player->Mouse vector: {toMouse}");
                    
                    // If mouse is pointing roughly in the direction of the object, count as hovering
                    if (angle <= directionalHoverAngleTolerance)
                    {
                        isMouseOver = true;
                        distance = distanceToObject;
                        detectionMethod = "DIRECTIONAL";
                        LogDebug($"    ? Method 3 (Directional): HIT! Angle {angle:F1}� within tolerance");
                    }
                    else
                    {
                        LogDebug($"    ? Method 3 (Directional): Angle too wide ({angle:F1}� > {directionalHoverAngleTolerance:F1}�)");
                    }
                }
                else
                {
                    LogDebug($"    ? Method 3 (Directional): Too far ({distanceToObject:F2} > {directionalHoverMaxDistance:F2})");
                }
            }
            else if (!isMouseOver && isDoor)
            {
                LogDebug($"    ? Method 3 (Directional): Skipped for door/teleporter (doors don't need directional hover)");
            }
            else if (!isMouseOver)
            {
                LogDebug($"    ? Method 3 (Directional): Skipped (enabled={enableDirectionalHover}, alreadyDetected={isMouseOver})");
            }
            
            // If mouse is over this interactable, check if it's the best one
            if (isMouseOver)
            {
                LogDebug($"    ? Detected via {detectionMethod}, Distance: {distance:F2}, Priority: {interactable.GetInteractionPriority()}");
                
                // Prefer higher priority (lower number) or closer distance if same priority
                if (newHovered == null || 
                    interactable.GetInteractionPriority() < newHovered.GetInteractionPriority() ||
                    (interactable.GetInteractionPriority() == newHovered.GetInteractionPriority() && distance < closestDistance))
                {
                    if (newHovered != null)
                    {
                        LogDebug($"    ? Replacing previous hover ({(newHovered as MonoBehaviour)?.gameObject.name}) with {mb.gameObject.name}");
                    }
                    newHovered = interactable;
                    closestDistance = distance;
                }
                else
                {
                    LogDebug($"    ? Not best option (current best: {(newHovered as MonoBehaviour)?.gameObject.name})");
                }
            }
        }
        
        // If no hover detected but there's a nearby door, use the door for cursor
        if (newHovered == null && nearestDoor != null)
        {
            newHovered = nearestDoor;
            LogDebug($">>> No mouse hover, but using nearest door for cursor: {(nearestDoor as MonoBehaviour)?.gameObject.name}");
        }
        
        // Update cursor if hover state changed
        if (newHovered != hoveredInteractable)
        {
            LogDebug($">>> HOVER CHANGED: {(hoveredInteractable as MonoBehaviour)?.gameObject.name ?? "NULL"} ? {(newHovered as MonoBehaviour)?.gameObject.name ?? "NULL"}");
            hoveredInteractable = newHovered;
            UpdateCursor();
        }
        else
        {
            LogDebug($">>> No hover change (still: {(hoveredInteractable as MonoBehaviour)?.gameObject.name ?? "NULL"})");
        }
    }

    private void UpdateCursor()
    {
        // Change cursor for any hovered interactable (including doors if enabled)
        if (hoveredInteractable != null)
        {
            if (interactCursor != null)
            {
                if (CursorManager.Instance != null)
                {
                    CursorManager.Instance.SetScaledCursor(interactCursor, interactCursorHotspot);
                }
                else
                {
                    Cursor.SetCursor(interactCursor, interactCursorHotspot, CursorMode.ForceSoftware);
                }
                LogDebug($"Cursor changed - hovering over: {(hoveredInteractable as MonoBehaviour)?.gameObject.name}");
            }
        }
        else
        {
            // Reset to default cursor
            if (defaultCursor != null)
            {
                if (CursorManager.Instance != null)
                {
                    CursorManager.Instance.SetScaledCursor(defaultCursor, defaultCursorHotspot);
                }
                else
                {
                    Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.ForceSoftware);
                }
            }
            else
            {
                if (CursorManager.Instance != null)
                {
                    CursorManager.Instance.SetScaledCursor(null, Vector2.zero);
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                }
            }
        }
    }

    private void HandleRightClick()
    {
        LogDebug($"Right-click detected!");
        
        // Priority 1: If hovering over something with cursor, interact with that
        if (enableHoverDetection && hoveredInteractable != null)
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
        // For doors/teleporters: works if player is in range (no directional hover required)
        // For NPCs/items: only works if directional hover is disabled
        IInteractable bestInteractable = GetBestInteractable();
        
        if (bestInteractable != null)
        {
            bool isDoor = IsDoorInteractable(bestInteractable);
            
            // If directional hover is enabled and this is NOT a door, must be hovering first
            if (enableDirectionalHover && !isDoor)
            {
                LogDebug($"Right-click blocked - directional hover enabled, must hover over {bestInteractable.GetType().Name} first");
                return;
            }
            
            if (Systems.InteractionLockManager.IsLocked)
            {
                LogDebug($"Cannot interact - lock is held");
                return;
            }
            
            LogDebug($"Right-click interacting with: {bestInteractable.GetType().Name} (isDoor: {isDoor})");
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

        if (popupPromptText != null)
        {
            popupPromptText.text = shouldShow && bestInteractable != null
                ? GetPromptText(bestInteractable)
                : string.Empty;
        }
    }

    private bool IsDoorInteractable(IInteractable interactable)
    {
        return interactable is Systems.TeleportSystem;
    }

    private void EnsurePromptText()
    {
        if (popupPromptText != null || popupImage == null)
            return;

        popupPromptText = popupImage.GetComponentInChildren<TMP_Text>(true);
        if (popupPromptText != null)
            return;

        TextMeshPro generatedPrompt = popupImage.AddComponent<TextMeshPro>();
        generatedPrompt.fontSize = 2.5f;
        generatedPrompt.color = Color.white;
        generatedPrompt.alignment = TextAlignmentOptions.Left;
        generatedPrompt.enableWordWrapping = false;
        generatedPrompt.text = string.Empty;
        generatedPrompt.transform.localPosition += new Vector3(0.8f, 0f, 0f);

        SpriteRenderer iconRenderer = popupImage.GetComponent<SpriteRenderer>();
        Renderer textRenderer = generatedPrompt.GetComponent<Renderer>();
        if (iconRenderer != null && textRenderer != null)
        {
            textRenderer.sortingLayerID = iconRenderer.sortingLayerID;
            textRenderer.sortingOrder = iconRenderer.sortingOrder + 1;
        }

        popupPromptText = generatedPrompt;
    }

    private string GetPromptText(IInteractable interactable)
    {
        return IsMinigameInteractable(interactable) ? playPromptText : interactPromptText;
    }

    private bool IsMinigameInteractable(IInteractable interactable)
    {
        return interactable is MinigameActivator
            || interactable is OverworldCoinGameLauncher
            || interactable is OverworldRiddleItem;
    }

    private void OnDisable()
    {
        if (defaultCursor != null)
        {
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetScaledCursor(defaultCursor, defaultCursorHotspot);
            }
            else
            {
                Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.ForceSoftware);
            }
        }
        else
        {
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetScaledCursor(null, Vector2.zero);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
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
            
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;
            
            bool showsPrompt = interactable.ShowInteractionPrompt();
            bool isDoor = IsDoorInteractable(interactable);
            
            // Draw yellow circle for radius check
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(mb.transform.position, hoverCheckRadius);
            
            // Draw filled circle if this is currently hovered
            if (interactable == hoveredInteractable)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                Gizmos.DrawSphere(mb.transform.position, hoverCheckRadius);
            }
            
            // Draw directional hover range if enabled
            if (enableDirectionalHover)
            {
                // Different color for doors vs items/NPCs
                Gizmos.color = isDoor ? new Color(1f, 0.5f, 0f, 0.2f) : new Color(0f, 0.5f, 1f, 0.2f);
                Gizmos.DrawWireSphere(mb.transform.position, directionalHoverMaxDistance);
            }
        }
        
        // Draw debug line showing mouse direction in editor (when hovering)
        if (enableDirectionalHover && _mainCamera != null && Application.isPlaying)
        {
            Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerPos = transform.position;
            
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            Gizmos.DrawLine(playerPos, mouseWorldPos);
        }
    }
#endif
}
