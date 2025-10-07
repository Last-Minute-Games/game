using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip musicClip;
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = musicClip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.7f;
    }

    void Start()
    {
        if (musicClip != null)
        {
            source.Play();
            Debug.Log($"🎶 Now playing: {musicClip.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No music clip assigned to MusicManager!");
        }
    }
}
