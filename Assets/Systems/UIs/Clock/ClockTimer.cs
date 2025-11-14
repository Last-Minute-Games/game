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
        
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            allLights.Add(light);
            originalLightIntensities.Add(light.intensity);
            light.intensity = 0f;
        }
        
        Debug.Log($"[ClockTimer] Disabled {allLights.Count} lights for death sequence");
    }
    
    private void RestoreAllLights()
    {
        for (int i = 0; i < allLights.Count; i++)
        {
            if (allLights[i] != null)
            {
                allLights[i].intensity = originalLightIntensities[i];
            }
        }
        
        Debug.Log($"[ClockTimer] Restored {allLights.Count} lights");
        
        allLights.Clear();
        originalLightIntensities.Clear();
    }

    private IEnumerator FadeMessageThenTransition()
    {
        if (warningAudioSource != null && warningAudioSource.isPlaying)
            warningAudioSource.Stop();
        
        if (bellAudioSource != null && bellAudioSource.isPlaying)
            bellAudioSource.Stop();

        DisableAllLights();

        if (screenFader != null && screenFader.topPanel != null && screenFader.bottomPanel != null)
        {
            screenFader.SetPanelAlpha(0f);
            yield return StartCoroutine(screenFader.EyesClosingEffect());
        }
        else
        {
            if (screenFader != null)
                yield return StartCoroutine(screenFader.FadeOut());
        }

        RestoreAllLights();

        if (endMessageText != null)
        {
            endMessageText.gameObject.SetActive(true);
            endMessageText.text = "YOU DIED!";
            endMessageText.color = Color.red;
            endMessageText.fontSize = 72;
            endMessageText.alignment = TMPro.TextAlignmentOptions.Center;

            Canvas textCanvas = endMessageText.GetComponent<Canvas>();
            if (textCanvas == null)
            {
                textCanvas = endMessageText.gameObject.AddComponent<Canvas>();
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = 1000;

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
            RectTransform textRect = endMessageText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.localScale = Vector3.zero;
            }

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
                    {
                        scale = Mathf.Lerp(0f, 1.2f, Mathf.SmoothStep(0f, 1f, scaleProgress));
                    }
                    else
                    {
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
                textRect.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(1.5f);

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
            {
                textRect.localScale = Vector3.one;
            }
            endMessageText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            screenFader.shouldOpenEyesOnSceneLoad = true;
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(nextSceneName));
        }
    }
}
