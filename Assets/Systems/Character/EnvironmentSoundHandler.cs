using UnityEngine;

public class EnvironmentSoundHandler : MonoBehaviour
{
    public AudioSource environmentAudioSource;
    
    private AudioClip _doorCloseClip;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        environmentAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
