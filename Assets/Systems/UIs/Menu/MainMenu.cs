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
    
    public GameObject playButton;          // The single Play button
    public GameObject settingsButton;
    public GameObject creditsButton;
    public GameObject quitButton;

    [Header("Play Choice Menu")]
    public GameObject playChoicePanel;
    public CanvasGroup playChoiceCanvasGroup;
    public Button newGameChoiceButton;
    public Button continueChoiceButton;
    public Button backChoiceButton;

    [Header("Save System References")]
    public SaveNamePrompt saveNamePrompt;  // Save name prompt UI
    public LoadGameUI loadGameUI;          // Load game UI

    [Header("Credits")]
    public CanvasGroup creditsCanvasGroup; // Parent of all credits UI
    public RectTransform creditLogo;       // The credits logo to scroll
    public RectTransform creditText;       // The credits text to scroll
    public float creditsScrollDuration = 10f;
    public float creditsHoldTime = 0.1f;   // Minimal hold time before fading back
    public float scrollSpeed = 1f;         // Runtime-adjustable scroll speed (1 = normal)

    [Header("Settings")]
    public Settings settingsComponent; // Reference to the Settings component

    // --- Music support for Main Menu ---
    [Header("Music")]
    [Tooltip("Optional: reference a MusicManager in the scene (preferred). If not set, a local AudioSource will be created.")]
    public MusicManager musicManager;
    public AudioClip menuIntro; // optional intro clip
    public AudioClip menuLoop;  // looped theme for the main menu
    public AudioSource menuAudioSource; // optional custom audio source
    public float musicFadeInDuration = 2f;

    private Coroutine _menuMusicCoroutine;

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
        // Find buttons
        playButton = GameObject.Find("PlayButton");
        settingsButton = GameObject.Find("SettingsButton");
        creditsButton = GameObject.Find("CreditsButton");
        quitButton = GameObject.Find("QuitButton");
        
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

        // NEW: Hide play choice panel initially
        if (playChoicePanel != null)
        {
            playChoicePanel.SetActive(false);
            if (playChoiceCanvasGroup != null)
            {
                playChoiceCanvasGroup.alpha = 0f;
                playChoiceCanvasGroup.blocksRaycasts = false;
                playChoiceCanvasGroup.interactable = false;
            }
        }

        // Suppress UI sounds during logo loading and suppress clicks on menu buttons
        UIButtonFX.globalAudioEnabled = false; // disable hover/click audio while loading logo
        UIButtonFX.suppressClickInMainMenu = true; // ensure clicks in main menu don't play click sounds
        
        StartCoroutine(LogoStartup());

        // --- Initialize main menu music ---
        InitializeMainMenuMusic();
    }

    private void InitializeMainMenuMusic()
    {
        // Try to find a MusicManager if one isn't assigned
        if (musicManager == null)
        {
            var go = GameObject.Find("MainMenuMusic");
            if (go != null)
                musicManager = go.GetComponent<MusicManager>();

            if (musicManager == null)
                musicManager = FindObjectOfType<MusicManager>();
        }

        // If we have a MusicManager, prefer using it
        if (musicManager != null)
        {
            if (menuIntro != null && menuLoop != null)
            {
                _menuMusicCoroutine = StartCoroutine(PlayIntroThenLoopWithManager());
            }
            else if (menuLoop != null)
            {
                musicManager.SetAudioClip(menuLoop, true);
                musicManager.FadeAndPlay(0.5f, musicFadeInDuration);
            }

            return;
        }

        // Fallback to a local AudioSource if no MusicManager
        if (menuAudioSource == null)
        {
            var handler = GameObject.Find("EnvironmentSoundHandler");
            GameObject audioObj = new GameObject("MainMenuMusicSource");
            audioObj.transform.SetParent(handler ? handler.transform : transform);
            audioObj.transform.localPosition = Vector3.zero;
            menuAudioSource = audioObj.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
            menuAudioSource.spatialBlend = 0f; // 2D
            menuAudioSource.loop = true;
        }

        if (menuIntro != null && menuLoop != null)
        {
            _menuMusicCoroutine = StartCoroutine(PlayIntroThenLoopLocal());
        }
        else if (menuLoop != null)
        {
            menuAudioSource.clip = menuLoop;
            menuAudioSource.loop = true;
            menuAudioSource.volume = 0f;
            menuAudioSource.Play();
            StartCoroutine(FadeInAudio(menuAudioSource, 0.5f, musicFadeInDuration));
        }
    }

    private IEnumerator PlayIntroThenLoopWithManager()
    {
        if (musicManager == null) yield break;

        musicManager.SetAudioClip(menuIntro, false);
        musicManager.Play();

        // Wait while intro plays
        var src = musicManager.GetAudioSource();
        if (src != null)
        {
            yield return new WaitWhile(() => src.isPlaying);
        }
        else
        {
            // fallback: wait length of clip
            yield return new WaitForSeconds(menuIntro.length);
        }

        musicManager.SetAudioClip(menuLoop, true);
        musicManager.FadeAndPlay(0.5f, musicFadeInDuration);
    }

    private IEnumerator PlayIntroThenLoopLocal()
    {
        if (menuAudioSource == null) yield break;

        menuAudioSource.clip = menuIntro;
        menuAudioSource.loop = false;
        menuAudioSource.volume = 0.5f;
        menuAudioSource.Play();

        yield return new WaitWhile(() => menuAudioSource.isPlaying);

        menuAudioSource.clip = menuLoop;
        menuAudioSource.loop = true;
        menuAudioSource.Play();

        StartCoroutine(FadeInAudio(menuAudioSource, 0.5f, musicFadeInDuration));
    }

    private IEnumerator FadeInAudio(AudioSource src, float targetVolume, float duration)
    {
        if (src == null) yield break;
        float start = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(start, targetVolume, Mathf.Clamp01(t / duration));
            yield return null;
        }
        src.volume = targetVolume;
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
        
        // REDUCED: Just need to be slightly above screen, not way off
        float offScreenY = canvasHeight + 500f;  // Just 200 units above screen top

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

        // NO HOLD TIME - fade immediately

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

    /// <summary>
    /// Called when Play button is clicked - smart flow based on save existence
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[MainMenu] Play button clicked - checking for saves");
        
        bool hasSaves = CheckIfAnySavesExist();
        
        if (hasSaves)
        {
            // Saves exist - show choice menu
            Debug.Log("[MainMenu] Saves detected - showing choice menu");
            ShowPlayChoiceMenu();
        }
        else
        {
            // No saves - skip to name prompt
            Debug.Log("[MainMenu] No saves detected - skipping to name prompt");
            ShowSaveNamePrompt();
        }
    }

    /// <summary>
    /// Check if any save files exist in the Saves folder
    /// </summary>
    private bool CheckIfAnySavesExist()
    {
        string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(saveDirectory))
        {
            Debug.Log("[MainMenu] Save directory does not exist");
            return false;
        }
            
        string[] saveFiles = System.IO.Directory.GetFiles(saveDirectory, "GameFlags_*.json");
        Debug.Log($"[MainMenu] Found {saveFiles.Length} save files");
        return saveFiles.Length > 0;
    }

    /// <summary>
    /// Show the play choice menu with fade animation
    /// </summary>
    private void ShowPlayChoiceMenu()
    {
        if (playChoicePanel == null)
        {
            Debug.LogError("[MainMenu] PlayChoicePanel not assigned!");
            return;
        }
        
        playChoicePanel.SetActive(true);
        
        // IMMEDIATELY disable interaction and block raycasts before fading
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;  // Stop clicks immediately
        }
        
        // Fade out main menu logo and buttons
        if (mainMenuLogoCanvasGroup != null)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 1f, 0f, 0.3f));
        
        if (buttonsCanvasGroup != null)
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 1f, 0f, 0.3f));
        
        StartCoroutine(FadeInPlayChoice());
    }

    /// <summary>
    /// Hide the play choice menu with fade animation
    /// </summary>
    private void HidePlayChoiceMenu()
    {
        if (playChoicePanel == null) return;
        
        StartCoroutine(FadeOutPlayChoice());
        
        // Fade main menu logo and buttons back in
        if (mainMenuLogoCanvasGroup != null)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 0f, 1f, 0.3f));
        
        if (buttonsCanvasGroup != null)
        {
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 0f, 1f, 0.3f));
            // FIXED: Re-enable blocksRaycasts immediately so buttons can receive clicks
            buttonsCanvasGroup.blocksRaycasts = true;
            // Re-enable interaction after fade completes
            StartCoroutine(ReEnableButtonsAfterDelay(0.3f));
        }
    }

    /// <summary>
    /// Re-enable buttons canvas group after delay
    /// </summary>
    private IEnumerator ReEnableButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (buttonsCanvasGroup != null)
            buttonsCanvasGroup.interactable = true;
    }

    /// <summary>
    /// Fade in the play choice menu
    /// </summary>
    private IEnumerator FadeInPlayChoice()
    {
        if (playChoiceCanvasGroup == null) yield break;
        
        playChoiceCanvasGroup.blocksRaycasts = true;
        playChoiceCanvasGroup.interactable = false;
        
        float duration = 0.3f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            playChoiceCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        
        playChoiceCanvasGroup.alpha = 1f;
        playChoiceCanvasGroup.interactable = true;
    }

    /// <summary>
    /// Fade out the play choice menu
    /// </summary>
    private IEnumerator FadeOutPlayChoice()
    {
        if (playChoiceCanvasGroup == null)
        {
            playChoicePanel.SetActive(false);
            yield break;
        }
        
        playChoiceCanvasGroup.interactable = false;
        
        float duration = 0.3f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            playChoiceCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }
        
        playChoiceCanvasGroup.alpha = 0f;
        playChoiceCanvasGroup.blocksRaycasts = false;
        playChoicePanel.SetActive(false);
    }

    /// <summary>
    /// Show save name prompt (called when New Game is chosen)
    /// </summary>
    private void ShowSaveNamePrompt()
    {
        if (saveNamePrompt != null)
        {
            Debug.Log("[MainMenu] Showing save name prompt");
            
            // IMMEDIATELY disable interaction and block raycasts before fading
            if (buttonsCanvasGroup != null)
            {
                buttonsCanvasGroup.interactable = false;
                buttonsCanvasGroup.blocksRaycasts = false;  // Stop clicks immediately
            }
            
            // Fade out main menu elements before showing prompt
            if (mainMenuLogoCanvasGroup != null)
                StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 1f, 0f, 0.3f));
            
            if (buttonsCanvasGroup != null)
                StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 1f, 0f, 0.3f));
            
            saveNamePrompt.Show(OnSaveNameConfirmed, OnSaveNameCancelled);
        }
        else
        {
            Debug.LogError("[MainMenu] SaveNamePrompt not assigned!");
        }
    }

    /// <summary>
    /// NEW GAME button clicked from choice menu
    /// </summary>
    public void OnNewGameFromChoice()
    {
        Debug.Log("[MainMenu] New Game selected from choice menu");
        HidePlayChoiceMenu();
        ShowSaveNamePrompt();
    }

    /// <summary>
    /// CONTINUE button clicked from choice menu
    /// </summary>
    public void OnContinueFromChoice()
    {
        Debug.Log("[MainMenu] Continue selected from choice menu");
        
        // Hide play choice menu WITHOUT restoring main menu elements
        StartCoroutine(FadeOutPlayChoiceOnly());
        
        // Then show load game menu
        ShowLoadGameMenu();
    }

    /// <summary>
    /// BACK button clicked from choice menu
    /// </summary>
    public void OnBackFromChoice()
    {
        Debug.Log("[MainMenu] Back clicked from choice menu");
        HidePlayChoiceMenu();
    }

    /// <summary>
    /// Fade out play choice menu without restoring main menu
    /// </summary>
    private IEnumerator FadeOutPlayChoiceOnly()
    {
        if (playChoiceCanvasGroup == null)
        {
            if (playChoicePanel != null)
                playChoicePanel.SetActive(false);
            yield break;
        }
        
        playChoiceCanvasGroup.interactable = false;
        
        float duration = 0.3f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            playChoiceCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }
        
        playChoiceCanvasGroup.alpha = 0f;
        playChoiceCanvasGroup.blocksRaycasts = false;
        if (playChoicePanel != null)
            playChoicePanel.SetActive(false);
    }

    /// <summary>
    /// Show the load game menu
    /// </summary>
    private void ShowLoadGameMenu()
    {
        if (loadGameUI != null)
        {
            Debug.Log("[MainMenu] Opening load game UI");
            
            // ALWAYS ensure main menu elements are faded out and interaction is disabled
            if (buttonsCanvasGroup != null)
            {
                buttonsCanvasGroup.interactable = false;
                buttonsCanvasGroup.blocksRaycasts = false;
            }
            
            // Fade out main menu elements if they're visible
            if (mainMenuLogoCanvasGroup != null && mainMenuLogoCanvasGroup.alpha > 0.1f)
                StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, mainMenuLogoCanvasGroup.alpha, 0f, 0.3f));
            
            if (buttonsCanvasGroup != null && buttonsCanvasGroup.alpha > 0.1f)
                StartCoroutine(FadeCoroutine(buttonsCanvasGroup, buttonsCanvasGroup.alpha, 0f, 0.3f));
            
            // Subscribe to load event to transition to game
            SaveGameEvents.OnSaveLoaded += OnGameLoaded;
            
            loadGameUI.Show(OnLoadGameBack);
        }
        else
        {
            Debug.LogError("[MainMenu] LoadGameUI not assigned!");
        }
    }
    
    /// <summary>
    /// Called when user confirms save name
    /// </summary>
    private void OnSaveNameConfirmed(string saveName)
    {
        Debug.Log($"[MainMenu] Save name confirmed: {saveName}");
        
        // Create new save with this name
        bool success = GameFlagsManager.CreateNewSave(saveName);
        
        if (success)
        {
            // Start the game
            StartCoroutine(FadeAndLoad());
        }
        else
        {
            Debug.LogError($"[MainMenu] Failed to create save: {saveName}");
            // Fade main menu back in on error
            RestoreMainMenuElements();
        }
    }
    
    /// <summary>
    /// Called when user cancels save name prompt
    /// </summary>
    private void OnSaveNameCancelled()
    {
        Debug.Log("[MainMenu] Save name prompt cancelled");
        // Restore main menu elements
        RestoreMainMenuElements();
    }

    /// <summary>
    /// Restore main menu logo and buttons with fade
    /// </summary>
    private void RestoreMainMenuElements()
    {
        if (mainMenuLogoCanvasGroup != null)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 0f, 1f, 0.3f));
        
        if (buttonsCanvasGroup != null)
        {
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 0f, 1f, 0.3f));
            buttonsCanvasGroup.blocksRaycasts = true;  // Re-enable raycasts
            StartCoroutine(ReEnableButtonsAfterDelay(0.3f));
        }
    }
    
    /// <summary>
    /// Called when a save is loaded from the load game UI
    /// </summary>
    private void OnGameLoaded(string saveName)
    {
        Debug.Log($"[MainMenu] Game loaded: {saveName}");
        
        // Unsubscribe
        SaveGameEvents.OnSaveLoaded -= OnGameLoaded;
        
        // Load the game scene
        StartCoroutine(FadeAndLoad());
    }
    
    /// <summary>
    /// Called when back button is clicked in load game UI
    /// </summary>
    private void OnLoadGameBack()
    {
        Debug.Log("[MainMenu] Back from load game UI");
        // Unsubscribe in case we didn't load
        SaveGameEvents.OnSaveLoaded -= OnGameLoaded;
        
        // Restore main menu elements
        RestoreMainMenuElements();
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

































































































































































































