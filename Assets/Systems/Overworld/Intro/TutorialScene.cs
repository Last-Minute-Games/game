using System.Collections;
using cherrydev;
using DG.Tweening;
using Dialogues;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
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

        public DialogBehaviour dialogBehaviour;

        private GameObject _plrObject;
        private PlayerInput2D _plrInput;
        private Camera _plrMainCamera;
        private CinemachinePositionComposer _cinemachinePositionComposer;

        private CharacterMotor2D _characterMotor2D;
        private CanvasGroup _fadeCanvasGroup;

        private CanvasGroup _journalPanel;
        private GameObject _journalPages;

        private GameObject _journalMovementPage;
        private Button _movementContinueButton;

        private EnvironmentSoundHandler _environmentSoundHandler;
        private AudioSource _glitchAudioSource;
        private AudioSource _meltingAudioSource;
        
        private GameObject _ballroomBloodPuddle;
        private GameObject _ballroomFootsteps;
        
        private AudioSource _ballroomThroneDoorSource;

        private MusicManager _introMusicManager;

        private GameObject _tutorialTriggers;

        private SpriteRenderer _kingSpriteRenderer;
        private Animator _kingAnimator;
        private AudioSource _kingAudioSource;

        private Camera _throneRoomCamera;

        private MysteriousManIntro _mysteriousManIntro;

        private bool _isPlayingIntroMusic = false;
        private bool _isBallroomMelting = false;

        private GameObject _blackScreen;
        private GameObject _corruptScreen;

        private GameObject _charactersGroup;
        
        private Light2D _characterLight2D;
        private Light2D _ballroomSpotlight;
        
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

        public void SetCinecamYOffset(float yOffset)
        {
            _cinemachinePositionComposer.TargetOffset.y = yOffset;
        }

        private SentenceNode CreateMysteriousSentenceNode(DialogNodeGraph nodeGraph)
        {
            SentenceNode node = ScriptableObject.CreateInstance<SentenceNode>();

            node.Initialize(new Rect(), "sentence", nodeGraph);

            return node;
        }

        private IEnumerator WaitAndOpenBigDoor()
        {
            var ballroomDoor = GameObject.Find("BallroomDoor");
            var openHash = Animator.StringToHash("OpenDoor");
            
            var spotlightClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/Spotlight");
            var bigDoorOpenClip = Resources.Load<AudioClip>("SFXs/Doors/BigDoorOpen");
            var tempBallroomBlock = GameObject.Find("TempBallroomBlock");
            
            yield return new WaitForSeconds(8f);

            _characterLight2D.enabled = false;
            _ballroomSpotlight.enabled = true;
            
            _ballroomThroneDoorSource.clip = spotlightClip;
            _ballroomThroneDoorSource.Play();
                
            yield return new WaitForSeconds(1.5f);

            _ballroomThroneDoorSource.clip = bigDoorOpenClip;
            _ballroomThroneDoorSource.Play();
            
            _plrInput.isInputEnabled = true;
            tempBallroomBlock.SetActive(false);
            
            yield return new WaitForSeconds(0.5f);

            foreach (Transform door in ballroomDoor.transform)
            {
                var animator = door.GetComponent<Animator>();
                if (animator) animator.SetTrigger(openHash);
            }
        }
        
        private void ActivateMeltingSequence()
        {
            if (_isBallroomMelting) return;
            _isBallroomMelting = true;
            
            _plrInput.isInputEnabled = false;
            
            _meltingAudioSource.Play();

            StartCoroutine(WaitAndOpenBigDoor());

            foreach (Transform puddleTrans in _ballroomBloodPuddle.transform)
            {
                var puddleRenderer = puddleTrans.GetComponent<SpriteRenderer>();
                var newColor = puddleRenderer.color;
                newColor.a = 1;
                puddleRenderer.DOColor(newColor, 4f).SetEase(Ease.InSine);
            }
            
            foreach (Transform npcTransform in _charactersGroup.transform)
            {
                var spriteRenderer = npcTransform.gameObject.GetComponent<SpriteRenderer>();
                var sinkDistance = spriteRenderer.transform.localScale.z * 2;
                
                var newColor = new Color(0.9f, 0.0f, 0.0f);
                spriteRenderer.DOColor(newColor, 3.5f).SetEase(Ease.Linear);;
                
                Vector3 targetPos = spriteRenderer.transform.position - new Vector3(0, sinkDistance, 0);
                spriteRenderer.transform.DOMove(targetPos, 7f).SetEase(Ease.Linear);
            }

            _characterLight2D.DOIntensity(0, 6.5f);
        }

        private void CreateScaryDialogue(Transform npcTransform)
        {
            var dialogTrigger = npcTransform.gameObject.AddComponent<DialogTrigger>();
            dialogTrigger.dialogBehaviour = dialogBehaviour;

            var newGraph = ScriptableObject.CreateInstance<DialogNodeGraph>();
            dialogTrigger.dialogGraph = newGraph;

            dialogTrigger.OnDialogCompleted = new UnityEvent();
            dialogTrigger.OnDialogCompleted.AddListener(ActivateMeltingSequence);

            var newSentenceNode = CreateMysteriousSentenceNode(newGraph);

            var tex = Resources.Load<Texture2D>("Dialogues/" + npcTransform.name + "/" + npcTransform.name +
                                                "Portrait");
            
            newSentenceNode.Sentence = new Sentence(npcTransform.name, "...");

            if (tex != null)
            {
                // Convert Texture2D → Sprite
                var portraitSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), // pivot center
                    100f // pixels per unit (adjust as needed)
                );

                newSentenceNode.Sentence.CharacterSprite = portraitSprite;
            }

            newGraph.NodesList.Add(newSentenceNode);
        }

        void Start()
        {
            _plrObject = GameObject.FindGameObjectWithTag("Player");

            _blackScreen = GameObject.Find("Blackout");
            _corruptScreen = GameObject.Find("CorruptScreen");

            _blackScreen.SetActive(false);
            _corruptScreen.SetActive(false);

            _plrInput = _plrObject.GetComponent<PlayerInput2D>();
            _plrInput.isInputEnabled = false;

            _characterMotor2D = _plrObject.GetComponent<CharacterMotor2D>();

            _plrMainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            _cinemachinePositionComposer =
                GameObject.Find("CinemachineCamera").GetComponent<CinemachinePositionComposer>();

            _environmentSoundHandler =
                GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();
            
            var glitchClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/KingGlitch");
            _glitchAudioSource = _environmentSoundHandler.CreateCustomSource("Glitch");
            _glitchAudioSource.clip = glitchClip;
            _glitchAudioSource.volume = 1f;
            
            var meltClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/Melting");
            _meltingAudioSource = _environmentSoundHandler.CreateCustomSource("Melting");
            _meltingAudioSource.clip = meltClip;
            _meltingAudioSource.volume = 1f;
            
            _ballroomFootsteps = GameObject.Find("BallroomFootsteps");
            foreach (var footstepObj in _ballroomFootsteps.transform)
            {
                var 
            }
            
            _ballroomBloodPuddle = GameObject.Find("BallroomBloodPuddle");
            foreach (Transform puddleTrans in _ballroomBloodPuddle.transform)
            {
                var puddleRenderer = puddleTrans.GetComponent<SpriteRenderer>();
                var color = puddleRenderer.color;
                color.a = 0;
                puddleRenderer.color = color;
            }

            _ballroomThroneDoorSource = GameObject.Find("Ballroom/Throne").GetComponent<AudioSource>();
            
            _characterLight2D = GameObject.Find("CharacterLight").GetComponent<Light2D>();
            _ballroomSpotlight = GameObject.Find("BallroomSpotlight").GetComponent<Light2D>();
            
            _characterLight2D.enabled = true;
            _ballroomSpotlight.enabled = false;
            
            _charactersGroup = GameObject.Find("BlockingCharacters");
            // iterate for each child
            foreach (Transform child in _charactersGroup.transform)
            {
                // add dialog trigger for each
                CreateScaryDialogue(child);
            }

            // change cinecam offset
            SetCinecamYOffset(0);

            _journalPanel = GameObject.Find("JournalPanel").GetComponent<CanvasGroup>();
            _journalPanel.alpha = 0f;

            _journalPages = _journalPanel.transform.Find("Pages").gameObject;

            _journalMovementPage = _journalPages.transform.Find("Movement").gameObject;
            _movementContinueButton = _journalMovementPage.transform.Find("Continue").GetComponent<Button>();

            SwitchJournalPage("Movement");

            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 1f; // Start opaque

            _introMusicManager = GameObject.Find("IntroMusic").GetComponent<MusicManager>();
            _introMusicManager.SetAudioClip(_introMusicManager.dreamIntro);

            _throneRoomCamera = GameObject.Find("Throne Assets").transform.Find("Main Camera").GetComponent<Camera>();
            _throneRoomCamera.gameObject.SetActive(false);

            _kingSpriteRenderer = GameObject.Find("KingNPC").GetComponent<SpriteRenderer>();

            _kingAnimator = GameObject.Find("KingNPC").GetComponent<Animator>();
            _kingAnimator.speed = 0; // freeze at start

            _kingAudioSource = GameObject.Find("KingNPC").GetComponent<AudioSource>();

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
                        _introMusicManager.FadeAndPlay(0.15f, 15f);

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

            _introMusicManager.FadeAndStop(0f, 8f);

            // TWEEN king camera slightly up
            _throneRoomCamera.transform.DOMoveY(_throneRoomCamera.transform.position.y + 5.45f, 6f)
                .SetEase(Ease.Linear);

            yield return new WaitForSeconds(5f);

            _kingSpriteRenderer.sprite = kingFrontSprite;

            yield return new WaitForSeconds(2f);

            yield return _mysteriousManIntro.FadeIn();

            yield return new WaitForSeconds(0.5f);

            _blackScreen.SetActive(true);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 5;

            // play sound asynchrously
            _kingAudioSource.Play();
            yield return _mysteriousManIntro.PlayAnimationOnce();

            yield return new WaitForSeconds(1f);

            _blackScreen.SetActive(false);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 0;

            _kingAnimator.speed = 1; // unfreeze

            yield return new WaitForSeconds(kingDeadAnimationClip.length * 0.97f);

            _kingAnimator.speed = 0;

            yield return new WaitForSeconds(0.5f);

            _glitchAudioSource.Play();
            _corruptScreen.SetActive(true);

            yield return new WaitForSeconds(1f);

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