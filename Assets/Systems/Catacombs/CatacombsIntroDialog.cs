using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using cherrydev;

public static class CatacombsIntroDialog
{
    private const string SceneName = "Catacombs";
    private const string FlagName = "catacombs.intro.dialog.shown";
    private const string DialogResourcePath = "Dialogues/Monologues/CatacombsIntro";

    private static bool _subscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryPlay(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryPlay(scene);
    }

    private static void TryPlay(Scene scene)
    {
        if (scene.name != SceneName)
        {
            return;
        }

        if (GameFlags.HasFlag(FlagName))
        {
            return;
        }

        DialogNodeGraph dialogGraph = Resources.Load<DialogNodeGraph>(DialogResourcePath);
        if (dialogGraph == null)
        {
            Debug.LogWarning($"[CatacombsIntroDialog] Dialog graph not found at Resources/{DialogResourcePath}.");
            return;
        }

        DialogBehaviour dialogBehaviour = Object.FindObjectOfType<DialogBehaviour>(true);
        if (dialogBehaviour == null)
        {
            Debug.LogWarning("[CatacombsIntroDialog] DialogBehaviour not found in scene. Add a dialog UI prefab to Catacombs.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerInput2D playerInput = null;
        CharacterMotor2D motor = null;
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
        dialogBehaviour.StartDialog(dialogGraph);
    }
}
