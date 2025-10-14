using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Overworld.Intro
{
    public class TutorialScene : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private PlayerInput2D _plrInput;
        
        private CharacterMotor2D _characterMotor2D;
        private CanvasGroup _fadeCanvasGroup;

        private CanvasGroup _journalPanel;
        private GameObject _journalPages;
    
        private GameObject _journalMovementPage;
        private Button _movementContinueButton;
    
        private EnvironmentSoundHandler _environmentSoundHandler;
        private MusicManager _introMusicManager;

        private GameObject _tutorialTriggers;
        
        private bool _isPlayingIntroMusic = false;
    
        private IEnumerator WaitDreamIntro()
        {
            yield return new WaitForSeconds(_introMusicManager.dreamLoop.length);
            // Your event code here
            Debug.Log("Dream intro finished playing!");
            // Example: Call a function, activate a GameObject, etc.
            _introMusicManager.SetAudioClip(_introMusicManager.dreamLoop, true);
            _introMusicManager.Play();
        }
    
        public void SwitchJournalPage(string pageName)
        {
            foreach (Transform page in _journalPages.transform)
            {
                page.gameObject.SetActive(page.name == pageName);
            }
        }
    
        void Start()
        {
            _plrInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput2D>();
            _plrInput.isInputEnabled = false;
            
            _characterMotor2D = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMotor2D>();
        
            _journalPanel = GameObject.Find("JournalPanel").GetComponent<CanvasGroup>();
            _journalPanel.alpha = 0f;

            _journalPages = _journalPanel.transform.Find("Pages").gameObject;
        
            _journalMovementPage = _journalPages.transform.Find("Movement").gameObject;
            _movementContinueButton = _journalMovementPage.transform.Find("Continue").GetComponent<Button>();
        
            SwitchJournalPage("Movement");
        
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 1f; // Start opaque
        
            _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();
        
            _introMusicManager = GameObject.Find("IntroMusic").GetComponent<MusicManager>();
            _introMusicManager.SetAudioClip(_introMusicManager.dreamIntro);
        
            // iterate buttons
            foreach (Transform page in _journalPages.transform)
            {
                var continueButton = page.Find("Continue").GetComponent<Button>();
         
                continueButton.onClick.AddListener(() =>
                {
                    StartCoroutine(CloseJournal());

                    if (!_isPlayingIntroMusic)
                    {
                        _introMusicManager.GetAudioSource().volume = 0f;
                        _introMusicManager.FadeAndPlay(0.35f, 15f);
            
                        StartCoroutine(WaitDreamIntro());
                        _isPlayingIntroMusic = true;
                    }
                });
            }
        
            StartCoroutine(BeginTutorialSeq());
        }

        public IEnumerator OpenJournal()
        {
            _plrInput.isInputEnabled = false;
        
            _environmentSoundHandler.PlayJournalSound(true);
            
            _journalPanel.DOFade(1f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _journalPanel.blocksRaycasts = true; // Enable blocking after fade-in
            });
            
            _fadeCanvasGroup.DOFade(0.6f, 0.15f).SetEase(Ease.InOutQuad);
        
            yield return null;
        }
    
        public IEnumerator CloseJournal()
        {
            _plrInput.isInputEnabled = true;
            _environmentSoundHandler.PlayJournalSound(false);
        
            _journalPanel.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _journalPanel.blocksRaycasts = false; // Disable blocking after fade-out
            });
            
            _fadeCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad);
        
            yield return new WaitForSeconds(0.2f);
            
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

            yield return OpenJournal();
        
            yield return null;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
