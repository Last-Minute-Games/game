using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClockTimer : MonoBehaviour
{
    [Header("Clock Setup")]
    public Image clockImage;
    public Sprite[] clockFrames; // assign all 13 sprites here
    public float totalTime = 60f; // default 1 minute timer
    public string nextSceneName = "NextScene"; // assign in inspector

    private float timeLeft;
    private int frameCount;
    private int lastFrameIndex = -1;
    private bool hasEnded = false;
    private bool isPaused = false;

    void Start()
    {
        Debug.Log("[ClockTimer] Started");

        frameCount = clockFrames.Length;
        if (frameCount == 0)
        {
            Debug.LogError("[ClockTimer] No clock frames assigned!");
            return;
        }

        if (clockImage == null)
        {
            Debug.LogError("[ClockTimer] Clock Image not assigned!");
            return;
        }

        clockImage.sprite = clockFrames[0];
        Debug.Log("[ClockTimer] Initial sprite set: " + clockFrames[0].name);

        StartTimer(totalTime);
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
                Debug.Log($"[ClockTimer] Frame changed: {frameIndex}/{frameCount - 1} ({clockFrames[frameIndex].name}) | Time left: {timeLeft:F2}s");
                clockImage.sprite = clockFrames[frameIndex];
                lastFrameIndex = frameIndex;
            }

            if (Mathf.FloorToInt(previousTime) != Mathf.FloorToInt(timeLeft))
            {
                Debug.Log($"[ClockTimer] Time left: {timeLeft:F1}s");
            }

            if (timeLeft <= 0f && !hasEnded)
            {
                hasEnded = true;
                Debug.Log("[ClockTimer] Timer finished! Loading scene...");
                StartCoroutine(LoadNextScene());
            }
        }
    }

    public void StartTimer(float seconds)
    {
        totalTime = Mathf.Max(0.01f, seconds);
        timeLeft = totalTime;
        lastFrameIndex = -1;
        hasEnded = false;
        isPaused = false;
        Debug.Log($"[ClockTimer] Timer started for {totalTime} seconds");
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
        Debug.Log(pause ? "[ClockTimer] Timer paused" : "[ClockTimer] Timer resumed");
    }

    public void AddTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft += seconds;
        totalTime += seconds; // Optional: affects frame pacing
        Debug.Log($"[ClockTimer] Added {seconds} seconds. New time left: {timeLeft:F2}s");
    }

    public void RemoveTime(float seconds)
    {
        if (seconds <= 0f) return;
        timeLeft = Mathf.Max(0f, timeLeft - seconds);
        totalTime = Mathf.Max(0.01f, totalTime - seconds); // Optional: shrink total scale
        Debug.Log($"[ClockTimer] Removed {seconds} seconds. New time left: {timeLeft:F2}s");

        // trigger end early if time hits zero
        if (timeLeft <= 0f && !hasEnded)
        {
            hasEnded = true;
            Debug.Log("[ClockTimer] Timer manually ended after time removal!");
            StartCoroutine(LoadNextScene());
        }
    }

    private System.Collections.IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.5f); // small delay for smooth transition
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[ClockTimer] Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[ClockTimer] No scene name set in 'nextSceneName'");
        }
    }
}
