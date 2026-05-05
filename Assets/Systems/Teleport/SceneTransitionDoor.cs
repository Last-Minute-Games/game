using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems
{
    public class SceneTransitionDoor : MonoBehaviour, IInteractable
    {
        [Header("Debug")]
        [Tooltip("Enable debug logs (Editor only)")]
        public bool enableDebugLogs = false;

        [Header("Scene Transition")]
        [Tooltip("Scene name to load when interacting with this door")]
        public string sceneName = "BattleScene";
        [Tooltip("Use the ScreenFader eyes-closing transition if available")]
        public bool useEyesClosing = true;

        [Header("Interaction")]
        [Tooltip("Distance from the door collider required to interact")]
        [SerializeField] private float interactionRange = 0.25f;
        [Tooltip("Interaction priority (lower = higher priority). Teleports=0, Dialogs=1-2, Minigames=5")]
        [SerializeField] private int interactionPriority = 0;

        private GameObject _player;
        private CharacterMotor2D _characterController2D;
        private Collider2D _playerCollider;
        private Collider2D _doorCollider;
        private EnvironmentSoundHandler _environmentSoundHandler;

        private bool _isPlayerNear;
        private bool _isTransitioning;
        private bool _hasInteractionLock;

        private void Start()
        {
            CachePlayerReferences();
            _doorCollider = GetComponent<Collider2D>();
            _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();

            if (_doorCollider == null)
            {
                Debug.LogWarning($"[SceneTransitionDoor] {name} is missing a Collider2D. Add one so the player can interact.");
            }
        }

        private void Update()
        {
            CachePlayerReferences();
            UpdatePlayerNearStatus();
        }

        private void CachePlayerReferences()
        {
            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                if (_player != null)
                {
                    _characterController2D = _player.GetComponent<CharacterMotor2D>();
                    _playerCollider = _player.GetComponent<Collider2D>();
                }
            }
        }

        private void UpdatePlayerNearStatus()
        {
            if (_player == null)
            {
                _isPlayerNear = false;
                return;
            }

            if (_doorCollider != null && _playerCollider != null)
            {
                var dist = _doorCollider.Distance(_playerCollider);
                _isPlayerNear = dist.isOverlapped || dist.distance <= interactionRange;
            }
            else
            {
                _isPlayerNear = Vector3.Distance(transform.position, _player.transform.position) <= interactionRange;
            }
        }

        public void Interact()
        {
            if (_isTransitioning)
            {
                LogDebug("Interact ignored - transition already in progress");
                return;
            }

            if (!Systems.InteractionLockManager.TryLock())
            {
                LogDebug("Interact blocked - interaction lock is held");
                return;
            }

            _hasInteractionLock = true;
            _isTransitioning = true;

            _environmentSoundHandler?.PlayDoorSound();
            StartCoroutine(TransitionRoutine());
        }

        private IEnumerator TransitionRoutine()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[SceneTransitionDoor] {name}: sceneName is empty - cannot transition");
                ReleaseInteractionLock();
                _isTransitioning = false;
                yield break;
            }

            LogDebug($"Transitioning to {sceneName}");

            ScreenFader fader = ScreenFader.Instance != null
                ? ScreenFader.Instance
                : FindObjectOfType<ScreenFader>();

            if (fader != null)
            {
                if (useEyesClosing)
                {
                    // Close eyes
                    yield return fader.EyesClosingEffect();

                    // Eyes should always open in the destination scene
                    fader.shouldOpenEyesOnSceneLoad = true;
                    LogDebug($"Eyes closing transition - shouldOpenEyesOnSceneLoad=true");

                    // Use the keep-panels-closed transition to maintain state
                    yield return fader.TransitionToSceneKeepPanelsClosed(sceneName);
                }
                else
                {
                    yield return fader.TransitionToScene(sceneName);
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

            ReleaseInteractionLock();
            _isTransitioning = false;
        }

        private void ReleaseInteractionLock()
        {
            if (_hasInteractionLock)
            {
                Systems.InteractionLockManager.Unlock();
                _hasInteractionLock = false;
            }
        }

        private void OnDestroy()
        {
            ReleaseInteractionLock();
        }

        public int GetInteractionPriority()
        {
            return interactionPriority;
        }

        public bool CanInteract()
        {
            if (_isTransitioning)
                return false;

            if (string.IsNullOrWhiteSpace(sceneName))
                return false;

            if (_characterController2D != null)
            {
                if (_characterController2D.IsTeleporting)
                    return false;
                if (_characterController2D.IsDialogueActive)
                    return false;
            }

            if (Systems.InteractionLockManager.IsLocked)
                return false;

            return _isPlayerNear;
        }

        public bool ShowInteractionPrompt()
        {
            return true;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SceneTransitionDoor] {message}");
        }
    }
}
