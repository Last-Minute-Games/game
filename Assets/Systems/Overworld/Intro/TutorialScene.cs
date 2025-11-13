using System.Collections;
using System.Collections.Generic;
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

        public List<AudioClip> bloodFootstepsClips;

        public List<Sprite> sleepingPlayerFrames;

        // New: list of lights that should be dimmed when the melting sequence starts
        public List<Light2D> meltingLights = new List<Light2D>();
        public float meltingDimDuration = 7f;

        private GameObject _plrObject;
        private PlayerInput2D _plrInput;
        private CharacterMotor2D _plrMotor2D;

        private Camera _plrMainCamera;
        private CinemachinePositionComposer _cinemachinePositionComposer;

        private GameObject _mysteriousPersonHallway;

        private CanvasGroup _fadeCanvasGroup;

        private CanvasGroup _journalPanel;
        private GameObject _journalPages;

        private GameObject _journalMovementPage;
        private Button _movementContinueButton;

        private EnvironmentSoundHandler _environmentSoundHandler;
        private AudioSource _meltingAudioSource;

        private GameObject _aspectBars;

        private GameObject _ballroomBloodPuddle;
        private GameObject _ballroomFootsteps;

        private AudioSource _ballroomThroneDoorSource;

        private MusicManager _introMusicManager;

        private GameObject _tutorialTriggers;

        private SpriteRenderer _kingSpriteRenderer;
        private Animator _kingAnimator;

        private Camera _spawnRoomCamera;
        private Camera _throneRoomCamera;

        private MysteriousManIntro _mysteriousManIntro;

        private bool _isPlayingIntroMusic = false;
        private bool _isBallroomMelting = false;

        private GameObject _blackScreen;
        private GameObject _corruptScreen;

        private GameObject _charactersGroup;

        private Light2D _globalLight;
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
            var node = ScriptableObject.CreateInstance<SentenceNode>();
            // SentenceNode doesn't have an Initialize method, just create it directly
            return node;
        }

        public void SetTutorialGlobalLightIntensity(float intensity)
        {
            _globalLight.intensity = intensity;
        }

        private IEnumerator StartPuddleSeq()
        {
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

                var npcDialog = npcTransform.GetComponent<DialogTrigger>();
                npcDialog.enabled = false;

                var newColor = new Color(0.9f, 0.0f, 0.0f);
                spriteRenderer.DOColor(newColor, 3.5f).SetEase(Ease.Linear);
                ;

                Vector3 targetPos = spriteRenderer.transform.position - new Vector3(0, sinkDistance, 0);
                spriteRenderer.transform.DOMove(targetPos, 6f).SetEase(Ease.Linear);
            }

            // global light dimming is now handled when the melting sequence is activated via the meltingLights list

            yield return null;
        }

        private IEnumerator WaitAndOpenBigDoor()
        {
            var ballroomDoor = GameObject.Find("BallroomDoor");
            var openHash = Animator.StringToHash("OpenDoor");

            var spotlightClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/Spotlight");
            var bigDoorOpenClip = Resources.Load<AudioClip>("SFXs/Doors/BigDoorOpen");
            var tempBallroomBlock = GameObject.Find("TempBallroomBlock");

            _characterLight2D.DOIntensity(0, 2f);
            
            yield return new WaitForSeconds(3.5f);

            _characterLight2D.enabled = false;
            _ballroomSpotlight.enabled = true;

            _ballroomThroneDoorSource.clip = spotlightClip;
            _ballroomThroneDoorSource.Play();

            yield return new WaitForSeconds(0.7f);

            var footstepSource = _environmentSoundHandler.CreateCustomSource("BloodFootsteps");
            footstepSource.volume = 0.8f;

            foreach (Transform footstepObj in _ballroomFootsteps.transform)
            {
                var footstepRenderer = footstepObj.GetComponent<SpriteRenderer>();
                var newColor = footstepRenderer.color;
                newColor.a = 1;
                footstepRenderer.color = newColor;

                var randomFootstepSfx = bloodFootstepsClips[Random.Range(0, bloodFootstepsClips.Count)];
                footstepSource.clip = randomFootstepSfx;
                footstepSource.Play();

                yield return new WaitForSeconds(0.7f);
            }

            _ballroomThroneDoorSource.clip = bigDoorOpenClip;
            _ballroomThroneDoorSource.Play();

            // _plrObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            _plrInput.isInputEnabled = true;
            
            Destroy(tempBallroomBlock);

            yield return new WaitForSeconds(0.5f);

            Destroy(footstepSource);

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

            var tutorialMeltingGraph = Resources.Load<DialogNodeGraph>("Dialogues/Nikolaus/TutorialMonologueMelting");
            dialogBehaviour.StartDialog(tutorialMeltingGraph);
            dialogBehaviour.OnDialogFinished.AddListener(StartFadeInDoor);

            // _plrObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            _plrInput.isInputEnabled = false;

            _meltingAudioSource.Play();

            // Dim any lights registered for the melting sequence
            foreach (var l in meltingLights)
            {
                if (l != null)
                {
                    l.DOIntensity(0, meltingDimDuration).SetEase(Ease.Linear);
                }
            }

            StartCoroutine(StartPuddleSeq());
            return;

            void StartFadeInDoor()
            {
                StartCoroutine(WaitAndOpenBigDoor());
                dialogBehaviour.OnDialogFinished.RemoveListener(StartFadeInDoor);
            }
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

        private IEnumerator MoveMysteriousHallway()
        {
            var candleWall4 = GameObject.Find("CandleWall 4").GetComponent<Light2D>();
            var breakingVaseSource = GameObject.Find("BreakingVase").GetComponent<AudioSource>();
            
            var mysteriousTarget =  GameObject.Find("MysteriousTarget");
            var npcBrain2D = _mysteriousPersonHallway.GetComponent<NpcBrain2D>();
            var moveToPosition = npcBrain2D.MoveToPosition(mysteriousTarget.transform.position);

            StartCoroutine(moveToPosition);
            yield return new WaitForSeconds(1.7f);

            _mysteriousPersonHallway.SetActive(false);
            
            candleWall4.enabled = false;
            var lightAudioSource = candleWall4.GetComponent<AudioSource>();
            if (lightAudioSource) lightAudioSource.Play();
            
            breakingVaseSource.Play();
        }
        
        public void TriggerMysteriousHallway()
        {
            _mysteriousPersonHallway.SetActive(true);
            
            StartCoroutine(MoveMysteriousHallway());
        }

        void Start()
        {
            _plrObject = GameObject.FindGameObjectWithTag("Player");

            _blackScreen = GameObject.Find("Blackout");
            _corruptScreen = GameObject.Find("CorruptScreen");
            _aspectBars = GameObject.Find("AspectBars");

            _blackScreen.SetActive(false);
            _corruptScreen.SetActive(false);

            _plrInput = _plrObject.GetComponent<PlayerInput2D>();
            _plrInput.isInputEnabled = false;

            _plrMotor2D = _plrObject.GetComponent<CharacterMotor2D>();

            _plrMainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            _cinemachinePositionComposer =
                GameObject.Find("CinemachineCamera").GetComponent<CinemachinePositionComposer>();

            _environmentSoundHandler =
                GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();
            
            _mysteriousPersonHallway = GameObject.Find("MysteriousHallway");
            _mysteriousPersonHallway.SetActive(false);

            var meltClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/Melting");
            _meltingAudioSource = _environmentSoundHandler.CreateCustomSource("Melting");
            _meltingAudioSource.clip = meltClip;
            _meltingAudioSource.volume = 1f;

            _ballroomFootsteps = GameObject.Find("BallroomFootsteps");
            foreach (Transform footstepObj in _ballroomFootsteps.transform)
            {
                var footstepRenderer = footstepObj.GetComponent<SpriteRenderer>();
                var newColor = footstepRenderer.color;
                newColor.a = 0;
                footstepRenderer.color = newColor;
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

            _globalLight = GameObject.Find("2D Global Light").GetComponent<Light2D>();
            _characterLight2D = GameObject.Find("CharacterLight").GetComponent<Light2D>();
            _ballroomSpotlight = GameObject.Find("BallroomSpotlight").GetComponent<Light2D>();

            // Ensure the global light is included in the meltingLights list so it will be dimmed
            if (_globalLight != null && !meltingLights.Contains(_globalLight))
                meltingLights.Add(_globalLight);

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

            _spawnRoomCamera = GameObject.Find("SpawnRoom").transform.Find("Main Camera").GetComponent<Camera>();
            _spawnRoomCamera.gameObject.SetActive(false);

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
                        _introMusicManager.FadeAndPlay(0.11f, 15f);

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
            var tutorialMonologueGraph = Resources.Load<DialogNodeGraph>("Dialogues/Nikolaus/TutorialMonologue");
            
            var grandfatherStroke = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/GrandfatherBell");
            var grandfatherSource = _environmentSoundHandler.CreateCustomSource("GrandfatherSource");
            
            grandfatherSource.clip = grandfatherStroke;
            grandfatherSource.volume = 0.5f;

            _aspectBars.SetActive(true);
            
            var plrSpriteRenderer = _plrObject.GetComponent<SpriteRenderer>();
            var sleepingPlrRenderer = GameObject.Find("SleepingMain").GetComponent<SpriteRenderer>();

            var cinemachineBrain = _plrMainCamera.GetComponent<CinemachineBrain>();

            _plrMainCamera.gameObject.SetActive(true);
            // _spawnRoomCamera.gameObject.SetActive(true);
            cinemachineBrain.enabled = false;

            _plrMainCamera.transform.position = _spawnRoomCamera.transform.position;
            _plrMainCamera.orthographicSize = 4f;

            plrSpriteRenderer.enabled = false;
            sleepingPlrRenderer.enabled = true;

            _fadeCanvasGroup.alpha = 1;

            yield return new WaitForSeconds(2f);

            sleepingPlrRenderer.sprite = sleepingPlayerFrames[0];

            _fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
            });

            yield return new WaitForSeconds(4.5f);

            IEnumerator GrandfatherTick()
            {
                for (var i = 0; i < 2; i++)
                {
                    grandfatherSource.Play();
                    yield return new WaitForSeconds(grandfatherStroke.length);
                }
            }

            StartCoroutine(GrandfatherTick());

            yield return new WaitForSeconds(2.5f);

            sleepingPlrRenderer.sprite = sleepingPlayerFrames[1];

            yield return new WaitForSeconds(2f);

            _fadeCanvasGroup.blocksRaycasts = true;
            _fadeCanvasGroup.DOFade(1f, 3f).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(5f);
            
            var aspectBarsCanvas = _aspectBars.transform.GetChild(0).GetComponent<Canvas>();
            var topBar = aspectBarsCanvas.transform.GetChild(0).GetComponent<RectTransform>();
            var bottomBar = aspectBarsCanvas.transform.GetChild(1).GetComponent<RectTransform>();
            
            topBar.DOAnchorPosY(67.5f, 3f).SetEase(Ease.Linear);
            bottomBar.DOAnchorPosY(-67.5f, 3f).SetEase(Ease.Linear);

            _plrMainCamera.orthographicSize = 7f;
            cinemachineBrain.enabled = true;

            plrSpriteRenderer.enabled = true;
            sleepingPlrRenderer.enabled = false;

            _fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
            });

            yield return new WaitForSeconds(4f);
            
            _aspectBars.SetActive(false);
            
            dialogBehaviour.IsCanSkippingText = false;
            dialogBehaviour.StartDialog(tutorialMonologueGraph);

            dialogBehaviour.OnDialogFinished.AddListener(OpenJournalCall);

            yield return null;
            yield break;

            void OpenJournalCall()
            {
                StartCoroutine(OpenJournal());
                dialogBehaviour.OnDialogFinished.RemoveListener(OpenJournalCall);
            }
        }

        public void ActivateDoorInstructions()
        {
            var tutorialDoorGraph = Resources.Load<DialogNodeGraph>("Dialogues/Nikolaus/TutorialMonologueDoor");
            
            _plrInput.isInputEnabled = false;
            
            dialogBehaviour.IsCanSkippingText = false;
            dialogBehaviour.StartDialog(tutorialDoorGraph);

            dialogBehaviour.OnDialogFinished.AddListener(OpenJournalDoorTutorial);
            return;

            void OpenJournalDoorTutorial()
            {
                SwitchJournalPage("Door");
                StartCoroutine(OpenJournal());
                dialogBehaviour.OnDialogFinished.RemoveListener(OpenJournalDoorTutorial);
            }
        }

        private IEnumerator BleedingKingHead()
        {
            var bloodDrip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/BloodDrip");
            var bloodSource = _environmentSoundHandler.CreateCustomSource("BloodSource", _kingSpriteRenderer.transform);
            bloodSource.volume = 0.7f;
            bloodSource.clip = bloodDrip;

            yield return new WaitForSeconds(1f);

            bloodSource.Play();

            yield return new WaitForSeconds(kingDeadAnimationClip.length * 0.97f - 1);

            Destroy(bloodSource);
        }

        public IEnumerator BeginKingSeq()
        {
            var throneReverbZone = GameObject.Find("ThroneReverbZone").GetComponent<AudioReverbZone>();
            
            var glitchClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/KingGlitch");

            var throneFearClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/ThroneFear");
            var throneMusic = _environmentSoundHandler.CreateCustomSource("ThroneMusic", _throneRoomCamera.transform);
            throneMusic.volume = 0f;
            throneMusic.clip = throneFearClip;

            var headSliceClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/HeadHack");
            var headRollClip = Resources.Load<AudioClip>("SFXs/Miscs/Tutorial/HeadRolling");

            var kingSliceSource =
                _environmentSoundHandler.CreateCustomSource("KingSeqSource", _throneRoomCamera.transform);
            kingSliceSource.volume = 0.35f;
            kingSliceSource.clip = headSliceClip;

            _plrMainCamera.gameObject.SetActive(false);
            _throneRoomCamera.gameObject.SetActive(true);

            _kingSpriteRenderer.sprite = kingBehindSprite;

            _plrMotor2D.forceIdleSprite = _plrMotor2D.idleUp;

            _plrInput.isInputEnabled = false;

            yield return new WaitForSeconds(1f);

            _fadeCanvasGroup.DOFade(0f, 4f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
            });

            throneMusic.Play();

            _introMusicManager.FadeAndStop(0f, 3f);
            throneMusic.DOFade(0.2f, 3f).SetEase(Ease.Linear);

            yield return new WaitForSeconds(3f);

            // TWEEN king camera slightly up
            _throneRoomCamera.transform.DOMoveY(_throneRoomCamera.transform.position.y + 5.45f, 6f)
                .SetEase(Ease.Linear);

            yield return new WaitForSeconds(5f);

            _kingSpriteRenderer.sprite = kingFrontSprite;

            yield return new WaitForSeconds(2f);

            throneMusic.DOFade(0.35f, 2.5f).SetEase(Ease.Linear);

            yield return _mysteriousManIntro.FadeIn();

            yield return new WaitForSeconds(0.5f);

            _blackScreen.SetActive(true);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 5;

            // play sound asynchrously
            kingSliceSource.enabled = true;
            kingSliceSource.Play();
            yield return _mysteriousManIntro.PlayAnimationOnce();

            yield return new WaitForSeconds(1f);

            _blackScreen.SetActive(false);
            _mysteriousManIntro.GetComponent<SpriteRenderer>().sortingOrder = 0;

            kingSliceSource.clip = headRollClip;
            kingSliceSource.Play();

            StartCoroutine(BleedingKingHead());

            _kingAnimator.speed = 1; // unfreeze

            yield return new WaitForSeconds(kingDeadAnimationClip.length * 0.97f);

            _kingAnimator.speed = 0;

            yield return new WaitForSeconds(0.5f);

            _corruptScreen.SetActive(true);
            throneMusic.Stop();

            throneReverbZone.enabled = false;
            throneMusic.clip = glitchClip;
            throneMusic.volume = 1f;
            throneMusic.Play();

            yield return new WaitForSeconds(1.4f);

            // Trigger the wake-up cutscene in Overworld
            Debug.Log("=== ABOUT TO TRIGGER WAKE UP CUTSCENE ===");
            OverworldWakeUpCutscene.TriggerWakeUpCutscene();
            Debug.Log("=== TRIGGERED - NOW LOADING OVERWORLD ===");

            // go to overworld scene
            AsyncOperation op = SceneManager.LoadSceneAsync("Overworld");
            op.allowSceneActivation = true; // or set false if you want to gate activation

            // Optionally wait until load is done (it's already black)
            while (!op.isDone)
                yield return null;
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}