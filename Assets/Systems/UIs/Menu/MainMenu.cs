using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class Startscreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    
    private CanvasGroup _fadeCanvasGroup;
    private CanvasGroup _logoCanvasGroup;
    
    // Drag your main menu Canvas (the one containing Play/Settings/Credits/Quit) here
    [SerializeField] private GraphicRaycaster mainMenuRaycaster;
    
    [Header("Main Menu Elements")]
    public GameObject mainMenuLogo;        // The "Castle of Time" title logo on the main menu
    public GameObject buttonsParent;       // Parent GameObject containing all menu buttons
    public CanvasGroup mainMenuLogoCanvasGroup;  // Canvas group for fading the logo
    public CanvasGroup buttonsCanvasGroup;       // Canvas group for fading the buttons
    
    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject creditsButton;
    public GameObject quitButton;

    [Header("Credits")]
    public CanvasGroup creditsCanvasGroup; // Parent of all credits UI
    public RectTransform creditLogo;       // The credits logo to scroll
    public RectTransform creditText;       // The credits text to scroll
    public float creditsScrollDuration = 10f;
    public float creditsHoldTime = 2f;
    public float scrollSpeed = 1f;         // Runtime-adjustable scroll speed (1 = normal)

    [Header("Settings")]
    public Settings settingsComponent; // Reference to the Settings component

    private Vector2 _creditLogoStartPos;
    private Vector2 _creditTextStartPos;
    private bool _creditsPlaying = false; // prevent re-entry and ensure scroll stops

    // Public runtime controls for scroll speed
    public void SetScrollSpeed(float speed)
    {
        // Prevent zero/negative values
        scrollSpeed = Mathf.Max(0.01f, speed);
    }

    public void IncreaseScrollSpeed(float delta)
    {
        SetScrollSpeed(scrollSpeed + delta);
    }

    public void DecreaseScrollSpeed(float delta)
    {
        SetScrollSpeed(scrollSpeed - delta);
    }

    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    private void FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        StartCoroutine(FadeCoroutine(canvasGroup, startAlpha, endAlpha, duration));
    }
    
    private IEnumerator LogoStartup()
    {
        // Disable the main menu raycaster to block all clicks/hover on that canvas while the logo plays
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = false;

        // Make sure the logo overlay is active and catching input
        _logoCanvasGroup.gameObject.SetActive(true);
        _logoCanvasGroup.blocksRaycasts = true;   // <- blocks all UI beneath
        _logoCanvasGroup.interactable = true;   // if you have a Skip button, etc.
        _logoCanvasGroup.alpha = 0f;

        // Fade logo in
        yield return StartCoroutine(FadeCoroutine(_logoCanvasGroup, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(3f); // your logo hold time (no need to add fadeDuration again)

        // Fade logo out
        yield return StartCoroutine(FadeCoroutine(_logoCanvasGroup, 1f, 0f, fadeDuration));

        // Stop blocking and hide
        _logoCanvasGroup.blocksRaycasts = false;
        _logoCanvasGroup.interactable = false;
        _logoCanvasGroup.gameObject.SetActive(false);

        // While the screen is black, also block input on the fade CG
        _fadeCanvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeCoroutine(_fadeCanvasGroup, 1f, 0f, 2f));
        _fadeCanvasGroup.blocksRaycasts = false;

        // Re-enable menu input and UI audio
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = true;
        UIButtonFX.globalAudioEnabled = true;
    }

    void Start()
    {
        playButton = GameObject.Find("PlayButton");
        quitButton = GameObject.Find("QuitButton");
        settingsButton = GameObject.Find("SettingsButton");
        creditsButton = GameObject.Find("CreditsButton");
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 1f; // Start transparent
        
        _logoCanvasGroup = GameObject.Find("LogoCanvasGroup").GetComponent<CanvasGroup>();
        _logoCanvasGroup.alpha = 0f; // Start transparent

        // Auto-setup canvas groups if they don't exist but GameObjects are assigned
        if (mainMenuLogo && !mainMenuLogoCanvasGroup)
        {
            mainMenuLogoCanvasGroup = mainMenuLogo.GetComponent<CanvasGroup>();
            if (!mainMenuLogoCanvasGroup)
                mainMenuLogoCanvasGroup = mainMenuLogo.AddComponent<CanvasGroup>();
        }
        
        if (buttonsParent && !buttonsCanvasGroup)
        {
            buttonsCanvasGroup = buttonsParent.GetComponent<CanvasGroup>();
            if (!buttonsCanvasGroup)
                buttonsCanvasGroup = buttonsParent.AddComponent<CanvasGroup>();
        }

        // Ensure main menu elements start visible
        if (mainMenuLogoCanvasGroup) mainMenuLogoCanvasGroup.alpha = 1f;
        if (buttonsCanvasGroup) buttonsCanvasGroup.alpha = 1f;

        // Store initial positions for credits scroll
        if (creditLogo) _creditLogoStartPos = creditLogo.anchoredPosition;
        if (creditText) _creditTextStartPos = creditText.anchoredPosition;
        if (creditsCanvasGroup)
        {
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.gameObject.SetActive(false); // Start with it inactive
        }

        // Suppress UI sounds during logo loading and suppress clicks on menu buttons
        UIButtonFX.globalAudioEnabled = false; // disable hover/click audio while loading logo
        UIButtonFX.suppressClickInMainMenu = true; // ensure clicks in main menu don't play click sounds
        
        StartCoroutine(LogoStartup());
    }

    public void ShowCredits()
    {
        if (_creditsPlaying) return; // already running
        if (creditsCanvasGroup != null && creditLogo != null && creditText != null)
        {
            StartCoroutine(RollCreditsCoroutine());
        }
        else
        {
            Debug.LogWarning("Credits UI elements are not assigned in the inspector!");
        }
    }

    /// <summary>
    /// Show the settings panel with fade animation
    /// </summary>
    public void ShowSettings()
    {
        if (settingsComponent != null)
        {
            Debug.Log("[MainMenu] Opening settings");
            settingsComponent.ShowSettings();
        }
        else
        {
            Debug.LogWarning("[MainMenu] Settings component not assigned in the inspector!");
        }
    }

    private IEnumerator RollCreditsCoroutine()
    {
        _creditsPlaying = true;
        // 1. Disable main menu raycaster
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = false;

        // 2. Fade out main menu logo and buttons
        if (mainMenuLogoCanvasGroup)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 1f, 0f, fadeDuration));
        if (buttonsCanvasGroup)
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 1f, 0f, fadeDuration));
        
        // Wait for fade out to complete
        yield return new WaitForSeconds(fadeDuration);

        // Hide the GameObjects after fade
        if (mainMenuLogo) mainMenuLogo.SetActive(false);
        if (buttonsParent) buttonsParent.SetActive(false);

        // 3. Fade in the credits canvas
        creditsCanvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCoroutine(creditsCanvasGroup, 0f, 1f, fadeDuration));

        // 4. Scroll the credits completely off-screen
        // Combine inspector scrollSpeed and subtract a small offset so it finishes a bit earlier
        float actualScrollDuration = creditsScrollDuration / scrollSpeed;
        float timer = 0f;
        
        // Calculate end position to be off the top of the screen
        Canvas parentCanvas = creditsCanvasGroup.GetComponentInParent<Canvas>();
        float canvasHeight = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.height : 1080f;
        
        // Calculate off-screen positions (way above the top of the canvas)
        float offScreenY = canvasHeight * 1.5f + 1000f;  // Much higher to ensure everything is off-screen

        // Compute a shared offset so both elements travel the same distance (units/sec will match)
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

            // move by units per second to keep both elements visually synchronized
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
                // snap to end positions to avoid tiny remaining movement
                if (creditLogo) creditLogo.anchoredPosition = logoEndPos;
                if (creditText) creditText.anchoredPosition = textEndPos;
                break;
            }

            yield return null;
        }

        // Ensure final positions are set (in case loop ended by time)
        if (creditLogo) creditLogo.anchoredPosition = logoEndPos;
        if (creditText) creditText.anchoredPosition = textEndPos;

        // 5. Hold at the end
        //yield return new WaitForSeconds(creditsHoldTime);

        // 6. Fade out the credits
        yield return StartCoroutine(FadeCoroutine(creditsCanvasGroup, 1f, 0f, fadeDuration));
        creditsCanvasGroup.gameObject.SetActive(false);

        // Reset positions for next time
        if (creditLogo) creditLogo.anchoredPosition = _creditLogoStartPos;
        if (creditText) creditText.anchoredPosition = _creditTextStartPos;

        // 7. Show main menu elements again
        if (mainMenuLogo) mainMenuLogo.SetActive(true);
        if (buttonsParent) buttonsParent.SetActive(true);

        // 8. Fade in main menu logo and buttons
        if (mainMenuLogoCanvasGroup)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 0f, 1f, fadeDuration));
        if (buttonsCanvasGroup)
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 0f, 1f, fadeDuration));
        
        // Wait for fade in to complete
        yield return new WaitForSeconds(fadeDuration);

        // 9. Re-enable main menu raycaster
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = true;
        _creditsPlaying = false;
    }

    private void SetMenuButtonsActive(bool state)
    {
        if(playButton) playButton.SetActive(state);
        if(settingsButton) settingsButton.SetActive(state);
        if(creditsButton) creditsButton.SetActive(state);
        if(quitButton) quitButton.SetActive(state);
    }

    public void StartGame()
    {
        StartCoroutine(FadeAndLoad());
    }
    
    private IEnumerator FlickerButton(GameObject button, float interval)
    {
        bool visible = true;

        // run until the fade finishes (you can break when alpha reaches 1)
        while (_fadeCanvasGroup.alpha < 1f)
        {
            visible = !visible;
            button.SetActive(visible);
            yield return new WaitForSeconds(interval);
        }

        // make sure it's visible again at the end (optional)
        button.SetActive(true);
    }

    private IEnumerator FadeAndLoad()
    {
        quitButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false); 
        playButton.SetActive(false);
        
        // start flickering the play button
        StartCoroutine(FlickerButton(playButton, 0.3f));

        // Fade to black
        yield return StartCoroutine(FadeCoroutine(_fadeCanvasGroup, 0f, 1f, fadeDuration * 2.5f));

        // Load the next scene asynchronously while screen is black
        AsyncOperation op = SceneManager.LoadSceneAsync("NewTutorial");
        op.allowSceneActivation = true; // or set false if you want to gate activation

        // Optionally wait until load is done (it's already black)
        while (!op.isDone)
            yield return null;

        // If you want to fade back in *after* the new scene is ready,
        // you'll need a fade canvas in the new scene too, or mark this object as persistent:
        // DontDestroyOnLoad(gameObject);  (then manage the fade-out there)
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
                // This makes the stop button in the editor work properly
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}


