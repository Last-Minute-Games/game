using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;

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

    [Header("Camera")]
    [SerializeField] private Transform sokobanCameraAnchor; // drag the anchor here in Inspector
    [SerializeField] private float sokobanCameraSize = 8f;  // set this to the size that shows whole room

    private float overworldCameraSize;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cmCamera;   // drag your CinemachineCamera here

    private Transform overworldFollowTarget;
    private float overworldOrthoSize;


    [Header("UI")]
    [SerializeField] GameObject hudRoot;
    private CanvasGroup hudCanvasGroup;
    private bool hudWasActive;

    [Header("Transition")]
    [Tooltip("CanvasGroup used to fade the screen when entering/exiting the minigame.")]
    [SerializeField] CanvasGroup transitionCanvasGroup;
    [Tooltip("Optional text element that displays the current transition message.")]
    [SerializeField] TMP_Text transitionStatusText;
    [Tooltip("How long (in seconds) the screen stays fully faded while we reposition objects.")]
    [SerializeField] float transitionCoveredDuration = 1.2f;
    [Tooltip("Fade-in duration in seconds.")]
    [SerializeField] float transitionFadeInDuration = 0.4f;
    [Tooltip("Fade-out duration in seconds.")]
    [SerializeField] float transitionFadeOutDuration = 0.4f;

    [Header("Objects to hide after Sokoban is completed")]
    [SerializeField] private GameObject[] sokobanShowFlags;

    [Header("Instructions")]
    [SerializeField] private MinigameInstructions sokobanInstructions;


    private Coroutine transitionRoutine;
    private bool isTransitionRunning;

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

        if (Camera.main != null)
        {
            overworldCameraSize = Camera.main.orthographicSize;
        }

        if (cmCamera != null)
        {
            overworldFollowTarget = cmCamera.Follow;
            overworldOrthoSize = cmCamera.Lens.OrthographicSize;
        }

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
    /// Immediately stops all player actions, then transitions and changes sprite after fade.
    /// </summary>
    public void StartSokoban()
    {
        if (player == null || sokobanRoot == null) return;
        
        // Immediately stop all player movement and actions
        if (overworldPlayerScript != null)
        {
            overworldPlayerScript.enabled = false;
        }
        
        // Immediately stop any physics-based movement (if using Rigidbody2D)
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        // Immediately stop all animations by disabling the Animator
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }
        
        // Sprite change happens during transition (after fade) in PerformSokobanStart
        // Now run the transition (which will complete the setup in PerformSokobanStart)
        RunTransition("ENTERING SOKOBAN", PerformSokobanStart);
    }

    /// <summary>
    /// Called by the WinConditionManager or the Quit Button.
    /// </summary>
    ///
    public void EndSokoban(bool solved)
    {
        if (player == null || sokobanRoot == null) return;
        RunTransition(solved ? "YOU WIN" : "EXITING SOKOBAN", () => PerformSokobanEnd(solved));
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

    private void RunTransition(string message, System.Action midAction)
    {
        if (isTransitionRunning)
        {
            return;
        }

        if (transitionCanvasGroup == null)
        {
            midAction?.Invoke();
            return;
        }

        transitionRoutine = StartCoroutine(TransitionRoutine(message, midAction));
    }

    private IEnumerator TransitionRoutine(string message, System.Action midAction)
    {
        isTransitionRunning = true;

        if (transitionStatusText != null)
        {
            transitionStatusText.text = message;
        }

        GameObject transitionObject = transitionCanvasGroup.gameObject;
        if (!transitionObject.activeSelf)
        {
            transitionObject.SetActive(true);
        }

        transitionCanvasGroup.blocksRaycasts = true;
        transitionCanvasGroup.interactable = true;

        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 1f, transitionFadeInDuration);

        midAction?.Invoke();

        if (transitionCoveredDuration > 0f)
        {
            yield return new WaitForSeconds(transitionCoveredDuration);
        }

        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 0f, transitionFadeOutDuration);

        transitionCanvasGroup.blocksRaycasts = false;
        transitionCanvasGroup.interactable = false;
        transitionObject.SetActive(false);

        transitionRoutine = null;
        isTransitionRunning = false;
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (duration <= 0f)
        {
            transitionCanvasGroup.alpha = end;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transitionCanvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        transitionCanvasGroup.alpha = end;
    }

    private void PerformSokobanStart()
    {
        HideHUD();

        // Tell Cinemachine to lock on the Sokoban room
        if (cmCamera != null)
        {
            if (sokobanCameraAnchor != null)
            {
                cmCamera.Follow = sokobanCameraAnchor;  // stop following player, follow anchor instead
            }

            cmCamera.Lens.OrthographicSize = sokobanCameraSize; // zoom out to see whole room
        }


        // Pause NPCs and timer, but NOT player input (using minigame pause)
        GlobalPause.SetMinigamePaused(true);

        overworldPlayerScript.enabled = false;
        sokobanPlayerScript.enabled = true;

        Vector3 targetPos = sokobanRoot.transform.position + new Vector3(playerStartPositionOffset.x, playerStartPositionOffset.y, 0f);
        player.transform.position = new Vector3(
            Mathf.Round(targetPos.x),
            Mathf.Round(targetPos.y),
            targetPos.z
        );

        if (playerSpriteRenderer != null && sokobanPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = sokobanPlayerSprite;
        }

        sokobanRoot.SetActive(true);

        Debug.Log("Sokoban Minigame started.");

        if (sokobanInstructions == null)
        {
            // Try to auto-find on children of the Sokoban root
            sokobanInstructions = sokobanRoot.GetComponentInChildren<MinigameInstructions>(true);
        }
        if (sokobanInstructions != null)
        {
            sokobanInstructions.OnPopupOpened();
        }
    }

    private void PerformSokobanEnd(bool solved)
    {
        ///GameFlags.RemoveFlag("InMinigame"); //somehting about line 77 in GAmeFlags.cs file

        // Resume NPCs and timer (using minigame pause)
        GlobalPause.SetMinigamePaused(false);

        sokobanRoot.SetActive(false);

        sokobanPlayerScript.enabled = false;
        overworldPlayerScript.enabled = true;

        // Re-enable the Animator when returning to overworld
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        if (playerSpriteRenderer != null && overworldPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = overworldPlayerSprite;
        }

        player.transform.position = overworldExitPosition;

        // Put Cinemachine back to overworld mode
        if (cmCamera != null)
        {
            cmCamera.Follow = overworldFollowTarget;
            cmCamera.Lens.OrthographicSize = overworldOrthoSize;
        }

        

        ShowHUD();

        if (solved)
        {
            // Turn off all the "show" markers for this minigame
            if (sokobanShowFlags != null)
            {
                foreach (GameObject flagObj in sokobanShowFlags)
                {
                    if (flagObj != null)
                    {
                        flagObj.SetActive(false);
                    }
                }
            }

            GameFlags.SetFlag("minigame.sokoban.finish");
        }

        Debug.Log($"Sokoban Minigame finished. Solved: {solved}");
    }
}
