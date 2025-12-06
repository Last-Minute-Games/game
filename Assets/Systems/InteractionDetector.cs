using UnityEngine;
using UnityEngine.UI;

public class InteractionDetector : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupImage; // Assign your PNG UI or world-space sprite

    private bool nearInteractive = false;
    private GameObject currentTarget;

    private void Start()
    {
        if (popupImage != null)
            popupImage.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactive"))
        {
            nearInteractive = true;
            currentTarget = other.gameObject;
            popupImage.SetActive(true);
        }
        if (other.CompareTag("NPC"))
        {
            nearInteractive = true;
            currentTarget = other.gameObject;
            popupImage.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactive"))
        {
            if (other.gameObject == currentTarget)
            {
                nearInteractive = false;
                currentTarget = null;
                popupImage.SetActive(false);
            }
        }
        if (other.CompareTag("NPC"))
        {
            if (other.gameObject == currentTarget)
            {
                nearInteractive = false;
                currentTarget = null;
                popupImage.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Example: press key to interact
        if (nearInteractive && Input.GetKeyDown(KeyCode.E))
        {
            // Check if any interaction is already in progress
            if (Systems.InteractionLockManager.IsLocked) return;
            
            // Trigger NPC dialog or item pickup
            currentTarget.GetComponent<IInteractable>()?.Interact();
        }
    }
}
