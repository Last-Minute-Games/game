using System.Collections;
using TMPro;
using UnityEngine;

public class OverworldCoinGameLauncher : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("Assign in Inspector")]
    [Tooltip("Direct reference to the coinflip popup GameObject in the scene.")]
    public GameObject coinFlipPopup;
    public MonoBehaviour[] controlsToDisable;

    [Header("HUD (optional)")]
    public GameObject hudGroup;

    [Header("Interaction")]
    [Tooltip("Maximum distance from the player to trigger the coinflip minigame.")]
    public float interactDistance = 2.5f;
    
    [Tooltip("Interaction priority (lower = higher priority). Teleports=0, Dialogs=1-2, Minigames=5")]
    [SerializeField] private int interactionPriority = 5;

    [Header("Protection")]
    public float sceneOpenDelay = 0.35f; // block instant open after load/room swap
    public float reopenCooldown = 0.25f; // block double taps

    [Header("Transition (Sokoban-style fade)")]
    [Tooltip("CanvasGroup used to fade the screen when entering/exiting. Assign a full-screen black panel with CanvasGroup.")]
    [SerializeField] CanvasGroup transitionCanvasGroup;
    [Tooltip("Optional text element that displays the current transition message.")]
    [SerializeField] TMP_Text transitionStatusText;
    [Tooltip("How long (in seconds) the screen stays fully faded while we swap UI.")]
    [SerializeField] float transitionCoveredDuration = 0.5f;
    [SerializeField] float transitionFadeInDuration = 0.4f;
    [SerializeField] float transitionFadeOutDuration = 0.4f;

    private Coroutine _transitionRoutine;
    private bool _isTransitionRunning;

    public MinigameInstructions coinFlipInstructions;

    private bool _canOpen = false;
    private float _lastCloseTime = -999f;
    private GameObject _player;
    private bool _isPlayerNear = false;

    void OnEnable()
    {
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay));
        // If you use the old Input Manager, this clears "stuck" inputs across scene loads:
        Input.ResetInputAxes();
    }

    void Start()
    {
        // Find player automatically
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogWarning($"[OverworldCoinGameLauncher] {name}: Player not found!");
        }
        
        // Ensure we have a trigger collider
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"[OverworldCoinGameLauncher] {name}: No BoxCollider2D found! Add one as a trigger for interaction to work.");
        }

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator EnableOpenAfterDelay(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        _canOpen = true;
        LogDebug("Can now open coinflip popup");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNear = true;
            LogDebug($"Player entered range of {name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNear = false;
            LogDebug($"Player left range of {name}");
        }
    }

    void Update()
    {
        // Update player near status (fallback if trigger events don't work)
        if (_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            _isPlayerNear = distance <= interactDistance;
        }
    }

    // IInteractable Implementation
    public void Interact()
    {
        LogDebug("Interact() called");
        
        // Try to acquire the interaction lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            LogDebug("Cannot interact - interaction lock is held");
            return;
        }
        
        OpenCoinFlipPopup();
    }

    public int GetInteractionPriority()
    {
        return interactionPriority;
    }

    public bool CanInteract()
    {
        // Can interact if player is near and conditions are met
        if (!_isPlayerNear)
        {
            LogDebug("Cannot interact - player not in range");
            return false;
        }
        
        if (Systems.InteractionLockManager.IsLocked)
        {
            LogDebug("Cannot interact - interaction locked");
            return false;
        }
        
        if (coinFlipPopup == null)
        {
            LogDebug("Cannot interact - coin flip popup not assigned");
            return false;
        }
        
        if (coinFlipPopup.activeSelf)
        {
            LogDebug("Cannot interact - popup already active");
            return false;
        }
        
        if (!_canOpen)
        {
            LogDebug("Cannot interact - cooldown active");
            return false;
        }
        
        if (Time.unscaledTime - _lastCloseTime < reopenCooldown)
        {
            LogDebug("Cannot interact - reopen cooldown active");
            return false;
        }
        
        return true;
    }

    public bool ShowInteractionPrompt()
    {
        // Coinflip minigame DOES show the popup icon (E to interact)
        return true;
    }

    private void RunTransition(string message, System.Action midAction, System.Action afterTransition = null)
    {
        if (_isTransitionRunning) return;
        if (transitionCanvasGroup == null) { midAction?.Invoke(); afterTransition?.Invoke(); return; }
        _transitionRoutine = StartCoroutine(TransitionRoutine(message, midAction, afterTransition));
    }

    private IEnumerator TransitionRoutine(string message, System.Action midAction, System.Action afterTransition)
    {
        _isTransitionRunning = true;
        if (transitionStatusText != null) transitionStatusText.text = message;
        GameObject go = transitionCanvasGroup.gameObject;
        if (!go.activeSelf) go.SetActive(true);
        transitionCanvasGroup.blocksRaycasts = true;
        transitionCanvasGroup.interactable = true;
        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 1f, transitionFadeInDuration);
        midAction?.Invoke();
        if (transitionCoveredDuration > 0f) yield return new WaitForSeconds(transitionCoveredDuration);
        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 0f, transitionFadeOutDuration);
        transitionCanvasGroup.blocksRaycasts = false;
        transitionCanvasGroup.interactable = false;
        go.SetActive(false);
        _transitionRoutine = null;
        _isTransitionRunning = false;

        afterTransition?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (duration <= 0f) { transitionCanvasGroup.alpha = end; yield break; }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transitionCanvasGroup.alpha = Mathf.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        transitionCanvasGroup.alpha = end;
    }

    public void OpenCoinFlipPopup()
    {
        if (!_canOpen)
        {
            LogDebug("OpenCoinFlipPopup blocked - not ready");
            return;
        }

        if (Time.unscaledTime - _lastCloseTime < reopenCooldown)
        {
            LogDebug("OpenCoinFlipPopup blocked - cooldown");
            return;
        }

        if (coinFlipPopup == null || coinFlipPopup.activeSelf)
        {
            LogDebug("OpenCoinFlipPopup blocked - null or already active");
            return;
        }

        LogDebug("Opening coinflip popup");

        if (transitionCanvasGroup != null)
        {
            RunTransition("COIN FLIP", PerformOpen, null);
            return;
        }
        PerformOpen();
    }

    private void PerformOpen()
    {
        coinFlipPopup.SetActive(true);

        if (coinFlipInstructions == null)
            coinFlipInstructions = coinFlipPopup.GetComponentInChildren<MinigameInstructions>(true);
        if (coinFlipInstructions != null)
            coinFlipInstructions.OnPopupOpened();

        GlobalPause.SetMinigamePaused(true);
        foreach (var c in controlsToDisable) if (c) c.enabled = false;
        if (hudGroup != null) hudGroup.SetActive(false);
    }

    public void CloseCoinFlipPopup()
    {
        if (coinFlipPopup == null || !coinFlipPopup.activeSelf) return;

        LogDebug("Closing coinflip popup");

        if (transitionCanvasGroup != null && gameObject.activeInHierarchy)
        {
            RunTransition("EXITING", () =>
            {
                PerformCloseLogic();
                PerformCloseFinal();
            }, null);
            return;
        }
        PerformCloseLogic();
        PerformCloseFinal();
    }

    /// <summary>Re-enable controls, HUD, unpause. Does NOT hide popup or unlock (safe to call during transition).</summary>
    private void PerformCloseLogic()
    {
        foreach (var c in controlsToDisable) if (c) c.enabled = true;
        if (hudGroup != null) hudGroup.SetActive(true);
        GlobalPause.SetMinigamePaused(false);
    }

    /// <summary>Hide popup, cooldown, unlock. Call this only when transition has finished (or when no transition).</summary>
    private void PerformCloseFinal()
    {
        if (coinFlipPopup != null && coinFlipPopup != gameObject)
            coinFlipPopup.SetActive(false);
        _lastCloseTime = Time.unscaledTime;
        _canOpen = false;
        if (gameObject.activeInHierarchy)
            StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay));
        Systems.InteractionLockManager.Unlock();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range
        Gizmos.color = new Color(1f, 0.84f, 0f, 0.3f); // Gold color for coinflip
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
#endif
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[OverworldCoinGameLauncher] {message}");
    }
}
