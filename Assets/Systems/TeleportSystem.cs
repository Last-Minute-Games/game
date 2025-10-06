using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems
{
    public class TeleportSystem : MonoBehaviour
    {
        public GameObject tptTo;
        public bool isRight;
        
        private GameObject _player;
        private CharacterController2D _characterController2D;
        private BoxCollider2D _characterCollider2D;
        
        private CinemachinePositionComposer _cinemachinePositionComposer;
        
        private CanvasGroup _fadeCanvasGroup;
        
        private BoxCollider2D _tptCollider;
        private BoxCollider2D _newCollider;
        
        private bool _isPlayerNear;
        private const float InteractionRange = 2f; // Distance from player to trigger
        
        private float _fadeTime = 0.3f;
        private float _fadeDuration = 0.2f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.blocksRaycasts = false;
            
            _tptCollider = transform.gameObject.GetComponent<BoxCollider2D>();
            _tptCollider.isTrigger = true;
            
            _player = GameObject.FindGameObjectWithTag("Player");
            _characterController2D = _player.GetComponent<CharacterController2D>();
            _characterCollider2D = _player.GetComponent<BoxCollider2D>();
            
            // Cinemachine
            _cinemachinePositionComposer = GameObject.Find("CinemachineCamera").GetComponent<CinemachinePositionComposer>();
            
            // make the new collider
            _newCollider = gameObject.AddComponent<BoxCollider2D>();
            _newCollider.isTrigger = false;
            _newCollider.size = Vector2.one * 0.99f;
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
        
        private IEnumerator TeleportWithFade(Collider2D other)
        {
            _characterController2D.SetTeleporting(true);
            
            var offset = isRight ? Vector3.left : Vector3.right;
            
            // Start fade-in
            _fadeCanvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeIn());

            // Teleport the object
            
            other.transform.position = tptTo.transform.position + offset * 1.5f;
            _cinemachinePositionComposer.Damping = Vector3.zero;
            
            yield return new WaitForSeconds(_fadeTime); // Adjust the wait time as needed
            
            // Start fade-out
            _characterController2D.SetTeleporting(false);
            
            _cinemachinePositionComposer.Damping = Vector3.one;
            
            yield return StartCoroutine(FadeOut());
            _fadeCanvasGroup.blocksRaycasts = false;
        }

        private void OnEnter()
        {
            StartCoroutine(TeleportWithFade(_characterCollider2D));
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_characterController2D.IsTeleporting) return;
            
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
