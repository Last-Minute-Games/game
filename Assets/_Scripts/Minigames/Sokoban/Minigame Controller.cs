using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The central manager for the Sokoban minigame within the Overworld scene.
/// Handles the activation/deactivation of the puzzle components and swaps player control.
/// </summary>
public class MinigameController : MonoBehaviour
{
    // Make this class accessible globally for scripts like WinConditionManager and Activator
    public static MinigameController Instance { get; private set; }

    [Header("Puzzle Components")]
    [Tooltip("The parent GameObject containing all walls, boxes, and goals.")]
    public GameObject sokobanRoot;
    [Tooltip("The starting X and Y offset for the player when the minigame starts.")]
    public Vector2 playerStartPositionOffset = new Vector2(0f, 0f);

    [Header("Player Control")]
    [Tooltip("The movement script for the Sokoban game (enable this, disable Overworld).")]
    public MonoBehaviour sokobanPlayerScript;
    [Tooltip("The main movement script for the Overworld (disable this, enable Overworld).")]
    public MonoBehaviour overworldPlayerScript;

    [Header("Visual Swap")]
    [Tooltip("The sprite to use when the player is inside the Sokoban minigame.")]
    public Sprite sokobanPlayerSprite;
    private SpriteRenderer playerSpriteRenderer;
    private Sprite overworldPlayerSprite; // Stores the original sprite

    [Header("Return Position")]
    [Tooltip("The position where the player should return to in the Overworld. Set dynamically by SokobanActivator.")]
    // This is now set by the Activator, but we keep it public for access.
    public Vector3 overworldExitPosition;

    [Header("UI")]
    [SerializeField] GameObject hudRoot;
    private CanvasGroup hudCanvasGroup;
    private bool hudWasActive;

    [Header("Fade Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    private Coroutine startRoutine;
    private Coroutine endRoutine;

    [Header("Transition Messaging")]
    [SerializeField] private TMP_Text transitionMessageText;
    [SerializeField] private int transitionFontSize = 64;
    [SerializeField] private string enteringMessage = "ENTERING SOKOBAN";
    [SerializeField] private string exitingMessage = "EXITING SOKOBAN";
    [SerializeField] private string winMessage = "YOU WON";
    [SerializeField] private Color enteringColor = Color.white;
    [SerializeField] private Color exitingColor = new Color(0.85f, 0.2f, 0.2f);
    [SerializeField] private Color winColor = new Color(0.2f, 0.75f, 0.2f);

    // References for internal logic
    private GameObject player;
    private WinConditionManager winManager;

    void Awake()
    {
        // Singleton pattern: Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("Player GameObject not found! Check 'Player' tag."); }

        // Get Player SpriteRenderer and store original sprite
        if (player != null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                overworldPlayerSprite = playerSpriteRenderer.sprite;
            }
        }

        winManager = sokobanRoot.GetComponentInChildren<WinConditionManager>();
        if (winManager == null) { Debug.LogError("WinConditionManager not found inside the Sokoban Root."); }

        if (hudRoot != null)
        {
            hudCanvasGroup = hudRoot.GetComponent<CanvasGroup>();
            hudWasActive = hudRoot.activeSelf;
        }

        EnsureFadeCanvasGroup();

        // --- FIX: SET INITIAL STATE (REQUIRED FOR IN-SCENE MINIGAMES) ---
        if (sokobanRoot != null)
        {
            sokobanRoot.SetActive(false); // Hide the puzzle
        }

        if (overworldPlayerScript != null && sokobanPlayerScript != null)
        {
            overworldPlayerScript.enabled = true; // Start with Overworld movement ON
            sokobanPlayerScript.enabled = false;  // Start with Sokoban movement OFF
        }
    }

    /// <summary>
    /// Called by the InteractiveSokobanActivator when the player interacts with the entrance.
    /// </summary>
    public void StartSokoban()
    {
        if (startRoutine != null || endRoutine != null)
        {
            return;
        }

        startRoutine = StartCoroutine(StartSokobanRoutine());
    }

    /// <summary>
    /// Called by the WinConditionManager or the Quit Button.
    /// </summary>
    ///
    public void EndSokoban(bool solved)
    {
        if (endRoutine != null || startRoutine != null)
        {
            return;
        }

        if (player == null || sokobanRoot == null) return;

        endRoutine = StartCoroutine(EndSokobanRoutine(solved));
    }

    /// <summary>
    /// Resets the positions of the player and all boxes to their starting locations.
    /// Called by the Reset UI Button.
    /// </summary>
    public void ResetPuzzle()
    {
        // 1. Reset all boxes and the player (if they have the InitialPosition script)
        InitialPosition[] allResettableObjects = FindObjectsOfType<InitialPosition>();
        foreach (InitialPosition resettable in allResettableObjects)
        {
            resettable.ResetPosition();
        }

        // 2. Reset the goal counter on the WinConditionManager
        winManager?.ForceResetGoals();

        foreach (Goal g in FindObjectsOfType<Goal>())
            g.ResetVisual();   // calls UpdateVisual(false)

        Debug.Log("Puzzle reset complete.");
    }

    private IEnumerator StartSokobanRoutine()
    {
        if (player == null || sokobanRoot == null)
        {
            Debug.LogError("Cannot start Sokoban: missing references.");
            startRoutine = null;
            yield break;
        }

        ShowTransitionMessage(enteringMessage, enteringColor);
        yield return FadeToBlack();
        ActivateSokobanGameplay();
        yield return FadeFromBlack();
        HideTransitionMessage();

        Debug.Log("Sokoban Minigame started.");
        startRoutine = null;
    }

    private IEnumerator EndSokobanRoutine(bool solved)
    {
        string messageToShow = solved ? winMessage : exitingMessage;
        Color colorToUse = solved ? winColor : exitingColor;

        ShowTransitionMessage(messageToShow, colorToUse);
        yield return FadeToBlack();
        DeactivateSokobanGameplay(solved);
        yield return FadeFromBlack();
        ShowHUD();
        HideTransitionMessage();

        endRoutine = null;
    }

    private void ActivateSokobanGameplay()
    {
        HideHUD();

        // Swap Player Controls: Disable Overworld, Enable Sokoban
        overworldPlayerScript.enabled = false;
        sokobanPlayerScript.enabled = true;

        // Teleport Player to the Puzzle Start Position (with rounding for grid alignment)
        Vector3 targetPos = sokobanRoot.transform.position + new Vector3(playerStartPositionOffset.x, playerStartPositionOffset.y, 0f);
        player.transform.position = new Vector3(
            Mathf.Round(targetPos.x),
            Mathf.Round(targetPos.y),
            targetPos.z
        );

        // Sprite swap
        if (playerSpriteRenderer != null && sokobanPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = sokobanPlayerSprite;
        }

        // Activate the Puzzle objects
        sokobanRoot.SetActive(true);
    }

    private void DeactivateSokobanGameplay(bool solved)
    {
        ClockTimer clock = FindObjectOfType<ClockTimer>();
        if (clock != null)
        {
            clock.PauseTimer(false);
        }

        // Hide puzzle pieces
        sokobanRoot.SetActive(false);

        // Swap Player Controls: Enable Overworld, Disable Sokoban
        sokobanPlayerScript.enabled = false;
        overworldPlayerScript.enabled = true;

        // Sprite swap back
        if (playerSpriteRenderer != null && overworldPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = overworldPlayerSprite;
        }

        // Teleport Player back to the Overworld Exit Position (set dynamically by Activator)
        player.transform.position = overworldExitPosition;

        // Snap camera to player position
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                Camera.main.transform.position.z
            );
        }

