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

    [Header("Grandfather Clock")]
    public AudioSource grandfatherAudioSource;
    public AudioClip grandfatherClip; // one-shot bell/clock sound
    [Range(0f, 1f)] public float grandfatherVolume = 1f;
    public float grandfatherThreshold = 5f; // play at 5 seconds
    private bool grandfatherPlayed = false;

    [Header("Break / Repair Animation")]
    public int breakStartIndex = 13;
    public int breakEndIndex = 19; // adjusted from 20 to 19
    public float breakFrameDuration = 0.08f;
    public float repairFrameDuration = 0.08f;

    private bool isSpecialAnimating = false;
    private Coroutine specialAnimRoutine;

    // Factor to speed up frames in the special range (13-19)
    private const float SPECIAL_FRAME_FACTOR = 0.4f;

    // Trigger the fast break once when hitting 4 seconds
    private bool fastBreakTriggered = false;

    void Start()
    {
        if (clockFrames == null || clockFrames.Length == 0 || clockImage == null || screenFader == null)
        {
            Debug.LogError("[ClockTimer] Missing references!");
            return;
        }
        warningPlayed = false;
        grandfatherPlayed = false;
        frameCount = clockFrames.Length;
        if (clockImage != null)
            clockImage.sprite = clockFrames[0];

        if (screenFader != null)
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

        // Setup grandfather clock audio source
        if (grandfatherAudioSource == null)
        {
            grandfatherAudioSource = gameObject.AddComponent<AudioSource>();
            grandfatherAudioSource.playOnAwake = false;
            grandfatherAudioSource.spatialBlend = 0f;
        }

        // Determine if HUD has been shown before (HudInitializer sets a per-scene flag)
        string hudFlag = GetCurrentSceneHudFlagName();
        // Backwards-compat: HudInitializer currently sets a global "hudshown" flag.
        // Check both the per-scene flag and the legacy/global flag so reconstruction
        // works whether the initializer used per-scene or global setting.
        bool hudShownBefore = GameFlags.HasFlag(hudFlag) || GameFlags.HasFlag("hudshown");
        Debug.Log($"[ClockTimer] Checking HUD flags: {hudFlag} -> {GameFlags.HasFlag(hudFlag)}, global hudshown -> {GameFlags.HasFlag("hudshown")} -> hudShownBefore={hudShownBefore}");
        if (hudShownBefore)
        {
            // HUD already shown previously — reconstruct clock (20 -> 13) then start
            specialAnimRoutine = StartCoroutine(ReconstructThenStart());
        }
        else
        {
            // First time — let HUD handle its intro, just start timer
            StartTimer(totalTime);
            StartCoroutine(InitialFadeIn()); // fade in at game start
        }
    }

    private void OnDisable()
    {
        // Stop any running special animation to avoid touching destroyed UI
        if (specialAnimRoutine != null)
        {
            StopCoroutine(specialAnimRoutine);
            specialAnimRoutine = null;
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    void Update()
    {
        if (isPaused || hasEnded) return;

        // Safety: ensure required UI still exists
        if (clockImage == null || clockFrames == null) return;

        if (timeLeft > 0f)
        {
            float previousTime = timeLeft;
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(timeLeft, 0f);

            // Trigger fast breaking animation at 4s
            if (!fastBreakTriggered && timeLeft <= 4f && !isSpecialAnimating)
            {
                fastBreakTriggered = true;

                // Only run fast break if we have enough frames to play the sequence
                if (clockFrames != null && frameCount > breakEndIndex)
                {
                    // stop warning heartbeat so it doesn't overlap
                    if (warningAudioSource != null && warningAudioSource.isPlaying)
                        warningAudioSource.Stop();

                    // compute a weighted base frameDuration so full break animation runs in ~4s
                    int step = breakStartIndex <= breakEndIndex ? 1 : -1;
                    int steps = Mathf.Abs(breakEndIndex - breakStartIndex);
                    float totalDuration = 4f; // desired total time for break

                    float totalWeight = 0f;
                    int idx = breakStartIndex;
                    for (int s = 0; s < steps; s++)
                    {
                        int next = idx + step;
                        bool special = (next >= breakStartIndex && next <= breakEndIndex) || (idx >= breakStartIndex && idx <= breakEndIndex);
                        totalWeight += special ? SPECIAL_FRAME_FACTOR : 1f;
                        idx += step;
                    }

                    float baseFrameDur = steps > 0 ? (totalDuration / Mathf.Max(0.0001f, totalWeight)) : 0.08f;

                    // Start the breaking sequence but don't end the timer
                    specialAnimRoutine = StartCoroutine(PlayClockSequence(breakStartIndex, breakEndIndex, baseFrameDur, false));
                }
                else
                {
                    Debug.LogWarning("[ClockTimer] Skipping fast break: not enough clock frames to play break sequence");
                }
            }

            // Clock animation (skip if special sequence is playing)
            if (!isSpecialAnimating)
            {
                float progress = 1f - (timeLeft / totalTime);
                int frameIndex = Mathf.FloorToInt(progress * frameCount);
                frameIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);

                if (frameIndex != lastFrameIndex)
                {
                    if (clockImage != null && clockFrames != null && frameIndex >= 0 && frameIndex < clockFrames.Length)
                    {
                        clockImage.sprite = clockFrames[frameIndex];
                        lastFrameIndex = frameIndex;
                        Debug.Log($"[ClockTimer] Frame changed: {frameIndex}/{frameCount - 1} | Time left: {timeLeft:F2}s");

                        // Play tick sound
                        PlayTickSound();
                    }
                }
            }

            // Debug per whole second
            int currentSecond = Mathf.FloorToInt(timeLeft);
            if (currentSecond != lastWholeSecond)
            {
                Debug.Log($"[ClockTimer] Time left: {timeLeft:F1}s");
                lastWholeSecond = currentSecond;
            }

            // Play grandfather clock sound at threshold
            if (!grandfatherPlayed && timeLeft <= grandfatherThreshold && grandfatherClip != null)
            {
                grandfatherPlayed = true;
                if (grandfatherAudioSource != null)
                {
                    grandfatherAudioSource.PlayOneShot(grandfatherClip, grandfatherVolume);
                    Debug.Log("[ClockTimer] Played grandfather clock sound at 5s");
                }
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
            if (screenFader != null && screenFader.fadePanel != null && timeLeft <= preFadeTime)
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
                Debug.Log("[ClockTimer] Timer finished! Breaking clock, then showing message and transitioning...");
                StartCoroutine(ClockBreakThenFade());
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

        warningPlayed = false;
        grandfatherPlayed = false;
        fastBreakTriggered = false;

        if (screenFader != null)
            screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;

        Debug.Log($"[ClockTimer] Timer started: {totalTime}s");
    }

    public void PauseTimer(bool pause) => isPaused = pause;

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
            IsTimeEnded = true;
            StartCoroutine(ClockBreakThenFade());
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

        // Do the eyes closing effect
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.EyesClosingEffect());
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

            // Fade in message
            float elapsed = 0f;
            while (elapsed < messageDisplayTime)
            {
                elapsed += Time.deltaTime;
                endMessageText.alpha = Mathf.Clamp01(elapsed / messageDisplayTime);
                yield return null;
            }

            endMessageText.alpha = 1f;

            // Hold the message
            yield return new WaitForSeconds(1.5f);

            // Fade out text
            elapsed = 0f;
            float fadeOutDuration = 1.5f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                endMessageText.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
                yield return null;
            }

            endMessageText.alpha = 0f;
            endMessageText.gameObject.SetActive(false);
        }

        // Now stop the warning sound before transitioning
        if (warningAudioSource != null && warningAudioSource.isPlaying)
            warningAudioSource.Stop();

        // Transition to the next scene - KEEP PANELS CLOSED
        if (!string.IsNullOrEmpty(nextSceneName) && screenFader != null)
        {
            // Tell ScreenFader to keep panels closed during transition
            screenFader.shouldOpenEyesOnSceneLoad = true;
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(nextSceneName));
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

    // --- Special sequences ---
    // added optional parameter to control whether ticks play during sequence
    private IEnumerator PlayClockSequence(int startIndex, int endIndex, float frameDuration, bool playTicks = true)
    {
        if (clockFrames == null || clockFrames.Length == 0) yield break;
        if (clockImage == null) yield break; // safety

        isSpecialAnimating = true;

        int step = startIndex <= endIndex ? 1 : -1;
        int index = startIndex;

        Debug.Log($"[ClockTimer] PlayClockSequence START: {startIndex} -> {endIndex}, frameDuration={frameDuration}, step={step}");

        while (true)
        {
            // If object destroyed or image missing, abort
            if (this == null || !this.isActiveAndEnabled || clockImage == null)
            {
                Debug.LogWarning("[ClockTimer] PlayClockSequence aborted - object or UI destroyed");
                break;
            }

            // Use a separate displayIndex so we don't clamp the progression index
            int displayIndex = Mathf.Clamp(index, 0, frameCount - 1);

            // Ensure index valid for array
            if (displayIndex >= 0 && displayIndex < clockFrames.Length)
            {
                clockImage.sprite = clockFrames[displayIndex];
                lastFrameIndex = displayIndex;
            }

            if (playTicks)
                PlayTickSound();

            if (index == endIndex)
                break;

            int nextIndex = index + step;

            // Use faster timing for frames within the special break range
            float wait = frameDuration;
            if ((nextIndex >= breakStartIndex && nextIndex <= breakEndIndex) || (index >= breakStartIndex && index <= breakEndIndex))
            {
                wait = frameDuration * SPECIAL_FRAME_FACTOR;
            }

            Debug.Log($"[ClockTimer] Playing frame {displayIndex} -> next {nextIndex}, wait={wait:F3}s");

            index += step;
            yield return new WaitForSeconds(wait);
        }

        Debug.Log($"[ClockTimer] PlayClockSequence COMPLETE: reached {endIndex}");
        isSpecialAnimating = false;
        specialAnimRoutine = null;
    }

    private IEnumerator ReconstructThenStart()
    {
        // Optional safety check
        if (clockFrames != null && frameCount > breakEndIndex && clockImage != null)
        {
            // Start in the fully broken state (frame 20)
            clockImage.sprite = clockFrames[breakEndIndex];

            // Play 20 -> 13 without ticks
            yield return StartCoroutine(PlayClockSequence(breakEndIndex, breakStartIndex, repairFrameDuration, false));
        }

        StartTimer(totalTime);
        yield return StartCoroutine(InitialFadeIn());
    }

    private IEnumerator ClockBreakThenFade()
    {
        // Play break anim 13 -> 20 before death sequence
        if (clockFrames != null && frameCount > breakEndIndex && clockImage != null)
        {
            yield return StartCoroutine(PlayClockSequence(breakStartIndex, breakEndIndex, breakFrameDuration, true));
        }

        yield return StartCoroutine(FadeMessageThenTransition());
    }

    private string GetCurrentSceneHudFlagName()
    {
        var scene = SceneManager.GetActiveScene();
        return "hudshown." + scene.name + "." + scene.buildIndex;
    }

}