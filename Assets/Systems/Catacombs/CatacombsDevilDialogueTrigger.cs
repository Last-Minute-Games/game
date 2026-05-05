using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using cherrydev;
using DG.Tweening;

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

        [Header("Door Opening")]
        [Tooltip("Should this dialogue trigger a door opening sequence?")]
        public bool opensDoor = false;

        [Tooltip("Name of the door GameObject to find and open")]
        public string doorObjectName = "";

        [Tooltip("Delay before starting door opening sequence")]
        public float doorOpenDelay = 3.5f;
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

    [Header("Door Opening Settings")]
    [Tooltip("Audio source for door sound effects")]
    [SerializeField] private AudioSource doorAudioSource;

    [Tooltip("Character light to fade out before door opens")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D characterLight2D;

    [Tooltip("Spotlight to enable when door opens")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D doorSpotlight;

    [Tooltip("Blood footsteps parent object")]
    [SerializeField] private GameObject bloodFootsteps;

    [Tooltip("Environment sound handler for footstep sounds")]
    [SerializeField] private EnvironmentSoundHandler environmentSoundHandler;

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

            if (step.opensDoor)
            {
                StartCoroutine(WaitAndOpenBigDoor(step));
            }
        };

        dialogBehaviour.OnDialogFinished.AddListener(onFinished);
        GameFlags.SetFlag(step.flagName);
        dialogBehaviour.StartDialog(dialogGraph);
    }

    private IEnumerator WaitAndOpenBigDoor(DialogueStep step)
    {
        if (string.IsNullOrEmpty(step.doorObjectName))
        {
            Debug.LogWarning("[CatacombsDevilDialogueTrigger] Door object name not specified for door opening.");
            yield break;
        }

        var door = GameObject.Find(step.doorObjectName);
        if (door == null)
        {
            Debug.LogWarning($"[CatacombsDevilDialogueTrigger] Door object '{step.doorObjectName}' not found in scene.");
            yield break;
        }

        var openHash = Animator.StringToHash("OpenDoor");
        var spotlightClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/Spotlight");
        var bigDoorOpenClip = Resources.Load<AudioClip>("SFXs/Doors/BigDoorOpen");
        var bloodFootstepsClips = new System.Collections.Generic.List<AudioClip>();

        for (int i = 1; i <= 4; i++)
        {
            var clip = Resources.Load<AudioClip>($"SFXs/Foosteps/BloodFootsteps{i}");
            if (clip != null) bloodFootstepsClips.Add(clip);
        }

        if (characterLight2D != null)
        {
            characterLight2D.DOIntensity(0, 2f);
        }

        yield return new WaitForSeconds(step.doorOpenDelay);

        if (characterLight2D != null)
        {
            characterLight2D.enabled = false;
        }

        if (doorSpotlight != null)
        {
            doorSpotlight.enabled = true;
        }

        if (doorAudioSource != null && spotlightClip != null)
        {
            doorAudioSource.clip = spotlightClip;
            doorAudioSource.Play();
        }

        yield return new WaitForSeconds(0.7f);

        AudioSource footstepSource = null;
        if (environmentSoundHandler != null)
        {
            footstepSource = environmentSoundHandler.CreateCustomSource("BloodFootsteps");
            if (footstepSource != null)
            {
                footstepSource.volume = 0.8f;
            }
        }

        if (bloodFootsteps != null && footstepSource != null && bloodFootstepsClips.Count > 0)
        {
            foreach (Transform footstepObj in bloodFootsteps.transform)
            {
                var footstepRenderer = footstepObj.GetComponent<SpriteRenderer>();
                if (footstepRenderer != null)
                {
                    var newColor = footstepRenderer.color;
                    newColor.a = 1;
                    footstepRenderer.color = newColor;

                    var randomFootstepSfx = bloodFootstepsClips[Random.Range(0, bloodFootstepsClips.Count)];
                    footstepSource.clip = randomFootstepSfx;
                    footstepSource.Play();
                }

                yield return new WaitForSeconds(0.7f);
            }
        }

        if (doorAudioSource != null && bigDoorOpenClip != null)
        {
            doorAudioSource.clip = bigDoorOpenClip;
            doorAudioSource.Play();
        }

        GameObject playerObj = _player != null ? _player.gameObject : GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerInput = playerObj.GetComponent<PlayerInput2D>();
            if (playerInput != null)
            {
                playerInput.isInputEnabled = true;
            }
        }

        var tempBlockerName = step.doorObjectName.Replace("Door", "Block");
        var tempBlocker = GameObject.Find(tempBlockerName);
        if (tempBlocker != null)
        {
            Destroy(tempBlocker);
        }

        yield return new WaitForSeconds(0.5f);

        if (footstepSource != null)
        {
            Destroy(footstepSource);
        }

        foreach (Transform doorPart in door.transform)
        {
            var animator = doorPart.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(openHash);
            }
        }
    }
}
