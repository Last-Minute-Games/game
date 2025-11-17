using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cherrydev
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dialogNameText;
        [SerializeField] private TextMeshProUGUI _dialogText;
        [SerializeField] private Image _dialogCharacterImage;

        [Header("Text Sound Settings")]
        [SerializeField] private AudioClip[] _textSounds; // Array of sound clips to randomize
        [SerializeField] private bool _playTextSound = true;
        [SerializeField] private float _soundVolume = 0.5f;
        [SerializeField] private int _soundEveryNthChar = 1; // Play sound every N characters (1 = every char, 2 = every other char, etc.)

        private string _currentFullText;
        private int _charCounter = 0;
        private AudioSource _textAudioSource;
        
        public void Awake()
        {
            transform.gameObject.SetActive(false);
            
            // Find EnvironmentSoundHandler in the scene using GameObject.Find
            GameObject handlerObject = GameObject.Find("EnvironmentSoundHandler");
            
            if (handlerObject != null)
            {
                // Use SendMessage to call CreateCustomSource (to avoid assembly reference issues)
                // Since we can't reference the type directly, we'll create an AudioSource manually
                // and parent it to the handler
                GameObject audioSourceObj = new GameObject("DialogTextSound");
                _textAudioSource = audioSourceObj.AddComponent<AudioSource>();
                _textAudioSource.playOnAwake = false;
                _textAudioSource.spatialBlend = 0f; // 2D sound
                audioSourceObj.transform.SetParent(handlerObject.transform);
                audioSourceObj.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("[SentencePanel] EnvironmentSoundHandler not found in scene. Creating standalone AudioSource.");
                // Fallback: create a standalone audio source
                GameObject audioSourceObj = new GameObject("DialogTextSound");
                _textAudioSource = audioSourceObj.AddComponent<AudioSource>();
                _textAudioSource.playOnAwake = false;
                _textAudioSource.spatialBlend = 0f;
                audioSourceObj.transform.SetParent(transform);
            }
        }
        
        /// <summary>
        /// Setting dialogText max visible characters to zero
        /// </summary>
        public void ResetDialogText()
        {
            _dialogText.maxVisibleCharacters = 0;
            _currentFullText = string.Empty;
            _charCounter = 0;
        }

        /// <summary>
        /// Set dialog text max visible characters to dialog text length
        /// </summary>
        /// <param name="text"></param>
        public void ShowFullDialogText(string text)
        {
            _currentFullText = text;
            _dialogText.text = text;
            _dialogText.maxVisibleCharacters = text.Length;
            _charCounter = 0;
        }

        /// <summary>
        /// Increasing max visible characters and playing text sound
        /// </summary>
        public void IncreaseMaxVisibleCharacters()
        {
            _dialogText.maxVisibleCharacters++;
            _charCounter++;
            
            // Play sound every Nth character
            if (_playTextSound && _textSounds != null && _textSounds.Length > 0 && _charCounter % _soundEveryNthChar == 0)
            {
                PlayTextSound();
            }
        }

        /// <summary>
        /// Plays a random text sound from the array
        /// </summary>
        private void PlayTextSound()
        {
            if (_textAudioSource == null || _textSounds == null || _textSounds.Length == 0)
                return;

            // Pick a random sound from the array
            AudioClip clip = _textSounds[Random.Range(0, _textSounds.Length)];
            
            if (clip != null)
            {
                _textAudioSource.PlayOneShot(clip, _soundVolume);
            }
        }
        
        /// <summary>
        /// Assigning dialog name text, character image sprite and dialog text
        /// </summary>
        public void Setup(string characterName, string text, Sprite sprite)
        {
            _dialogNameText.text = characterName;
            _dialogText.text = text;
            _currentFullText = text;
            _charCounter = 0;

            if (sprite == null)
            {
                _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                    _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 0);
                return;
            }

            _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 255);
            _dialogCharacterImage.sprite = sprite;
        }
    }
}