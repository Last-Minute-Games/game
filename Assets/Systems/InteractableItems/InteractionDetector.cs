using UnityEngine;
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
    public float hoverCheckRadius = 1.0f;
    [Tooltip("Enable lenient directional hover (Stardew Valley style - just point mouse in general direction)")]
    public bool enableDirectionalHover = true;
    [Tooltip("Max distance for directional hover to work")]
    public float directionalHoverMaxDistance = 3f;
    [Tooltip("Angle tolerance for directional hover (degrees) - higher = more forgiving")]
    public float directionalHoverAngleTolerance = 60f;
    [Tooltip("Use raycast for hover detection (more reliable, recommended)")]
    public bool useRaycastDetection = true;
    [Tooltip("Layer mask for raycast detection (set to layers containing interactables)")]
    public LayerMask raycastLayerMask = -1;
    
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable hoveredInteractable = null;
    
    // Performance: Cached camera reference
    private Camera _mainCamera;
    
    // Performance: Track last mouse position to avoid unnecessary checks
    private Vector3 _lastMousePosition;
    private const float MOUSE_MOVEMENT_THRESHOLD = 0.1f; // Skip hover updates if mouse barely moved
    
    // Performance: Cache raycast results buffer to avoid allocations
    private RaycastHit2D[] _raycastBuffer = new RaycastHit2D[10];
    
    /// <summary>Result from hover detection</summary>
    private struct HoverDetectionResult
    {
        public bool isDetected;
        public float distance;
        public string method;
        
        public HoverDetectionResult(bool detected, float dist, string detectionMethod)
        {
            isDetected = detected;
            distance = dist;
            method = detectionMethod;
        }
        
        public static HoverDetectionResult None => new HoverDetectionResult(false, float.MaxValue, "NONE");
    }

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
        
        // Validate collider setup
        ValidateColliderSetup();
    }
    
    /// <summary>
    /// Validates that the InteractionDetector has a properly sized trigger collider.
    /// This determines how close NPCs/items need to be for interaction.
    /// </summary>
    private void ValidateColliderSetup()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        
        if (colliders.Length == 0)
        {
            Debug.LogError($"[InteractionDetector] {gameObject.name} is missing a Collider2D!\n" +
                          "Add a CircleCollider2D or BoxCollider2D and set it as a TRIGGER.\n" +
                          "Recommended: CircleCollider2D with radius 1.5-2.0 for full body coverage.");
            return;
        }
        
        Collider2D triggerCollider = null;
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
                break;
            }
        }
        
        if (triggerCollider == null)
        {
            Debug.LogWarning($"[InteractionDetector] {gameObject.name} has colliders but none are set as TRIGGER!\n" +
                           "Set 'Is Trigger' checkbox in the collider inspector.");
            return;
        }
        
        // Check if collider is too small
        float effectiveRadius = GetEffectiveRadius(triggerCollider);
        if (effectiveRadius < 1.0f)
        {
            Debug.LogWarning($"[InteractionDetector] {gameObject.name}'s trigger collider is quite small ({effectiveRadius:F2} units).\n" +
                           "For full body interaction, consider increasing to 1.5-2.0 units.\n" +
                           "Current setup will only detect NPCs very close to the player center.");
        }
    }
    
    private float GetEffectiveRadius(Collider2D collider)
    {
        if (collider is CircleCollider2D circle)
        {
            return circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        }
        else if (collider is BoxCollider2D box)
        {
            Vector2 size = box.size;
            return Mathf.Max(size.x, size.y) * 0.5f * Mathf.Max(transform.localScale.x, transform.localScale.y);
        }
        return 0.5f;
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

        // Handle E key (keyboard interaction)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Check if we're in dialog exit cooldown (prevents immediately re-entering dialog)
            if (cherrydev.DialogDisplayer.IsInDialogExitCooldown)
            {
                LogDebug("E key ignored - dialog exit cooldown active");
                return;
            }
            
            IInteractable bestInteractable = GetBestInteractable();
            LogDebug($"E key pressed! Nearby interactables: {nearbyInteractables.Count}, Best: {(bestInteractable != null ? bestInteractable.GetType().Name : "NONE")}");

            // Debug: Log why each interactable can't interact
            if (bestInteractable == null && nearbyInteractables.Count > 0)
            {
                foreach (var interactable in nearbyInteractables)
                {
                    if (interactable != null)
                    {
                        LogDebug($"  - {interactable.GetType().Name}: CanInteract={interactable.CanInteract()}");
                    }
                }
            }

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
        
        // Performance: Skip hover updates if mouse hasn't moved significantly
        if (Vector3.Distance(Input.mousePosition, _lastMousePosition) < MOUSE_MOVEMENT_THRESHOLD)
            return;
            
        _lastMousePosition = Input.mousePosition;
        
        // Get mouse position in world space
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        
        if (enableDebugLogs)
            LogDebug($"=== Hover Update === Mouse: {mouseWorldPos}, Player: {playerPos}, Count: {nearbyInteractables.Count}");
        
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
            
            // Try multiple detection methods in priority order
            HoverDetectionResult result = HoverDetectionResult.None;
            
            // Method 1: Raycast detection (most reliable)
            if (useRaycastDetection)
            {
                result = TryRaycastDetection(mouseWorldPos, mb);
            }
            
            // Method 2: Collider overlap detection (fallback)
            if (!result.isDetected)
            {
                result = TryColliderDetection(mouseWorldPos, mb);
            }
            
            // Method 3: Radius detection (simple distance check)
            if (!result.isDetected)
            {
                result = TryRadiusDetection(mouseWorldPos, mb);
            }
            
            // Method 4: Directional hover (Stardew Valley style) - skip for doors
            if (!result.isDetected && enableDirectionalHover && !isDoor)
            {
                result = TryDirectionalDetection(mouseWorldPos, playerPos, mb);
            }
            
            bool isMouseOver = result.isDetected;
            float distance = result.distance;
            string detectionMethod = result.method;
            
            // If mouse is over this interactable, check if it's the best one
            if (isMouseOver)
            {
                // Prefer higher priority (lower number) or closer distance if same priority
                if (ShouldReplaceHoveredInteractable(newHovered, interactable, distance, closestDistance))
                {
                    if (enableDebugLogs && newHovered != null)
                        LogDebug($"  > Hover: {mb.name} [{detectionMethod}] replaces {(newHovered as MonoBehaviour)?.name}");
                    newHovered = interactable;
                    closestDistance = distance;
                }
            }
        }
        
        // If no hover detected but there's a nearby door, use the door for cursor
        if (newHovered == null && nearestDoor != null)
        {
            newHovered = nearestDoor;
        }
        
        // Update cursor if hover state changed
        if (newHovered != hoveredInteractable)
        {
            if (enableDebugLogs)
            {
                string oldName = (hoveredInteractable as MonoBehaviour)?.name ?? "None";
                string newName = (newHovered as MonoBehaviour)?.name ?? "None";
                LogDebug($"Hover: {oldName} -> {newName}");
            }
            hoveredInteractable = newHovered;
            UpdateCursor();
        }
    }

    private void UpdateCursor()
    {
        // Change cursor for any hovered interactable (including doors if enabled)
        if (hoveredInteractable != null)
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
        
        // Check if we're in dialog exit cooldown (prevents immediately re-entering dialog)
        if (cherrydev.DialogDisplayer.IsInDialogExitCooldown)
        {
            LogDebug("Right-click ignored - dialog exit cooldown active");
            return;
        }
        
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
        return interactable is Systems.TeleportSystem
            || interactable is Systems.SceneTransitionDoor;
    }

    private void EnsurePromptText()
    {
        if (popupPromptText != null || popupImage == null)
            return;

        popupPromptText = popupImage.GetComponentInChildren<TMP_Text>(true);
        if (popupPromptText != null)
            return;

        TextMeshPro generatedPrompt = popupImage.AddComponent<TextMeshPro>();
        if (generatedPrompt == null)
            return;

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
            Cursor.SetCursor(defaultCursor, defaultCursorHotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    /// <summary>Check if a new interactable should replace the currently hovered one</summary>
    private bool ShouldReplaceHoveredInteractable(IInteractable current, IInteractable candidate, float candidateDistance, float currentDistance)
    {
        if (current == null) return true;
        
        int currentPriority = current.GetInteractionPriority();
        int candidatePriority = candidate.GetInteractionPriority();
        
        // Higher priority (lower number) always wins
        if (candidatePriority < currentPriority) return true;
        
        // Same priority: closer wins
        if (candidatePriority == currentPriority && candidateDistance < currentDistance) return true;
        
        return false;
    }
    
    /// <summary>Try to detect hover via raycast</summary>
    private HoverDetectionResult TryRaycastDetection(Vector2 mouseWorldPos, MonoBehaviour target)
    {
        int hitCount = Physics2D.RaycastNonAlloc(mouseWorldPos, Vector2.zero, _raycastBuffer, 0.01f, raycastLayerMask);
        
        for (int i = 0; i < hitCount; i++)
        {
            var hit = _raycastBuffer[i];
            if (hit.collider == null) continue;
            
            // Direct hit on target GameObject
            if (hit.collider.gameObject == target.gameObject)
            {
                float distance = Vector2.Distance(mouseWorldPos, target.transform.position);
                return new HoverDetectionResult(true, distance, "RAYCAST");
            }
            
            // Hit on child collider
            if (hit.collider.transform.IsChildOf(target.transform))
            {
                float distance = Vector2.Distance(mouseWorldPos, target.transform.position);
                return new HoverDetectionResult(true, distance, "RAYCAST_CHILD");
            }
        }
        
        return HoverDetectionResult.None;
    }
    
    /// <summary>Try to detect hover via collider overlap</summary>
    private HoverDetectionResult TryColliderDetection(Vector2 mouseWorldPos, MonoBehaviour target)
    {
        Collider2D[] colliders = target.GetComponents<Collider2D>();
        
        // Prioritize trigger colliders (interaction zones)
        foreach (var collider in colliders)
        {
            if (collider != null && collider.enabled && collider.isTrigger && collider.OverlapPoint(mouseWorldPos))
            {
                float distance = Vector2.Distance(mouseWorldPos, target.transform.position);
                return new HoverDetectionResult(true, distance, "TRIGGER");
            }
        }
        
        // Fallback to solid colliders
        foreach (var collider in colliders)
        {
            if (collider != null && collider.enabled && !collider.isTrigger && collider.OverlapPoint(mouseWorldPos))
            {
                float distance = Vector2.Distance(mouseWorldPos, target.transform.position);
                return new HoverDetectionResult(true, distance, "COLLIDER");
            }
        }
        
        return HoverDetectionResult.None;
    }
    
    /// <summary>Try to detect hover via simple radius check</summary>
    private HoverDetectionResult TryRadiusDetection(Vector2 mouseWorldPos, MonoBehaviour target)
    {
        float distance = Vector2.Distance(mouseWorldPos, target.transform.position);
        
        if (distance <= hoverCheckRadius)
        {
            return new HoverDetectionResult(true, distance, "RADIUS");
        }
        
        return HoverDetectionResult.None;
    }
    
    /// <summary>Try to detect hover via directional pointing (Stardew Valley style)</summary>
    private HoverDetectionResult TryDirectionalDetection(Vector2 mouseWorldPos, Vector2 playerPos, MonoBehaviour target)
    {
        Vector2 targetPos = target.transform.position;
        float distanceToTarget = Vector2.Distance(playerPos, targetPos);
        
        // Only check if player is close enough
        if (distanceToTarget > directionalHoverMaxDistance)
            return HoverDetectionResult.None;
        
        // Check if mouse is pointing toward the target
        Vector2 toTarget = (targetPos - playerPos).normalized;
        Vector2 toMouse = (mouseWorldPos - playerPos).normalized;
        float angle = Vector2.Angle(toTarget, toMouse);
        
        if (angle <= directionalHoverAngleTolerance)
        {
            return new HoverDetectionResult(true, distanceToTarget, "DIRECTIONAL");
        }
        
        return HoverDetectionResult.None;
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[InteractionDetector] {message}");
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Editor-time validation to help catch setup issues
        if (Application.isPlaying) return;
        
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning($"[InteractionDetector] {gameObject.name} needs a Collider2D (trigger) to detect nearby NPCs/items!");
        }
        else
        {
            bool hasTrigger = false;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }
            if (!hasTrigger)
            {
                Debug.LogWarning($"[InteractionDetector] {gameObject.name} has colliders but none are set as TRIGGER!");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize the interaction trigger zone
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                
                if (col is CircleCollider2D circle)
                {
                    Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
                }
                else if (col is BoxCollider2D box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.offset, box.size);
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }
        }
        
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
        if (enableDirectionalHover && enableDebugLogs && _mainCamera != null && Application.isPlaying)
        {
            Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerPos = transform.position;
            
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            Gizmos.DrawLine(playerPos, mouseWorldPos);
        }
    }
#endif
}
