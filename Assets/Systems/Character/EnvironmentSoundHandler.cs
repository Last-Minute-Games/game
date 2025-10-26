using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentSoundHandler : MonoBehaviour
{
    private AudioSource _ambientAudioSource;
    private AudioSource _doorAudioSource;
    private AudioSource _journalAudioSource;
    
    [Header("Door Sounds")]
    [SerializeField] private AudioClip doorOpenClip;
    
    [Header("Journal Sounds")]
    [SerializeField] private AudioClip journalOpenClip;
    [SerializeField] private AudioClip journalCloseClip;
    
    [Header("Ambience")]
    [SerializeField] private AudioClip ambienceClip;
    
    private List<AudioSource> _playerAudioSources = new List<AudioSource>();
    
    void Start()
    {
        // default creation
        _doorAudioSource = CreateCustomSource("DoorSource");
        _journalAudioSource = CreateCustomSource("JournalSource");

        if (ambienceClip)
        {
            _ambientAudioSource = CreateCustomSource("AmbientSource");
            _ambientAudioSource.loop = true;
            _ambientAudioSource.clip = ambienceClip;
            _ambientAudioSource.volume = 0.15f;
            _ambientAudioSource.Play();
        }
    }

    public AudioSource CreateCustomSource(string sourceName)
    {
        var newAudioSource = new GameObject(sourceName).AddComponent<AudioSource>();
        _playerAudioSources.Add(newAudioSource);
        
        newAudioSource.playOnAwake = false;
        
        newAudioSource.transform.parent = transform;
        newAudioSource.transform.localPosition = Vector3.zero;
        
        return newAudioSource;
    }

    public void PlayDoorSound()
    {
        _doorAudioSource.volume = 0.9f; 
        _doorAudioSource.clip = doorOpenClip;
        _doorAudioSource.Play();
    }
    
    public void PlayJournalSound(bool isOpening)
    {
        _journalAudioSource.volume = 0.71f; 
        _journalAudioSource.clip = isOpening ? journalOpenClip : journalCloseClip;
        _journalAudioSource.time = isOpening ? 0.05f : 0f;
        _journalAudioSource.Play();
    }
    
}
