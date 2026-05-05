using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ClockTimer : MonoBehaviour
{
    [Header("Debug Logging")]
    [Tooltip("Enable debug logs for ClockTimer (Editor only - logs are stripped from builds)")]
    public bool enableDebugLogs = false;
    
    [Header("Clock Setup")]
    public Image clockImage;
    public Sprite[] clockFrames;
    public float totalTime = 60f;
    public string nextSceneName = "NextScene";
    
    [Header("Ending Scene")]
    [Tooltip("Scene to load when the game ends (e.g., after day five is completed). If empty, will use nextSceneName.")]
    public string endingSceneName = "";

    [Header("Overworld Timeout")]
    [Tooltip("Scene name that represents the overworld")]
    public string overworldSceneName = "Overworld";

    [Tooltip("Scene to load when the overworld timer hits 0 (FIRST TIME)")]
    public string overworldTimeoutSceneName = "Catacombs";

    [Tooltip("Scene to load when the overworld timer hits 0 (SECOND TIME and onwards)")]
    public string overworldTimeoutBattleSceneName = "BattleScene";

    private const string FIRST_TIMEOUT_FLAG = "clock.first.timeout.complete";
    
    [Header("Day Five Ending")]
    [Tooltip("EndTransition component to trigger when day.five timer runs out")]
    public EndTransition endTransition;
    
    [Tooltip("Auto-find EndTransition if not assigned")]
    public bool autoFindEndTransition = true;

    [Header("Transition / Fade")]
    public ScreenFader screenFader;
    public TMP_Text endMessageText;
    public float preFadeTime = 15f;
    public float messageDisplayTime = 2f;

    [Header("Timing")]
    // How many seconds before actual 0 the clock should stop normal progression and play the break sequence
    public float endEarlyBy = 0; // Changed from 2f to match grandfatherThreshold

    [Header("Debug Controls")]
    [Tooltip("Enable debug time controls (K to subtract 10s, L to add 10s)")]
    public bool enableDebugControls = true;

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
    public float grandfatherThreshold = 3f; // play at 3 seconds
    private bool grandfatherPlayed = false;

    [Header("Break / Repair Animation")]
    public int breakStartIndex = 13;
    public int breakEndIndex = 19; // adjusted from 20 to 19
    public float breakFrameDuration = 0.08f;
    public float repairFrameDuration = 0.08f;

    // When to trigger the destruction/break sequence (seconds left)
    [Tooltip("Seconds left at which the break/destruction sequence begins")]
    public float breakTriggerTime = 2f;

    // Durations for the reconstruct (re-animate) visual: move/scale and repair sequence
    [Tooltip("Duration to move/scale the clock to center and back (seconds)")]
    public float reconstructMoveDuration = 1.5f;
    [Tooltip("Total duration for the repair sequence while centered (seconds)")]
    public float reconstructSequenceDuration = 1.5f;

    private bool isSpecialAnimating = false;
    private Coroutine specialAnimRoutine;

    // Factor to speed up frames in the special range (13-19)
    private const float SPECIAL_FRAME_FACTOR = 0.4f;

    // Trigger the breaking sequence when hitting grandfatherThreshold
    private bool breakSequenceTriggered = false;

    void Start()
    {
        // If we don't have a screenFader assigned try to find one in the scene
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader == null)
            {
                Debug.LogWarning("[ClockTimer] screenFader reference missing in inspector and none found in scene. Scene transition will fall back to direct load.");
            }
        }

        if (clockFrames == null || clockFrames.Length == 0 || clockImage == null)
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
        
        // Find EndTransition if needed
        if (autoFindEndTransition && endTransition == null)
        {
            endTransition = FindObjectOfType<EndTransition>();
            if (endTransition != null)
            {
                LogDebug("Found EndTransition component");
            }
        }

        // DO NOT START TIMER HERE
        // OverworldWakeUpCutscene will call StartTimer() or ReconstructClock() when ready
        LogDebug("Initialized - waiting for OverworldWakeUpCutscene to start timer");
        
        // Only fade in if we're not waiting for a cutscene to control the visuals
        // StartCoroutine(InitialFadeIn()); // Removed - cutscene will handle this
    }

    private void OnEnable()
    {
        // Subscribe to scene load events so if this component persists we can reset
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Stop any running special animation to avoid touching destroyed UI
        if (specialAnimRoutine != null)
        {
            StopCoroutine(specialAnimRoutine);
            specialAnimRoutine = null;
        }

        // Ensure the static time-ended flag doesn't persist across scenes
        IsTimeEnded = false;

        // Unsubscribe scene loaded
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"Scene loaded: {scene.name}. Resetting timer state for loop.");

        // Try to find a ScreenFader in the newly loaded scene if we don't have one
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader != null)
                LogDebug("Found ScreenFader in new scene during OnSceneLoaded.");
        }

        // Reset states so the death sequence plays again on the next run
        ResetForNewScene();
    }

    private void ResetForNewScene()
    {
        // Stop any audio
        if (warningAudioSource != null && warningAudioSource.isPlaying)
            warningAudioSource.Stop();

        // Stop special animation
        if (specialAnimRoutine != null)
        {
            try { StopCoroutine(specialAnimRoutine); } catch { }
            specialAnimRoutine = null;
        }
        isSpecialAnimating = false;

        // Reset timing and flags
        hasEnded = false;
        IsTimeEnded = false;
        isPaused = false;
        warningPlayed = false;
        grandfatherPlayed = false;
        breakSequenceTriggered = false;
        lastFrameIndex = -1;
        lastWholeSecond = -1;

        // Reset UI
        if (screenFader != null)
            screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;
        if (clockImage != null && clockFrames != null && clockFrames.Length > 0)
            clockImage.sprite = clockFrames[0];

        // Restart timer for the new scene so the sequence can play again
        StartTimer(totalTime);
    }

    void Update()
    {
        // Debug controls
        if (enableDebugControls)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                RemoveTime(10f);
                LogDebug($"DEBUG: Removed 10 seconds (K pressed). Time left: {timeLeft:F2}s");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                AddTime(10f);
                LogDebug($"DEBUG: Added 10 seconds (L pressed). Time left: {timeLeft:F2}s");
            }
        }

        if (isPaused || hasEnded) return;

        // Safety: ensure required UI still exists
        if (clockImage == null || clockFrames == null) return;

        if (timeLeft > 0f)
        {
            float previousTime = timeLeft;
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(timeLeft, 0f);

            // Trigger breaking animation at configured breakTriggerTime (e.g. 2s)
            if (!breakSequenceTriggered && timeLeft <= breakTriggerTime && !isSpecialAnimating)
            {
                breakSequenceTriggered = true;

                // Play grandfather clock sound (still uses grandfatherThreshold)
                if (!grandfatherPlayed && grandfatherClip != null && timeLeft <= grandfatherThreshold)
                {
                    grandfatherPlayed = true;
                    if (grandfatherAudioSource != null)
                    {
                        grandfatherAudioSource.PlayOneShot(grandfatherClip, grandfatherVolume);
                        LogDebug($"Played grandfather clock sound at {grandfatherThreshold}s");
                    }
                }

                // Stop warning heartbeat so it doesn't overlap
                if (warningAudioSource != null && warningAudioSource.isPlaying)
                    warningAudioSource.Stop();

                // Only run break sequence if we have enough frames
                if (clockFrames != null && frameCount > breakEndIndex)
                {
                    // Calculate frame duration so the break sequence (13->19) takes exactly breakTriggerTime seconds
                    int numBreakFrames = Mathf.Abs(breakEndIndex - breakStartIndex);
                    float breakSequenceDuration = Mathf.Max(0.01f, breakTriggerTime);
                    float calculatedFrameDur = numBreakFrames > 0 ? (breakSequenceDuration / (float)numBreakFrames) : breakFrameDuration;

                    // Ensure we don't leave a previous special routine running
                    if (specialAnimRoutine != null)
                    {
                        StopCoroutine(specialAnimRoutine);
                        specialAnimRoutine = null;
                        isSpecialAnimating = false;
                    }

                    // Start the breaking sequence (no ticks)
                    LogDebug($"Starting break sequence at {timeLeft:F2}s, duration: {breakSequenceDuration}s, frameDur: {calculatedFrameDur:F3}s");
                    specialAnimRoutine = StartCoroutine(PlayClockSequence(breakStartIndex, breakEndIndex, calculatedFrameDur, false));
                }
                else
                {
                    Debug.LogWarning("[ClockTimer] Skipping break sequence: not enough clock frames");
                }
            }

            // Clock animation (skip if special sequence is playing)
            if (!isSpecialAnimating)
            {
                // Normal phase: play frames from 0 up to (breakStartIndex - 1) over the time until breakTriggerTime
                if (timeLeft > breakTriggerTime)
                {
                    float effectiveDuration = Mathf.Max(0.01f, totalTime - breakTriggerTime);
                    float elapsed = totalTime - timeLeft;
                    float normalProgress = Mathf.Clamp01(elapsed / effectiveDuration);

                    // number of normal frames (0 .. breakStartIndex-1)
                    int normalFrameCount = Mathf.Clamp(breakStartIndex, 0, frameCount);

                    int normalFrameIndex = 0;
                    if (normalFrameCount > 0)
                    {
                        // Map progress [0,1) to frames [0, normalFrameCount-1]. When progress==1 clamp ensures last frame is normalFrameCount-1
                        normalFrameIndex = Mathf.FloorToInt(normalProgress * (float)normalFrameCount);
                        normalFrameIndex = Mathf.Clamp(normalFrameIndex, 0, normalFrameCount - 1);
                    }

                    if (normalFrameIndex != lastFrameIndex)
                    {
                        if (clockImage != null && clockFrames != null && normalFrameIndex >= 0 && normalFrameIndex < clockFrames.Length)
                        {
                            clockImage.sprite = clockFrames[normalFrameIndex];
                            lastFrameIndex = normalFrameIndex;
                            LogDebug($"Frame changed: {normalFrameIndex}/{normalFrameCount - 1} | Time left: {timeLeft:F2}s");

                            // Play tick sound
                            PlayTickSound();
                        }
                    }
                }
            }

            // Debug per whole second
            int currentSecond = Mathf.FloorToInt(timeLeft);
            if (currentSecond != lastWholeSecond)
            {
                LogDebug($"Time left: {timeLeft:F1}s");
                lastWholeSecond = currentSecond;
            }

            // Handle warning heartbeat when time is low (but stop before breakTriggerTime)
            if (timeLeft <= warningThreshold && timeLeft > breakTriggerTime && warningClip != null)
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

            // Timer ends when reaching 0
            if (timeLeft <= 0f && !hasEnded)
            {
                hasEnded = true;
                IsTimeEnded = true;
                LogDebug("Timer reached 0! Starting death sequence...");
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

        warningPlayed = false;
        grandfatherPlayed = false;
        breakSequenceTriggered = false;

        if (screenFader != null)
            screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;

        LogDebug($"Timer started: {totalTime}s");
    }

    public void PauseTimer(bool pause) => isPaused = pause;

    public void AddTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft += seconds;
        totalTime += seconds;
        LogDebug($"Added {seconds}s. Time left: {timeLeft:F2}s");
    }

    public void RemoveTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft = Mathf.Max(0f, timeLeft - seconds);
        totalTime = Mathf.Max(0.01f, totalTime - seconds);
        LogDebug($"Removed {seconds}s. Time left: {timeLeft:F2}s");

        if (timeLeft <= 0f && !hasEnded)
        {
            hasEnded = true;
            IsTimeEnded = true;
            StartCoroutine(ClockBreakThenFade());
        }
    }

    /// <summary>
    /// Get the current time left on the clock (for saving)
    /// </summary>
    public float GetTimeLeft()
    {
        return timeLeft;
    }

    /// <summary>
    /// Restore the time left on the clock (for loading)
    /// </summary>
    public void RestoreTimeLeft(float time)
    {
        timeLeft = Mathf.Max(0f, time);
        totalTime = Mathf.Max(timeLeft, totalTime);
        LogDebug($"Time restored to: {timeLeft:F2}s");
    }

    private IEnumerator InitialFadeIn()
    {
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }

    private IEnumerator FadeMessageThenTransition()
    {
        // Try to find ScreenFader if we lost the reference
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader != null)
            {
                LogDebug("Found ScreenFader during death sequence");
            }
        }

        // Keep the warning sound playing (don't stop it yet)
        // It will continue until the new scene loads

        // Do the eyes closing effect
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.EyesClosingEffect());
        }

        // Check if day five timer ran out - skip "YOU DIED" message and trigger ending immediately
        if (GameFlags.HasFlag("day.five"))
        {
            LogDebug("🎬 Day five timer ended - triggering ending sequence (skipping death message)");
            
            // Stop warning sound before transitioning
            if (warningAudioSource != null && warningAudioSource.isPlaying)
                warningAudioSource.Stop();
            
            // Set the start.ending flag to allow EndTransition to proceed
            if (!GameFlags.HasFlag("start.ending"))
            {
                LogDebug("Setting start.ending flag for day.five");
                GameFlags.SetFlag("start.ending");
            }
            
            // Screen stays black (eyes closed) - wait a moment for dramatic effect
            yield return new WaitForSeconds(1f);
            
            // Trigger the ending transition
            if (endTransition != null)
            {
                LogDebug("Triggering EndTransition for day.five");
                endTransition.TriggerEndTransition();
                yield break; // EndTransition will handle the scene load
            }
            else
            {
                Debug.LogError("[ClockTimer] EndTransition component not found! Falling back to normal scene transition.");
                // Fall through to normal transition as fallback
            }
        }

        // For non-day.five deaths: Show "YOU DIED!" message
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

        // Normal transition for other days
        string sceneToLoad = nextSceneName;
        string activeSceneName = SceneManager.GetActiveScene().name;

        // Check if we're in Overworld and timer expired
        if (!string.IsNullOrEmpty(overworldSceneName)
            && !string.IsNullOrEmpty(overworldTimeoutSceneName)
            && activeSceneName == overworldSceneName)
        {
            // Check if this is the first time the timer has run out
            if (!GameFlags.HasFlag(FIRST_TIMEOUT_FLAG))
            {
                // FIRST TIME: Go to Catacombs
                sceneToLoad = overworldTimeoutSceneName;
                LogDebug($"Overworld timer ended - FIRST TIME - going to Catacombs: '{sceneToLoad}'.");

                // Set flag so next time we go straight to battle
                GameFlags.SetFlag(FIRST_TIMEOUT_FLAG);
                LogDebug($"Set flag '{FIRST_TIMEOUT_FLAG}' - next timeout will go to battle");
            }
            else
            {
                // SECOND TIME and onwards: Go straight to Battle
                sceneToLoad = string.IsNullOrEmpty(overworldTimeoutBattleSceneName) 
                    ? nextSceneName 
                    : overworldTimeoutBattleSceneName;
                LogDebug($"Overworld timer ended - SECOND+ TIME - going straight to Battle: '{sceneToLoad}'.");
            }
        }

        // Transition to the next scene
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[ClockTimer] Scene name is empty or null - cannot transition.");
            yield break;
        }

        LogDebug($"Preparing transition to scene '{sceneToLoad}'. ScreenFader assigned: {screenFader != null}");

        if (screenFader != null)
        {
            // Eyes should always open in the destination scene
            screenFader.shouldOpenEyesOnSceneLoad = true;
            LogDebug($"Calling ScreenFader.TransitionToSceneKeepPanelsClosed('{sceneToLoad}'), shouldOpenEyesOnSceneLoad=true");
            yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(sceneToLoad));
            LogDebug($"Returned from ScreenFader.TransitionToSceneKeepPanelsClosed('{sceneToLoad}')");

            // Note: if the scene did not change, check build settings and logs.
            LogDebug($"Current active scene after ScreenFader call: {SceneManager.GetActiveScene().name} (expected: {sceneToLoad})");
        }
        else
        {
            Debug.LogWarning("[ClockTimer] screenFader is null - attempting direct async load of next scene.");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            if (asyncLoad == null)
            {
                Debug.LogError($"[ClockTimer] SceneManager.LoadSceneAsync returned null for '{sceneToLoad}'. Make sure the scene is added to Build Settings.");
                yield break;
            }

            LogDebug($"Started direct async load for '{sceneToLoad}'. allowSceneActivation={asyncLoad.allowSceneActivation}");
            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone)
                yield return null;
            LogDebug($"Direct async load finished for '{sceneToLoad}'. Active scene is now: {SceneManager.GetActiveScene().name}");
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

        LogDebug($"PlayClockSequence START: {startIndex} -> {endIndex}, frameDuration={frameDuration:F3}s, step={step}");

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

            LogDebug($"Playing frame {displayIndex}, waiting {frameDuration:F3}s");

            index += step;
            yield return new WaitForSeconds(frameDuration);
        }

        LogDebug($"PlayClockSequence COMPLETE: reached {endIndex}");
        isSpecialAnimating = false;
        specialAnimRoutine = null;
    }

    private IEnumerator AnimateRectTransform(Transform t, Vector2 targetAnchoredPos, Vector3 targetScale, float duration)
    {
        if (t == null) yield break;
        var rt = t as RectTransform;
        if (rt == null) yield break;

        RectTransform parentRt = rt.parent as RectTransform;
        if (parentRt == null) yield break;

        Vector2 startPos = rt.anchoredPosition;
        Vector3 startScale = rt.localScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (this == null || !this.isActiveAndEnabled || rt == null) yield break;
            elapsed += Time.deltaTime;
            float f = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, f);
            rt.localScale = Vector3.Lerp(startScale, targetScale, f);
            yield return null;
        }

        rt.anchoredPosition = targetAnchoredPos;
        rt.localScale = targetScale;
    }

    /// <summary>
    /// Plays the clock reconstruction animation without starting the timer.
    /// Call StartTimer() separately after this completes to begin countdown.
    /// </summary>
    public IEnumerator ReconstructClock()
    {
        LogDebug("Starting clock reconstruction (without auto-starting timer)");
        
        // PAUSE the timer immediately so it doesn't tick during reconstruction
        isPaused = true;
        LogDebug("Timer paused for reconstruction");
        
        // Initialize frameCount if it wasn't set (e.g., if Start() returned early)
        if (frameCount == 0 && clockFrames != null && clockFrames.Length > 0)
        {
            frameCount = clockFrames.Length;
            LogDebug($"Initialized frameCount to {frameCount} (was 0)");
        }
        
        // Debug the condition checks with detailed info
        LogDebug($"ReconstructClock checks:");
        LogDebug($"  - clockFrames: {(clockFrames != null ? $"not null, length={clockFrames.Length}" : "NULL")}");
        LogDebug($"  - frameCount: {frameCount}");
        LogDebug($"  - breakEndIndex: {breakEndIndex}");
        LogDebug($"  - frameCount > breakEndIndex: {frameCount > breakEndIndex}");
        LogDebug($"  - clockImage: {(clockImage != null ? "not null" : "NULL")}");

        // Optional safety check
        if (clockFrames != null && frameCount > breakEndIndex && clockImage != null)
        {
            LogDebug("✓ All checks passed - starting reconstruction animation");

            // Start in the fully broken state (frame 19, which is breakEndIndex)
            clockImage.sprite = clockFrames[breakEndIndex];

            // Disable player input / movement while reconstructing
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerInput2D playerInput = null;
            CharacterMotor2D motor = null;
            bool prevInputEnabled = true;
            bool prevMotorDialogue = false;

            if (playerObj != null)
            {
                playerInput = playerObj.GetComponent<PlayerInput2D>();
                motor = playerObj.GetComponent<CharacterMotor2D>();

                if (playerInput != null)
                {
                    prevInputEnabled = playerInput.isInputEnabled;
                    playerInput.isInputEnabled = false;
                }

                if (motor != null)
                {
                    prevMotorDialogue = motor.IsDialogueActive;
                    motor.SetDialogueActive(true);
                }
            }

            // First, play the eyes opening animation if panels are closed
            if (screenFader != null && screenFader.shouldOpenEyesOnSceneLoad)
            {
                LogDebug("Playing eyes opening before clock reconstruction");
                screenFader.shouldOpenEyesOnSceneLoad = false;
                yield return StartCoroutine(screenFader.EyesOpeningEffect());
            }

            // Move clock to center and enlarge while KEEPING frame 19 visible
            RectTransform rt = clockImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Save original transform state
                Vector2 origAnchored = rt.anchoredPosition;
                Vector3 origScale = rt.localScale;
                int origSibling = rt.GetSiblingIndex();

                // Bring to front
                rt.SetAsLastSibling();

                // Target: centered in parent (anchoredPosition = 0) and doubled in scale
                Vector2 centerAnchored = Vector2.zero;
                Vector3 targetScale = origScale * 4f;

                // Ensure we're showing frame 19 and set isSpecialAnimating to prevent Update() from changing frames
                clockImage.sprite = clockFrames[breakEndIndex];
                isSpecialAnimating = true;
                LogDebug($"Starting zoom-in animation with frame {breakEndIndex} locked");

                // Animate to center/scale (while frame 19 stays visible)
                float moveDur = Mathf.Clamp(reconstructMoveDuration, 0.2f, 5f);
                
                Vector2 startPos = rt.anchoredPosition;
                Vector3 startScale = rt.localScale;
                float elapsed = 0f;
                
                while (elapsed < moveDur)
                {
                    if (this == null || !this.isActiveAndEnabled || rt == null) 
                    {
                        isSpecialAnimating = false;
                        isPaused = false; // Restore pause state if aborted
                        yield break;
                    }
                    
                    elapsed += Time.deltaTime;
                    float f = Mathf.Clamp01(elapsed / moveDur);
                    rt.anchoredPosition = Vector2.Lerp(startPos, centerAnchored, f);
                    rt.localScale = Vector3.Lerp(startScale, targetScale, f);
                    
                    // Keep frame 19 locked during animation
                    clockImage.sprite = clockFrames[breakEndIndex];
                    yield return null;
                }
                
                rt.anchoredPosition = centerAnchored;
                rt.localScale = targetScale;
                clockImage.sprite = clockFrames[breakEndIndex];

                LogDebug("Zoom-in complete, starting repair sequence");

                // NOW play repair sequence in two parts: 19->13 then 13->1
                // Calculate total steps and per-frame duration so the whole sequence fits reconstructSequenceDuration
                int stepsPartA = Mathf.Abs(breakEndIndex - breakStartIndex); // e.g. 19->13
                int stepsPartB = Mathf.Abs(breakStartIndex - 1); // 13->1
                int totalSteps = Mathf.Max(1, stepsPartA + stepsPartB);
                float seqDur = Mathf.Max(0.01f, reconstructSequenceDuration);
                float frameDur = seqDur / totalSteps;

                // Play 19 -> 13 (repair)
                yield return StartCoroutine(PlayClockSequence(breakEndIndex, breakStartIndex, frameDur, false));

                // Play 13 -> 1 (clock whole)
                yield return StartCoroutine(PlayClockSequence(breakStartIndex, 1, frameDur, false));

                LogDebug("Repair sequence complete, zooming back out");

                // Animate back to original position/scale
                startPos = rt.anchoredPosition;
                startScale = rt.localScale;
                elapsed = 0f;
                
                // Keep the final frame (frame 1) locked during zoom-out
                int finalFrame = 1;
                
                while (elapsed < moveDur)
                {
                    if (this == null || !this.isActiveAndEnabled || rt == null) 
                    {
                        isSpecialAnimating = false;
                        isPaused = false; // Restore pause state if aborted
                        yield break;
                    }
                    
                    elapsed += Time.deltaTime;
                    float f = Mathf.Clamp01(elapsed / moveDur);
                    rt.anchoredPosition = Vector2.Lerp(startPos, origAnchored, f);
                    rt.localScale = Vector3.Lerp(startScale, origScale, f);
                    
                    // Keep frame 1 locked during zoom-out
                    clockImage.sprite = clockFrames[finalFrame];
                    yield return null;
                }
                
                rt.anchoredPosition = origAnchored;
                rt.localScale = origScale;
                clockImage.sprite = clockFrames[finalFrame];

                // Restore sibling index
                rt.SetSiblingIndex(origSibling);
                
                isSpecialAnimating = false;
            }
            else
            {
                // Fallback: just play the sequences if no RectTransform
                // Ensure frame 19 is visible
                clockImage.sprite = clockFrames[breakEndIndex];
                
                int stepsPartA = Mathf.Abs(breakEndIndex - breakStartIndex);
                int stepsPartB = Mathf.Abs(breakStartIndex - 1);
                int totalSteps = Mathf.Max(1, stepsPartA + stepsPartB);
                float seqDur = Mathf.Max(0.01f, reconstructSequenceDuration);
                float frameDur = seqDur / totalSteps;

                yield return StartCoroutine(PlayClockSequence(breakEndIndex, breakStartIndex, frameDur, false));
                yield return StartCoroutine(PlayClockSequence(breakStartIndex, 1, frameDur, false));
            }

            // Always restore player input/movement after reconstruction
            LogDebug("Restoring player input/movement after reconstruction");
            if (playerInput != null)
            {
                playerInput.isInputEnabled = prevInputEnabled;
            }
            if (motor != null)
            {
                motor.SetDialogueActive(prevMotorDialogue);
            }
            
            // Timer remains paused - caller must call StartTimer() to begin countdown
            LogDebug("Clock reconstruction complete - timer still paused (caller must start it)");
        }
        else
        {
            Debug.LogWarning($"[ClockTimer] Skipping reconstruction animation - " +
                $"clockFrames={(clockFrames != null ? "OK" : "NULL")}, " +
                $"frameCount={frameCount} > breakEndIndex={breakEndIndex} = {frameCount > breakEndIndex}, " +
                $"clockImage={(clockImage != null ? "OK" : "NULL")}");
            // Timer remains paused but no animation played
        }
    }

    private IEnumerator ClockBreakThenFade()
    {
        // The break animation should already be playing from the Update loop
        // Wait for it to complete if still running
        if (specialAnimRoutine != null && isSpecialAnimating)
        {
            LogDebug("Waiting for break sequence to complete...");
            yield return specialAnimRoutine;
        }

        // Now proceed with death sequence
        yield return StartCoroutine(FadeMessageThenTransition());
    }

    private string GetCurrentSceneHudFlagName()
    {
        var scene = SceneManager.GetActiveScene();
        return "hudshown." + scene.name + "." + scene.buildIndex;
    }
    
    // Debug logging wrapper - only logs in Editor when enableDebugLogs is true
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[ClockTimer] {message}");
    }
}
