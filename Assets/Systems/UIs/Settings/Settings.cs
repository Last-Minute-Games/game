using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class Settings : MonoBehaviour
{
    [Header("UI Elements (TMP)")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private Slider masterSlider;
    
    [Header("Optional")]
    [SerializeField] private GameObject settingsPanel; // The root panel GameObject (if you want to control a specific panel)
    [SerializeField] private Button applyButton; // Wire up the Apply button

    [Header("Main Menu References (for MainMenu scene only)")]
    [SerializeField] private CanvasGroup settingsCanvasGroup; // For fading settings in/out
    [SerializeField] private GameObject mainMenuLogo;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private CanvasGroup mainMenuLogoCanvasGroup;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private GraphicRaycaster mainMenuRaycaster; // To block input during transitions
    [SerializeField] private float fadeDuration = 1f;

    private SimplePauseMenu _pauseMenu;
    private bool _isMainMenu = false;
    private bool _settingsPlaying = false; // Prevent re-entry

    void Awake()
    {
        // If no specific panel is assigned, assume this GameObject is the panel
        if (settingsPanel == null)
            settingsPanel = gameObject;

        // Check if we're in the MainMenu scene
        _isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

        // Find SimplePauseMenu if not in main menu
        if (!_isMainMenu)
        {
            _pauseMenu = FindFirstObjectByType<SimplePauseMenu>();
        }

        // Auto-setup canvas groups if in main menu
        if (_isMainMenu)
        {
            if (settingsPanel && !settingsCanvasGroup)
            {
                settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
                if (!settingsCanvasGroup)
                    settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            }

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

            // Initialize settings panel to hidden in main menu
            if (settingsCanvasGroup)
            {
                settingsCanvasGroup.alpha = 0f;
                settingsPanel.SetActive(false);
            }
        }

        // Wire up Apply button
        if (applyButton)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(OnApplyClicked);
        }
    }

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    void Start()
    {
        StartCoroutine(BindWhenReady());
    }

    IEnumerator BindWhenReady()
    {
        // Wait up to ~2s for SettingsManager to exist, or create it
        float t = 0f;
        while (SettingsManager.I == null && t < 2f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // If still null, auto-create it
        var S = SettingsManager.I ?? SettingsManager.GetOrCreate();
        if (S == null)
        {
            Debug.LogError("[Settings] Failed to get or create SettingsManager!");
            yield break;
        }

        // --- RESOLUTIONS ---
        var resLabels = new List<string>();
        if (S.ResList.Count > 0)
        {
            foreach (var r in S.ResList)
                resLabels.Add($"{r.w} x {r.h}");
        }
        else
        {
            var res = Screen.resolutions
                .OrderByDescending(r => r.width * r.height)
                .ThenByDescending(r => r.refreshRate)
                .GroupBy(r => (r.width, r.height))
                .Select(g => g.First())
                .ToList();

            foreach (var r in res)
                resLabels.Add($"{r.width} x {r.height}");
        }

        if (resolutionDropdown)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resLabels);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(S.ResolutionIndex, 0, Mathf.Max(0, resLabels.Count - 1)));
            
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(i => S.ApplyResolution(i));
        }

        // --- SCREEN MODE ---
        if (screenModeDropdown)
        {
            var modes = new List<string> { "Windowed", "Borderless", "Fullscreen" };
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(modes);
            screenModeDropdown.SetValueWithoutNotify((int)S.ScreenMode);
            
            screenModeDropdown.onValueChanged.RemoveAllListeners();
            screenModeDropdown.onValueChanged.AddListener(i => S.ApplyScreenMode(i));
        }

        // --- MASTER VOLUME ---
        if (masterSlider)
        {
            masterSlider.SetValueWithoutNotify(S.MasterVolume);
            
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(v => S.ApplyMaster(v));
        }

        Debug.Log("[Settings] UI bound successfully");
    }

    /// <summary>
    /// Called when Apply button is clicked
    /// </summary>
    public void OnApplyClicked()
    {
        Debug.Log($"[Settings] Apply clicked - IsMainMenu: {_isMainMenu}");

        if (_isMainMenu)
        {
            // Fade out settings and fade in main menu
            StartCoroutine(ApplyInMainMenu());
        }
        else
        {
            // Simply hide settings and show pause menu
            HideSettings();
            if (_pauseMenu)
            {
                _pauseMenu.CloseSettings();
            }
        }
    }

    private IEnumerator ApplyInMainMenu()
    {
        if (_settingsPlaying) yield break; // Prevent re-entry
        _settingsPlaying = true;

        // 1. Fade out settings
        if (settingsCanvasGroup)
        {
            yield return StartCoroutine(FadeCoroutine(settingsCanvasGroup, 1f, 0f, fadeDuration));
            settingsCanvasGroup.blocksRaycasts = false;
            settingsCanvasGroup.interactable = false;
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(false);
        }

        // 2. Show main menu elements
        if (mainMenuLogo) mainMenuLogo.SetActive(true);
        if (buttonsParent) buttonsParent.SetActive(true);

        // 3. Fade in main menu logo and buttons
        if (mainMenuLogoCanvasGroup)
        {
            mainMenuLogoCanvasGroup.alpha = 0f; // Ensure it starts at 0
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 0f, 1f, fadeDuration));
        }
        if (buttonsCanvasGroup)
        {
            buttonsCanvasGroup.alpha = 0f; // Ensure it starts at 0
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 0f, 1f, fadeDuration));
        }

        yield return new WaitForSeconds(fadeDuration);

        // Re-enable raycaster
        if (mainMenuRaycaster)
            mainMenuRaycaster.enabled = true;

        _settingsPlaying = false;
        Debug.Log("[Settings] Returned to main menu");
    }

    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Use unscaled time for pause menu compatibility
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Show or hide the settings panel
    /// </summary>
    public void Show(bool show)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(show);
            Debug.Log($"[Settings] Panel {(show ? "shown" : "hidden")}");
        }
    }

    /// <summary>
    /// Show the settings panel (with fade if in MainMenu)
    /// </summary>
    public void ShowSettings()
    {
        if (_isMainMenu)
        {
            if (_settingsPlaying) return; // Prevent re-entry like credits
            StartCoroutine(ShowSettingsInMainMenu());
        }
        else
        {
            Show(true);
        }
    }

    /// <summary>
    /// Hide the settings panel
    /// </summary>
    public void HideSettings()
    {
        Show(false);
    }

    private IEnumerator ShowSettingsInMainMenu()
    {
        _settingsPlaying = true;
        
        // 1. Disable main menu raycaster (block all input)
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = false;

        // 2. Fade out main menu logo and buttons
        if (mainMenuLogoCanvasGroup)
            StartCoroutine(FadeCoroutine(mainMenuLogoCanvasGroup, 1f, 0f, fadeDuration));
        if (buttonsCanvasGroup)
            StartCoroutine(FadeCoroutine(buttonsCanvasGroup, 1f, 0f, fadeDuration));

        // Wait for fade out to complete
        yield return new WaitForSeconds(fadeDuration);

        // 3. Hide main menu elements
        if (mainMenuLogo) mainMenuLogo.SetActive(false);
        if (buttonsParent) buttonsParent.SetActive(false);

        // 4. Show and fade in settings
        settingsPanel.SetActive(true);
        if (settingsCanvasGroup)
        {
            settingsCanvasGroup.alpha = 0f; // Ensure it starts at 0
            settingsCanvasGroup.blocksRaycasts = true;
            settingsCanvasGroup.interactable = true;
            yield return StartCoroutine(FadeCoroutine(settingsCanvasGroup, 0f, 1f, fadeDuration));
        }

        // 5. Re-enable raycaster for settings interaction
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = true;

        _settingsPlaying = false;
        Debug.Log("[Settings] Opened in main menu with fade");
    }
}
