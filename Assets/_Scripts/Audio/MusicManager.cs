using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip dreamIntro;
    public AudioClip dreamLoop;
    
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = dreamIntro;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.7f;
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
        source.DOFade(endValue, duration);
    }
    
    public void Play()
    {
        source.Play();
    }

    void Start()
    {
        
    }
}
