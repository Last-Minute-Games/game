using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using cherrydev;

public class CatacombsDevilDialogueTrigger : MonoBehaviour
{
    private const string SceneName = "Catacombs";

    private readonly struct DialogueStep
    {
        public readonly float TriggerY;
        public readonly string FlagName;
        public readonly string DialogResourcePath;

        public DialogueStep(float triggerY, string flagName, string dialogResourcePath)
        {
            TriggerY = triggerY;
            FlagName = flagName;
            DialogResourcePath = dialogResourcePath;
        }
    }

    private static readonly DialogueStep[] Steps =
    {
        new DialogueStep(0f, "catacombs.devil.dialog.shown", "Dialogues/Monologues/CatacombsDevilWelcome"),
        new DialogueStep(13f, "catacombs.devil.king.dead.shown", "Dialogues/Monologues/CatacombsDevilKingDead"),
        new DialogueStep(26f, "catacombs.devil.fault.shown", "Dialogues/Monologues/CatacombsDevilFault"),
        new DialogueStep(38f, "catacombs.devil.survive.shown", "Dialogues/Monologues/CatacombsDevilSurvive"),
    };

    private static bool s_created;

    private bool _isWatching;
    private bool _isDialogPlaying;
    private bool _warnedMissingDialog;
    private Transform _player;
    private DialogBehaviour _dialogBehaviour;
    private int _currentStepIndex = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (s_created)
        {
            return;
        }

        s_created = true;
        GameObject runner = new GameObject("CatacombsDevilDialogueTrigger");
        DontDestroyOnLoad(runner);
        runner.AddComponent<CatacombsDevilDialogueTrigger>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneName)
        {
            _isWatching = true;
            _isDialogPlaying = false;
            _warnedMissingDialog = false;
            _player = null;
            _dialogBehaviour = null;
            _currentStepIndex = FindNextStepIndex();
        }
        else
        {
            _isWatching = false;
        }
    }

    private void Update()
    {
        if (!_isWatching || _isDialogPlaying)
        {
            return;
        }

        if (_currentStepIndex < 0 || _currentStepIndex >= Steps.Length)
        {
            return;
        }

        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                return;
            }

            _player = playerObj.transform;
        }

        DialogueStep step = Steps[_currentStepIndex];
        if (_player.position.y < step.TriggerY)
        {
            return;
        }

        if (_dialogBehaviour == null)
        {
            _dialogBehaviour = Object.FindObjectOfType<DialogBehaviour>(true);
            if (_dialogBehaviour == null)
            {
                if (!_warnedMissingDialog)
                {
                    Debug.LogWarning("[CatacombsDevilDialogueTrigger] DialogBehaviour not found in scene. Add a dialog UI prefab to Catacombs.");
                    _warnedMissingDialog = true;
                }

                return;
            }
        }

        TriggerDialogue(step);
    }

    private int FindNextStepIndex()
    {
        for (int i = 0; i < Steps.Length; i++)
        {
            if (!GameFlags.HasFlag(Steps[i].FlagName))
            {
                return i;
            }
        }

        return -1;
    }

    private void TriggerDialogue(DialogueStep step)
    {
        _isDialogPlaying = true;

        DialogNodeGraph dialogGraph = Resources.Load<DialogNodeGraph>(step.DialogResourcePath);
        if (dialogGraph == null)
        {
            Debug.LogWarning($"[CatacombsDevilDialogueTrigger] Dialog graph not found at Resources/{step.DialogResourcePath}.");
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
            if (_dialogBehaviour != null)
            {
                _dialogBehaviour.OnDialogFinished.RemoveListener(onFinished);
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

        _dialogBehaviour.OnDialogFinished.AddListener(onFinished);
        GameFlags.SetFlag(step.FlagName);
        _dialogBehaviour.StartDialog(dialogGraph);
    }
}
