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

    void Start()
    {
        if (clockFrames.Length == 0 || clockImage == null || screenFader == null)
        {
            Debug.LogError("[ClockTimer] Missing references!");
            return;
        }

        frameCount = clockFrames.Length;
        clockImage.sprite = clockFrames[0];

        screenFader.SetPanelAlpha(0f);
        if (endMessageText != null)
            endMessageText.alpha = 0f;

        StartTimer(totalTime);
        StartCoroutine(InitialFadeIn()); // fade in at game start
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
            }

            // Debug per whole second
            int currentSecond = Mathf.FloorToInt(timeLeft);
            if (currentSecond != lastWholeSecond)
            {
                Debug.Log($"[ClockTimer] Time left: {timeLeft:F1}s");
                lastWholeSecond = currentSecond;
            }

            if (screenFader != null && timeLeft <= preFadeTime)
            {
                float fadeTarget = Mathf.Lerp(0f, 0.8f, 1f - (timeLeft / preFadeTime));
                // Smoothly approach the target each frame
                float currentAlpha = screenFader.fadePanel.color.a;
                screenFader.SetPanelAlpha(Mathf.MoveTowards(currentAlpha, fadeTarget, Time.deltaTime / preFadeTime));
            }

            // Timer ends
            if (timeLeft <= 0f && !hasEnded)
            {
                hasEnded = true;
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
        isPaused = false;

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
            StartCoroutine(FadeMessageThenTransition());
        }
    }

    private IEnumerator InitialFadeIn()
    {
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn());
    }

    private IEnumerator FadeMessageThenTransition()
    {
        // 1️⃣ Smoothly fade overlay fully
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut());

        // 2️⃣ Now show message text
        if (endMessageText != null)
        {
            endMessageText.gameObject.SetActive(true);
            endMessageText.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < messageDisplayTime)
            {
                elapsed += Time.deltaTime;
                endMessageText.alpha = Mathf.Clamp01(elapsed / messageDisplayTime);
                yield return null;
            }

            // Optional hold
            yield return new WaitForSeconds(0.5f);

            // Fade out text
            elapsed = 0f;
            float fadeOutDuration = 1.5f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                endMessageText.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
                yield return null;
            }
        }

        // 3️⃣ Transition scene
        if (!string.IsNullOrEmpty(nextSceneName) && screenFader != null)
            yield return StartCoroutine(screenFader.TransitionToScene(nextSceneName));
    }


}
