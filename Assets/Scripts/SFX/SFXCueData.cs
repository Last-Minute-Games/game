using UnityEngine;

[CreateAssetMenu(menuName = "SFX/SFX Cue Data", fileName = "NewSFXCueData")]
public class SFXCueData : ScriptableObject
{
    [Header("SFX Info")]
    [Tooltip("Name of this sound cue.")]
    public string cueName = "New SFX Cue";

    [Tooltip("List of audio clips this cue can randomly select from.")]
    public AudioClip[] audioClips;

    [Tooltip("Playback volume (0.0 - 1.0).")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private void OnValidate()
    {
        if (audioClips == null || audioClips.Length == 0)
            Debug.LogWarning($"[SFXCueData] '{name}' has no AudioClips assigned!", this);
    }

    public AudioClip GetRandomClip()
    {
        if (audioClips == null || audioClips.Length == 0)
            return null;

        int index = Random.Range(0, audioClips.Length);
        return audioClips[index];
    }
}
