using UnityEngine;
using System.Collections;

public class RoomAudioZone : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    public AudioSource roomMusic;

    [Header("Death Fade Settings")]
    [Tooltip("How long it takes to fade out the audio when the player dies")]
    public float deathFadeDuration = 2f;

    private bool isFadingOut = false;
    private Coroutine fadeCoroutine;

    private void Reset()
    {
        // Auto-fill roomMusic if you forget to drag it
        if (roomMusic == null)
            roomMusic = GetComponent<AudioSource>();

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // Check if time has ended and we need to fade out
        if (ClockTimer.IsTimeEnded && !isFadingOut && roomMusic != null && roomMusic.isPlaying)
        {
            isFadingOut = true;
            fadeCoroutine = StartCoroutine(FadeOutAudio());
        }
    }

    private IEnumerator FadeOutAudio()
    {
        if (roomMusic == null) yield break;

        float startVolume = roomMusic.volume;
        float elapsed = 0f;

        Debug.Log($"[RoomAudioZone] {name}: Starting death fade out from volume {startVolume}");

        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathFadeDuration;
            roomMusic.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        roomMusic.volume = 0f;
        roomMusic.Stop();
        Debug.Log($"[RoomAudioZone] {name}: Death fade out complete");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        LogDebug("OnTriggerEnter2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        // Don't start audio if time has already ended
        if (ClockTimer.IsTimeEnded) return;

        LogDebug("PLAYER ENTERED zone: " + name);
        if (roomMusic != null) roomMusic.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LogDebug("OnTriggerExit2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        LogDebug("PLAYER EXITED zone: " + name);
        if (roomMusic != null) roomMusic.Stop();
    }

    private void OnDisable()
    {
        // Clean up coroutine if zone is disabled
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        isFadingOut = false;
    }
    
    // Debug logging wrapper - only logs in Editor when enableDebugLogs is true
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log(message);
    }
}
