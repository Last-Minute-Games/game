using cherrydev;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dialogues
{
    public class DialogTrigger : MonoBehaviour, IInteractable
    {
        [Header("Debug")]
        [Tooltip("Enable debug logs (Editor only)")]
        public bool enableDebugLogs = false;
        
        [Header("Dialog Settings")]
        public DialogBehaviour dialogBehaviour; // likely shared UI
        
        public DialogNodeGraph dialogGraph;
        public KeyCode interactKey = KeyCode.E;

        [Header("Events")]
        public UnityEvent OnDialogCompleted; // 👈 Custom callback
        
        [Header("Conversation Audio")]
        [Tooltip("Music/audio to play during the conversation with this NPC")]
        [SerializeField] private AudioClip conversationMusic;
        [Tooltip("Volume for the conversation music (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;
        [Tooltip("How long to fade in/out the music")]
        [SerializeField] private float musicFadeDuration = 0.5f;
        [Tooltip("Should the music loop during the conversation?")]
        [SerializeField] private bool loopMusic = true;
        [Tooltip("Volume to reduce room audio to during conversation (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float roomAudioDuckVolume = 0f;
        [Tooltip("How long to fade room audio in/out")]
        [SerializeField] private float roomAudioFadeDuration = 1f;
        
        [FormerlySerializedAs("_interactionRange")] [Header("Detection Settings")] 
        public float interactionRange = 1f;

        private CharacterMotor2D _npcController;
        private NpcBrain2D _npcBrain;
        private NpcBrain2D.NpcMode _previousMode;

        private GameObject _player;
        private CharacterMotor2D _playerController;

        private bool _isPlayerNear = false;
        private bool _isMyConversation = false; // <-- key flag
        private bool _dialogActive = false; // <-- NEW: Track if ANY dialog is active
        
        // Audio
        private AudioSource conversationAudioSource;
        private Coroutine fadeCoroutine;
        private Coroutine roomAudioFadeCoroutine;
        private RoomAudioZone[] roomAudioZones;
        private float[] originalRoomVolumes;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerController = _player.GetComponent<CharacterMotor2D>();

            _npcController = GetComponent<CharacterMotor2D>();

            if (_npcController)
                _npcBrain = GetComponent<NpcBrain2D>();

            // Add null check for dialogBehaviour before subscribing
            if (dialogBehaviour == null)
            {
                DebugLogger.LogError($"[DialogTrigger] '{gameObject.name}' has no DialogBehaviour assigned! Dialog will not trigger GlobalPause.");
                return;
            }

            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
            
            LogDebug($"'{gameObject.name}' successfully subscribed to DialogBehaviour events");
            
            // Setup audio source for conversation music
            if (conversationMusic != null)
            {
                conversationAudioSource = gameObject.AddComponent<AudioSource>();
                conversationAudioSource.clip = conversationMusic;
                conversationAudioSource.loop = loopMusic;
                conversationAudioSource.playOnAwake = false;
                conversationAudioSource.volume = 0f;
                conversationAudioSource.spatialBlend = 0f; // 2D audio
                LogDebug($"Audio source created with clip '{conversationMusic.name}'");
            }
            else
            {
                LogDebug("No conversation music assigned");
            }

            // Find all room audio zones for ducking
            roomAudioZones = FindObjectsOfType<RoomAudioZone>();
            originalRoomVolumes = new float[roomAudioZones.Length];
            for (int i = 0; i < roomAudioZones.Length; i++)
            {
                if (roomAudioZones[i].roomMusic != null)
                    originalRoomVolumes[i] = roomAudioZones[i].roomMusic.volume;
            }
        }

        private void OnDestroy()
        {
            // Always unsubscribe to prevent ghost callbacks in the editor
            if (dialogBehaviour)
            {
                dialogBehaviour.OnDialogStarted.RemoveListener(OnDialogStart);
                dialogBehaviour.OnDialogFinished.RemoveListener(OnDialogFinished);
            }
        }

        void FacePlayer()
        {
            // Get normalized direction from NPC to player
            Vector3 currentDirection = (_player.transform.position - transform.position).normalized;

            // Determine main axis of direction
            if (Mathf.Abs(currentDirection.y) > Mathf.Abs(currentDirection.x))
            {
                // Vertical is dominant
                _npcController.forceIdleSprite =
                    currentDirection.y > 0 ? _npcController.idleUp : _npcController.idleDown;
            }
            else
            {
                // Horizontal is dominant
                _npcController.forceIdleSprite =
                    currentDirection.x > 0 ? _npcController.idleRight : _npcController.idleLeft;
            }
        }

        private void OnDialogStart()
        {
            // Ignore global start events unless they were initiated by THIS trigger
            if (!_isMyConversation) return;

            _dialogActive = true; // Mark dialog as active

            // Use GlobalPause to pause NPCs and timer (but not player input or timescale)
            GlobalPause.SetMinigamePaused(true);
            LogDebug("GlobalPause minigame pause enabled (NPCs and timer paused)");

            if (_npcBrain)
            {
                _previousMode = _npcBrain.mode;
                _npcBrain.mode = NpcBrain2D.NpcMode.Idle;

                FacePlayer();
            }

            if (_playerController) _playerController.SetDialogueActive(true);
            
            // Start conversation music
            if (conversationAudioSource != null && conversationMusic != null)
            {
                LogDebug($"Starting conversation music '{conversationMusic.name}' - Current volume: {conversationAudioSource.volume}, Is playing: {conversationAudioSource.isPlaying}");
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeMusic(true));
            }
            else
            {
                LogDebug($"Cannot start music - AudioSource: {(conversationAudioSource != null ? "OK" : "NULL")}, Clip: {(conversationMusic != null ? "OK" : "NULL")}");
            }

            // Duck room audio
            LogDebug("Ducking room audio...");
            if (roomAudioFadeCoroutine != null)
                StopCoroutine(roomAudioFadeCoroutine);
            roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(true));
        }

        private void OnDialogFinished()
        {
            // Only unfreeze if this NPC was the one talking
            if (!_isMyConversation) return;

            _dialogActive = false; // Mark dialog as no longer active

            // Resume NPCs and timer via GlobalPause
            GlobalPause.SetMinigamePaused(false);
            LogDebug("GlobalPause minigame pause disabled (NPCs and timer resumed)");

            if (_npcBrain)
            {
                _npcBrain.mode = _previousMode;
                _npcController.forceIdleSprite = null; // clear forced facing
            }

            if (_playerController) _playerController.SetDialogueActive(false);

            // Stop conversation music
            if (conversationAudioSource != null && conversationAudioSource.isPlaying)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeMusic(false));
                LogDebug("Stopping conversation music");
            }

            // Restore room audio
            if (roomAudioFadeCoroutine != null)
                StopCoroutine(roomAudioFadeCoroutine);
            roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(false));

            _isMyConversation = false; // reset
            
            OnDialogCompleted?.Invoke();
        }

        void Update()
        {
            // Don't process input if dialog is already active
            if (_dialogActive) return;
            
            if (_playerController && _playerController.IsDialogueActive) return;

            if (_player)
                _isPlayerNear = Vector3.Distance(transform.position, _player.transform.position) <= interactionRange;

            // Note: Input checking removed - now handled by InteractionDetector for proper priority
        }

        public void Interact()
        {
            StartDialogue();
        }

        private void StartDialogue()
        {
            if (_dialogActive)
            {
                LogDebug("Attempted to start dialog while one is already active");
                return;
            }

            if (dialogBehaviour == null)
            {
                DebugLogger.LogError($"[DialogTrigger] '{gameObject.name}' cannot start dialog - DialogBehaviour is null!");
                return;
            }
            
            if (dialogGraph == null)
            {
                DebugLogger.LogError($"[DialogTrigger] '{gameObject.name}' cannot start dialog - DialogGraph is null!");
                return;
            }

            _isMyConversation = true;
            LogDebug($"'{gameObject.name}' starting dialog (will trigger GlobalPause)");
            dialogBehaviour.StartDialog(dialogGraph);
        }

        public int GetInteractionPriority()
        {
            // Dialog triggers have second-highest priority (after teleports)
            return 1;
        }

        public bool CanInteract()
        {
            // Debug.Log("[DialogTrigger] Checking CanInteract()");
            // Can interact if we have dialog setup, player is near, and no other interaction is in progress
            if (dialogBehaviour == null || dialogGraph == null)
            {
                // Debug.Log("[DialogTrigger] Cannot interact - dialogBehaviour or dialogGraph is null");
                return false;
            }
            
            if (_dialogActive) return false;
            
            if (_playerController != null && _playerController.IsDialogueActive) return false;
            
            if (Systems.InteractionLockManager.IsLocked) return false;
            return _isPlayerNear;
        }

        public bool ShowInteractionPrompt()
        {
            // NPCs DO show the popup icon
            return true;
        }
        
        private System.Collections.IEnumerator FadeMusic(bool fadeIn)
        {
            if (conversationAudioSource == null) 
            {
                LogDebug("FadeMusic: conversationAudioSource is NULL!");
                yield break;
            }

            float startVolume = conversationAudioSource.volume;
            float targetVolume = fadeIn ? musicVolume : 0f;
            float elapsed = 0f;

            LogDebug($"FadeMusic: {(fadeIn ? "Fading IN" : "Fading OUT")} from {startVolume} to {targetVolume} over {musicFadeDuration}s");

            if (fadeIn && !conversationAudioSource.isPlaying)
            {
                conversationAudioSource.Play();
                LogDebug($"FadeMusic: Started playing audio clip '{conversationMusic.name}'");
            }

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / musicFadeDuration;
                conversationAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            conversationAudioSource.volume = targetVolume;
            LogDebug($"FadeMusic: Fade complete. Final volume = {conversationAudioSource.volume}, Is playing: {conversationAudioSource.isPlaying}");

            if (!fadeIn)
            {
                conversationAudioSource.Stop();
                LogDebug("FadeMusic: Stopped audio playback");
            }

            fadeCoroutine = null;
        }

        private System.Collections.IEnumerator FadeRoomAudio(bool duck)
        {
            if (roomAudioZones == null || roomAudioZones.Length == 0) 
            {
                LogDebug($"No room audio zones found to {(duck ? "duck" : "restore")}");
                yield break;
            }

            LogDebug($"Found {roomAudioZones.Length} room audio zones to {(duck ? "duck" : "restore")}");

            float elapsed = 0f;

            // Store current volumes as starting point
            float[] startVolumes = new float[roomAudioZones.Length];
            for (int i = 0; i < roomAudioZones.Length; i++)
            {
                if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
                {
                    startVolumes[i] = roomAudioZones[i].roomMusic.volume;
                    LogDebug($"  Zone {i} '{roomAudioZones[i].name}': Current volume = {startVolumes[i]}, Target = {(duck ? roomAudioDuckVolume : originalRoomVolumes[i])}");
                }
            }

            while (elapsed < roomAudioFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / roomAudioFadeDuration;

                for (int i = 0; i < roomAudioZones.Length; i++)
                {
                    if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
                    {
                        float targetVolume = duck ? roomAudioDuckVolume : originalRoomVolumes[i];
                        roomAudioZones[i].roomMusic.volume = Mathf.Lerp(startVolumes[i], targetVolume, t);
                    }
                }
                yield return null;
            }

            // Ensure final volumes are set
            for (int i = 0; i < roomAudioZones.Length; i++)
            {
                if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
                {
                    float targetVolume = duck ? roomAudioDuckVolume : originalRoomVolumes[i];
                    roomAudioZones[i].roomMusic.volume = targetVolume;
                    
                    // If ducking to 0 or near 0, also pause the audio to save CPU
                    if (duck && roomAudioDuckVolume < 0.01f)
                    {
                        if (roomAudioZones[i].roomMusic.isPlaying)
                        {
                            roomAudioZones[i].roomMusic.Pause();
                            LogDebug($"  Paused room audio zone '{roomAudioZones[i].name}'");
                        }
                    }
                    else if (!duck)
                    {
                        // Resume if it was paused
                        if (!roomAudioZones[i].roomMusic.isPlaying)
                        {
                            roomAudioZones[i].roomMusic.UnPause();
                            LogDebug($"  Unpaused room audio zone '{roomAudioZones[i].name}'");
                        }
                    }
                }
            }

            LogDebug($"Room audio {(duck ? "ducked to " + roomAudioDuckVolume : "restored")}");
            roomAudioFadeCoroutine = null;
        }
        
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                DebugLogger.LogDialogue(message);
        }
    }
}