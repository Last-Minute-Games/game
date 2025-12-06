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
    
    [Header("Credits (MainMenu Style)")]
    [Tooltip("Parent canvas group of all credits UI")]
    public CanvasGroup creditsCanvasGroup;
    [Tooltip("The credits logo to scroll")]
    public RectTransform creditLogo;
    [Tooltip("The credits text to scroll")]
    public RectTransform creditText;
    [Tooltip("Duration for credits to scroll completely off screen")]
    public float creditsScrollDuration = 10f;
    [Tooltip("Scroll speed multiplier (1 = normal)")]
    public float creditsScrollSpeed = 1f;
    
    [Header("Canvas Groups")]
    public CanvasGroup endingCanvasGroup;
    public CanvasGroup textCanvasGroup;
    
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
    private bool _waitingForAdvance = false;
    private string _currentFullText = "";
    private Coroutine _typingCoroutine;
    private Sprite _customBackgroundImage; // Optional custom background override
    
    // Credits tracking
    private Vector2 _creditLogoStartPos;
    private Vector2 _creditTextStartPos;
    private bool _creditsPlaying = false;

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
        
        // Subscribe to dialog events
        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogFinished.AddListener(OnEndingDialogFinished);
            dialogBehaviour.SentenceNodeActivated += OnSentenceNodeActivated;
            dialogBehaviour.SentenceEnded += OnSentenceTypingEnded;
        }
        else
        {
            Debug.LogError("[EndingCutsceneManager] DialogBehaviour is not assigned!");
        }
        
        // Store initial positions for credits scroll
        if (creditLogo) _creditLogoStartPos = creditLogo.anchoredPosition;
        if (creditText) _creditTextStartPos = creditText.anchoredPosition;
        if (creditsCanvasGroup)
        {
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.gameObject.SetActive(false);
        }
        
        // Initialize canvas groups
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 1f;
        }
        
        // Initialize text
        if (endingText != null)
        {
            endingText.text = "";
            endingText.maxVisibleCharacters = 0;
        }
        
        // Ensure background is visible and ready
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 1f, 1f, 1f);
            backgroundImage.enabled = true;
            Debug.Log("[EndingCutsceneManager] Background image component initialized");
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
            dialogBehaviour.SentenceEnded -= OnSentenceTypingEnded;
        }
    }

    private void Update()
    {
        // Handle text advancement
        if (_isTyping || _waitingForAdvance)
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
                    else if (_waitingForAdvance)
                    {
                        // Advance to next sentence node
                        AdvanceToNextSentence();
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
            Debug.Log($"[EndingCutsceneManager] New sentence: {text.Substring(0, Mathf.Min(50, text.Length))}...");
            DisplayText(text);
        }
    }
    
    /// <summary>
    /// Called when sentence typing completes (from DialogBehaviour)
    /// </summary>
    private void OnSentenceTypingEnded()
    {
        Debug.Log("[EndingCutsceneManager] Sentence typing ended, waiting for player input");
        _waitingForAdvance = true;
    }
    
    /// <summary>
    /// Display text with letter-by-letter typing effect
    /// </summary>
    private void DisplayText(string text)
    {
        _currentFullText = text;
        _waitingForAdvance = false;
        
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
        _waitingForAdvance = true;
        Debug.Log("[EndingCutsceneManager] Finished typing, waiting for advance input");
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
        _waitingForAdvance = true;
        Debug.Log("[EndingCutsceneManager] Skipped typing, waiting for advance input");
    }
    
    /// <summary>
    /// Advance to next sentence node
    /// </summary>
    private void AdvanceToNextSentence()
    {
        _waitingForAdvance = false;
        
        if (dialogBehaviour != null && dialogBehaviour.CurrentSentenceNode != null)
        {
            SentenceNode currentNode = dialogBehaviour.CurrentSentenceNode;
            
            if (currentNode.ChildNode != null)
            {
                Debug.Log($"[EndingCutsceneManager] Advancing to next node: {currentNode.ChildNode.GetType().Name}");
                dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(currentNode.ChildNode);
            }
            else
            {
                // No more nodes, we've reached the end
                Debug.Log("[EndingCutsceneManager] No more nodes, dialog complete - triggering ending sequence");
                // Manually trigger the ending sequence since we've reached the end
                OnEndingDialogFinished();
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
        
        // HIDE DIALOG UI IMMEDIATELY - This ensures the background is visible
        DialogDisplayer dialogDisplayer = FindObjectOfType<DialogDisplayer>();
        if (dialogDisplayer != null)
        {
            dialogDisplayer.DisableDialogPanel();
            Debug.Log("[EndingCutsceneManager] Dialog panels hidden at start");
        }
        
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
            Debug.Log("[EndingCutsceneManager] Starting ending dialog graph");
            
            // Disable DialogBehaviour's automatic text skipping since we handle it manually
            dialogBehaviour.IsCanSkippingText = false;
            dialogBehaviour.IsActive = false; // Prevent DialogBehaviour from handling input
            
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
    /// Called when the ending dialog finishes (all nodes processed)
    /// </summary>
    private void OnEndingDialogFinished()
    {
        Debug.Log("[EndingCutsceneManager] All dialog nodes finished, lingering before credits");
        StartCoroutine(LingerThenShowCredits());
    }
    
    /// <summary>
    /// Linger on the final text, then show credits
    /// </summary>
    private IEnumerator LingerThenShowCredits()
    {
        // CLOSE THE DIALOG - Fade out text first
        Debug.Log("[EndingCutsceneManager] Dialog complete - closing dialog");
        yield return StartCoroutine(FadeOutText());
        
        // Hide the dialog panels using DialogDisplayer if it exists
        DialogDisplayer dialogDisplayer = FindObjectOfType<DialogDisplayer>();
        if (dialogDisplayer != null)
        {
            dialogDisplayer.DisableDialogPanel();
            Debug.Log("[EndingCutsceneManager] Dialog panels disabled via DialogDisplayer");
        }
        else
        {
            Debug.LogWarning("[EndingCutsceneManager] DialogDisplayer not found in scene");
        }
        
        // Show credits using MainMenu-style scrolling
        if (creditsCanvasGroup != null && creditLogo != null && creditText != null)
        {
            Debug.Log("[EndingCutsceneManager] Starting MainMenu-style credits");
            yield return StartCoroutine(RollCreditsMainMenuStyle());
        }
        else
        {
            Debug.LogWarning("[EndingCutsceneManager] No credits system configured, skipping to menu");
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
    /// Roll credits using MainMenu-style scrolling (smooth, synchronized)
    /// </summary>
    private IEnumerator RollCreditsMainMenuStyle()
    {
        if (_creditsPlaying) yield break; // prevent re-entry
        _creditsPlaying = true;
        
        // Fade in the credits canvas
        creditsCanvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCreditsCanvasGroup(creditsCanvasGroup, 0f, 1f, fadeInDuration));
        
        // Scroll the credits completely off-screen
        Debug.Log("[EndingCutsceneManager] Starting credits scroll");
        float actualScrollDuration = creditsScrollDuration / creditsScrollSpeed;
        float timer = 0f;
        
        // Calculate end position to be off the top of the screen
        Canvas parentCanvas = creditsCanvasGroup.GetComponentInParent<Canvas>();
        float canvasHeight = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.height : 1080f;
        
        // Just need to be slightly above screen top
        float offScreenY = canvasHeight + 500f;
        
        // Compute a shared offset so both elements travel the same distance
        float minStartY = Mathf.Min(_creditLogoStartPos.y, _creditTextStartPos.y);
        float sharedOffset = offScreenY - minStartY;
        
        Vector2 logoEndPos = _creditLogoStartPos + Vector2.up * sharedOffset;
        Vector2 textEndPos = _creditTextStartPos + Vector2.up * sharedOffset;
        
        // Movement speed in units per second
        float moveSpeed = sharedOffset / actualScrollDuration;
        
        while (timer < actualScrollDuration)
        {
            float dt = Time.deltaTime;
            timer += dt;
            
            // Move by units per second to keep both elements visually synchronized
            Vector2 delta = Vector2.up * (moveSpeed * dt);
            
            if (creditLogo)
                creditLogo.anchoredPosition += delta;
            if (creditText)
                creditText.anchoredPosition += delta;
            
            // If both have reached (or passed) the offscreen target, break early
            bool logoDone = creditLogo == null || creditLogo.anchoredPosition.y >= logoEndPos.y - 0.5f;
            bool textDone = creditText == null || creditText.anchoredPosition.y >= textEndPos.y - 0.5f;
            if (logoDone && textDone)
            {
                // Snap to end positions to avoid tiny remaining movement
                if (creditLogo) creditLogo.anchoredPosition = logoEndPos;
                if (creditText) creditText.anchoredPosition = textEndPos;
                break;
            }
            
            yield return null;
        }
        
        // Ensure final positions are set
        if (creditLogo) creditLogo.anchoredPosition = logoEndPos;
        if (creditText) creditText.anchoredPosition = textEndPos;
        
        Debug.Log("[EndingCutsceneManager] Credits scroll complete - idling for 10 seconds");
        
        // IDLE FOR 10 SECONDS - Let the credits sit finished before fading out
        yield return new WaitForSeconds(10f);
        
        Debug.Log("[EndingCutsceneManager] Fading out credits");
        
        // Fade out the credits
        yield return StartCoroutine(FadeCreditsCanvasGroup(creditsCanvasGroup, 1f, 0f, fadeOutDuration));
        creditsCanvasGroup.gameObject.SetActive(false);
        
        // Reset positions for next time
        if (creditLogo) creditLogo.anchoredPosition = _creditLogoStartPos;
        if (creditText) creditText.anchoredPosition = _creditTextStartPos;
        
        _creditsPlaying = false;
    }
    
    /// <summary>
    /// Fade credits canvas group helper method
    /// </summary>
    private IEnumerator FadeCreditsCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
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
    /// This takes the backgroundImage sprite from the EndingDefinition and applies it to the Background Image component in the scene
    /// </summary>
    private void SetupEndingVisuals(EndingDefinition ending)
    {
        // Setup background - Apply the sprite from the ending definition to the scene's Background Image component
        if (backgroundImage != null)
        {
            if (ending.backgroundImage != null)
            {
                // Apply the ending's background sprite to the Image component
                backgroundImage.sprite = ending.backgroundImage;
                backgroundImage.color = ending.backgroundColor;
                backgroundImage.enabled = true;
                
                // Ensure the image is set to preserve aspect or stretch to fill
                if (backgroundImage.GetComponent<AspectRatioFitter>() == null)
                {
                    // If no aspect ratio fitter, make sure it fills the screen
                    RectTransform rectTransform = backgroundImage.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.one;
                        rectTransform.offsetMin = Vector2.zero;
                        rectTransform.offsetMax = Vector2.zero;
                    }
                }
                
                Debug.Log($"[EndingCutsceneManager] Set background for {ending.endingName} - Sprite: {ending.backgroundImage.name}");
            }
            else
            {
                Debug.LogWarning($"[EndingCutsceneManager] No background image sprite assigned for {ending.endingName}!");
                // Set to solid color as fallback
                backgroundImage.sprite = null;
                backgroundImage.color = ending.backgroundColor;
            }
        }
        else
        {
            Debug.LogError("[EndingCutsceneManager] Background Image component reference is not assigned in the inspector! Please assign the Background Image from your Canvas.");
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

    /// <summary>
    /// Public method to play a specific ending by name
    /// </summary>
    public void PlayEnding(string endingName)
    {
        // Stop any ongoing sequences
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        // Explicitly clear current ending
        _currentEnding = null;

        // Find the ending by name
        foreach (EndingDefinition ending in endings)
        {
            if (ending.endingName == endingName)
            {
                _currentEnding = ending;
                break;
            }
        }

        if (_currentEnding == null)
        {
            Debug.LogError($"[EndingCutsceneManager] Ending not found: {endingName}");
            return;
        }

        Debug.Log($"[EndingCutsceneManager] Playing selected ending: {_currentEnding.endingName}");

        // Setup background image if provided
        if (_customBackgroundImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = _customBackgroundImage;
            backgroundImage.color = Color.white;
            backgroundImage.enabled = true;
            Debug.Log("[EndingCutsceneManager] Custom background image applied");
        }

        // Start the ending sequence
        StartCoroutine(PlayAppropriateEnding());
    }
}
