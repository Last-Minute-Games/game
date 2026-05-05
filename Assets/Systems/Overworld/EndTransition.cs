using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public ScreenFader screenFader;
    public string endingSceneName = "ending";
    
    [Header("Timing")]
    public float delayBeforeTransition = 1f;
    
    [Header("Flag Settings")]
    [Tooltip("The flag that must exist to trigger the transition")]
    public string requiredFlagName = "start.ending";
    
    // Cache references to avoid repeated FindObjectOfType calls
    private ClockTimer _clockTimer;
    private JournalUI _journalUI;
    private bool _transitionStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Try to find ScreenFader if not assigned
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader == null)
            {
                Debug.LogWarning("[EndTransition] ScreenFader not found in scene. Transition will use direct load.");
            }
        }
        
        // Cache references once at start
        _clockTimer = FindObjectOfType<ClockTimer>();
        _journalUI = FindObjectOfType<JournalUI>();
    }

    /// <summary>
    /// Trigger the transition to the ending scene (only if the required flag exists)
    /// </summary>
    public void TriggerEndTransition()
    {
        // Check if the required flag exists
        if (!GameFlags.HasFlag(requiredFlagName))
        {
            Debug.Log($"[EndTransition] Flag '{requiredFlagName}' does not exist - transition cancelled");
            return;
        }

        // Check if transition already started
        if (_transitionStarted)
        {
            Debug.Log($"[EndTransition] Transition already in progress - ignoring duplicate call");
            return;
        }

        Debug.Log($"[EndTransition] Flag '{requiredFlagName}' exists - starting transition");
        _transitionStarted = true;
        StartCoroutine(TransitionToEnding());
    }

    private IEnumerator TransitionToEnding()
    {
        Debug.Log("[EndTransition] Starting transition to ending scene");

        // Pause player input and environment
        PauseEnvironmentAndPlayer(true);

        // Optional delay before transition starts
        if (delayBeforeTransition > 0f)
        {
            yield return new WaitForSeconds(delayBeforeTransition);
        }

        // Try to find ScreenFader if we lost the reference
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader != null)
            {
                Debug.Log("[EndTransition] Found ScreenFader during transition");
            }
        }

        // Check if eyes are already closed - if so, skip the closing effect
        bool eyesAlreadyClosed = false;
        if (screenFader != null)
        {
            // Check if split panels exist and are in closed position
            // This would indicate eyes are already closed from ClockTimer
            eyesAlreadyClosed = screenFader.ArePanelsClosed();
        }

        // Do the eyes closing effect (only if not already closed)
        if (screenFader != null && !eyesAlreadyClosed)
        {
            Debug.Log("[EndTransition] Eyes not yet closed - performing eyes closing effect");
            yield return StartCoroutine(screenFader.EyesClosingEffect());
        }
        else if (eyesAlreadyClosed)
        {
            Debug.Log("[EndTransition] Eyes already closed - skipping closing effect");
        }
        else
        {
            Debug.LogWarning("[EndTransition] No ScreenFader available, skipping eyes closing effect");
        }

        // Transition to the ending scene - KEEP PANELS CLOSED
        if (string.IsNullOrEmpty(endingSceneName))
        {
            Debug.LogError("[EndTransition] Ending scene name is empty or null - cannot transition.");
            PauseEnvironmentAndPlayer(false); // Restore if we fail
            yield break;
        }

        Debug.Log($"[EndTransition] Preparing transition to scene '{endingSceneName}'. ScreenFader assigned: {screenFader != null}");

        if (screenFader != null)
        {
            // Use ScreenFader transition coroutine
            screenFader.shouldOpenEyesOnSceneLoad = true;
            Debug.Log($"[EndTransition] Calling ScreenFader.TransitionToSceneKeepPanelsClosed('{endingSceneName}')");
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(endingSceneName));
            Debug.Log($"[EndTransition] Returned from ScreenFader.TransitionToSceneKeepPanelsClosed('{endingSceneName}')");

            Debug.Log($"[EndTransition] Current active scene after ScreenFader call: {SceneManager.GetActiveScene().name} (expected: {endingSceneName})");
        }
        else
        {
            Debug.LogWarning("[EndTransition] screenFader is null - attempting direct async load of ending scene.");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(endingSceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[EndTransition] SceneManager.LoadSceneAsync returned null for '{endingSceneName}'. Make sure the scene is added to Build Settings.");
                PauseEnvironmentAndPlayer(false); // Restore if we fail
                yield break;
            }

            Debug.Log($"[EndTransition] Started direct async load for '{endingSceneName}'. allowSceneActivation={asyncLoad.allowSceneActivation}");
            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone)
                yield return null;
            Debug.Log($"[EndTransition] Direct async load finished for '{endingSceneName}'. Active scene is now: {SceneManager.GetActiveScene().name}");
        }
    }

    /// <summary>
    /// Pause or unpause the player and environment (clock timer, player input, etc.)
    /// Uses cached references to avoid repeated FindObjectOfType calls
    /// </summary>
    private void PauseEnvironmentAndPlayer(bool pause)
    {
        Debug.Log($"[EndTransition] {(pause ? "Pausing" : "Unpausing")} environment and player");

        // Find and pause/unpause player input
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInput2D playerInput = player.GetComponent<PlayerInput2D>();
            if (playerInput != null)
            {
                playerInput.isInputEnabled = !pause;
                Debug.Log($"[EndTransition] Player input enabled: {playerInput.isInputEnabled}");
            }

            CharacterMotor2D motor = player.GetComponent<CharacterMotor2D>();
            if (motor != null)
            {
                motor.SetDialogueActive(pause);
                Debug.Log($"[EndTransition] Player motor dialogue state: {pause}");
            }
        }

        // Use cached clock timer reference
        if (_clockTimer != null)
        {
            _clockTimer.PauseTimer(pause);
            Debug.Log($"[EndTransition] Clock timer paused: {pause}");
        }

        // Use cached journal UI reference
        if (_journalUI != null)
        {
            _journalUI.SetInputEnabled(!pause);
            Debug.Log($"[EndTransition] Journal input enabled: {!pause}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Continuously check if the flag is set and trigger transition if not already started
        if (!_transitionStarted && GameFlags.HasFlag(requiredFlagName))
        {
            Debug.Log($"[EndTransition] Flag '{requiredFlagName}' detected - auto-triggering transition");
            _transitionStarted = true;
            StartCoroutine(TransitionToEnding());
        }
    }
}
