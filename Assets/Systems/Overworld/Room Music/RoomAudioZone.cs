using UnityEngine;

public class RoomAudioZone : MonoBehaviour
{
    public AudioSource roomMusic;

    private void Reset()
    {
        // Auto-fill roomMusic if you forget to drag it
        if (roomMusic == null)
            roomMusic = GetComponent<AudioSource>();

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER ENTERED zone: " + name);
        if (roomMusic != null) roomMusic.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D with: " + other.name);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER EXITED zone: " + name);
        if (roomMusic != null) roomMusic.Stop();
    }
}
