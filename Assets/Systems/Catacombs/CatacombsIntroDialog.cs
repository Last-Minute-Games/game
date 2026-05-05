using UnityEngine;
using UnityEngine.Events;
using cherrydev;

public class CatacombsIntroDialog : MonoBehaviour
{
    private const string FlagName = "catacombs.intro.dialog.shown";

    [Header("Dialogue")]
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph introDialogGraph;

    [Header("Auto-Find Components")]
    [SerializeField] private bool autoFindDialogBehaviour = true;
    [SerializeField] private bool autoFindPlayer = true;

    private bool hasPlayed = false;
    private PlayerInput2D playerInput;
    private CharacterMotor2D motor;

    private void Start()
    {
        if (hasPlayed)
        {
            return;
        }

        if (GameFlags.HasFlag(FlagName))
        {
            hasPlayed = true;
            return;
        }

        if (autoFindDialogBehaviour && dialogBehaviour == null)
        {
            dialogBehaviour = FindObjectOfType<DialogBehaviour>(true);
        }

        if (dialogBehaviour == null)
        {
            Debug.LogWarning("[CatacombsIntroDialog] DialogBehaviour not found. Please assign it in the inspector or enable auto-find.");
            return;
        }

        if (introDialogGraph == null)
        {
            introDialogGraph = Resources.Load<DialogNodeGraph>("Dialogues/Monologues/CatacombsIntro");
            if (introDialogGraph == null)
            {
                Debug.LogWarning("[CatacombsIntroDialog] Dialog graph not found. Please assign it in the inspector.");
                return;
            }
        }

        PlayIntroDialog();
    }

    private void PlayIntroDialog()
    {
        hasPlayed = true;

        GameObject player = null;
        if (autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        bool prevInputEnabled = true;
        bool prevDialogueActive = false;

        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput2D>();
            motor = player.GetComponent<CharacterMotor2D>();

            if (playerInput != null)
            {
                prevInputEnabled = playerInput.isInputEnabled;
                playerInput.isInputEnabled = false;
            }

            if (motor != null)
            {
                prevDialogueActive = motor.IsDialogueActive;
                motor.SetDialogueActive(true);
            }
        }

        UnityAction onFinished = null;
        onFinished = () =>
        {
            if (dialogBehaviour != null)
            {
                dialogBehaviour.OnDialogFinished.RemoveListener(onFinished);
            }

            if (playerInput != null)
            {
                playerInput.isInputEnabled = prevInputEnabled;
            }

            if (motor != null)
            {
                motor.SetDialogueActive(prevDialogueActive);
            }
        };

        dialogBehaviour.OnDialogFinished.AddListener(onFinished);
        GameFlags.SetFlag(FlagName);
        dialogBehaviour.StartDialog(introDialogGraph);
    }
}
