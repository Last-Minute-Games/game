using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader s_instance;

    [Header("UI")]
    public Image fadePanel;
    public float fadeDuration = 2f;

    [Header("Split Panel (Eyes Closing)")]
    public float splitPanelDuration = 1.5f;

    private RectTransform topPanel;
    private RectTransform bottomPanel;
    private bool isTransitioning = false;

    [HideInInspector]
    public bool shouldOpenEyesOnSceneLoad = false; // Flag to control eyes opening

    private void Awake()
    {
        // Singleton: ensure only one ScreenFader persists
        if (s_instance != null && s_instance != this)
        {
            Debug.Log("[ScreenFader] Duplicate instance detected, destroying new one.");
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadePanel == null)
        {
            Debug.LogError("ScreenFader: Fade panel not assigned!");
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start fully transparent and disabled
        fadePanel.color = new Color(0, 0, 0, 0f);
        if (fadePanel.gameObject.activeSelf)
            fadePanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe and clear singleton reference
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (s_instance == this) s_instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If a duplicate ScreenFader existed in the new scene it's already been destroyed in Awake of that instance.

        // If not transitioning, ensure the fade overlay isn't accidentally left active
        if (fadePanel != null && !isTransitioning)
        {
            fadePanel.gameObject.SetActive(false);
        }

        // Check if we should open eyes on this scene load
        if (shouldOpenEyesOnSceneLoad)
        {
            Debug.Log("[ScreenFader] Scene loaded - opening eyes!");
            shouldOpenEyesOnSceneLoad = false; // Reset flag
            StartCoroutine(EyesOpeningEffect());
        }
        else
        {
            // Only fade in if we just came from a transition
            if (isTransitioning)
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    private void CreateSplitPanels()
    {
        // Get the Canvas (parent of fadePanel)
        Canvas canvas = fadePanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ScreenFader] Cannot find Canvas to create split panels!");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create Top Panel
        GameObject topPanelObj = new GameObject("EyeTopPanel");
        topPanelObj.transform.SetParent(canvas.transform, false);
        topPanel = topPanelObj.AddComponent<RectTransform>();
        Image topImage = topPanelObj.AddComponent<Image>();
        topImage.color = Color.black;
        topImage.raycastTarget = false;

        // Setup top panel RectTransform (stretches across top, half screen height)
        topPanel.anchorMin = new Vector2(0, 0.5f);
        topPanel.anchorMax = new Vector2(1, 1);
        topPanel.pivot = new Vector2(0.5f, 0f);
        topPanel.offsetMin = Vector2.zero;
        topPanel.offsetMax = Vector2.zero;

        // Create Bottom Panel
        GameObject bottomPanelObj = new GameObject("EyeBottomPanel");
        bottomPanelObj.transform.SetParent(canvas.transform, false);
        bottomPanel = bottomPanelObj.AddComponent<RectTransform>();
        Image bottomImage = bottomPanelObj.AddComponent<Image>();
        bottomImage.color = Color.black;
        bottomImage.raycastTarget = false;

        // Setup bottom panel RectTransform (stretches across bottom, half screen height)
        bottomPanel.anchorMin = new Vector2(0, 0);
        bottomPanel.anchorMax = new Vector2(1, 0.5f);
        bottomPanel.pivot = new Vector2(0.5f, 1f);
        bottomPanel.offsetMin = Vector2.zero;
        bottomPanel.offsetMax = Vector2.zero;

        Debug.Log("[ScreenFader] Split panels created");
    }

    private void DestroySplitPanels()
    {
        if (topPanel != null)
        {
            Destroy(topPanel.gameObject);
            topPanel = null;
        }

        if (bottomPanel != null)
        {
            Destroy(bottomPanel.gameObject);
            bottomPanel = null;
        }

        Debug.Log("[ScreenFader] Split panels destroyed");
    }

    public IEnumerator FadeOut()
    {
        isTransitioning = true;

        // Ensure overlay is visible
        if (fadePanel != null && !fadePanel.gameObject.activeSelf)
            fadePanel.gameObject.SetActive(true);

        float t = 0f;
        Color c = fadePanel.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        fadePanel.color = Color.black;
    }

    public IEnumerator FadeIn()
    {
        // Ensure overlay is visible to fade from
        if (fadePanel != null && !fadePanel.gameObject.activeSelf)
            fadePanel.gameObject.SetActive(true);

        float t = 0f;
        Color c = fadePanel.color;
        c.a = 1f;
        fadePanel.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.SmoothStep(1f, 0f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, 0f);
        isTransitioning = false;

        // Hide overlay once finished
        if (fadePanel != null && fadePanel.gameObject.activeSelf)
            fadePanel.gameObject.SetActive(false);
    }

    public void SetPanelAlpha(float alpha)
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = Mathf.Clamp01(alpha);
            fadePanel.color = c;

            // If alpha > 0 ensure it's active
            if (alpha > 0f && !fadePanel.gameObject.activeSelf)
                fadePanel.gameObject.SetActive(true);
            else if (alpha <= 0f && fadePanel.gameObject.activeSelf && !isTransitioning)
                fadePanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Eyes closing effect: two black panels slide in from top and bottom
    /// </summary>
    public IEnumerator EyesClosingEffect()
    {
        // Ensure overlay is visible so panels render on the same canvas
        if (fadePanel != null && !fadePanel.gameObject.activeSelf)
            fadePanel.gameObject.SetActive(true);

        // Create panels on demand
        if (topPanel == null || bottomPanel == null)
        {
            CreateSplitPanels();
        }

        if (topPanel == null || bottomPanel == null)
        {
            Debug.LogWarning("[ScreenFader] Failed to create split panels, using regular fade");
            yield return StartCoroutine(FadeOut());
            yield break;
        }

        isTransitioning = true;

        // Get the screen height (RectTransform height)
        Canvas canvas = fadePanel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float screenHeight = canvasRect.rect.height;
        float halfHeight = screenHeight / 2f;

        // Start positions (off screen)
        Vector2 topStart = new Vector2(0, halfHeight);
        Vector2 bottomStart = new Vector2(0, -halfHeight);

        // End positions (covering screen)
        Vector2 topEnd = new Vector2(0, 0);
        Vector2 bottomEnd = new Vector2(0, 0);

        topPanel.anchoredPosition = topStart;
        bottomPanel.anchoredPosition = bottomStart;

        float t = 0f;
        while (t < splitPanelDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / splitPanelDuration);

            topPanel.anchoredPosition = Vector2.Lerp(topStart, topEnd, progress);
            bottomPanel.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, progress);

            yield return null;
        }

        topPanel.anchoredPosition = topEnd;
        bottomPanel.anchoredPosition = bottomEnd;

        Debug.Log("[ScreenFader] Eyes closed!");
    }

    /// <summary>
    /// Eyes opening effect: two black panels slide out to top and bottom
    /// </summary>
    public IEnumerator EyesOpeningEffect()
    {
        if (topPanel == null || bottomPanel == null)
        {
            Debug.LogWarning("[ScreenFader] Split panels not assigned, using regular fade");
            yield return StartCoroutine(FadeIn());
            yield break;
        }

        // Get the screen height
        Canvas canvas = fadePanel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float screenHeight = canvasRect.rect.height;
        float halfHeight = screenHeight / 2f;

        // Start positions (covering screen)
        Vector2 topStart = new Vector2(0, 0);
        Vector2 bottomStart = new Vector2(0, 0);

        // End positions (off screen)
        Vector2 topEnd = new Vector2(0, halfHeight);
        Vector2 bottomEnd = new Vector2(0, -halfHeight);

        topPanel.anchoredPosition = topStart;
        bottomPanel.anchoredPosition = bottomStart;

        float t = 0f;
        while (t < splitPanelDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / splitPanelDuration);

            topPanel.anchoredPosition = Vector2.Lerp(topStart, topEnd, progress);
            bottomPanel.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, progress);

            yield return null;
        }

        topPanel.anchoredPosition = topEnd;
        bottomPanel.anchoredPosition = bottomEnd;

        // Destroy the panels after opening
        DestroySplitPanels();

        // Clear the fade panel completely
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
            // Also disable the FadeOverlay GameObject to be safe
            if (fadePanel.gameObject.activeSelf)
                fadePanel.gameObject.SetActive(false);
            Debug.Log("[ScreenFader] Cleared and disabled fade panel");
        }

        isTransitioning = false;
        Debug.Log("[ScreenFader] Eyes opened!");
    }

    public IEnumerator TransitionToScene(string nextScene)
    {
        // Keep the panels closed during transition
        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        // Reset split panels after scene loads
        if (topPanel != null && bottomPanel != null)
        {
            // Ensure panels are positioned off-screen for the new scene
            Canvas canvas = fadePanel.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            topPanel.anchoredPosition = new Vector2(0, screenHeight / 2f);
            bottomPanel.anchoredPosition = new Vector2(0, -screenHeight / 2f);
        }
    }

    /// <summary>
    /// Transition to scene with eyes closing effect
    /// </summary>
    public IEnumerator TransitionToSceneWithEyesClosing(string nextScene)
    {
        yield return StartCoroutine(EyesClosingEffect());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;
    }

    /// <summary>
    /// Transition to scene but KEEP the panels closed (for death sequence)
    /// The panels will stay closed until the new scene opens them
    /// </summary>
    public IEnumerator TransitionToSceneKeepPanelsClosed(string nextScene)
    {
        isTransitioning = true;

        Debug.Log($"[ScreenFader] Starting transition to {nextScene} - keeping panels closed");

        // Just load the scene, don't reset panels
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log("[ScreenFader] Scene loaded - panels still closed, waiting for eyes to open");
    }
}