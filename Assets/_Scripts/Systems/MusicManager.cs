using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip dreamIntro;
    public AudioClip dreamLoop;
    
    public AudioClip defaultLoop;
    
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = dreamIntro;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.7f;
        
        if (defaultLoop != null)
        {
            source.clip = defaultLoop;
            source.loop = true;
            source.Play();
        }
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
