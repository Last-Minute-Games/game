using System;
using cherrydev;
using Unity.Cinemachine;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;   // Drag your Dialog prefab instance here
    [SerializeField] private DialogNodeGraph dialogGraph;       // Drag your custom DialogNodeGraph asset here
    [SerializeField] private KeyCode interactKey = KeyCode.E;   // Default key to trigger dialog

    [Header("Detection Settings")]
    private readonly float _interactionRange = 1f;       // Distance from player to trigger
    
    private GameObject _player;                  // Assign the Player transform in Inspector
    private CharacterController2D _characterController;
    
    private bool _isPlayerNear = false;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterController = _player.GetComponent<CharacterController2D>();
        
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
        if (_player)
            _isPlayerNear = Vector3.Distance(transform.position, _player.transform.position) <= _interactionRange;

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