using UnityEngine;

public class JournalUI_Named : MonoBehaviour
{
    [Header("Pages (drag from: JournalPanel/Pages/...)")]
    public GameObject CharactersPage;
    public GameObject EvidencePage;
    public GameObject InformationPage;
    public GameObject MonstersPage;
    public GameObject TutorialsPage;

    [Header("Optional")]
    public CanvasGroup cg;                // CanvasGroup on JournalPanel
    public KeyCode toggleKey = KeyCode.J; // open/close with J

    GameObject[] allPages;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        allPages = new[] { CharactersPage, EvidencePage, InformationPage, MonstersPage, TutorialsPage };
        SetOnly(CharactersPage);

    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    public void Toggle() { if (gameObject.activeSelf) Close(); else Open(); }
    public void Open()
    {
        gameObject.SetActive(true);
        if (cg) { cg.alpha = 1; cg.blocksRaycasts = true; cg.interactable = true; }
        Time.timeScale = 0f;         // pause while journal is open (optional)
        ShowInformation();           // pick your default tab here
    }
    public void Close()
    {
        if (cg) { cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false; }
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    // BUTTON HOOKS (match your names exactly)
    public void ShowCharacters() => SetOnly(CharactersPage);
    public void ShowEvidence() => SetOnly(EvidencePage);
    public void ShowInformation() => SetOnly(InformationPage);
    public void ShowMonsters() => SetOnly(MonstersPage);
    public void ShowTutorials() => SetOnly(TutorialsPage);

    void SetOnly(GameObject target)
    {
        foreach (var p in allPages) if (p) p.SetActive(p == target);
    }
}
