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

    GameObject[] allPages;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        allPages = new[] { CharactersPage, EvidencePage, InformationPage, MonstersPage, TutorialsPage };
        SetOnly(CharactersPage);
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