        GameFlags.SetFlag("minigame.sokoban.finish");

        Debug.Log($"Sokoban Minigame finished. Solved: {solved}");
    }

    private void HideHUD()
    {
        if (hudRoot == null) return;

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 0f;
            hudCanvasGroup.interactable = false;
            hudCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            hudWasActive = hudRoot.activeSelf;
            hudRoot.SetActive(false);
        }
    }

    private void ShowHUD()
    {
        if (hudRoot == null) return;

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 1f;
            hudCanvasGroup.interactable = true;
            hudCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            hudRoot.SetActive(hudWasActive);
        }
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        if (!fadeCanvasGroup.gameObject.activeSelf)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
        }

        fadeCanvasGroup.blocksRaycasts = true;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    private void EnsureTransitionMessageText()
    {
        if (fadeCanvasGroup == null)
        {
            transitionMessageText = null;
            return;
        }

        if (transitionMessageText == null)
        {
            Transform existing = fadeCanvasGroup.transform.Find("SokobanTransitionMessage");
            if (existing != null)
            {
                transitionMessageText = existing.GetComponent<TMP_Text>();
            }
        }

        if (transitionMessageText == null)
        {
            GameObject textObj = new GameObject("SokobanTransitionMessage");
            textObj.transform.SetParent(fadeCanvasGroup.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            transitionMessageText = tmp;
        if (transitionMessageText.font == null && TMP_Settings.defaultFontAsset != null)
        {
            transitionMessageText.font = TMP_Settings.defaultFontAsset;
        }
        }

        RectTransform rect = transitionMessageText.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        transitionMessageText.alignment = TextAlignmentOptions.Center;
        transitionMessageText.fontSize = transitionFontSize;
        transitionMessageText.enableWordWrapping = false;
        transitionMessageText.raycastTarget = false;
        transitionMessageText.text = string.Empty;
        transitionMessageText.alpha = 0f;
        transitionMessageText.gameObject.SetActive(false);
    }

    private void EnsureFadeCanvasGroup()
    {
        if (fadeCanvasGroup == null)
        {
            GameObject fadeObj = GameObject.Find("FadeCanvasGroup");
            if (fadeObj != null)
            {
                fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
                if (fadeCanvasGroup == null)
                {
                    fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
                }
            }
        }

        if (fadeCanvasGroup == null)
        {
            GameObject canvasObj = new GameObject("SokobanFadeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);

        EnsureTransitionMessageText();
    }

    private void ShowTransitionMessage(string message, Color color)
    {
        if (transitionMessageText == null)
        {
            Debug.Log($"[MinigameController] Transition: {message}");
            return;
        }

        transitionMessageText.fontSize = transitionFontSize;
        transitionMessageText.text = message;
        transitionMessageText.color = color;
        transitionMessageText.alpha = 1f;
        transitionMessageText.gameObject.SetActive(true);
    }

    private void HideTransitionMessage()
    {
        if (transitionMessageText == null)
        {
            return;
        }

        transitionMessageText.alpha = 0f;
        transitionMessageText.text = string.Empty;
        transitionMessageText.gameObject.SetActive(false);
    }
}
