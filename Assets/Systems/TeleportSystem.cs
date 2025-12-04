    using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Systems
{
    public class TeleportSystem : MonoBehaviour
    {
        public GameObject tptTo;
        public Vector3 direction;
        
        public UnityEvent OnTeleport;
        
        private GameObject _player;
        private CharacterMotor2D _characterController2D;
        private BoxCollider2D _characterCollider2D;
        
        private CinemachinePositionComposer _cinemachinePositionComposer;
        
        private CanvasGroup _fadeCanvasGroup;
        
        private EnvironmentSoundHandler _environmentSoundHandler;
        
        private BoxCollider2D _tptCollider;
        private BoxCollider2D _newCollider;
        
        private bool _isPlayerNear;
        private const float InteractionRange = 0.25f; // Distance from player to trigger
        
        private float _fadeTime = 0.3f;
        private float _fadeDuration = 0.2f;
        
        private Overworld.Intro.TutorialScene _tutorialScene;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.blocksRaycasts = false;
            _tutorialScene = FindFirstObjectByType<Overworld.Intro.TutorialScene>();

            
            _tptCollider = transform.gameObject.GetComponent<BoxCollider2D>();
            _tptCollider.isTrigger = true;
            
            _player = GameObject.FindGameObjectWithTag("Player");
            _characterController2D = _player.GetComponent<CharacterMotor2D>();
            _characterCollider2D = _player.GetComponent<BoxCollider2D>();
            
            // Cinemachine
            _cinemachinePositionComposer = GameObject.Find("CinemachineCamera").GetComponent<CinemachinePositionComposer>();
            
            // make the new collider
            _newCollider = gameObject.AddComponent<BoxCollider2D>();
            _newCollider.isTrigger = false;
            _newCollider.offset = _tptCollider.offset;
            _newCollider.size = _tptCollider.size * 0.99f;
            
            _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler").GetComponent<EnvironmentSoundHandler>();
        }
        
        private IEnumerator FadeOut()
        {
            float timer = 0f;
            while (timer < _fadeDuration)
            {
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = 1f - (timer / _fadeDuration); // Decrease alpha
                yield return null; // Wait for the next frame
            }
            _fadeCanvasGroup.alpha = 0f; // Ensure it's fully transparent
        }

        private IEnumerator FadeIn()
        {
            float timer = 0f;
            while (timer < _fadeDuration)
            {
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = timer / _fadeDuration; // Increase alpha
                yield return null; // Wait for the next frame
            }
            _fadeCanvasGroup.alpha = 1f; // Ensure it's fully opaque
        }
        
        Vector3 GetTeleportPosition(Collider2D other)
        {
            var tptPos = tptTo.transform.position + new Vector3(direction.x, direction.y, 0) * 1.5f;
            
            if (direction.x != 0 && direction.y == 0)
            {
                tptPos.y -= (_tptCollider.bounds.size.y / 2);
            }
            
            return tptPos;
        }
        
        private IEnumerator TeleportWithFade(Collider2D other)
        {
            // Try to acquire the interaction lock
            if (!InteractionLockManager.TryLock())
            {
                yield break; // Another interaction is in progress
            }
            
            _characterController2D.SetTeleporting(true);
            
            // Start fade-in
            _fadeCanvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeIn());

            var isTutorial = FindFirstObjectByType<Overworld.Intro.TutorialScene>() is not null;
            
            if (isTutorial && _tutorialScene)
            {
                if (tptTo.transform.name == "Throne" )
                {
                    _tutorialScene.StartCoroutine(_tutorialScene.BeginKingSeq());
                    InteractionLockManager.Unlock(); // Release lock before yielding
                    yield break;
                }
 
                _tutorialScene.SetCinecamYOffset(tptTo.transform.name == "Hallway" ? 2.5f : 0f);
            };
            
            OnTeleport?.Invoke();
            
            // Teleport the object
            
            other.transform.position = GetTeleportPosition(other);
                
            _cinemachinePositionComposer.Damping = Vector3.zero;
            
            yield return new WaitForSeconds(_fadeTime); // Adjust the wait time as needed
                
            // Start fade-out
            _characterController2D.SetTeleporting(false);
            
            _cinemachinePositionComposer.Damping = Vector3.one;
            
            yield return StartCoroutine(FadeOut());
            _fadeCanvasGroup.blocksRaycasts = false;
            
            // Release the lock after teleport is fully complete
            InteractionLockManager.Unlock();
        }

        private void OnEnter()
        {
            _environmentSoundHandler.PlayDoorSound();
            StartCoroutine(TeleportWithFade(_characterCollider2D));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!tptTo || !other.CompareTag("NPC")) return;
            other.transform.position = GetTeleportPosition(other);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!tptTo) return;
            
            if (!EnsureTeleportReferencesAreValid())
            {
                return;
            }
            
            // prevent interaction during teleport, dialogue, or if any interaction is in progress
            if (_characterController2D.IsTeleporting || _characterController2D.IsDialogueActive || InteractionLockManager.IsLocked) return;
            
            if (_player != null && _tptCollider != null && _characterCollider2D != null)
            {
                var dist = _tptCollider.Distance(_characterCollider2D);
                _isPlayerNear = dist.isOverlapped || dist.distance < InteractionRange;
            }
            
            if (!_isPlayerNear) return;
            
            if (Input.GetMouseButtonDown(1)) // 1 = right mouse
            {
                var world = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
                var p = new Vector2(world.x, world.y);

                // Use OverlapPoint (simpler than a ray)
                var hit = Physics2D.OverlapPoint(p);
                
                if (hit && (hit == _tptCollider || hit == _newCollider))
                {
                    OnEnter();
                }
            } else if (Input.GetKeyDown(KeyCode.E))
            {
                OnEnter();
            }
        }

        private bool EnsureTeleportReferencesAreValid()
        {
            if (_tptCollider == null)
            {
                _tptCollider = GetComponent<BoxCollider2D>();
                if (_tptCollider != null)
                    _tptCollider.isTrigger = true;
            }

            if (_newCollider == null && _tptCollider != null)
            {
                _newCollider = gameObject.AddComponent<BoxCollider2D>();
                _newCollider.isTrigger = false;
                _newCollider.offset = _tptCollider.offset;
                _newCollider.size = _tptCollider.size * 0.99f;
            }

            if (_player == null || _characterController2D == null || _characterCollider2D == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                if (_player != null)
                {
                    _characterController2D = _player.GetComponent<CharacterMotor2D>();
                    _characterCollider2D = _player.GetComponent<BoxCollider2D>();
                }
            }

            return _player != null &&
                   _characterController2D != null &&
                   _characterCollider2D != null &&
                   _tptCollider != null &&
                   _newCollider != null;
        }
    }
}
