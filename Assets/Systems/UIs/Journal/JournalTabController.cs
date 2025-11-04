using UnityEngine;

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
    
    [Tooltip("Flag name to track if tutorial has been shown")]
    public string tutorialShownFlagName = "journal.tutorial.shown";

    [Header("Optional")]
    public CanvasGroup cg;                // CanvasGroup on JournalPanel

    GameObject[] allPages;
    private EnvironmentSoundHandler _environmentSoundHandler;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        allPages = new[] { CharactersPage, EvidencePage, InformationPage, MonstersPage, TutorialsPage };

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

        SetOnly(CharactersPage);

        // Find the EnvironmentSoundHandler
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();
        if (_environmentSoundHandler == null)
            Debug.LogWarning("[JournalUI_Named] EnvironmentSoundHandler not found in scene");
        
        // Hide tutorial overlay initially
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
        }
    }

    void OnEnable()
    {
        // Check if we should show the tutorial when journal is opened
        if (tutorialOverlay != null && !GameFlags.HasFlag(tutorialShownFlagName))
        {
            ShowTutorial();
        }
    }

    void ShowTutorial()
    {
        Debug.Log("[JournalUI_Named] Showing journal tutorial for first time");
        tutorialOverlay.SetActive(true);
        
        // Set the flag so it never shows again
        GameFlags.SetFlag(tutorialShownFlagName);
    }

    /// <summary>
    /// Call this method from a button on the tutorial overlay to close it
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
            Debug.Log("[JournalUI_Named] Tutorial closed");
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