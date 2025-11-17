using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ClockTimer : MonoBehaviour
{
    [Header("Clock Setup")]
    public Image clockImage;
    public Sprite[] clockFrames;
    public float totalTime = 60f;
    public string nextSceneName = "NextScene";
    public bool startAutomatically = true;

    [Header("Transition / Fade")]
    public ScreenFader screenFader;
    public TMP_Text endMessageText;
    public float preFadeTime = 15f;
    public float messageDisplayTime = 2f;

    private float timeLeft;
    private int frameCount;
    private int lastFrameIndex = -1;
    private bool hasEnded = false;
    private bool isPaused = false;
    private int lastWholeSecond = -1;
    public static bool IsTimeEnded { get; private set; } = false;

    [Header("Audio Warning (Heartbeat)")]
    public AudioSource warningAudioSource;
    public AudioClip warningClip;
    [Range(0f, 1f)] public float warningVolume = 0.7f;
    public float warningThreshold = 10f;
    private bool warningPlayed = false;

    [Header("Clock Bell Warning (5 seconds)")]
    public AudioSource bellAudioSource;
    public AudioClip bellClip;
    [Range(0f, 1f)] public float bellVolume = 0.8f;
    public float bellTriggerTime = 5f;
    private bool bellPlayed = false;

    [Header("Clock Tick Audio")]
    public AudioSource tickAudioSource;
    public AudioClip tickClip;
    [Range(0f, 1f)] public float tickVolume = 0.5f;

    public static ClockTimer Instance { get; private set; }
    
    private List<Light> allLights = new List<Light>();
    private List<float> originalLightIntensities = new List<float>();
    private List<UnityEngine.Rendering.Universal.Light2D> allLight2Ds = new List<UnityEngine.Rendering.Universal.Light2D>();
    private List<float> originalLight2DIntensities = new List<float>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[ClockTimer] Multiple ClockTimer instances found!");
        }
    }

    void Start()
    {
        if (clockFrames.Length == 0 || clockImage == null || screenFader == null)
        {
            Debug.LogError("[ClockTimer] Missing references!");
            return;
        }
        
        warningPlayed = false;
        bellPlayed = false;
        frameCount = clockFrames.Length;
        clockImage.sprite = clockFrames[0];
        screenFader.SetPanelAlpha(0f);

        if (endMessageText != null)
        {
            endMessageText.alpha = 0f;
            endMessageText.text = "YOU DIED!";
            endMessageText.alignment = TMPro.TextAlignmentOptions.Center;
            endMessageText.fontSize = 72;
            endMessageText.color = Color.red;

            RectTransform textRect = endMessageText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            endMessageText.gameObject.SetActive(false);
        }

        if (warningAudioSource == null)
        {
            warningAudioSource = gameObject.AddComponent<AudioSource>();
            warningAudioSource.playOnAwake = false;
            warningAudioSource.spatialBlend = 0f;
        }

        if (bellAudioSource == null)
        {
            bellAudioSource = gameObject.AddComponent<AudioSource>();
            bellAudioSource.playOnAwake = false;
            bellAudioSource.spatialBlend = 0f;
        }

        if (tickAudioSource == null)
        {
            tickAudioSource = gameObject.AddComponent<AudioSource>();
            tickAudioSource.playOnAwake = false;
            tickAudioSource.spatialBlend = 0f;
        }

        if (startAutomatically)
        {
            StartTimer(totalTime);
            Debug.Log("[ClockTimer] Timer started automatically");
        }
        
        StartCoroutine(InitialFadeIn());
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (isPaused || hasEnded) return;

        if (timeLeft > 0f)
        {
            float previousTime = timeLeft;
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(timeLeft, 0f);

            float progress = 1f - (timeLeft / totalTime);
            int frameIndex = Mathf.FloorToInt(progress * frameCount);
            frameIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);

            if (frameIndex != lastFrameIndex)
            {
                clockImage.sprite = clockFrames[frameIndex];
                lastFrameIndex = frameIndex;
                Debug.Log($"[ClockTimer] Frame changed: {frameIndex}/{frameCount - 1} | Time left: {timeLeft:F2}s");
                PlayTickSound();
            }

            int currentSecond = Mathf.FloorToInt(timeLeft);
            if (currentSecond != lastWholeSecond)
            {
                Debug.Log($"[ClockTimer] Time left: {timeLeft:F1}s");
                lastWholeSecond = currentSecond;
            }

            if (timeLeft <= bellTriggerTime && !bellPlayed && bellClip != null)
            {
                PlayBellSound();
                bellPlayed = true;
                Debug.Log("[ClockTimer] Grandfather clock bell triggered at 5 seconds!");
            }

            if (timeLeft <= warningThreshold && warningClip != null)
            {
                if (!warningAudioSource.isPlaying)
                {
                    warningAudioSource.clip = warningClip;
                    warningAudioSource.loop = true;
                    warningAudioSource.volume = 0f;
                    warningAudioSource.Play();
                }

                float volumeFactor = 1f - (timeLeft / warningThreshold);
                warningAudioSource.volume = Mathf.Lerp(0.2f, warningVolume, volumeFactor);
                warningAudioSource.pitch = Mathf.Lerp(1f, 1.5f, volumeFactor);
            }
            else if (timeLeft > warningThreshold && warningAudioSource.isPlaying)
            {
                warningAudioSource.Stop();
            }

            if (screenFader != null && timeLeft <= preFadeTime)
            {
                float fadeTarget = Mathf.Lerp(0f, 0.8f, 1f - (timeLeft / preFadeTime));
                float currentAlpha = screenFader.fadePanel.color.a;
                screenFader.SetPanelAlpha(Mathf.MoveTowards(currentAlpha, fadeTarget, Time.deltaTime / preFadeTime));
            }

            if (timeLeft <= 0f && !hasEnded)
            {
                hasEnded = true;
                IsTimeEnded = true;
                Debug.Log("[ClockTimer] Timer finished! Showing message and transitioning...");
                StartCoroutine(FadeMessageThenTransition());
            }
        }
    }

    public void StartTimer(float seconds)
    {
        totalTime = Mathf.Max(0.01f, seconds);
        timeLeft = totalTime;
        lastFrameIndex = -1;
        lastWholeSecond = -1;
        hasEnded = false;
        IsTimeEnded = false;
        isPaused = false;
        bellPlayed = false;

        screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;

        Debug.Log($"[ClockTimer] Timer started: {totalTime}s");
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
        Debug.Log($"[ClockTimer] Timer {(pause ? "paused" : "resumed")}");
        
        if (warningAudioSource != null && warningAudioSource.isPlaying)
        {
            if (pause)
                warningAudioSource.Pause();
            else
                warningAudioSource.UnPause();
        }
        
        if (bellAudioSource != null && bellAudioSource.isPlaying)
        {
            if (pause)
                bellAudioSource.Pause();
            else
                bellAudioSource.UnPause();
        }
    }

    public void AddTime(float seconds)
    {
        if (seconds <= 0f) return;
        
        float previousTime = timeLeft;
        timeLeft += seconds;
        totalTime += seconds;
        
        if (previousTime <= bellTriggerTime && timeLeft > bellTriggerTime)
        {
            bellPlayed = false;
            Debug.Log("[ClockTimer] Bell flag reset due to time addition");
        }
        
        Debug.Log($"[ClockTimer] Added {seconds}s. Time left: {timeLeft:F2}s");
    }

    public void RemoveTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft = Mathf.Max(0f, timeLeft - seconds);
        totalTime = Mathf.Max(0.01f, totalTime - seconds);
        Debug.Log($"[ClockTimer] Removed {seconds}s. Time left: {timeLeft:F2}s");

        if (timeLeft <= 0f && !hasEnded)
        {
            hasEnded = true;
            StartCoroutine(FadeMessageThenTransition());
        }
    }

    private void PlayTickSound()
    {
        if (tickAudioSource != null && tickClip != null)
        {
            tickAudioSource.volume = tickVolume;
            tickAudioSource.PlayOneShot(tickClip);
        }
    }

    private void PlayBellSound()
    {
        if (bellAudioSource != null && bellClip != null)
        {
            bellAudioSource.volume = bellVolume;
            bellAudioSource.PlayOneShot(bellClip);
            Debug.Log("[ClockTimer] Grandfather clock bell sound played");
        }
    }

    private IEnumerator InitialFadeIn()
    {
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }

    private void DisableAllLights()
    {
        allLights.Clear();
        originalLightIntensities.Clear();
        allLight2Ds.Clear();
        originalLight2DIntensities.Clear();

        // 3D lights
        Light[] lights3D = FindObjectsOfType<Light>(true);
        foreach (Light light in lights3D)
        {
            allLights.Add(light);
            originalLightIntensities.Add(light.intensity);
            // set to low but not zero so UI animation remains visible
            light.intensity = 0.1f;
        }

        // 2D lights (Universal RP)
        var lights2D = FindObjectsOfType<UnityEngine.Rendering.Universal.Light2D>(true);
        foreach (var l2 in lights2D)
        {
            allLight2Ds.Add(l2);
            originalLight2DIntensities.Add(l2.intensity);
            l2.intensity = 0.1f;
        }

        Debug.Log($"[ClockTimer] Dimmed {allLights.Count} 3D lights and {allLight2Ds.Count} 2D lights for death sequence");
    }

    private void RestoreAllLights()
    {
        for (int i = 0; i < allLights.Count; i++)
        {
            if (allLights[i] != null)
                allLights[i].intensity = originalLightIntensities[i];
        }

        for (int i = 0; i < allLight2Ds.Count; i++)
        {
            if (allLight2Ds[i] != null)
                allLight2Ds[i].intensity = originalLight2DIntensities[i];
        }

        Debug.Log($"[ClockTimer] Restored {allLights.Count} 3D lights and {allLight2Ds.Count} 2D lights");

        allLights.Clear();
        originalLightIntensities.Clear();
        allLight2Ds.Clear();
        originalLight2DIntensities.Clear();
    }

    // Ensure split panels and fade canvas are prepared so the EyesClosingEffect is visible and doesn't create duplicates behind other UI
    private void PrepareScreenFaderForEyes()
    {
        if (screenFader == null || screenFader.fadePanel == null) 
        {
            Debug.LogWarning("[ClockTimer] ScreenFader or fadePanel is null, cannot prepare eyes closing effect");
            return;
        }

        // Make sure fade panel is active first
        if (!screenFader.fadePanel.gameObject.activeInHierarchy)
            screenFader.fadePanel.gameObject.SetActive(true);

        // Make sure fade panel's canvas renders on top
        var canvas = screenFader.fadePanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            // Use a very high sorting order but keep room for other UI
            canvas.sortingOrder = 32767;
            canvas.enabled = true;
        }
        else
        {
            Debug.LogWarning("[ClockTimer] Could not find Canvas for fadePanel");
        }

        screenFader.fadePanel.transform.SetAsLastSibling();

        // Reset fade panel alpha to ensure clean start
        screenFader.SetPanelAlpha(0f);

        // If ScreenFader provides split panels, make sure they are active and placed above other UI
        if (screenFader.topPanel != null && screenFader.bottomPanel != null)
        {
            // Parent panels to the same canvas as the fadePanel if possible so sorting works predictably
            Canvas parentCanvas = null;
            if (screenFader.fadePanel != null)
                parentCanvas = screenFader.fadePanel.GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                if (screenFader.topPanel.GetComponentInParent<Canvas>() == null)
                    screenFader.topPanel.SetParent(parentCanvas.transform, false);
                if (screenFader.bottomPanel.GetComponentInParent<Canvas>() == null)
                    screenFader.bottomPanel.SetParent(parentCanvas.transform, false);

                // Ensure panels are visible and on top
                screenFader.topPanel.gameObject.SetActive(true);
                screenFader.bottomPanel.gameObject.SetActive(true);
                screenFader.topPanel.SetAsLastSibling();
                screenFader.bottomPanel.SetAsLastSibling();
            }
            else
            {
                // No parent canvas found; just ensure panels are active and last sibling under their current parent
                screenFader.topPanel.gameObject.SetActive(true);
                screenFader.bottomPanel.gameObject.SetActive(true);
                screenFader.topPanel.SetAsLastSibling();
                screenFader.bottomPanel.SetAsLastSibling();
            }
        }
    }

    private IEnumerator FadeMessageThenTransition()
    {
        if (warningAudioSource != null && warningAudioSource.isPlaying)
            warningAudioSource.Stop();

        if (bellAudioSource != null && bellAudioSource.isPlaying)
            bellAudioSource.Stop();

        // Give engine a frame or two to ensure game updates before starting the effect
        yield return null;
        yield return null;

        // Play eyes-closing effect (ScreenFader will create panels if needed)
        if (screenFader != null && screenFader.fadePanel != null)
        {
            // Ensure fade panel is active and reset before eyes closing effect
            if (!screenFader.fadePanel.gameObject.activeInHierarchy)
                screenFader.fadePanel.gameObject.SetActive(true);

            // Prepare fade canvas and split panels so they render on top and aren't obscured or duplicated
            PrepareScreenFaderForEyes();

            // Ensure fade panel's Canvas is on top so split panels are visible
            var canvas = screenFader.fadePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 32767;
                // Ensure canvas is enabled
                canvas.enabled = true;
            }

            screenFader.fadePanel.transform.SetAsLastSibling();
            
            // Reset fade panel alpha to 0 before starting eyes closing
            screenFader.SetPanelAlpha(0f);
            
            // Wait a frame to ensure everything is set up
            yield return null;
            
            yield return StartCoroutine(screenFader.EyesClosingEffect());

            // Now that the panels have closed, dim lights so the scene stays dark behind the panels
            DisableAllLights();

            // Give a frame to let lights update
            yield return null;
        }
        else
        {
            // No screen fader available - dim lights then short pause so player sees dark
            DisableAllLights();
            yield return new WaitForSeconds(0.25f);
        }

        // Show death message while lights remain dimmed. Restore lights when message is fully visible.
        if (endMessageText != null)
        {
            // Ensure text is active FIRST before doing any setup
            endMessageText.gameObject.SetActive(true);
            
            // Wait a frame for the GameObject to fully initialize
            yield return null;

            // Configure text properties
            endMessageText.text = "YOU DIED!";
            endMessageText.color = Color.red;
            endMessageText.fontSize = 72;
            endMessageText.alignment = TMPro.TextAlignmentOptions.Center;

            // If we have a fadePanel canvas, reparent the message under it and ensure sorting order is above
            Canvas fadeCanvas = null;
            if (screenFader != null && screenFader.fadePanel != null)
            {
                fadeCanvas = screenFader.fadePanel.GetComponentInParent<Canvas>();
                if (fadeCanvas != null)
                {
                    // Reparent to fade canvas so it renders within the same UI layer
                    endMessageText.transform.SetParent(fadeCanvas.transform, false);
                    endMessageText.transform.SetAsLastSibling();
                }
            }

            // Ensure the text has proper Canvas setup - check if it already has a Canvas component
            Canvas textCanvas = endMessageText.GetComponent<Canvas>();
            if (textCanvas == null)
            {
                textCanvas = endMessageText.gameObject.AddComponent<Canvas>();
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = fadeCanvas != null ? Mathf.Max(fadeCanvas.sortingOrder + 1, 32768) : 32768;

                if (endMessageText.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    endMessageText.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            else
            {
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = fadeCanvas != null ? Mathf.Max(fadeCanvas.sortingOrder + 1, 32768) : 32768;
            }

            // Ensure Canvas is enabled
            textCanvas.enabled = true;

            // Set up RectTransform for proper sizing
            RectTransform textRect = endMessageText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                // Start with very small scale instead of zero to ensure it renders
                textRect.localScale = Vector3.one * 0.01f;
            }

            // Initialize alpha
            endMessageText.alpha = 0f;
            
            // Wait a frame to ensure everything is initialized
            yield return null;

            float elapsed = 0f;
            float scaleInDuration = messageDisplayTime * 0.8f;
            while (elapsed < messageDisplayTime)
            {
                elapsed += Time.deltaTime;
                float fadeProgress = Mathf.Clamp01(elapsed / messageDisplayTime);
                float scaleProgress = Mathf.Clamp01(elapsed / scaleInDuration);

                endMessageText.alpha = fadeProgress;

                if (textRect != null)
                {
                    float scale;
                    if (scaleProgress < 1f)
                        // Start from 0.01 instead of 0 to ensure rendering
                        scale = Mathf.Lerp(0.01f, 1.2f, Mathf.SmoothStep(0f, 1f, scaleProgress));
                    else
                    {
                        float settleProgress = (elapsed - scaleInDuration) / Mathf.Max(0.0001f, (messageDisplayTime - scaleInDuration));
                        scale = Mathf.Lerp(1.2f, 1f, Mathf.Clamp01(settleProgress));
                    }
                    textRect.localScale = Vector3.one * scale;
                }

                // Force update the text mesh to ensure it renders
                endMessageText.ForceMeshUpdate();

                yield return null;
            }

            // Ensure fully visible
            endMessageText.alpha = 1f;
            if (textRect != null)
                textRect.localScale = Vector3.one;
            
            // Force final mesh update
            endMessageText.ForceMeshUpdate();

            // Restore lights now that the message is fully visible
            RestoreAllLights();

            // Keep message visible for a short time
            yield return new WaitForSeconds(1.5f);

            // Fade out message
            elapsed = 0f;
            float fadeOutDuration = 1.5f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;

                endMessageText.alpha = Mathf.Clamp01(1f - progress);

                if (textRect != null)
                {
                    float scale = Mathf.Lerp(1f, 0.8f, progress);
                    textRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            endMessageText.alpha = 0f;
            if (textRect != null)
                textRect.localScale = Vector3.one;
            endMessageText.gameObject.SetActive(false);
        }

        // Transition to next scene keeping panels closed
        if (!string.IsNullOrEmpty(nextSceneName) && screenFader != null)
        {
            screenFader.shouldOpenEyesOnSceneLoad = true;
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(nextSceneName));
        }
    }
}
