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
    [SerializeField] private Settings settingsComponent; // Changed from GameObject to Settings component
    
    [Header("Blur Effect")]
    [SerializeField] private Image blurOverlay;
    [Tooltip("Color and alpha of the blur overlay (darker = more visible blur effect)")]
    [SerializeField] private Color blurColor = new Color(0, 0, 0, 0.7f);
    [Tooltip("Fade duration for blur effect in seconds")]
    [SerializeField] private float blurFadeDuration = 0.2f;

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

        // Find player input
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            _playerInput = player.GetComponent<PlayerInput2D>();
        }

        // Find clock timer
        _clockTimer = FindFirstObjectByType<ClockTimer>();
        if (_clockTimer)
            Debug.Log("[SimplePauseMenu] ClockTimer found");
        else
            Debug.Log("[SimplePauseMenu] No ClockTimer in scene (this is OK if not in Overworld)");

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
        ShowPauseMenu(false);
    }

    void Start()
    {
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
            if (settingsComponent && settingsComponent.gameObject.activeSelf)
            {
                // Close settings, back to pause menu
                CloseSettings();
            }
            else
            {
                // Toggle pause
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Disable player input
        if (_playerInput != null)
        {
            _playerInput.isInputEnabled = false;
        }

        // Pause clock timer
        if (_clockTimer != null)
        {
            _clockTimer.PauseTimer(true);
            Debug.Log("[SimplePauseMenu] ClockTimer paused");
        }

        // Disable journal UI
        if (_journalUI != null)
        {
            _journalUI.SetInputEnabled(false);
        }

        ShowPauseMenu(true);
        ShowBlurEffect(true);
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Re-enable player input
        if (_playerInput != null)
        {
            _playerInput.isInputEnabled = true;
        }

        // Resume clock timer
        if (_clockTimer != null)
        {
            _clockTimer.PauseTimer(false);
            Debug.Log("[SimplePauseMenu] ClockTimer resumed");
        }

        // Re-enable journal UI
        if (_journalUI != null)
        {
            _journalUI.SetInputEnabled(true);
        }

        ShowBlurEffect(false);
        ShowPauseMenu(false);
    }

    void OpenSettings()
    {
        if (pausePanel)
        {
            pausePanel.SetActive(false);
        }
        if (settingsComponent)
        {
            settingsComponent.ShowSettings();
        }
    }

    public void CloseSettings()
    {
        if (settingsComponent)
        {
            settingsComponent.HideSettings();
        }
        if (pausePanel)
        {
            pausePanel.SetActive(true);
        }
    }

    void QuitToMenu()
    {
        // Re-enable everything before leaving
        Time.timeScale = 1f;
        if (_playerInput != null)
            _playerInput.isInputEnabled = true;
        if (_clockTimer != null)
            _clockTimer.PauseTimer(false);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ShowPauseMenu(bool show)
    {
        if (pausePanel)
        {
            pausePanel.SetActive(show);
        }

        if (settingsComponent)
        {
            settingsComponent.gameObject.SetActive(false);
        }
    }

    void ShowBlurEffect(bool show)
    {
        if (blurOverlay == null) return;

        // Stop any existing fade coroutine
        if (_blurFadeCoroutine != null)
        {
            StopCoroutine(_blurFadeCoroutine);
        }

        if (show)
        {
            blurOverlay.gameObject.SetActive(true);
            _blurFadeCoroutine = StartCoroutine(FadeBlur(0f, blurColor.a));
        }
        else
        {
            _blurFadeCoroutine = StartCoroutine(FadeBlur(blurOverlay.color.a, 0f));
        }
    }

    private System.Collections.IEnumerator FadeBlur(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = blurColor;

        while (elapsed < blurFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
            float t = elapsed / blurFadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            blurOverlay.color = color;
            yield return null;
        }

        // Ensure final alpha is set
        color.a = endAlpha;
        blurOverlay.color = color;

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
        Time.timeScale = 1f;
        if (_playerInput != null)
            _playerInput.isInputEnabled = true;
        if (_clockTimer != null)
            _clockTimer.PauseTimer(false);
    }

    public bool IsPaused => isPaused;
}
