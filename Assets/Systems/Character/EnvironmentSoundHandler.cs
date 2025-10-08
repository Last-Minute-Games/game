using UnityEngine;

public class EnvironmentSoundHandler : MonoBehaviour
{
    private AudioSource _doorAudioSource;
    private AudioSource _journalAudioSource;
    
    [Header("Door Sounds")]
    [SerializeField] private AudioClip doorOpenClip;
    
    [Header("Journal Sounds")]
    [SerializeField] private AudioClip journalOpenClip;
    [SerializeField] private AudioClip journalCloseClip;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get by name in children
        _doorAudioSource = transform.Find("DoorAudioSource").GetComponent<AudioSource>();
        _journalAudioSource = transform.Find("JournalAudioSource").GetComponent<AudioSource>();
    }

    public void PlayDoorSound()
    {
        _doorAudioSource.clip = doorOpenClip;
        _doorAudioSource.Play();
    }
    
    public void PlayJournalSound(bool isOpening)
    {
        _journalAudioSource.clip = isOpening ? journalOpenClip : journalCloseClip;
        _journalAudioSource.time = isOpening ? 0.05f : 0f;
        _journalAudioSource.Play();
    }
    
}
