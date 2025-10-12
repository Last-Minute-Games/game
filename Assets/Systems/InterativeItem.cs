using System;
using cherrydev;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractiveItem : MonoBehaviour
{
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;  // The dialogue UI prefab instance
    [SerializeField] private DialogNodeGraph firstDialogGraph; // Dialogue for first interaction
    [SerializeField] private DialogNodeGraph repeatDialogGraph; // Dialogue for repeated interactions

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1f;

    private bool hasInteracted = false;
    private GameObject player;
    private CharacterMotor2D characterController;
    private bool isPlayerNear = false;

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
    }

    void Update()
    {
        if (player == null) return;

        // Detect if player is close enough
        isPlayerNear = Vector3.Distance(transform.position, player.transform.position) <= interactionRange;

        // Player presses E while near and not in dialogue
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

        if (!hasInteracted)
        {
            hasInteracted = true;

            if (firstDialogGraph)
                dialogBehaviour.StartDialog(firstDialogGraph);
            else
                Debug.LogWarning($"{name}: Missing firstDialogGraph reference.");
        }
        else
        {
            if (repeatDialogGraph)
                dialogBehaviour.StartDialog(repeatDialogGraph);
            else
                Debug.LogWarning($"{name}: Missing repeatDialogGraph reference.");
        }
    }

    void OnDialogStart()
    {
        if (characterController != null)
            characterController.SetDialogueActive(true);
    }

    void OnDialogFinished()
    {
        if (characterController != null)
            characterController.SetDialogueActive(false);
    }
}
