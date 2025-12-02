using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MinigameAudioHook : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public AudioClip openClip;       // mp3 for when the minigame opens
    public bool loopWhileActive = false;
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;   // we trigger manually
        source.loop = false;          // will be overridden in PlayOnOpen
    }

    public void PlayOnOpen()
    {
        if (openClip == null) return;

        source.clip = openClip;
        source.volume = volume;
        source.loop = loopWhileActive;
        source.Play();
    }

    public void StopOnClose()
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }
}
