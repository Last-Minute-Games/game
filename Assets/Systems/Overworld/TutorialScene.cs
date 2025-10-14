using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TutorialScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CharacterMotor2D _characterMotor2D;
    private CanvasGroup _fadeCanvasGroup;

    private CanvasGroup _journalPanel;
    private GameObject _journalPages;
    
    private CanvasGroup _journalMovementPage;
    private Button _movementContinueButton;
    
    private EnvironmentSoundHandler _environmentSoundHandler; 
    
    void Start()
    {
        _characterMotor2D = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMotor2D>();
        _characterMotor2D.SetSpeed(0f); // Start stationary
        
        _journalPanel = GameObject.Find("JournalPanel").GetComponent<CanvasGroup>();
        _journalPanel.alpha = 0f;

        _journalPages = _journalPanel.transform.Find("Pages").gameObject;
        
        _journalMovementPage = _journalPages.transform.Find("Movement").GetComponent<CanvasGroup>();
        _movementContinueButton = _journalMovementPage.transform.Find("Continue").GetComponent<Button>();
        
        _journalMovementPage.alpha = 0f;
        
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 1f; // Start opaque
        
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();
        
        _movementContinueButton.onClick.AddListener(() =>
        {
            StartCoroutine(CloseJournal());
        });
        
        StartCoroutine(BeginTutorialSeq());
    }

    private IEnumerator OpenJournal()
    {
        _characterMotor2D.SetSpeed(0f);
        
        _environmentSoundHandler.PlayJournalSound(true);
            
        _journalPanel.DOFade(1f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _journalPanel.blocksRaycasts = true; // Enable blocking after fade-in
        });
        
        yield return null;
    }
    
    private IEnumerator CloseJournal()
    {
        _environmentSoundHandler.PlayJournalSound(false);
        
        _journalPanel.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _journalPanel.blocksRaycasts = false; // Disable blocking after fade-out
        });

        _fadeCanvasGroup.DOFade(0f, .15f).SetEase(Ease.InOutQuad);
        
        yield return new WaitForSeconds(0.2f);
        
        _characterMotor2D.SetSpeed(3f);
        yield return null;
    }
    
    private IEnumerator BeginTutorialSeq()
    {
        yield return new WaitForSeconds(2f);
        
        _fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
        });
        
        yield return new WaitForSeconds(4f);
        
        _journalMovementPage.alpha = 1f;

        yield return OpenJournal();
        
        // _characterMotor2D.SetSpeed(3f);
        
        
        
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
