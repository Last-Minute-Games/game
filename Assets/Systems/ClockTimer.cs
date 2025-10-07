using UnityEngine;
using UnityEngine.UI;

public class ClockTimer : MonoBehaviour
{
    [Header("Clock Setup")]
    public Image clockImage;
    public Sprite[] clockFrames; // assign all 13 sprites here
    public float totalTime = 60f; // default 1 minute timer

    private float timeLeft;
    private int frameCount;

    void Start()
    {
        Debug.Log("ClockTimer started");

        // Make sure we have frames
        frameCount = clockFrames.Length;
        if (frameCount == 0)
        {
            Debug.LogError("No clock frames assigned!");
            return;
        }

        if (clockImage == null)
        {
            Debug.LogError("Clock Image not assigned!");
            return;
        }

        // Initialize first frame and timer
        clockImage.sprite = clockFrames[0];
        Debug.Log("Clock sprite assigned: " + clockFrames[0].name);

        StartTimer(totalTime);
    }

    void Update()
    {
        // Only run if timer is active
        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(timeLeft, 0f); // avoid negative time

            // Calculate progress (0 to 1)
            float progress = 1f - (timeLeft / totalTime);

            // Map progress to frame index
            int frameIndex = Mathf.FloorToInt(progress * frameCount);
            frameIndex = Mathf.Clamp(frameIndex, 0, frameCount - 1);

            // Update clock sprite
            clockImage.sprite = clockFrames[frameIndex];
        }
    }

    public void StartTimer(float seconds)
    {
        totalTime = Mathf.Max(0.01f, seconds); // avoid zero
        timeLeft = totalTime;
    }
}
