using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClockTimer : MonoBehaviour
{
    [Header("Clock Setup")]
    public Image clockImage;
    public Sprite[] clockFrames;
    public float totalTime = 60f;

    [Header("Transition")]
    public ScreenFader screenFader; // Drag your ScreenFader here
    public float preFadeTime = 5f;  // Start darkening 5 seconds before time up

    private float timeLeft;
    private int frameCount;
    private int lastFrameIndex = -1;
    private bool hasEnded = false;

    void Start()
    {
        if (clockFrames.Length == 0)
            Debug.LogError("No clock frames assigned!");
        if (clockImage == null)
            Debug.LogError("Clock Image not assigned!");
        if (screenFader == null)
            Debug.LogError("ScreenFader not assigned!");

        frameCount = clockFrames.Length;
        clockImage.sprite = clockFrames[0];

        // Ensure fade panel starts transparent
        screenFader.SetPanelAlpha(0f);

        StartTimer(totalTime);
    }

    void Update()
    {
        if (timeLeft > 0f)
        {
            float previousTime = timeLeft;
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(timeLeft, 0f);

            // Update clock sprite
            float progress = 1f - (timeLeft / totalTime);
            int frameIndex = Mathf.FloorToInt(progress * frameCount);
            frameIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);

            if (frameIndex != lastFrameIndex)
            {
                clockImage.sprite = clockFrames[frameIndex];
                lastFrameIndex = frameIndex;
            }

            // Pre-fade effect in last few seconds
            if (screenFader != null && timeLeft <= preFadeTime && timeLeft > 0f)
            {
                float fadeProgress = 1f - (timeLeft / preFadeTime); // 0 → 1
                float targetAlpha = Mathf.Lerp(0f, 0.8f, fadeProgress); // slightly transparent, not full black yet
                screenFader.SetPanelAlpha(targetAlpha);
            }

            // When timer runs out, fully fade + transition
            if (timeLeft <= 0f && !hasEnded)
            {
                hasEnded = true;
                StartCoroutine(FadeThenTransition());
            }
        }
    }

    private IEnumerator FadeThenTransition()
    {
        // Fade out fully
        yield return StartCoroutine(screenFader.FadeOut());

        // Then transition to battle scene
        yield return StartCoroutine(screenFader.TransitionToScene("BattleScene"));
    }

    public void StartTimer(float seconds)
    {
        totalTime = Mathf.Max(0.01f, seconds);
        timeLeft = totalTime;
        lastFrameIndex = -1;
        hasEnded = false;
    }
}
