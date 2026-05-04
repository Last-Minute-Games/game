using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// Temporary diagnostic tool to debug interaction issues in builds.
/// Attach this to your Player GameObject alongside InteractionDetector.
/// REMOVE THIS AFTER FIXING THE BUILD INTERACTION ISSUE!
/// </summary>
public class BuildDebugHelper : MonoBehaviour
{
    [Header("Debug Display")]
    [Tooltip("Optional: Assign a TextMeshProUGUI to show debug info on screen")]
    public TMP_Text debugText;

    [Header("Logging Options")]
    [Tooltip("Show logs even in builds (normally logs are Editor-only)")]
    public bool enableBuildLogging = true;

    [Tooltip("Show on-screen debug text (requires debugText to be assigned)")]
    public bool showOnScreenDebug = true;

    private InteractionDetector interactionDetector;

    void Start()
    {
        interactionDetector = GetComponent<InteractionDetector>();

        if (interactionDetector == null)
        {
            Debug.LogError("[BuildDebugHelper] No InteractionDetector found on this GameObject!");
            enabled = false;
            return;
        }

        // Validate collider setup
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0)
        {
            Debug.LogError("[BuildDebugHelper] NO COLLIDERS FOUND! InteractionDetector needs a trigger collider!");
        }
        else
        {
            bool hasTrigger = false;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    hasTrigger = true;
                    Debug.Log($"[BuildDebugHelper] Found trigger collider: {col.GetType().Name}");
                }
            }

            if (!hasTrigger)
            {
                Debug.LogError("[BuildDebugHelper] Colliders found but NONE are set as TRIGGER!");
            }
        }

        // List all interactables in scene at start
        if (enableBuildLogging)
        {
            var allInteractables = FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>().ToList();
            Debug.Log($"[BuildDebugHelper] Found {allInteractables.Count} interactables in scene:");
            foreach (var interactable in allInteractables)
            {
                MonoBehaviour mb = interactable as MonoBehaviour;
                if (mb != null)
                {
                    Collider2D col = mb.GetComponent<Collider2D>();
                    Debug.Log($"  - {mb.gameObject.name} (Layer: {LayerMask.LayerToName(mb.gameObject.layer)}, " +
                             $"HasCollider: {col != null}, Type: {interactable.GetType().Name})");
                }
            }
        }
    }

    void Update()
    {
        if (interactionDetector == null) return;

        // Get nearby interactables via reflection (since the field is private)
        var nearbyField = typeof(InteractionDetector).GetField("nearbyInteractables", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nearbyInteractables = nearbyField?.GetValue(interactionDetector) as System.Collections.Generic.List<IInteractable>;

        // Log E key presses
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (enableBuildLogging)
            {
                Debug.Log($"[BuildDebugHelper] E KEY PRESSED! Nearby count: {nearbyInteractables?.Count ?? -1}");

                if (nearbyInteractables != null && nearbyInteractables.Count > 0)
                {
                    foreach (var interactable in nearbyInteractables)
                    {
                        if (interactable != null)
                        {
                            MonoBehaviour mb = interactable as MonoBehaviour;
                            Debug.Log($"  - {mb?.gameObject.name ?? "NULL"}: CanInteract={interactable.CanInteract()}");
                        }
                    }
                }
                else
                {
                    Debug.Log("  NO NEARBY INTERACTABLES DETECTED!");
                }
            }
        }

        // Update on-screen debug display
        if (showOnScreenDebug && debugText != null && nearbyInteractables != null)
        {
            debugText.text = $"DEBUG INFO:\n" +
                           $"Nearby Interactables: {nearbyInteractables.Count}\n" +
                           $"E Key: {(Input.GetKey(KeyCode.E) ? "PRESSED" : "not pressed")}\n" +
                           $"Position: {transform.position}\n" +
                           $"Layer: {LayerMask.LayerToName(gameObject.layer)}";

            if (nearbyInteractables.Count > 0)
            {
                debugText.text += "\n\nNearby:";
                foreach (var interactable in nearbyInteractables)
                {
                    if (interactable != null)
                    {
                        MonoBehaviour mb = interactable as MonoBehaviour;
                        if (mb != null)
                        {
                            float dist = Vector2.Distance(transform.position, mb.transform.position);
                            debugText.text += $"\n- {mb.gameObject.name} ({dist:F2}m)";
                        }
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (enableBuildLogging)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"[BuildDebugHelper] TRIGGER ENTER: {other.gameObject.name} " +
                         $"(Layer: {LayerMask.LayerToName(other.gameObject.layer)}, " +
                         $"IsTrigger: {other.isTrigger})");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (enableBuildLogging)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"[BuildDebugHelper] TRIGGER EXIT: {other.gameObject.name}");
            }
        }
    }
}
