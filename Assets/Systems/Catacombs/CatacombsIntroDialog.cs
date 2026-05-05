using UnityEngine;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
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

    [Header("Screen Transition")]
    [Tooltip("IMPORTANT: Must be TRUE for eyes to open when scene loads!")]
    [SerializeField] private bool openEyesOnStart = true;
    [Tooltip("Delay before opening eyes (seconds)")]
    [SerializeField] private float eyeOpenDelay = 0.5f;

    private bool hasPlayed = false;
    private PlayerInput2D playerInput;
    private CharacterMotor2D motor;

    private void Start()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        Debug.Log("[CatacombsIntroDialog] ===== START SEQUENCE BEGIN =====");

        // FORCE EYE OPENING - ALWAYS RUN THIS NO MATTER WHAT
        var fader = ScreenFader.Instance;
        Debug.Log($"[CatacombsIntroDialog] ScreenFader.Instance = {(fader != null ? "FOUND" : "NULL")}");

        if (fader == null)
        {
            Debug.LogError("[CatacombsIntroDialog] CRITICAL: ScreenFader.Instance is NULL! Cannot open eyes!");
            Debug.LogError("[CatacombsIntroDialog] Trying to find ScreenFader in scene...");
            fader = FindObjectOfType<ScreenFader>();
            Debug.Log($"[CatacombsIntroDialog] FindObjectOfType result: {(fader != null ? "FOUND" : "STILL NULL")}");

            if (fader == null)
            {
                Debug.LogError("[CatacombsIntroDialog] Creating new ScreenFader GameObject...");

                // Create a new ScreenFader GameObject
                GameObject faderObj = new GameObject("ScreenFader");
                fader = faderObj.AddComponent<ScreenFader>();

                // Create a Canvas for the fade panel
                GameObject canvasObj = new GameObject("ScreenFader Canvas");
                canvasObj.transform.SetParent(faderObj.transform);
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // Very high to be on top

                UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                // Create fade panel
                GameObject panelObj = new GameObject("FadeOverlay");
                panelObj.transform.SetParent(canvasObj.transform, false);

                RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;

                UnityEngine.UI.Image image = panelObj.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(0, 0, 0, 1); // Start with black, fully opaque

                // Assign to ScreenFader
                fader.fadePanel = image;
                fader.fadeDuration = 2f;
                fader.splitPanelDuration = 1.5f;

                Debug.Log("[CatacombsIntroDialog] ScreenFader created successfully!");

                // Give it one frame to initialize
                yield return null;
            }
        }

        if (fader != null)
        {
            Debug.Log("[CatacombsIntroDialog] ScreenFader found, checking eye panels...");

            // Check if panels exist and are closed
            bool panelsClosed = fader.ArePanelsClosed();

            Debug.Log($"[CatacombsIntroDialog] ArePanelsClosed returned: {panelsClosed}");

            if (!panelsClosed)
            {
                // Panels don't exist - need to create them first, then open them
                Debug.Log("[CatacombsIntroDialog] Eye panels don't exist or aren't closed - creating and closing them first");

                // Create panels by calling EyesClosingEffect (which creates them)
                // But we'll skip the animation by setting duration to 0 temporarily
                float originalDuration = fader.splitPanelDuration;
                fader.splitPanelDuration = 0.01f; // Near-instant close

                Debug.Log("[CatacombsIntroDialog] Starting instant EyesClosingEffect...");
                yield return fader.EyesClosingEffect();
                Debug.Log("[CatacombsIntroDialog] Instant EyesClosingEffect complete");

                // Restore original duration for opening
                fader.splitPanelDuration = originalDuration;

                Debug.Log("[CatacombsIntroDialog] Panels created and closed, restored duration");
            }
            else
            {
                Debug.Log("[CatacombsIntroDialog] Eyes are closed (panels exist) - will open them");
            }

            // Small delay for scene to settle
            if (eyeOpenDelay > 0)
            {
                Debug.Log($"[CatacombsIntroDialog] Waiting {eyeOpenDelay}s before opening eyes...");
                yield return new WaitForSeconds(eyeOpenDelay);
            }

            // Now open the eyes
            Debug.Log("[CatacombsIntroDialog] Starting EyesOpeningEffect...");
            yield return fader.EyesOpeningEffect();
            Debug.Log("[CatacombsIntroDialog] Eyes opened!");
        }
        else
        {
            Debug.LogError("[CatacombsIntroDialog] FAILED TO CREATE SCREENFADER - CANNOT OPEN EYES!");
        }

        Debug.Log("[CatacombsIntroDialog] Eye opening complete, proceeding to dialog check...");

        // Then check if we should play the intro dialog
        if (hasPlayed)
        {
            Debug.Log("[CatacombsIntroDialog] hasPlayed=true, exiting");
            yield break;
        }

        if (GameFlags.HasFlag(FlagName))
        {
            Debug.Log($"[CatacombsIntroDialog] Flag '{FlagName}' already set, exiting");
            hasPlayed = true;
            yield break;
        }

        Debug.Log("[CatacombsIntroDialog] Proceeding to play intro dialog...");

        if (autoFindDialogBehaviour && dialogBehaviour == null)
        {
            dialogBehaviour = FindObjectOfType<DialogBehaviour>(true);
        }

        if (dialogBehaviour == null)
        {
            Debug.LogWarning("[CatacombsIntroDialog] DialogBehaviour not found. Please assign it in the inspector or enable auto-find.");
            yield break;
        }

        if (introDialogGraph == null)
        {
            introDialogGraph = Resources.Load<DialogNodeGraph>("Dialogues/Monologues/CatacombsIntro");
            if (introDialogGraph == null)
            {
                Debug.LogWarning("[CatacombsIntroDialog] Dialog graph not found. Please assign it in the inspector.");
                yield break;
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
