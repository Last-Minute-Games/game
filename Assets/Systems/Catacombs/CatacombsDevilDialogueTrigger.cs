using UnityEngine;
using UnityEngine;
using UnityEngine.Events;
using cherrydev;

public class CatacombsDevilDialogueTrigger : MonoBehaviour
{
    [System.Serializable]
    public class DialogueStep
    {
        [Tooltip("Y position threshold where this dialogue triggers")]
        public float triggerY;

        [Tooltip("Game flag to check/set for this dialogue")]
        public string flagName;

        [Tooltip("Dialogue graph to play for this step")]
        public DialogNodeGraph dialogGraph;

        [Tooltip("Resource path as fallback if dialogGraph is not assigned")]
        public string dialogResourcePath;
    }

    [Header("Dialogue Steps")]
    [SerializeField] private DialogueStep[] dialogueSteps = new DialogueStep[]
    {
        new DialogueStep
        {
            triggerY = 0f,
            flagName = "catacombs.devil.dialog.shown",
            dialogResourcePath = "Dialogues/Monologues/CatacombsDevilWelcome"
        },
        new DialogueStep
        {
            triggerY = 13f,
            flagName = "catacombs.devil.king.dead.shown",
            dialogResourcePath = "Dialogues/Monologues/CatacombsDevilKingDead"
        },
        new DialogueStep
        {
            triggerY = 26f,
            flagName = "catacombs.devil.fault.shown",
            dialogResourcePath = "Dialogues/Monologues/CatacombsDevilFault"
        },
        new DialogueStep
        {
            triggerY = 38f,
            flagName = "catacombs.devil.survive.shown",
            dialogResourcePath = "Dialogues/Monologues/CatacombsDevilSurvive"
        },
    };

    [Header("References")]
    [SerializeField] private DialogBehaviour dialogBehaviour;

    [Header("Auto-Find Components")]
    [SerializeField] private bool autoFindDialogBehaviour = true;
    [SerializeField] private bool autoFindPlayer = true;

    private bool _isDialogPlaying;
    private bool _warnedMissingDialog;
    private Transform _player;
    private int _currentStepIndex = -1;

    private void Start()
    {
        _currentStepIndex = FindNextStepIndex();

        if (autoFindDialogBehaviour && dialogBehaviour == null)
        {
            dialogBehaviour = FindObjectOfType<DialogBehaviour>(true);
        }
    }

    private void Update()
    {
        if (_isDialogPlaying)
        {
            return;
        }

        if (_currentStepIndex < 0 || _currentStepIndex >= dialogueSteps.Length)
        {
            return;
        }

        if (_player == null && autoFindPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                return;
            }

            _player = playerObj.transform;
        }

        if (_player == null)
        {
            return;
        }

        DialogueStep step = dialogueSteps[_currentStepIndex];
        if (_player.position.y < step.triggerY)
        {
            return;
        }

        if (dialogBehaviour == null)
        {
            if (autoFindDialogBehaviour)
            {
                dialogBehaviour = FindObjectOfType<DialogBehaviour>(true);
            }

            if (dialogBehaviour == null)
            {
                if (!_warnedMissingDialog)
                {
                    Debug.LogWarning("[CatacombsDevilDialogueTrigger] DialogBehaviour not found. Please assign it in the inspector or enable auto-find.");
                    _warnedMissingDialog = true;
                }

                return;
            }
        }

        TriggerDialogue(step);
    }

    private int FindNextStepIndex()
    {
        for (int i = 0; i < dialogueSteps.Length; i++)
        {
            if (!GameFlags.HasFlag(dialogueSteps[i].flagName))
            {
                return i;
            }
        }

        return -1;
    }

    private void TriggerDialogue(DialogueStep step)
    {
        _isDialogPlaying = true;

        DialogNodeGraph dialogGraph = step.dialogGraph;
        if (dialogGraph == null && !string.IsNullOrEmpty(step.dialogResourcePath))
        {
            dialogGraph = Resources.Load<DialogNodeGraph>(step.dialogResourcePath);
        }

        if (dialogGraph == null)
        {
            Debug.LogWarning($"[CatacombsDevilDialogueTrigger] Dialog graph not found for step with flag '{step.flagName}'. Please assign it in the inspector or provide a valid resource path.");
            _isDialogPlaying = false;
            return;
        }

        GameObject playerObj = _player != null ? _player.gameObject : GameObject.FindGameObjectWithTag("Player");
        PlayerInput2D playerInput = null;
        CharacterMotor2D motor = null;
        bool prevInputEnabled = true;
        bool prevDialogueActive = false;

        if (playerObj != null)
        {
            playerInput = playerObj.GetComponent<PlayerInput2D>();
            motor = playerObj.GetComponent<CharacterMotor2D>();

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

            _isDialogPlaying = false;
            _currentStepIndex = FindNextStepIndex();
        };

        dialogBehaviour.OnDialogFinished.AddListener(onFinished);
        GameFlags.SetFlag(step.flagName);
        dialogBehaviour.StartDialog(dialogGraph);
    }
}
