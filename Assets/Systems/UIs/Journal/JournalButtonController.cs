using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JournalUI : MonoBehaviour
{
    [Header("Refs")]
    public Button journalButton;         // hook your JournalButton here
    public CanvasGroup journalPanel;     // the content panel that appears when open
    public GameObject journalRoot;       // The root GameObject with the Animator (can be disabled)
    
    private EnvironmentSoundHandler _environmentSoundHandler;
    private PlayerInput2D _playerInput;
    private Animator anim;

    [Header("UI Behavior")]
    public float fadeDuration = 0.15f;   // fade for the contents
    public KeyCode toggleKey = KeyCode.Q; // Q key to toggle journal

    bool isOpen;

    void Awake()
    {
        // Force toggle key to Q (override any Inspector changes)
        toggleKey = KeyCode.Q;
        
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) _playerInput = player.GetComponent<PlayerInput2D>();
        
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
        SetOpen(!isOpen);
    }

    public void Open()
    {
        Debug.Log("[JournalUI] Open() called.");
        SetOpen(true);
    }

    public void Close()
    {
        Debug.Log("[JournalUI] Close() called.");
        SetOpen(false);
    }

    void SetOpen(bool value, bool instant = false)
    {
        Debug.Log($"[JournalUI] SetOpen called. Target state: {(value ? "Open" : "Closed")}, Instant: {instant}");

        // Try to play sound, but don't crash if handler is null or throws
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
        
        isOpen = value;


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
}
