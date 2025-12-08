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
            Debug.Log($"[TutorialScene] === Creating scary dialogue for {npcTransform.name} ===");
            
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
            
            // CRITICAL: Ensure NPC has a Rigidbody2D for trigger detection to work
            var npcRigidbody = npcTransform.GetComponent<Rigidbody2D>();
            if (npcRigidbody == null)
            {
                npcRigidbody = npcTransform.gameObject.AddComponent<Rigidbody2D>();
                npcRigidbody.bodyType = RigidbodyType2D.Kinematic;
                npcRigidbody.simulated = true;
                npcRigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
                Debug.Log($"[TutorialScene] Added Rigidbody2D (Kinematic, NeverSleep) to {npcTransform.name} for trigger detection");
            }
            else
            {
                npcRigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
                npcRigidbody.WakeUp();
                Debug.Log($"[TutorialScene] {npcTransform.name} already has Rigidbody2D: BodyType={npcRigidbody.bodyType}, SleepMode={npcRigidbody.sleepMode}");
            }
            
            // NPCs need TWO colliders:
            // 1. A TRIGGER collider for InteractionDetector to detect via OnTriggerEnter2D
            // 2. A NON-TRIGGER collider for physics (preventing player from walking through)
            
            // Check for existing CircleCollider2D (trigger for interaction)
            var existingCircleCollider = npcTransform.GetComponent<CircleCollider2D>();
            if (existingCircleCollider != null)
            {
                // Ensure the circle collider is a trigger
                existingCircleCollider.isTrigger = true;
                existingCircleCollider.enabled = true;
                
                // Make it larger if it's too small
                if (existingCircleCollider.radius < 1.2f)
                {
                    existingCircleCollider.radius = 1.2f;
                    Debug.Log($"[TutorialScene] Increased {npcTransform.name} CircleCollider radius to {existingCircleCollider.radius}");
                }
                
                Debug.Log($"[TutorialScene] Using existing CircleCollider2D on {npcTransform.name} as TRIGGER - radius: {existingCircleCollider.radius}");
            }
            
            // Check for existing BoxCollider2D components
            var existingBoxColliders = npcTransform.GetComponents<BoxCollider2D>();
            BoxCollider2D triggerCollider = null;
            BoxCollider2D physicsCollider = null;
            
            // Identify existing box colliders
            foreach (var col in existingBoxColliders)
            {
                if (col.isTrigger)
                {
                    // If we already have a circle trigger, we don't need a box trigger
                    if (existingCircleCollider != null)
                    {
                        Debug.Log($"[TutorialScene] Removing redundant BoxCollider2D trigger from {npcTransform.name} (CircleCollider2D is already the trigger)");
                        Destroy(col);
                    }
                    else
                    {
                        triggerCollider = col;
                    }
                }
                else
                {
                    physicsCollider = col;
                }
            }
            
            // If no circle collider exists and no box trigger exists, create a box trigger
            if (existingCircleCollider == null && triggerCollider == null)
            {
                triggerCollider = npcTransform.gameObject.AddComponent<BoxCollider2D>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector2(2.5f, 2.5f);
                triggerCollider.offset = Vector2.zero;
                Debug.Log($"[TutorialScene] Added TRIGGER BoxCollider2D to {npcTransform.name} - size: {triggerCollider.size}");
            }
            else if (triggerCollider != null)
            {
                // Ensure existing trigger collider is properly configured
                triggerCollider.enabled = true;
                if (triggerCollider.size.x < 2f || triggerCollider.size.y < 2f)
                {
                    triggerCollider.size = new Vector2(2.5f, 2.5f);
                    Debug.Log($"[TutorialScene] Increased {npcTransform.name} trigger BoxCollider size to {triggerCollider.size}");
                }
                Debug.Log($"[TutorialScene] Using existing trigger BoxCollider on {npcTransform.name} - size: {triggerCollider.size}");
            }
            
            // Ensure we have a PHYSICS (non-trigger) collider to prevent walking through
            if (physicsCollider == null)
            {
                physicsCollider = npcTransform.gameObject.AddComponent<BoxCollider2D>();
                physicsCollider.isTrigger = false; // CRITICAL: This must be FALSE
                physicsCollider.size = new Vector2(1.0f, 1.0f); // Smaller than trigger for natural spacing
                physicsCollider.offset = Vector2.zero;
                Debug.Log($"[TutorialScene] Added PHYSICS BoxCollider2D to {npcTransform.name} - size: {physicsCollider.size}, isTrigger: FALSE");
            }
            else
            {
                // Make absolutely sure the physics collider is NOT a trigger
                if (physicsCollider.isTrigger)
                {
                    Debug.LogWarning($"[TutorialScene] {npcTransform.name} physics collider was incorrectly set as trigger! Fixing...");
                    physicsCollider.isTrigger = false;
                }
                physicsCollider.enabled = true;
                Debug.Log($"[TutorialScene] Using existing physics BoxCollider on {npcTransform.name} - size: {physicsCollider.size}, isTrigger: {physicsCollider.isTrigger}");
            }
            
            // Log layer info and final setup
            var currentLayer = npcTransform.gameObject.layer;
            var currentLayerName = LayerMask.LayerToName(currentLayer);
            
            string triggerInfo = existingCircleCollider != null 
                ? $"CircleCollider(radius={existingCircleCollider.radius})" 
                : (triggerCollider != null ? $"BoxCollider({triggerCollider.size})" : "NONE");
            
            string physicsInfo = physicsCollider != null 
                ? $"BoxCollider({physicsCollider.size}, isTrigger={physicsCollider.isTrigger})" 
                : "NONE";
            
            Debug.Log($"[TutorialScene] {npcTransform.name} setup complete: Layer={currentLayerName} ({currentLayer}), Position={npcTransform.position}, Trigger={triggerInfo}, Physics={physicsInfo}");
            
            // Verify the setup immediately
            StartCoroutine(VerifyNPCSetup(npcTransform));
        }
        
        // New method to verify NPC setup after a frame
        private IEnumerator VerifyNPCSetup(Transform npcTransform)
        {
            yield return null; // Wait one frame
            
            var rb = npcTransform.GetComponent<Rigidbody2D>();
            var circleCollider = npcTransform.GetComponent<CircleCollider2D>();
            var boxColliders = npcTransform.GetComponents<BoxCollider2D>();
            var trigger = npcTransform.GetComponent<DialogTrigger>();
            
            Collider2D triggerCollider = circleCollider; // Prefer circle as trigger
            BoxCollider2D physicsCollider = null;
            
            // If no circle collider, look for a trigger box collider
            if (triggerCollider == null)
            {
                foreach (var col in boxColliders)
                {
                    if (col.isTrigger)
                        triggerCollider = col;
                    else
                        physicsCollider = col;
                }
            }
            else
            {
                // We have a circle trigger, so find the physics box collider
                foreach (var col in boxColliders)
                {
                    if (!col.isTrigger)
                    {
                        physicsCollider = col;
                        break;
                    }
                }
            }
            
            // Verify critical components
            bool hasCriticalComponents = rb != null && triggerCollider != null && trigger != null;
            bool triggerEnabled = triggerCollider != null && triggerCollider.enabled;
            bool hasPhysicsCollider = physicsCollider != null;
            bool physicsIsNonTrigger = physicsCollider != null && !physicsCollider.isTrigger;
            
            if (!hasCriticalComponents)
            {
                Debug.LogError($"[TutorialScene] VERIFICATION FAILED for {npcTransform.name}! Rigidbody2D: {rb != null}, TriggerCollider: {triggerCollider != null}, DialogTrigger: {trigger != null}");
            }
            else if (!triggerEnabled)
            {
                Debug.LogError($"[TutorialScene] VERIFICATION FAILED for {npcTransform.name}! Trigger collider is DISABLED!");
            }
            else if (!hasPhysicsCollider)
            {
                Debug.LogWarning($"[TutorialScene] WARNING for {npcTransform.name}: No physics collider found. Player may walk through NPC.");
            }
            else if (!physicsIsNonTrigger)
            {
                Debug.LogError($"[TutorialScene] VERIFICATION FAILED for {npcTransform.name}! Physics collider is incorrectly set as TRIGGER!");
            }
            else
            {
                string triggerType = circleCollider != null ? "CircleCollider2D" : "BoxCollider2D";
                string triggerSize = circleCollider != null ? $"radius={circleCollider.radius}" : $"size={((BoxCollider2D)triggerCollider).size}";
                string physicsSize = physicsCollider != null ? physicsCollider.size.ToString() : "None";
                
                Debug.Log($"[TutorialScene] VERIFICATION PASSED for {npcTransform.name}. Trigger: {triggerType}({triggerSize}), Physics: BoxCollider2D({physicsSize}, isTrigger=false)");
            }
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

            // DEBUG: Check InteractionDetector setup on player and fix if needed
            var interactionDetector = _plrObject.GetComponentInChildren<InteractionDetector>();
            if (interactionDetector != null)
            {
                var detectorCollider = interactionDetector.GetComponent<Collider2D>();
                if (detectorCollider != null)
                {
                    Debug.Log($"[TutorialScene] Player InteractionDetector found! Collider type: {detectorCollider.GetType().Name}, IsTrigger: {detectorCollider.isTrigger}");
                    
                    // Ensure it's a trigger and properly sized
                    if (detectorCollider is CircleCollider2D circleCollider)
                    {
                        if (circleCollider.radius < 1.5f)
                        {
                            Debug.LogWarning($"[TutorialScene] InteractionDetector radius too small ({circleCollider.radius}). Increasing to 1.5f");
                            circleCollider.radius = 1.5f;
                        }
                        if (!circleCollider.isTrigger)
                        {
                            Debug.LogWarning("[TutorialScene] InteractionDetector is not a trigger! Fixing...");
                            circleCollider.isTrigger = true;
                        }
                        Debug.Log($"[TutorialScene] InteractionDetector CircleCollider: radius={circleCollider.radius}, isTrigger={circleCollider.isTrigger}");
                    }
                    else if (detectorCollider is BoxCollider2D boxCollider)
                    {
                        if (boxCollider.size.x < 2f || boxCollider.size.y < 2f)
                        {
                            Debug.LogWarning($"[TutorialScene] InteractionDetector box too small ({boxCollider.size}). Increasing to (2, 2)");
                            boxCollider.size = new Vector2(2f, 2f);
                        }
                        if (!boxCollider.isTrigger)
                        {
                            Debug.LogWarning("[TutorialScene] InteractionDetector is not a trigger! Fixing...");
                            boxCollider.isTrigger = true;
                        }
                        Debug.Log($"[TutorialScene] InteractionDetector BoxCollider: size={boxCollider.size}, isTrigger={boxCollider.isTrigger}");
                    }
                }
                else
                {
                    Debug.LogError("[TutorialScene] InteractionDetector has NO COLLIDER! This is why it can't detect NPCs.");
                }
            }
            else
            {
                Debug.LogError("[TutorialScene] NO InteractionDetector found on player or children!");
            }

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

            // Configure dialogue to only accept left mouse click (no keyboard keys)
            dialogBehaviour.SetNextSentenceKeyCodes(new List<KeyCode> { KeyCode.Mouse0 });

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
            // DON'T play journal sound here - let the button click handle it if needed
            // _environmentSoundHandler.PlayJournalSound(false);

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
            // Try to acquire the interaction lock to prevent conflicts
            if (!InteractionLockManager.TryLock())
            {
                Debug.LogWarning("[TutorialScene] Cannot activate door instructions - interaction already in progress");
                return;
            }
            
            var tutorialDoorGraph = Resources.Load<DialogNodeGraph>("Dialogues/Nikolaus/TutorialMonologueDoor");
            
            _plrInput.isInputEnabled = false;
            
            dialogBehaviour.IsCanSkippingText = false;
            dialogBehaviour.StartDialog(tutorialDoorGraph);

            dialogBehaviour.OnDialogFinished.AddListener(OpenJournalDoorTutorial);
            return;

            void OpenJournalDoorTutorial()
            {
                SwitchJournalPage("Door");
                // Only call OpenJournalAndReleaseLock - it handles both opening the journal AND releasing the lock
                StartCoroutine(OpenJournalAndReleaseLock());
                dialogBehaviour.OnDialogFinished.RemoveListener(OpenJournalDoorTutorial);
            }
        }
        
        private IEnumerator OpenJournalAndReleaseLock()
        {
            yield return StartCoroutine(OpenJournal());
            
            // The journal will be closed when the user clicks Continue
            // We need to wait for CloseJournal to finish, then release the lock
            // For now, release the lock immediately after opening since the journal
            // doesn't block other interactions (it's a tutorial UI)
            InteractionLockManager.Unlock();
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

            // Trigger the wake-up cutscene in Overworld using PlayerPrefs flag
            Debug.Log("=== ABOUT TO TRIGGER WAKE UP CUTSCENE ===");
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 1);
            PlayerPrefs.Save();
            Debug.Log($"=== TRIGGERED - NOW LOADING OVERWORLD ===");

            // go to overworld scene
            AsyncOperation op = SceneManager.LoadSceneAsync("Overworld");
            op.allowSceneActivation = true; // or set false if you want to gate activation

            // Optionally wait until load is done (it's already black)
            while (!op.isDone)
                yield return null;
        }
    }
}