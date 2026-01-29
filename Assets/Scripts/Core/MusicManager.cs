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
    
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = dreamIntro;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = defaultVolume;
        
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
