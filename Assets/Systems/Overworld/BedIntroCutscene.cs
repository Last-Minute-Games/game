using System.Collections;
using cherrydev;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

namespace Systems.Overworld
{
    public class BedIntroCutscene : MonoBehaviour
    {
        private CanvasGroup _fadeCanvasGroup;
        private AudioSource _audioSource;
        
        private NpcBrain2D _maidBrain2D;
        
        private SpriteRenderer _mainCharSpriteRenderer;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip doorKnockSound;
        [SerializeField] private AudioClip doorOpenSound;
        
        [Header("Sprites")]
        [SerializeField] private Sprite nikolausSleepSprite;
        [SerializeField] private Sprite nikolausAwakeSprite;
        
        [Header("Dialogue")]
        [SerializeField] private DialogBehaviour dialogBehaviour;   // Drag your Dialog prefab instance here
        [SerializeField] private DialogNodeGraph dialogGraph;       // Drag your custom DialogNodeGraph asset here
    
        private IEnumerator BeginIntroSequence()
        {
            yield return new WaitForSeconds(1.5f);
            
            _audioSource.clip = doorKnockSound;
            _audioSource.volume = 1f;
            _audioSource.Play();
            
            yield return new WaitForSeconds(2f);
            
            _audioSource.clip = doorOpenSound;
            _audioSource.volume = 0.7f;
            _audioSource.Play();
            
            yield return new WaitForSeconds(0.1f);
            
            _fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _fadeCanvasGroup.blocksRaycasts = false; // Disable blocking after fade-in
            });
            
            yield return new WaitForSeconds(1f);
            
            yield return _maidBrain2D.MoveToPosition(_maidBrain2D.waypoints[0].position);
            yield return _maidBrain2D.MoveToPosition(_maidBrain2D.waypoints[1].position);
            
            // make maid look right
            yield return _maidBrain2D.MoveToPosition(_maidBrain2D.waypoints[2].position);
            
            yield return new WaitForSeconds(1.5f);
            
            _mainCharSpriteRenderer.sprite = nikolausAwakeSprite;
            
            yield return new WaitForSeconds(1.5f);
            
            dialogBehaviour.StartDialog(dialogGraph);
            
            yield return null;
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            
            _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 1f; // Start opaque
            
            _maidBrain2D = GameObject.Find("MaidNPC").GetComponent<NpcBrain2D>();
            
            _mainCharSpriteRenderer = GameObject.Find("MainCharacter").GetComponent<SpriteRenderer>();
            _mainCharSpriteRenderer.sprite = nikolausSleepSprite;
            // wait 3 seconds
        
            StartCoroutine(BeginIntroSequence());
            // Start the fade-in effect
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
