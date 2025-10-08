using UnityEngine;

public class EnvironmentSoundHandler : MonoBehaviour
{
    private AudioSource _doorAudioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get by name in children
        _doorAudioSource = transform.Find("DoorAudioSource").GetComponent<AudioSource>();
    }

    public void PlayDoorSound()
    {
        _doorAudioSource.Play();
    }
}
