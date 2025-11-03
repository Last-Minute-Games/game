using System.Collections.Generic;
using cherrydev;
using UnityEngine;

/// <summary>
/// ADVANCED: For NPCs with multiple dialog states based on flags.
/// Most NPCs should use simple InteractiveItem instead!
/// </summary>
// Removed RequireComponent to allow flexible setup
// Runtime check will warn if collider is missing
public class ConditionalInteractiveItem : MonoBehaviour
{
    [System.Serializable]
    public class DialogOption
    {
        [Tooltip("Name for this dialog state (just for reference)")]
        public string stateName = "Default";
        
        [Tooltip("Flags that must exist for this dialog to play")]
        public string[] requiredFlags;
        
        [Tooltip("The dialog to play")]
        public DialogNodeGraph dialog;
        
        [Tooltip("Flag to set after this dialog finishes")]
        public string flagToSet;
    }
    
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;
    
    [Header("Dialog Options (first match wins)")]
    [Tooltip("Add dialog options in priority order. First matching option will play.")]
    [SerializeField] private List<DialogOption> dialogOptions = new();
    
    [Header("Fallback Dialog")]
    [Tooltip("Plays if no other options match")]
    [SerializeField] private DialogNodeGraph fallbackDialog;
    
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1f;
    
    private GameObject player;
    private CharacterMotor2D characterController;
    private ClockTimer clockTimer;
    private bool isPlayerNear = false;
    private string currentFlagToSet;
    
    void Start()
    {
        // Runtime validation for collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"[ConditionalInteractiveItem] {name} is missing a Collider2D component! " +
                           "Add a BoxCollider2D, CircleCollider2D, or other 2D collider for player interaction to work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        // Find ClockTimer in the scene
        clockTimer = FindObjectOfType<ClockTimer>();
        if (clockTimer == null)
            Debug.LogWarning($"[ConditionalInteractiveItem] {name}: No ClockTimer found in scene. Timer pause will not work.");
        
        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        isPlayerNear = Vector3.Distance(transform.position, player.transform.position) <= interactionRange;
        
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            if (characterController != null && characterController.IsDialogueActive) return;
            Interact();
        }
    }
    
    void Interact()
    {
        if (!dialogBehaviour)
        {
            Debug.LogWarning($"{name}: Missing DialogBehaviour.");
            return;
        }
        
        // Find first matching dialog option
        DialogNodeGraph dialogToPlay = null;
        currentFlagToSet = null;
        
        foreach (var option in dialogOptions)
        {
            if (option.dialog == null) continue;
            
            // Check if all required flags exist
            bool allFlagsExist = true;
            if (option.requiredFlags != null)
            {
                foreach (var flag in option.requiredFlags)
                {
                    if (!GameFlags.HasFlag(flag))
                    {
                        allFlagsExist = false;
                        break;
                    }
                }
            }
            
            if (allFlagsExist)
            {
                dialogToPlay = option.dialog;
                currentFlagToSet = option.flagToSet;
                Debug.Log($"[ConditionalInteractive] Playing dialog: {option.stateName}");
                break;
            }
        }
        
        // Use fallback if no option matched
        if (dialogToPlay == null)
        {
            dialogToPlay = fallbackDialog;
            currentFlagToSet = null;
        }
        
        if (dialogToPlay != null)
        {
            dialogBehaviour.StartDialog(dialogToPlay);
        }
        else
        {
            Debug.LogWarning($"{name}: No dialog to play!");
        }
    }
    
    void OnDialogStart()
    {
        // Pause the clock timer
        if (clockTimer != null)
        {
            clockTimer.PauseTimer(true);
            Debug.Log($"[ConditionalInteractiveItem] {name}: Clock timer paused");
        }

        if (characterController != null)
            characterController.SetDialogueActive(true);
    }
    
    void OnDialogFinished()
    {
        // Resume the clock timer
        if (clockTimer != null)
        {
            clockTimer.PauseTimer(false);
            Debug.Log($"[ConditionalInteractiveItem] {name}: Clock timer resumed");
        }

        if (!string.IsNullOrEmpty(currentFlagToSet))
        {
            GameFlags.SetFlag(currentFlagToSet);
            Debug.Log($"[ConditionalInteractive] Set flag: {currentFlagToSet}");
        }
        
        if (characterController != null)
            characterController.SetDialogueActive(false);
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
}
