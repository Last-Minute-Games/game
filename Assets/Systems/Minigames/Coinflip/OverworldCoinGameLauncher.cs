using System.Collections;
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

    [Header("Transition")]
    [Tooltip("Shared transition component (add MinigameTransition to this or a child and assign).")]
    [SerializeField] MinigameTransition transition;

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

        if (transition != null && gameObject.activeInHierarchy)
        {
            transition.RunTransition("COIN FLIP", PerformOpen);
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

        MinigameTransition t = transition != null ? transition : FindObjectOfType<MinigameTransition>();
        if (t != null)
        {
            t.RunTransition("EXITING", PerformCloseLogic, null, PerformCloseFinal, instantBlack: true);
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
