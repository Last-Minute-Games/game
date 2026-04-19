using System;
using System.Collections.Generic;
using cherrydev;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractiveItem : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("Dialog Settings")]
    public DialogBehaviour dialogBehaviour;
    public DialogNodeGraph dialogGraph;

    [Header("Flags to Set After Dialog")]
    [Tooltip("These flags will be set when the dialog finishes (e.g., 'talked_to_npc', 'quest_completed')")]
    [SerializeField] private List<string> flagsToSet = new List<string>();

    [Header("Events")]
    [Tooltip("Invoked when the dialog completes - use this to trigger custom scripts or actions")]
    public UnityEvent OnDialogCompleted;

    // Runtime
    private GameObject player;
    private CharacterMotor2D characterController;
    private ClockTimer clockTimer;

    // Track if THIS item started the current conversation
    private bool isMyConversation = false;

    void Start()
    {
        // Runtime validation for collider
        if (GetComponent<Collider2D>() == null)
        {
            DebugLogger.LogWarning($"[InteractiveItem] {name} is missing a Collider2D component! " +
                           "Add a BoxCollider2D, CircleCollider2D, or other 2D collider for player interaction to work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        // Find ClockTimer in the scene
        clockTimer = FindObjectOfType<ClockTimer>();
        if (clockTimer == null)
            DebugLogger.LogWarning($"[InteractiveItem] {name}: No ClockTimer found in scene. Timer pause will not work.");

        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
        }
    }

    void Update()
    {
        // Note: Player proximity is now handled by InteractionDetector's trigger collider
        // This prevents mismatches between trigger size and interaction range
        // Individual input checking removed - now handled by InteractionDetector
        // This prevents duplicate E key checks and ensures proper priority ordering
    }

    public void Interact()
    {
        if (!dialogBehaviour)
        {
            DebugLogger.LogWarning($"{name}: Missing DialogBehaviour reference.");
            return;
        }

        if (dialogGraph == null)
        {
            DebugLogger.LogWarning($"{name}: Missing DialogGraph reference.");
            return;
        }

        // Try to acquire the lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            return; // Another interaction is in progress
        }

        // Mark that THIS item is starting the conversation
        isMyConversation = true;
        dialogBehaviour.StartDialog(dialogGraph);
    }

    public int GetInteractionPriority()
    {
        // Dialog interactions have third priority (after teleports and dialog triggers)
        return 2;
    }

    public bool CanInteract()
    {
        // Can interact if we have dialog setup and no other interaction is in progress
        // Note: Proximity is validated by InteractionDetector's trigger - if this method is called,
        // the player is already within range
        if (dialogBehaviour == null || dialogGraph == null)
        {
            DebugLogger.LogInteractiveItem($"CanInteract=false: dialogBehaviour={dialogBehaviour != null}, dialogGraph={dialogGraph != null}", name);
            return false;
        }
        if (Systems.InteractionLockManager.IsLocked)
        {
            DebugLogger.LogInteractiveItem($"CanInteract=false: InteractionLockManager is locked", name);
            return false;
        }
        if (characterController != null && characterController.IsDialogueActive)
        {
            DebugLogger.LogInteractiveItem($"CanInteract=false: Dialogue is active", name);
            return false;
        }
        return true;
    }

    public bool ShowInteractionPrompt()
    {
        // Interactive items DO show the popup icon
        return true;
    }

    void OnDialogStart()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) 
        {
            DebugLogger.LogInteractiveItem("OnDialogStart called but not my conversation - ignoring", name);
            return;
        }

        DebugLogger.LogInteractiveItem("=== DIALOG START ===", name);

        // Pause NPCs and timer via GlobalPause (but not player input or timescale)
        GlobalPause.SetMinigamePaused(true);
        DebugLogger.LogInteractiveItem("GlobalPause minigame pause enabled (NPCs and timer paused)", name);

        if (characterController != null)
            characterController.SetDialogueActive(true);
    }

    void OnDialogFinished()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) return;

        // Resume NPCs and timer via GlobalPause
        GlobalPause.SetMinigamePaused(false);
        DebugLogger.LogInteractiveItem("GlobalPause minigame pause disabled (NPCs and timer resumed)", name);

        // Set all flags when dialog finishes
        if (flagsToSet != null && flagsToSet.Count > 0)
        {
            foreach (string flag in flagsToSet)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    GameFlags.SetFlag(flag);
                    DebugLogger.LogInteractiveItem($"Set flag '{flag}'", name);
                }
            }
        }
        else
        {
            DebugLogger.LogInteractiveItem("No flags to set (flagsToSet is empty)", name);
        }

        if (characterController != null)
            characterController.SetDialogueActive(false);

        // Invoke the custom callback
        OnDialogCompleted?.Invoke();

        // Reset the flag so we don't respond to other conversations
        isMyConversation = false;
        
        // Release the interaction lock
        Systems.InteractionLockManager.Unlock();
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            DebugLogger.LogInteractiveItem(message, name);
    }
}


/// <summary>
/// Simple attribute so the field is visible but not editable in the Inspector (runtime-safe).
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// Editor-only drawer so fields marked [ReadOnly] appear disabled in Inspector.
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool prev = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = prev;
    }
}
#endif
