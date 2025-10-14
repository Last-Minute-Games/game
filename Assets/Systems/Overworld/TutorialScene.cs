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

        _journalMovementPage.alpha = 0f;
        
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 1f; // Start opaque
        
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();

        StartCoroutine(BeginTutorialSeq());
    }
    
    private IEnumerator CloseJournal()
    {
        _environmentSoundHandler.PlayJournalSound(false);
        
        _journalPanel.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _journalPanel.blocksRaycasts = false; // Disable blocking after fade-out
        });
        
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
        
        _environmentSoundHandler.PlayJournalSound(true);
        
        // fade in journal
        _journalPanel.DOFade(1f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _journalPanel.blocksRaycasts = true; // Enable blocking after fade-in
        });
        
        // _characterMotor2D.SetSpeed(3f);
        
        
        
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
