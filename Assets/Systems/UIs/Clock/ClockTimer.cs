using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("Audio Warning")]
    public AudioSource warningAudioSource;
    public AudioClip warningClip;
    [Range(0f, 1f)] public float warningVolume = 0.7f;
    public float warningThreshold = 10f; // time before end to start sound
    private bool warningPlayed = false;

    [Header("Clock Tick Audio")]
    public AudioSource tickAudioSource;
    public AudioClip tickClip;
    [Range(0f, 1f)] public float tickVolume = 0.5f;

    // Singleton-like reference for easy access
    public static ClockTimer Instance { get; private set; }

    void Awake()
    {
        // Set up singleton instance
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
        frameCount = clockFrames.Length;
        clockImage.sprite = clockFrames[0];

        screenFader.SetPanelAlpha(0f);

        // Setup end message text
        if (endMessageText != null)
        {
            endMessageText.alpha = 0f;
            endMessageText.text = "YOU DIED!";
            endMessageText.alignment = TMPro.TextAlignmentOptions.Center;
            endMessageText.fontSize = 72; // Large dramatic text
            endMessageText.color = Color.red; // Red for dramatic effect

            // Ensure the text is positioned correctly in the center
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
            warningAudioSource.spatialBlend = 0f; // make it 2D
        }

        // Setup tick audio source
        if (tickAudioSource == null)
        {
            tickAudioSource = gameObject.AddComponent<AudioSource>();
            tickAudioSource.playOnAwake = false;
            tickAudioSource.spatialBlend = 0f; // make it 2D
        }

        if (startAutomatically)
        {
            StartTimer(totalTime);
            Debug.Log("[ClockTimer] Timer started automatically");
        }
        
        StartCoroutine(InitialFadeIn()); // fade in at game start
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

            // Clock animation
            float progress = 1f - (timeLeft / totalTime);
            int frameIndex = Mathf.FloorToInt(progress * frameCount);
            frameIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);

            if (frameIndex != lastFrameIndex)
            {
                clockImage.sprite = clockFrames[frameIndex];
                lastFrameIndex = frameIndex;
                Debug.Log($"[ClockTimer] Frame changed: {frameIndex}/{frameCount - 1} | Time left: {timeLeft:F2}s");
                
                // Play tick sound
                PlayTickSound();
            }

            // Debug per whole second
            int currentSecond = Mathf.FloorToInt(timeLeft);
            if (currentSecond != lastWholeSecond)
            {
                Debug.Log($"[ClockTimer] Time left: {timeLeft:F1}s");
                lastWholeSecond = currentSecond;
            }

            // Handle warning heartbeat when time is low
            if (timeLeft <= warningThreshold && warningClip != null)
            {
                if (!warningAudioSource.isPlaying)
                {
                    warningAudioSource.clip = warningClip;
                    warningAudioSource.loop = true;
                    warningAudioSource.volume = 0f; // start quietly
                    warningAudioSource.Play();
                }

                // Volume increases as time approaches 0
                float volumeFactor = 1f - (timeLeft / warningThreshold); // 0 → 1
                warningAudioSource.volume = Mathf.Lerp(0.2f, warningVolume, volumeFactor);

                // Pitch increases slightly to create tension
                warningAudioSource.pitch = Mathf.Lerp(1f, 1.5f, volumeFactor);
            }
            else if (timeLeft > warningThreshold && warningAudioSource.isPlaying)
            {
                // stop early if we regained time (optional)
                warningAudioSource.Stop();
            }

            // Fade overlay near end
            if (screenFader != null && timeLeft <= preFadeTime)
            {
                float fadeTarget = Mathf.Lerp(0f, 0.8f, 1f - (timeLeft / preFadeTime));
                float currentAlpha = screenFader.fadePanel.color.a;
                screenFader.SetPanelAlpha(Mathf.MoveTowards(currentAlpha, fadeTarget, Time.deltaTime / preFadeTime));
            }

            // Timer ends
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

        screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;

        Debug.Log($"[ClockTimer] Timer started: {totalTime}s");
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
        Debug.Log($"[ClockTimer] Timer {(pause ? "paused" : "resumed")}");
        
        // Pause/resume warning audio if playing
        if (warningAudioSource != null && warningAudioSource.isPlaying)
        {
            if (pause)
            {
                warningAudioSource.Pause();
            }
            else
            {
                warningAudioSource.UnPause();
            }
        }
    }

    public void AddTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft += seconds;
        totalTime += seconds;
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

    private IEnumerator InitialFadeIn()
    {
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }

    private IEnumerator FadeMessageThenTransition()
    {
        // Keep the warning sound playing (don't stop it yet)
        // It will continue until the new scene loads

        // First, do the eyes closing effect if split panels are available
        if (screenFader != null && screenFader.topPanel != null && screenFader.bottomPanel != null)
        {
            yield return StartCoroutine(screenFader.EyesClosingEffect());
        }
        else
        {
            // Fallback to regular fade if no split panels
            if (screenFader != null)
                yield return StartCoroutine(screenFader.FadeOut());
        }

        // Now show "YOU DIED!" message
        if (endMessageText != null)
        {
            endMessageText.gameObject.SetActive(true);

            // Force the text properties again to ensure they're applied
            endMessageText.text = "YOU DIED!";
            endMessageText.color = Color.red;
            endMessageText.fontSize = 72;
            endMessageText.alignment = TMPro.TextAlignmentOptions.Center;

            // Bring to front (set high sorting order)
            Canvas textCanvas = endMessageText.GetComponent<Canvas>();
            if (textCanvas == null)
            {
                textCanvas = endMessageText.gameObject.AddComponent<Canvas>();
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = 1000; // Very high to be on top

                // Add GraphicRaycaster if needed
                if (endMessageText.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    endMessageText.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
            }
            else
            {
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = 1000;
            }

            endMessageText.alpha = 0f;

            // Get or set the RectTransform for scaling
            RectTransform textRect = endMessageText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.localScale = Vector3.zero; // Start from zero scale
            }

            // Fade in and scale up message simultaneously
            float elapsed = 0f;
            float scaleInDuration = messageDisplayTime * 0.8f; // Scale faster than fade
            while (elapsed < messageDisplayTime)
            {
                elapsed += Time.deltaTime;
                float fadeProgress = Mathf.Clamp01(elapsed / messageDisplayTime);
                float scaleProgress = Mathf.Clamp01(elapsed / scaleInDuration);

                // Fade in
                endMessageText.alpha = fadeProgress;

                // Scale up with overshoot effect (elastic)
                if (textRect != null)
                {
                    float scale;
                    if (scaleProgress < 1f)
                    {
                        // Overshoot effect: go slightly over 1.0 then settle back
                        scale = Mathf.Lerp(0f, 1.2f, Mathf.SmoothStep(0f, 1f, scaleProgress));
                    }
                    else
                    {
                        // Settle back to 1.0
                        float settleProgress = (elapsed - scaleInDuration) / (messageDisplayTime - scaleInDuration);
                        scale = Mathf.Lerp(1.2f, 1f, settleProgress);
                    }
                    textRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            endMessageText.alpha = 1f;
            if (textRect != null)
            {
                textRect.localScale = Vector3.one; // Ensure final scale is exactly 1
            }

            // Hold the message
            yield return new WaitForSeconds(1.5f);

            // Fade out text and scale down
            elapsed = 0f;
            float fadeOutDuration = 1.5f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;

                // Fade out
                endMessageText.alpha = Mathf.Clamp01(1f - progress);

                // Scale down slightly
                if (textRect != null)
                {
                    float scale = Mathf.Lerp(1f, 0.8f, progress);
                    textRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            endMessageText.alpha = 0f;
            if (textRect != null)
            {
                textRect.localScale = Vector3.one; // Reset scale
            }
            endMessageText.gameObject.SetActive(false);
        }

        // Now stop the warning sound before transitioning
        if (warningAudioSource != null && warningAudioSource.isPlaying)
            warningAudioSource.Stop();

        // Transition to the next scene - KEEP PANELS CLOSED
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Tell ScreenFader to keep panels closed during transition
            screenFader.shouldOpenEyesOnSceneLoad = true;
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(nextSceneName));
        }
    }
}
