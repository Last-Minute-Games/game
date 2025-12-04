using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using cherrydev;

/// <summary>
/// Manages ending cutscenes with direct text rendering.
/// Checks flags to determine which ending to play (good, neutral, or bad).
/// Displays text letter-by-letter directly on screen, then shows scrolling credits.
/// 
/// Ending Definitions:
/// - Bad Ending: No flags present (default)
/// - Neutral Ending: Only "ending.killer.found" flag exists
/// - Good Ending: Both "ending.killer.found" AND "character.avant.heir" flags exist
/// </summary>
public class EndingCutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class EndingDefinition
    {
        [Header("Ending Identification")]
        [Tooltip("Name of this ending (for debugging)")]
        public string endingName = "Good Ending";
        
        [Header("Flag Requirements")]
        [Tooltip("All of these flags must exist for this ending to play")]
        public string[] requiredFlags;
        
        [Tooltip("None of these flags can exist for this ending to play")]
        public string[] forbiddenFlags;
        
        [Header("Priority")]
        [Tooltip("Higher priority endings are checked first (useful for handling conflicts)")]
        public int priority = 0;
        
        [Header("Dialog Content")]
        [Tooltip("The dialog node graph for this ending sequence")]
        public DialogNodeGraph endingDialogGraph;
        
        [Header("Background Image")]
        [Tooltip("Background image for this ending (required)")]
        public Sprite backgroundImage;
        
        [Tooltip("Background color tint for this ending")]
        public Color backgroundColor = Color.white;
        
        [Header("Audio")]
        [Tooltip("Optional audio clip to play during this ending")]
        public AudioClip endingMusic;
    }
    
    [Header("Ending Definitions")]
    [Tooltip("Define your endings here. Higher priority endings are checked first.")]
    public EndingDefinition[] endings = new EndingDefinition[3]
    {
        // Good Ending - Highest priority
        new EndingDefinition
        {
            endingName = "Good Ending",
            priority = 100,
            requiredFlags = new string[] { "ending.killer.found", "character.avant.heir" },
            forbiddenFlags = new string[0]
        },
        // Neutral Ending - Medium priority
        new EndingDefinition
        {
            endingName = "Neutral Ending",
            priority = 50,
            requiredFlags = new string[] { "ending.killer.found" },
            forbiddenFlags = new string[] { "character.avant.heir" }
        },
        // Bad Ending - Lowest priority (default)
        new EndingDefinition
        {
            endingName = "Bad Ending",
            priority = 0,
            requiredFlags = new string[0],
            forbiddenFlags = new string[0]
        }
    };
    
    [Header("Dialog System")]
    [Tooltip("The DialogBehaviour component that will handle the node graph")]
    public DialogBehaviour dialogBehaviour;
    
    [Header("UI References")]
    [Tooltip("Full-width text display for ending dialog")]
    public TextMeshProUGUI endingText;
    
    [Tooltip("Background image")]
    public Image backgroundImage;
    
    [Header("Credits")]
    [Tooltip("The CreditsScroller component that handles scrolling credits")]
    public CreditsScroller creditsScroller;
    
    [Header("Canvas Groups")]
    public CanvasGroup endingCanvasGroup;
    public CanvasGroup textCanvasGroup;
    public CanvasGroup creditsCanvasGroup;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    [Header("Text Settings")]
    [Tooltip("Delay between each character appearing")]
    public float charDelay = 0.05f;
    
    [Tooltip("Keys to advance/skip text")]
    public KeyCode[] advanceKeys = { KeyCode.Space, KeyCode.Return, KeyCode.Mouse0 };
    
    [Header("Transition Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;
    public float lingerDuration = 3f; // Time to linger after dialog before credits
    public string mainMenuSceneName = "MainMenu";
    
    [Header("Screen Fader")]
    public ScreenFader screenFader;

    private EndingDefinition _currentEnding;
    private bool _isTyping = false;
    private bool _textComplete = false;
    private string _currentFullText = "";
    private Coroutine _typingCoroutine;

    private void Start()
    {
        // Try to find ScreenFader if not assigned
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
        }
        
        // Setup audio source if needed
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }
        
        // Subscribe to dialog finished event
        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogFinished.AddListener(OnEndingDialogFinished);
            dialogBehaviour.SentenceNodeActivated += OnSentenceNodeActivated;
        }
        else
        {
            Debug.LogError("[EndingCutsceneManager] DialogBehaviour is not assigned!");
        }
        
        // Validate credits scroller
        if (creditsScroller == null)
        {
            Debug.LogWarning("[EndingCutsceneManager] CreditsScroller is not assigned!");
        }
        
        // Initialize canvas groups
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 1f;
        }
        
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0f;
        }
        
        // Initialize text
        if (endingText != null)
        {
            endingText.text = "";
            endingText.maxVisibleCharacters = 0;
        }
        
        // Start the ending sequence
        StartCoroutine(PlayAppropriateEnding());
    }

    private void OnDestroy()
    {
        // Unsubscribe from dialog events
        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogFinished.RemoveListener(OnEndingDialogFinished);
            dialogBehaviour.SentenceNodeActivated -= OnSentenceNodeActivated;
        }
    }

    private void Update()
    {
        // Handle text advancement
        if (_isTyping || _textComplete)
        {
            foreach (KeyCode key in advanceKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    if (_isTyping)
                    {
                        // Skip to end of current text
                        SkipTyping();
                    }
                    else if (_textComplete)
                    {
                        // Advance to next node
                        AdvanceDialog();
                    }
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Called when a sentence node is activated in the dialog graph
    /// </summary>
    private void OnSentenceNodeActivated()
    {
        if (dialogBehaviour.CurrentSentenceNode != null)
        {
            string text = dialogBehaviour.CurrentSentenceNode.GetText();
            DisplayText(text);
        }
    }
    
    /// <summary>
    /// Display text with letter-by-letter typing effect
    /// </summary>
    private void DisplayText(string text)
    {
        _currentFullText = text;
        _textComplete = false;
        
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        
        _typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    /// <summary>
    /// Type text letter by letter
    /// </summary>
    private IEnumerator TypeText(string text)
    {
        _isTyping = true;
        
        if (endingText != null)
        {
            endingText.text = text;
            endingText.maxVisibleCharacters = 0;
            
            for (int i = 0; i <= text.Length; i++)
            {
                endingText.maxVisibleCharacters = i;
                yield return new WaitForSeconds(charDelay);
            }
        }
        
        _isTyping = false;
        _textComplete = true;
    }
    
    /// <summary>
    /// Skip typing animation and show full text
    /// </summary>
    private void SkipTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        
        if (endingText != null)
        {
            endingText.maxVisibleCharacters = _currentFullText.Length;
        }
        
        _isTyping = false;
        _textComplete = true;
    }
    
    /// <summary>
    /// Advance to next dialog node
    /// </summary>
    private void AdvanceDialog()
    {
        _textComplete = false;
        
        if (dialogBehaviour != null && dialogBehaviour.CurrentSentenceNode != null)
        {
            SentenceNode currentNode = dialogBehaviour.CurrentSentenceNode;
            
            if (currentNode.ChildNode != null)
            {
                dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(currentNode.ChildNode);
            }
            else
            {
                // No more nodes, dialog is finished
                Debug.Log("[EndingCutsceneManager] No more dialog nodes");
            }
        }
    }
    
    /// <summary>
    /// Determine which ending to play based on flags and play it
    /// </summary>
    private IEnumerator PlayAppropriateEnding()
    {
        Debug.Log("[EndingCutsceneManager] Determining which ending to play...");
        
        // Log current flags for debugging
        LogCurrentFlags();
        
        // First, open eyes if the ScreenFader has them closed
        if (screenFader != null && screenFader.shouldOpenEyesOnSceneLoad)
        {
            Debug.Log("[EndingCutsceneManager] Opening eyes before ending");
            screenFader.shouldOpenEyesOnSceneLoad = false;
            yield return StartCoroutine(screenFader.EyesOpeningEffect());
            yield return new WaitForSeconds(0.5f);
        }
        
        // Find the appropriate ending
        _currentEnding = DetermineEnding();
        
        if (_currentEnding == null)
        {
            Debug.LogError("[EndingCutsceneManager] No valid ending found! Using fallback.");
            _currentEnding = CreateFallbackEnding();
        }
        
        Debug.Log($"[EndingCutsceneManager] Playing ending: {_currentEnding.endingName}");
        
        // Setup background and music
        SetupEndingVisuals(_currentEnding);
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Start the dialog
        if (dialogBehaviour != null && _currentEnding.endingDialogGraph != null)
        {
            Debug.Log("[EndingCutsceneManager] Starting ending dialog");
            dialogBehaviour.StartDialog(_currentEnding.endingDialogGraph);
        }
        else
        {
            Debug.LogError("[EndingCutsceneManager] DialogBehaviour or EndingDialogGraph is null!");
            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(ReturnToMainMenu());
        }
    }
    
    /// <summary>
    /// Log current flags for debugging
    /// </summary>
    private void LogCurrentFlags()
    {
        Debug.Log("[EndingCutsceneManager] Checking flags:");
        Debug.Log($"  - ending.killer.found: {GameFlags.HasFlag("ending.killer.found")}");
        Debug.Log($"  - character.avant.heir: {GameFlags.HasFlag("character.avant.heir")}");
    }
    
    /// <summary>
    /// Called when the ending dialog finishes
    /// </summary>
    private void OnEndingDialogFinished()
    {
        Debug.Log("[EndingCutsceneManager] Dialog finished, lingering before credits");
        StartCoroutine(LingerThenShowCredits());
    }
    
    /// <summary>
    /// Linger on the final text, then show credits
    /// </summary>
    private IEnumerator LingerThenShowCredits()
    {
        // Linger on the final text
        Debug.Log($"[EndingCutsceneManager] Lingering for {lingerDuration} seconds");
        yield return new WaitForSeconds(lingerDuration);
        
        // Fade out text
        Debug.Log("[EndingCutsceneManager] Fading out text");
        yield return StartCoroutine(FadeOutText());
        
        // Show credits
        if (creditsScroller != null)
        {
            Debug.Log("[EndingCutsceneManager] Starting credits");
            yield return StartCoroutine(ShowCredits());
        }
        else
        {
            Debug.LogWarning("[EndingCutsceneManager] No credits scroller, skipping to menu");
        }
        
        // Return to menu
        yield return StartCoroutine(FinishEndingSequence());
    }
    
    /// <summary>
    /// Fade out the text canvas
    /// </summary>
    private IEnumerator FadeOutText()
    {
        if (textCanvasGroup != null)
        {
            float elapsed = 0f;
            float duration = 1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                textCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            
            textCanvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// Show and scroll the credits
    /// </summary>
    private IEnumerator ShowCredits()
    {
        // Fade in credits
        if (creditsCanvasGroup != null)
        {
            float elapsed = 0f;
            float duration = 1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                creditsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            
            creditsCanvasGroup.alpha = 1f;
        }
        
        // Start scrolling credits
        if (creditsScroller != null)
        {
            creditsScroller.StartScrolling();
            
            // Wait for credits to finish
            while (creditsScroller.IsScrolling)
            {
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// Finish the ending sequence and return to main menu
    /// </summary>
    private IEnumerator FinishEndingSequence()
    {
        // Fade out everything
        yield return StartCoroutine(FadeOut());
        
        // Return to main menu
        Debug.Log("[EndingCutsceneManager] Ending complete, returning to main menu");
        yield return StartCoroutine(ReturnToMainMenu());
    }
    
    /// <summary>
    /// Setup background and music for the ending
    /// </summary>
    private void SetupEndingVisuals(EndingDefinition ending)
    {
        // Setup background
        if (backgroundImage != null)
        {
            backgroundImage.sprite = ending.backgroundImage;
            backgroundImage.color = ending.backgroundColor;
            Debug.Log($"[EndingCutsceneManager] Set background for {ending.endingName}");
        }
        else
        {
            Debug.LogWarning("[EndingCutsceneManager] Background image is not assigned!");
        }
        
        // Start music
        if (ending.endingMusic != null && audioSource != null)
        {
            audioSource.clip = ending.endingMusic;
            audioSource.volume = 1f;
            audioSource.Play();
            Debug.Log($"[EndingCutsceneManager] Playing music for {ending.endingName}");
        }
    }
    
    /// <summary>
    /// Fade in the ending canvas
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (endingCanvasGroup != null)
        {
            endingCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                endingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            
            endingCanvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Fade out the ending canvas and music
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (endingCanvasGroup != null)
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                endingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                
                // Also fade out music
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.volume = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                }
                
                yield return null;
            }
            
            endingCanvasGroup.alpha = 0f;
        }
        
        // Stop music
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Determine which ending should play based on flags
    /// </summary>
    private EndingDefinition DetermineEnding()
    {
        if (endings == null || endings.Length == 0)
        {
            Debug.LogWarning("[EndingCutsceneManager] No endings defined!");
            return null;
        }
        
        // Sort endings by priority (highest first)
        System.Array.Sort(endings, (a, b) => b.priority.CompareTo(a.priority));
        
        foreach (EndingDefinition ending in endings)
        {
            if (ending == null) continue;
            
            bool meetsRequirements = CheckEndingRequirements(ending);
            
            if (meetsRequirements)
            {
                Debug.Log($"[EndingCutsceneManager] Ending '{ending.endingName}' meets all requirements");
                return ending;
            }
            else
            {
                Debug.Log($"[EndingCutsceneManager] Ending '{ending.endingName}' does not meet requirements");
            }
        }
        
        Debug.LogWarning("[EndingCutsceneManager] No ending met the requirements!");
        return null;
    }
    
    /// <summary>
    /// Check if an ending's flag requirements are met
    /// </summary>
    private bool CheckEndingRequirements(EndingDefinition ending)
    {
        // Check required flags
        if (ending.requiredFlags != null && ending.requiredFlags.Length > 0)
        {
            foreach (string flag in ending.requiredFlags)
            {
                if (string.IsNullOrEmpty(flag)) continue;
                
                if (!GameFlags.HasFlag(flag))
                {
                    Debug.Log($"[EndingCutsceneManager] Missing required flag: {flag}");
                    return false;
                }
            }
        }
        
        // Check forbidden flags
        if (ending.forbiddenFlags != null && ending.forbiddenFlags.Length > 0)
        {
            foreach (string flag in ending.forbiddenFlags)
            {
                if (string.IsNullOrEmpty(flag)) continue;
                
                if (GameFlags.HasFlag(flag))
                {
                    Debug.Log($"[EndingCutsceneManager] Has forbidden flag: {flag}");
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Return to the main menu
    /// </summary>
    private IEnumerator ReturnToMainMenu()
    {
        // Brief pause before transition
        yield return new WaitForSeconds(1f);
        
        // Load main menu
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[EndingCutsceneManager] Main menu scene name not set!");
        }
    }
    
    /// <summary>
    /// Create a fallback ending in case no endings are defined or none match
    /// </summary>
    private EndingDefinition CreateFallbackEnding()
    {
        return new EndingDefinition
        {
            endingName = "Fallback Ending",
            backgroundColor = Color.black,
            endingDialogGraph = null
        };
    }
}
