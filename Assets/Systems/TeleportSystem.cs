    using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems
{
    public class TeleportSystem : MonoBehaviour
    {
        public GameObject tptTo;
        public Vector3 direction;
        
        private GameObject _player;
        private CharacterMotor2D _characterController2D;
        private BoxCollider2D _characterCollider2D;
        
        private CinemachinePositionComposer _cinemachinePositionComposer;
        
        private CanvasGroup _fadeCanvasGroup;
        
        private EnvironmentSoundHandler _environmentSoundHandler;
        
        private BoxCollider2D _tptCollider;
        private BoxCollider2D _newCollider;
        
        private bool _isPlayerNear;
        private const float InteractionRange = 2f; // Distance from player to trigger
        
        private float _fadeTime = 0.3f;
        private float _fadeDuration = 0.2f;
        
        private Systems.Overworld.Intro.TutorialScene _tutorialScene;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.blocksRaycasts = false;
            _tutorialScene = FindFirstObjectByType<Systems.Overworld.Intro.TutorialScene>();

            
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
            _characterController2D.SetTeleporting(true);
            
            // Start fade-in
            _fadeCanvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeIn());
            
            // check if scene is tutorial
            if (tptTo.transform.name != "Throne" || !FindFirstObjectByType<Systems.Overworld.Intro.TutorialScene>())
            {
                // Teleport the object
            
                other.transform.position = GetTeleportPosition(other);
                
                _cinemachinePositionComposer.Damping = Vector3.zero;
                
                if (tptTo.transform.name == "Hallway")
                {
                    _tutorialScene.SetCinecamYOffset(2.5f);   
                }
            
                yield return new WaitForSeconds(_fadeTime); // Adjust the wait time as needed
                
                // Start fade-out
                _characterController2D.SetTeleporting(false);
            
                _cinemachinePositionComposer.Damping = Vector3.one;
            
                yield return StartCoroutine(FadeOut());
                _fadeCanvasGroup.blocksRaycasts = false;

                yield break;
            }
            
            if (_tutorialScene) _tutorialScene.StartCoroutine(_tutorialScene.BeginKingSeq());
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
            
            // prevent interaction during teleport or dialogue
            if (_characterController2D.IsTeleporting || _characterController2D.IsDialogueActive) return;
            
            if (_player)
                _isPlayerNear = Vector3.Distance(_tptCollider.transform.position, _player.transform.position) < InteractionRange;
            
            // Debug.Log(Vector3.Distance(_tptCollider.transform.position, _player.transform.position));
            
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
    }
}
