using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class InteractionDetector : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupImage; // Assign your PNG UI or world-space sprite

    private List<IInteractable> nearbyInteractables = new List<IInteractable>();

    private void Start()
    {
        if (popupImage != null)
            popupImage.SetActive(false);
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
        // Get the highest priority valid interactable
        IInteractable bestInteractable = GetBestInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
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

    private void UpdatePopupVisibility()
    {
        if (popupImage == null) return;

        // Show popup only if there's a valid interactable that wants to show the popup
        IInteractable bestInteractable = GetBestInteractable();
        
        // Only show popup if the interactable wants it shown (check ShowInteractionPrompt)
        bool shouldShow = bestInteractable != null && bestInteractable.ShowInteractionPrompt();
        popupImage.SetActive(shouldShow);
    }
}
