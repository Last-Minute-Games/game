using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Dead simple pause menu. Just add this to a Canvas GameObject in your scene.
/// Press Escape to pause/unpause. That's it.
/// </summary>
public class SimplePauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Blur Effect")]
    [SerializeField] private Image blurOverlay;
    [Tooltip("Color and alpha of the blur overlay (darker = more visible blur effect)")]
    [SerializeField] private Color blurColor = new Color(0, 0, 0, 0.7f);
    [Tooltip("Fade duration for blur effect in seconds")]
    [SerializeField] private float blurFadeDuration = 0.2f;

    [Header("Quit Fade Effect")]
    [SerializeField] private Image quitFadeOverlay;
    [Tooltip("Duration for fade to black when quitting")]
    [SerializeField] private float quitFadeDuration = 1f;

    [Header("Buttons - Wire these up in inspector")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private Canvas canvas;
    private PlayerInput2D _playerInput;
    private ClockTimer _clockTimer;
    private JournalUI _journalUI;
    private Coroutine _blurFadeCoroutine;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SimplePauseMenu] This script must be on a GameObject with a Canvas component!");
            enabled = false;
            return;
        }

        // Setup blur overlay if assigned
        if (blurOverlay != null)
        {
            blurOverlay.gameObject.SetActive(false);
            blurOverlay.raycastTarget = false;
            Color transparent = blurColor;
            transparent.a = 0f;
            blurOverlay.color = transparent;
        }

        // Setup quit fade overlay if assigned
        if (quitFadeOverlay != null)
        {
            quitFadeOverlay.gameObject.SetActive(false);
            quitFadeOverlay.raycastTarget = true;
            quitFadeOverlay.color = new Color(0, 0, 0, 0);
        }

        // Find player input
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            _playerInput = player.GetComponent<PlayerInput2D>();
        }

        // Find clock timer
        _clockTimer = FindFirstObjectByType<ClockTimer>();

        // Find journal UI
        _journalUI = FindFirstObjectByType<JournalUI>();

        // Wire up buttons
        if (resumeButton)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (settingsButton)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (quitButton)
        {
            quitButton.onClick.AddListener(QuitToMenu);
        }

        // Start hidden
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Don't allow pausing if journal is open
        if (_journalUI != null && _journalUI.IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Escape should close catalog first (both BattleScene and Overworld).
            if (BattleCardCatalogPopup.CloseAnyOpenCatalog())
            {
                return;
            }

            if (RoomMapUI.IsMapVisible)
            {
                OpenPauseFromMap();
                return;
            }

            if (settingsPanel && settingsPanel.activeSelf)
            {
                // Close settings, back to pause menu
                CloseSettings();
            }
            else if (isPaused)
            {
                // Resume if pause menu is open
                Resume();
            }
            else
            {
                // Open pause menu
                Pause();
            }
        }
    }

    void Pause()
    {
        if (BattleCardCatalogPopup.IsAnyCatalogOpen())
            return;

        isPaused = true;
        GlobalPause.SetPaused(true);

        if (pausePanel) pausePanel.SetActive(true);
        ShowBlurEffect(true);
    }

    void Resume()
    {
        isPaused = false;
        GlobalPause.SetPaused(false);

        ShowBlurEffect(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void OpenSettings()
    {
        if (BattleCardCatalogPopup.IsAnyCatalogOpen())
            return;

        // Just show settings on top of pause menu
        if (settingsPanel)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        // Just hide settings, pause menu is still visible underneath
        if (settingsPanel)
        {
            settingsPanel.SetActive(false);
        }

        if (isPaused && pausePanel && !pausePanel.activeSelf)
        {
            pausePanel.SetActive(true);
        }
    }

    private void OpenPauseFromMap()
    {
        var mapUI = FindFirstObjectByType<RoomMapUI>();
        if (mapUI != null)
        {
            mapUI.CloseFromExternal(true);
        }

        Pause();

        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void QuitToMenu()
    {
        StartCoroutine(QuitWithFade());
    }

    private System.Collections.IEnumerator QuitWithFade()
    {
        // Enable the quit fade overlay
        if (quitFadeOverlay != null)
        {
            quitFadeOverlay.gameObject.SetActive(true);

            // Fade to black
            float elapsed = 0f;
            Color fadeColor = new Color(0, 0, 0, 0);

            while (elapsed < quitFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / quitFadeDuration;
                fadeColor.a = Mathf.Lerp(0f, 1f, t);
                quitFadeOverlay.color = fadeColor;
                yield return null;
            }

            // Ensure fully black
            fadeColor.a = 1f;
            quitFadeOverlay.color = fadeColor;
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // Re-enable everything before leaving via GlobalPause
        GlobalPause.SetPaused(false);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ShowBlurEffect(bool show)
    {
        if (blurOverlay == null) return;

        // Stop any existing fade coroutine
        if (_blurFadeCoroutine != null)
        {
            StopCoroutine(_blurFadeCoroutine);
            _blurFadeCoroutine = null;
        }

        if (show)
        {
            blurOverlay.gameObject.SetActive(true);
            float currentAlpha = blurOverlay.color.a;
            _blurFadeCoroutine = StartCoroutine(FadeBlur(currentAlpha, blurColor.a));
        }
        else
        {
            float currentAlpha = blurOverlay.color.a;
            _blurFadeCoroutine = StartCoroutine(FadeBlur(currentAlpha, 0f));
        }
    }

    private System.Collections.IEnumerator FadeBlur(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < blurFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / blurFadeDuration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color color = blurColor;
            color.a = alpha;
            blurOverlay.color = color;
            yield return null;
        }

        // Ensure final alpha is set
        Color finalColor = blurColor;
        finalColor.a = endAlpha;
        blurOverlay.color = finalColor;

        // Deactivate if fully transparent
        if (endAlpha == 0f)
        {
            blurOverlay.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Stop any running coroutines
        if (_blurFadeCoroutine != null)
        {
            StopCoroutine(_blurFadeCoroutine);
        }

        // Always reset everything when destroyed
        GlobalPause.SetPaused(false);
    }

    public bool IsPaused => isPaused;

    /// <summary>True when the escape pause stack is visible (pause root or settings panel).</summary>
    public bool IsPauseOrSettingsOpen =>
        (pausePanel != null && pausePanel.activeSelf) ||
        (settingsPanel != null && settingsPanel.activeSelf);
}