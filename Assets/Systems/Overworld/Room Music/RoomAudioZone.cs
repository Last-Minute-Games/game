using UnityEngine;
using System.Collections;
using Systems.Overworld;

public class RoomAudioZone : MonoBehaviour
{
    public AudioSource roomMusic;

    [Header("Death Fade Settings")]
    [Tooltip("How long it takes to fade out the audio when the player dies")]
    public float deathFadeDuration = 2f;

    private bool isFadingOut = false;
    private Coroutine fadeCoroutine;
    
    private LightOptimizer lightOptimizer;
    
    private void Awake()
    {
        lightOptimizer = GameObject.Find("Room Areas").GetComponent<LightOptimizer>();
    }

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
        Debug.Log("OnTriggerEnter2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        // Don't start audio if time has already ended
        if (ClockTimer.IsTimeEnded) return;

        Debug.Log("PLAYER ENTERED zone: " + name);
        
        lightOptimizer.EnableLight(name);
        
        if (roomMusic != null) roomMusic.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER EXITED zone: " + name);
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
}
