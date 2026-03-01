using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Tutorial Dream Settings")]
    public AudioClip dreamIntro;
    public AudioClip dreamLoop;

    [Header("Default Settings")] 
    public AudioClip defaultIntro;
    public AudioClip defaultLoop;

    public float defaultVolume = 0.5f;

    [Header("Background Noise Settings")]
    public AudioClip[] backgroundNoises;
    public bool invokeBackgroundNoise;
    public float backgroundNoiseVolume = 0.25f;
    
    private AudioSource source;
    private AudioSource[] backgroundSources;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = dreamIntro;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = defaultVolume;

        // play ALL background noises (layered) only if invoked
        if (invokeBackgroundNoise && backgroundNoises != null && backgroundNoises.Length > 0)
        {
            backgroundSources = new AudioSource[backgroundNoises.Length];

            for (int i = 0; i < backgroundNoises.Length; i++)
            {
                var clip = backgroundNoises[i];
                if (clip == null) continue;

                var s = gameObject.AddComponent<AudioSource>();
                s.clip = clip;
                s.loop = true;
                s.playOnAwake = false;

                // Optional: reduce per-layer volume so multiple layers don't get too loud
                s.volume = backgroundNoiseVolume / Mathf.Max(1, backgroundNoises.Length);

                s.Play();
                backgroundSources[i] = s;
            }
        }
        
        if (defaultLoop != null)
        {
            source.clip = defaultLoop;
            source.loop = true;

            if (defaultIntro)
            {
                source.clip = defaultIntro;
                source.loop = false;
                source.Play();
                
                StartCoroutine(WaitForDefaultIntro());
            }
            else
            {
                source.Play();
            }
        }
    }
    
    private IEnumerator WaitForDefaultIntro()
    {
        // Wait while the audio source is playing
        while (source.isPlaying)
        {
            yield return null; // Yield control back to Unity for one frame
        }

        // This code will execute once the audio has finished playing
        DebugLogger.LogMusic("Audio has finished playing! Executing action...");
        // Place your desired action here, e.g.,
        source.clip = defaultLoop;
        source.loop = true;
        source.Play();
    }
    
    public AudioSource GetAudioSource()
    {
        return source;
    }
    
    public void SetAudioClip(AudioClip clip, bool loop = false)
    {
        source.clip = clip;
        source.loop = loop;
    }
    
    public void FadeAndPlay(float endValue, float duration)
    {
        source.Play();
        source.DOFade(endValue, duration).SetEase(Ease.Linear);
    }
    
    public void FadeAndStop(float endValue, float duration)
    {
        source.DOFade(endValue, duration).SetEase(Ease.Linear).OnComplete(() => source.Stop());
    }
    
    public void Play()
    {  
        source.Play();
    }

    void Start()
    {
        
    }
}
