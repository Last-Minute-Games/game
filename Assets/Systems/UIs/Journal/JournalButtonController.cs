using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class JournalUI : MonoBehaviour
{
    [Header("Refs")]
    public Button journalButton;         // hook your JournalButton here
    public CanvasGroup journalPanel;     // the content panel that appears when open
    public GameObject journalRoot;       // The root GameObject with the Animator (can be disabled)
    
    private EnvironmentSoundHandler _environmentSoundHandler;
    private PlayerInput2D _playerInput;
    private ClockTimer _clockTimer;
    private SimplePauseMenu _pauseMenu;
    private Animator anim;

    [Header("UI Behavior")]
    public float fadeDuration = 0.15f;   // fade for the contents
    public KeyCode toggleKey = KeyCode.Q; // Q key to toggle journal

    bool isOpen;
    bool isInputEnabled = true;
    
    void Awake()
    {
        // Force toggle key to Q (override any Inspector changes)
        toggleKey = KeyCode.Q;
        
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) _playerInput = player.GetComponent<PlayerInput2D>();
        
        // Find the ClockTimer in the scene
        _clockTimer = FindFirstObjectByType<ClockTimer>();
        if (_clockTimer == null)
            Debug.LogWarning("[JournalUI] ClockTimer not found in scene");
        
        // Find the SimplePauseMenu in the scene
        _pauseMenu = FindFirstObjectByType<SimplePauseMenu>();
        if (_pauseMenu == null)
            Debug.LogWarning("[JournalUI] SimplePauseMenu not found in scene");
        
        // Get animator from journalRoot if assigned, otherwise try this GameObject
        if (journalRoot)
            anim = journalRoot.GetComponent<Animator>();
        else
            anim = GetComponent<Animator>();
        
        Debug.Log("[JournalUI] Awake called.");
        Debug.Log($"[JournalUI] GameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"[JournalUI] Component enabled: {enabled}");
        Debug.Log($"[JournalUI] GameObject name: {gameObject.name}");
        Debug.Log($"[JournalUI] Toggle key set to: {toggleKey}");
        Debug.Log($"[JournalUI] Script type: {GetType().Name}");

        if (journalButton)
        {
            journalButton.onClick.AddListener(Toggle);
            Debug.Log("[JournalUI] Journal button listener attached.");
        }
        else
        {
            Debug.LogWarning("[JournalUI] Journal button reference not assigned!");
        }

        SetOpen(false, instant: true);
        
        Debug.Log("[JournalUI] Awake complete - Update() should start running now");
    }

    void Start()
    {
        Debug.Log("[JournalUI] Start() called - component is definitely active");
    }

    void Update()
    {
        // CRITICAL: Completely consume Space key when journal is open - prevent ALL processing
        // This runs FIRST before any other logic to ensure Space never does anything
        if (isOpen && Input.GetKeyDown(KeyCode.Space))
        {
            // Clear EventSystem selection to prevent button activation
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            
            // Completely consume the input - don't process ANYTHING else
            return;
        }
        
        // Don't allow opening if input is disabled or if game is paused
        if (!isInputEnabled)
        {
            return;
        }

        // Check if pause menu is open
        if (_pauseMenu != null && _pauseMenu.IsPaused)
        {
            return;
        }

        // Check if Q is pressed
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log($"[JournalUI] {toggleKey} key pressed - Toggle() will be called");
            Toggle();
        }
    }

    public void Toggle()
    {
        Debug.Log($"[JournalUI] Toggle pressed. Current state: {(isOpen ? "Open" : "Closed")}");
        
        // Prevent toggling if input is disabled or paused
        if (!isInputEnabled || (_pauseMenu != null && _pauseMenu.IsPaused))
        {
            Debug.Log("[JournalUI] Toggle blocked - input disabled or game paused");
            return;
        }
        
        SetOpen(!isOpen, playSound: true);
    }

    public void Open()
    {
        Debug.Log("[JournalUI] Open() called.");
        
        // Prevent opening if input is disabled or paused
        if (!isInputEnabled || (_pauseMenu != null && _pauseMenu.IsPaused))
        {
            Debug.Log("[JournalUI] Open blocked - input disabled or game paused");
            return;
        }
        
        SetOpen(true, playSound: true);
    }

    /// <summary>
    /// Force open the journal, bypassing pause menu and input checks.
    /// Useful for tutorials or scripted sequences where you want to show the journal to the player.
    /// Always plays the open sound effect.
    /// </summary>
    public void ForceOpen()
    {
        Debug.Log("[JournalUI] ForceOpen() called - bypassing input checks for tutorial/scripted sequence");
        SetOpen(true, playSound: true);
    }

    public void Close()
    {
        Debug.Log("[JournalUI] Close() called.");
        SetOpen(false, playSound: true);
    }

    void SetOpen(bool value, bool instant = false, bool playSound = true)
    {
        Debug.Log($"[JournalUI] SetOpen called. Target state: {(value ? "Open" : "Closed")}, Instant: {instant}, PlaySound: {playSound}");

        // Play sound only if explicitly requested
        if (playSound)
        {
            try
            {
                if (_environmentSoundHandler != null)
                    _environmentSoundHandler.PlayJournalSound(value);
                else
                    Debug.LogWarning("[JournalUI] EnvironmentSoundHandler is null - skipping sound");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[JournalUI] Failed to play journal sound: {ex.Message}");
            }
        }
        
        isOpen = value;

        // Clear EventSystem selection when opening to prevent any button activation
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[JournalUI] Cleared EventSystem selection");
        }

        // Pause/unpause the clock timer
        if (_clockTimer != null)
        {
            _clockTimer.PauseTimer(isOpen);
            Debug.Log($"[JournalUI] ClockTimer paused: {isOpen}");
        }

        // Disable/enable player input
        if (_playerInput != null)
        {
            _playerInput.isInputEnabled = !isOpen;
            Debug.Log($"[JournalUI] Player input enabled: {_playerInput.isInputEnabled}");
        }

        if (anim != null)
        {
            anim.SetBool("Open", isOpen);
            Debug.Log($"[JournalUI] Animator parameter 'Open' set to {isOpen}");
        }

        if (!journalPanel)
        {
            Debug.LogWarning("[JournalUI] No journalPanel assigned, skipping fade.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(FadePanel(isOpen, instant ? 0f : fadeDuration));
    }

    IEnumerator FadePanel(bool show, float dur)
    {
        Debug.Log($"[JournalUI] FadePanel started. Show: {show}, Duration: {dur}");

        journalPanel.blocksRaycasts = show;
        journalPanel.interactable = show;

        float start = journalPanel.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float progress = dur <= 0 ? 1f : t / dur;
            journalPanel.alpha = Mathf.Lerp(start, end, progress);

            yield return null;
        }

        journalPanel.alpha = end;
        Debug.Log($"[JournalUI] Fade complete. Final alpha={journalPanel.alpha:F2}");
    }

    /// <summary>
    /// Public API to enable/disable journal input (called by SimplePauseMenu)
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        Debug.Log($"[JournalUI] Input enabled set to: {enabled}");
    }

    /// <summary>
    /// Public property to check if journal is currently open
    /// </summary>
    public bool IsOpen => isOpen;
}
