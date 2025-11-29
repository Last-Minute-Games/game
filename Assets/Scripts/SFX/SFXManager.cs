using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Source Pooling")]
    [Tooltip("Number of audio sources to pre-spawn for simultaneous SFX.")]
    public int sourcePoolSize = 10;

    [Tooltip("Parent object for all SFX sources (auto-created).")]
    public Transform sourceParent;

    private List<AudioSource> audioSources = new List<AudioSource>();
    private bool isPaused = false;

    private void Awake()
    {
        // Singleton pattern (global access)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        if (sourceParent == null)
        {
            GameObject parentObj = new GameObject("SFX_AudioSources");
            parentObj.transform.SetParent(transform);
            sourceParent = parentObj.transform;
        }

        for (int i = 0; i < sourcePoolSize; i++)
        {
            AudioSource src = new GameObject($"SFXSource_{i}").AddComponent<AudioSource>();
            src.transform.SetParent(sourceParent);
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D only
            audioSources.Add(src);
        }
        
        Debug.Log($"[SFXManager] Initialized {audioSources.Count} audio sources");
    }

    /// <summary>
    /// Plays a sound cue (random clip, automatic volume).
    /// </summary>
    public void Play(SFXCueData cue)
    {
        if (cue == null)
        {
            Debug.LogWarning("[SFXManager] Play called with null cue!");
            return;
        }

        Debug.Log($"[SFXManager] Play called with cue: {cue.cueName}");

        AudioClip clip = cue.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[SFXManager] No audio clip found in cue: {cue.cueName}");
            return;
        }

        Debug.Log($"[SFXManager] Got clip: {clip.name}, length: {clip.length}s");

        AudioSource source = GetAvailableSource();
        if (source == null)
        {
            Debug.LogWarning("[SFXManager] No available audio sources!");
            return;
        }

        Debug.Log($"[SFXManager] Using audio source: {source.name}, volume: {cue.volume}");

        source.clip = clip;
        source.volume = cue.volume;
        source.pitch = 1f;
        source.spatialBlend = 0f;
        source.Play();
        
        Debug.Log($"[SFXManager] Audio source is playing: {source.isPlaying}");
    }

    /// <summary>
    /// Pauses all currently playing SFX.
    /// </summary>
    public void PauseAll()
    {
        foreach (var src in audioSources)
        {
            if (src.isPlaying)
                src.Pause();
        }

        isPaused = true;
    }

    /// <summary>
    /// Resumes all paused SFX.
    /// </summary>
    public void ResumeAll()
    {
        foreach (var src in audioSources)
        {
            // UnPause also resumes sources that were paused
            src.UnPause();
        }

        isPaused = false;
    }

    /// <summary>
    /// Stops all currently playing sounds.
    /// </summary>
    public void StopAll()
    {
        foreach (var src in audioSources)
            src.Stop();
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var src in audioSources)
        {
            if (!src.isPlaying)
                return src;
        }
        return null;
    }
}
