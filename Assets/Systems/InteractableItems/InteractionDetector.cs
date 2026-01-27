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

    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private Camera _mainCamera;

    private void Start()
    {
        if (popupImage != null)
            popupImage.SetActive(false);
            
        // Cache main camera reference for performance
        _mainCamera = Camera.main;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
        // Handle E key (keyboard interaction)
        if (Input.GetKeyDown(KeyCode.E))
        {
            IInteractable bestInteractable = GetBestInteractable();
            Debug.Log($"[InteractionDetector] E key pressed! Nearby interactables: {nearbyInteractables.Count}, Best: {(bestInteractable != null ? bestInteractable.GetType().Name : "NONE")}");
            
            if (bestInteractable != null)
            {
                if (Systems.InteractionLockManager.IsLocked)
                {
                    Debug.Log($"[InteractionDetector] Cannot interact - lock is held");
                    return;
                }
                
                Debug.Log($"[InteractionDetector] Calling Interact() on {bestInteractable.GetType().Name}");
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

    private void HandleRightClick()
    {
        Debug.Log($"[InteractionDetector] Right-click detected!");
        
        // Just get the best interactable in range and interact with it
        // This works for both NPCs and doors - no hover required!
        IInteractable bestInteractable = GetBestInteractable();
        
        if (bestInteractable != null)
        {
            if (Systems.InteractionLockManager.IsLocked)
            {
                Debug.Log($"[InteractionDetector] Cannot interact - lock is held");
                return;
            }
            
            Debug.Log($"[InteractionDetector] Right-click interacting with: {bestInteractable.GetType().Name}");
            bestInteractable.Interact();
            return;
        }
        
        Debug.Log($"[InteractionDetector] Right-click found nothing to interact with");
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
}
