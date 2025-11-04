using System;
using cherrydev;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Removed RequireComponent to allow flexible setup
// Runtime check will warn if collider is missing
public class InteractiveItem : MonoBehaviour, IInteractable
{
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1f;

    [Header("Flag to Set After Dialog")]
    [Tooltip("This flag will be set when the dialog finishes (e.g., 'talked_to_npc')")]
    [SerializeField] private string flagToSet;

    // Runtime
    private GameObject player;
    private CharacterMotor2D characterController;
    private ClockTimer clockTimer;
    private bool isPlayerNear = false;

    void Start()
    {
        // Runtime validation for collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"[InteractiveItem] {name} is missing a Collider2D component! " +
                           "Add a BoxCollider2D, CircleCollider2D, or other 2D collider for player interaction to work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        // Find ClockTimer in the scene
        clockTimer = FindObjectOfType<ClockTimer>();
        if (clockTimer == null)
            Debug.LogWarning($"[InteractiveItem] {name}: No ClockTimer found in scene. Timer pause will not work.");

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

    public void Interact()
    {
        if (!dialogBehaviour)
        {
            Debug.LogWarning($"{name}: Missing DialogBehaviour reference.");
            return;
        }

        if (dialogGraph == null)
        {
            Debug.LogWarning($"{name}: Missing DialogGraph reference.");
            return;
        }

        dialogBehaviour.StartDialog(dialogGraph);
    }

    void OnDialogStart()
    {
        // Pause the clock timer
        if (clockTimer != null)
        {
            clockTimer.PauseTimer(true);
            Debug.Log($"[InteractiveItem] {name}: Clock timer paused");
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
            Debug.Log($"[InteractiveItem] {name}: Clock timer resumed");
        }

        // Set the flag when dialog finishes
        if (!string.IsNullOrEmpty(flagToSet))
        {
            GameFlags.SetFlag(flagToSet);
            Debug.Log($"[InteractiveItem] Set flag: {flagToSet}");
        }

        if (characterController != null)
            characterController.SetDialogueActive(false);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
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
