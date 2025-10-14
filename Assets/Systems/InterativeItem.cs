using System;
using System.Collections.Generic;
using cherrydev;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class InteractiveItem : MonoBehaviour
{
    [Header("Global Services")]
    [SerializeField] private GameFlags flags;
    [SerializeField] private JournalManager journal; // optional

    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;    // Dialogue UI instance
    [SerializeField] private DialogNodeGraph firstDialogGraph;   // First interaction
    [SerializeField] private DialogNodeGraph repeatDialogGraph;  // Repeated interactions
    [Tooltip("If set, this global flag will replace the local hasInteracted bool.")]
    [SerializeField] private string hasInteractedFlagKey;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1f;

    [Header("Conditional Dialogs (first match wins)")]
    [SerializeField] private List<ConditionalDialog> conditionalDialogs = new();

    [Header("Flags / Journal to set AFTER dialog ends (fallbacks)")]
    [SerializeField] private string[] setTrueOnFirst;
    [SerializeField] private string[] setTrueOnRepeat;
    [SerializeField] private string[] unlockJournalOnFirst;
    [SerializeField] private string[] unlockJournalOnRepeat;

    [Header("Debug Only")]
    [SerializeField, ReadOnly] private bool debugHasInteracted; // Shows runtime state in Inspector

    // Runtime
    private GameObject player;
    private CharacterMotor2D characterController;
    private bool isPlayerNear = false;

    private bool localHasInteracted = false;
    private List<string> pendingFlags = new();
    private List<string> pendingJournal = new();

    [Serializable]
    public class ConditionalDialog
    {
        [Tooltip("All of these must be TRUE.")]
        public string[] requireAllTrue;
        [Tooltip("At least one of these must be TRUE (optional).")]
        public string[] requireAnyTrue;
        [Tooltip("All of these must be FALSE (optional).")]
        public string[] requireAllFalse;

        [Tooltip("Dialog to use when the conditions match.")]
        public DialogNodeGraph graph;

        [Header("Apply AFTER dialog finishes")]
        [Tooltip("Flags to set TRUE after this conditional dialog ends.")]
        public string[] setTrueOnFinish;
        [Tooltip("Journal entries to unlock after this conditional dialog ends.")]
        public string[] unlockJournalOnFinish;
    }

    private bool HasInteracted
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(hasInteractedFlagKey) && flags != null)
                return flags.Get(hasInteractedFlagKey);
            return localHasInteracted;
        }
        set
        {
            if (!string.IsNullOrWhiteSpace(hasInteractedFlagKey) && flags != null)
                flags.Set(hasInteractedFlagKey, value);
            else
                localHasInteracted = value;

            debugHasInteracted = value; // Mirror to Inspector for debugging
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
        }

        if (journal != null && flags != null)
        {
            // Let the journal auto-unlock entries when flags change
            journal.Hook(flags);
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
            Debug.LogWarning($"{name}: Missing DialogBehaviour reference.");
            return;
        }

        pendingFlags.Clear();
        pendingJournal.Clear();

        // 1) Try conditional dialogs first
        if (TryConditionalDialogs(out var chosenGraph) && chosenGraph != null)
        {
            dialogBehaviour.StartDialog(chosenGraph);
            return;
        }

        // 2) Fallback to first/repeat dialog
        if (!HasInteracted)
        {
            HasInteracted = true;
            if (firstDialogGraph != null)
            {
                dialogBehaviour.StartDialog(firstDialogGraph);
                Enqueue(pendingFlags, setTrueOnFirst);
                Enqueue(pendingJournal, unlockJournalOnFirst);
            }
            else
            {
                Debug.LogWarning($"{name}: Missing firstDialogGraph reference.");
            }
        }
        else
        {
            if (repeatDialogGraph != null)
            {
                dialogBehaviour.StartDialog(repeatDialogGraph);
                Enqueue(pendingFlags, setTrueOnRepeat);
                Enqueue(pendingJournal, unlockJournalOnRepeat);
            }
            else
            {
                Debug.LogWarning($"{name}: Missing repeatDialogGraph reference.");
            }
        }
    }

    private bool TryConditionalDialogs(out DialogNodeGraph graph)
    {
        graph = null;
        foreach (var cd in conditionalDialogs)
        {
            if (cd.graph == null) continue;

            if (Pass(cd.requireAllTrue, true) &&
                PassAny(cd.requireAnyTrue) &&
                Pass(cd.requireAllFalse, false))
            {
                graph = cd.graph;
                Enqueue(pendingFlags, cd.setTrueOnFinish);
                Enqueue(pendingJournal, cd.unlockJournalOnFinish);
                return true;
            }
        }
        return false;
    }

    private bool Pass(string[] keys, bool mustBeTrue)
    {
        if (keys == null || keys.Length == 0) return true;
        foreach (var k in keys)
        {
            bool v = flags != null && flags.Get(k);
            if (mustBeTrue && !v) return false;
            if (!mustBeTrue && v) return false;
        }
        return true;
    }

    private bool PassAny(string[] keys)
    {
        if (keys == null || keys.Length == 0) return true;
        foreach (var k in keys)
        {
            if (flags != null && flags.Get(k)) return true;
        }
        return false;
    }

    private void Enqueue(List<string> list, string[] items)
    {
        if (items == null) return;
        foreach (var i in items)
        {
            if (!string.IsNullOrWhiteSpace(i))
                list.Add(i);
        }
    }

    void OnDialogStart()
    {
        if (characterController != null)
            characterController.SetDialogueActive(true);
    }

    void OnDialogFinished()
    {
        // Apply queued flags
        if (flags != null)
        {
            foreach (var f in pendingFlags)
            {
                flags.Set(f, true);
                Debug.Log($"[InteractiveItem] Flag '{f}' set to TRUE");
            }
        }

        // Unlock queued journal entries
        if (journal != null)
        {
            foreach (var id in pendingJournal)
            {
                journal.AddEntry(id);
                Debug.Log($"[InteractiveItem] Journal entry '{id}' unlocked");
            }
        }

        pendingFlags.Clear();
        pendingJournal.Clear();

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

#if UNITY_EDITOR
// ReadOnly attribute for Inspector
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}

public class ReadOnlyAttribute : PropertyAttribute { }
#endif
