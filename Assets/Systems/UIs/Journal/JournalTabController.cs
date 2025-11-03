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