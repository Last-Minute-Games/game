using System;
using cherrydev;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;   // Drag your Dialog prefab instance here
    [SerializeField] private DialogNodeGraph dialogGraph;       // Drag your custom DialogNodeGraph asset here
    [SerializeField] private KeyCode interactKey = KeyCode.E;   // Default key to trigger dialog

    [Header("Detection Settings")]
    [SerializeField] private float interactionRange = 3f;       // Distance from player to trigger
    [SerializeField] private GameObject player;                  // Assign the Player transform in Inspector

    private CharacterController2D _characterController;
    
    private bool _isPlayerNear = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _characterController = player.GetComponent<CharacterController2D>();
        
        dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
        dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
    }
    
    private void OnDialogStart()
    {
        _characterController.SetDialogueActive(true);
    }

    private void OnDialogFinished()
    {
        _characterController.SetDialogueActive(false);
    }

    void Update()
    {
        // Distance-based check (if no collider)
        if (player)
            _isPlayerNear = Vector3.Distance(transform.position, player.transform.position) <= interactionRange;

        // Listen for E press when near
        if (_isPlayerNear && Input.GetKeyDown(interactKey))
        {
            if (_characterController.IsDialogueActive) return;

            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialogBehaviour && dialogGraph)
        {
            dialogBehaviour.StartDialog(dialogGraph);
        }
        else
        {
            Debug.LogWarning("DialogTrigger: Missing DialogBehaviour or DialogNodeGraph reference.");
        }
    }
}