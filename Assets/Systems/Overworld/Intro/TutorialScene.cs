using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Systems.Overworld.Intro
{
    public class TutorialScene : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public Sprite kingBehindSprite;
        public Sprite kingFrontSprite;
        
        public AnimationClip kingDeadAnimationClip;
        
        private GameObject _plrObject;
        private PlayerInput2D _plrInput;
        private Camera _plrMainCamera;
        
        private CharacterMotor2D _characterMotor2D;
        private CanvasGroup _fadeCanvasGroup;

        private CanvasGroup _journalPanel;
        private GameObject _journalPages;
    
        private GameObject _journalMovementPage;
        private Button _movementContinueButton;
    
        private EnvironmentSoundHandler _environmentSoundHandler;
        private MusicManager _introMusicManager;

        private GameObject _tutorialTriggers;
        
        private SpriteRenderer _kingSpriteRenderer;
        private Animator _kingAnimator;
        
        private Camera _throneRoomCamera;
        
        private MysteriousManIntro _mysteriousManIntro;
        
        private bool _isPlayingIntroMusic = false;

        private GameObject _blackScreen;
    
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
            _plrObject = GameObject.FindGameObjectWithTag("Player");
            _blackScreen = GameObject.Find("Blackout");
            
            _blackScreen.SetActive(false);
            
            _plrInput = _plrObject.GetComponent<PlayerInput2D>();
            _plrInput.isInputEnabled = false;
            
            _characterMotor2D = _plrObject.GetComponent<CharacterMotor2D>();
            
            _plrMainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
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
            
            _throneRoomCamera = GameObject.Find("Throne Assets").transform.Find("Main Camera").GetComponent<Camera>();
            _throneRoomCamera.gameObject.SetActive(false);
            
            _kingSpriteRenderer = GameObject.Find("KingNPC").GetComponent<SpriteRenderer>();
            
            _kingAnimator = GameObject.Find("KingNPC").GetComponent<Animator>();
            _kingAnimator.speed = 0; // freeze at start
            
            _mysteriousManIntro = GameObject.Find("MysteriousManNPC").GetComponent<MysteriousManIntro>();
            
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
            _journalPanel.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _journalPanel.blocksRaycasts = false; // Disable blocking after fade-out
            });
            
            _fadeCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InOutQuad);
        
            yield return new WaitForSeconds(0.2f);
            
            _plrInput.isInputEnabled = true;
            _environmentSoundHandler.PlayJournalSound(false);
            
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

        public IEnumerator BeginKingSeq()
        {
            _plrMainCamera.gameObject.SetActive(false);
            _throneRoomCamera.gameObject.SetActive(true);
            
            _kingSpriteRenderer.sprite = kingBehindSprite;
            
            _characterMotor2D.forceIdleSprite = _characterMotor2D.idleUp;
            
            _plrInput.isInputEnabled = false;
            
            yield return new WaitForSeconds(1f);
            
            _fadeCanvasGroup.DOFade(0f, 4f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
            });
            
            yield return new WaitForSeconds(3f);

            _introMusicManager.FadeAndStop(0f, 6f);
            
            // TWEEN king camera slightly up
            _throneRoomCamera.transform.DOMoveY( _throneRoomCamera.transform.position.y + 5.45f, 6f).SetEase(Ease.Linear);
            
            yield return new WaitForSeconds(5f);
            
            _kingSpriteRenderer.sprite = kingFrontSprite;
            
            yield return new WaitForSeconds(2f);
            
            yield return _mysteriousManIntro.FadeIn();
            
            yield return new WaitForSeconds(0.5f);
            
            _blackScreen.SetActive(true);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 5;

            yield return _mysteriousManIntro.PlayAnimationOnce();
            
            yield return new WaitForSeconds(1f);
            
            _blackScreen.SetActive(false);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 0;
            
            _kingAnimator.speed = 1; // unfreeze
            
            yield return new WaitForSeconds(kingDeadAnimationClip.length);
            
            yield return new WaitForSeconds(3f);
            
            // go to overworld scene
            AsyncOperation op = SceneManager.LoadSceneAsync("Overworld");
            op.allowSceneActivation = true; // or set false if you want to gate activation

            // Optionally wait until load is done (it’s already black)
            while (!op.isDone)
                yield return null;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
