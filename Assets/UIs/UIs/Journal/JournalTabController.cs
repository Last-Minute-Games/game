using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalUI_Named : MonoBehaviour
{
    [Header("Journal Manager")]
    public JournalManager journalManager;

    [Header("Pages (drag from: JournalPanel/Pages/...)")]
    public GameObject CharactersPage;
    public GameObject EvidencePage;
    public GameObject InformationPage;
    public GameObject MonstersPage;
    public GameObject TutorialsPage;

    [Header("Tutorial")]
    [Tooltip("Optional tutorial overlay to show on first open")]
    public GameObject tutorialOverlay;
    
    [Tooltip("Button to close the tutorial (will auto-wire to CloseTutorial)")]
    public Button tutorialCloseButton;
    
    [Tooltip("Flag name to track if tutorial has been shown")]
    public string tutorialShownFlagName = "journal.tutorial.shown";

    [Header("Optional")]
    public CanvasGroup cg;                // CanvasGroup on JournalPanel

    GameObject[] allPages;
    private EnvironmentSoundHandler _environmentSoundHandler;
    private bool isTutorialActive = false;

    // Cache all Buttons under this journal so we can disable keyboard navigation/submit
    private Button[] _journalButtons = new Button[0];

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        allPages = new[] { CharactersPage, EvidencePage, InformationPage, MonstersPage, TutorialsPage };

        // Cache buttons (include inactive so tutorial-close button is covered)
        _journalButtons = GetComponentsInChildren<Button>(true);

        // Initialize journal manager AFTER GameFlags has time to initialize
        if (journalManager != null)
        {
            Debug.Log($"[JournalUI_Named] Initializing JournalManager: {journalManager.name}");
            journalManager.Initialize();
        }
        else
        {
            Debug.LogError("[JournalUI_Named] JournalManager reference is missing! Please assign it in the Inspector.");
            
            // Try to find it as a fallback
            journalManager = Resources.Load<JournalManager>("JournalManager");
            if (journalManager != null)
            {
                Debug.Log("[JournalUI_Named] Found JournalManager in Resources, initializing...");
                journalManager.Initialize();
            }
            else
            {
                Debug.LogError("[JournalUI_Named] Could not find JournalManager in Resources either!");
            }
        }

        // Find the EnvironmentSoundHandler
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();
        if (_environmentSoundHandler == null)
            Debug.LogWarning("[JournalUI_Named] EnvironmentSoundHandler not found in scene");
        
        // Hide tutorial overlay initially
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
        }
        
        // Auto-wire the tutorial close button
        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.RemoveAllListeners();
            tutorialCloseButton.onClick.AddListener(CloseTutorial);
            Debug.Log("[JournalUI_Named] Tutorial close button wired up");
        }

        // Disable keyboard navigation/selection for all journal buttons to prevent Space/Submit activation
        DisableKeyboardNavigationOnButtons();
    }

    void OnEnable()
    {
        // Clear any currently selected UI element so keyboard Submit/Space won't activate it
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Ensure buttons still have navigation disabled (in case they were enabled elsewhere)
        DisableKeyboardNavigationOnButtons();

        // Check if we should show the tutorial when journal is opened
        if (tutorialOverlay != null && !GameFlags.HasFlag(tutorialShownFlagName))
        {
            ShowTutorial();
        }
        else
        {
            // If tutorial already shown, display normal journal
            SetOnly(CharactersPage);
        }
    }

    void Update()
    {
        // CRITICAL: Completely consume Space key ALWAYS when this component is active
        // This prevents Space from doing ANYTHING in the journal
        if (gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            // Clear EventSystem selection
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            
            // Completely consume the input - don't do anything else
            return;
        }
    }

    private void DisableKeyboardNavigationOnButtons()
    {
        if (_journalButtons == null) return;

        foreach (var btn in _journalButtons)
        {
            if (btn == null) continue;

            try
            {
                var nav = btn.navigation;
                nav.mode = Navigation.Mode.None;
                btn.navigation = nav;
            }
            catch { }
        }
    }

    void ShowTutorial()
    {
        Debug.Log("[JournalUI_Named] Showing journal tutorial for first time");
        
        // Hide all pages
        foreach (var p in allPages)
        {
            if (p) p.SetActive(false);
        }
        
        // Show tutorial
        tutorialOverlay.SetActive(true);
        isTutorialActive = true;
        
        // DON'T set the flag here - wait until the player dismisses it

        // Make sure nothing is selected so keyboard won't close it
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Call this method from a button on the tutorial overlay to close it
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialOverlay != null && isTutorialActive)
        {
            tutorialOverlay.SetActive(false);
            isTutorialActive = false;
            Debug.Log("[JournalUI_Named] Tutorial closed");
            
            // Set the flag NOW when the player dismisses it
            GameFlags.SetFlag(tutorialShownFlagName);
            
            // Show the default page after tutorial closes
            SetOnly(CharactersPage);
        }
    }

    // BUTTON HOOKS (match your names exactly)
    public void ShowCharacters() => SetOnlyWithSound(CharactersPage);
    public void ShowEvidence() => SetOnlyWithSound(EvidencePage);
    public void ShowInformation() => SetOnlyWithSound(InformationPage);
    public void ShowMonsters() => SetOnlyWithSound(MonstersPage);
    public void ShowTutorials() => SetOnlyWithSound(TutorialsPage);

    void SetOnlyWithSound(GameObject target)
    {
        // Don't allow page switching while tutorial is active
        if (isTutorialActive)
        {
            CloseTutorial();
            return;
        }
        
        // Play journal sound when switching tabs
        PlayTabSound();
        SetOnly(target);
    }

    void SetOnly(GameObject target)
    {
        foreach (var p in allPages) if (p) p.SetActive(p == target);
    }

    void PlayTabSound()
    {
        try
        {
            if (_environmentSoundHandler != null)
            {
                // Play the journal open sound for tab switches (you can use true or false)
                // Using true (open sound) for tab clicks as a "click" effect
                _environmentSoundHandler.PlayJournalSound(true);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[JournalUI_Named] Failed to play tab sound: {ex.Message}");
        }
    }
}