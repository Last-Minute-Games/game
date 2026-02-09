using UnityEngine;

/// <summary>
/// Attach to every room's RoomAudioZone GameObject.
/// When the player enters the trigger, it tells RoomTracker which room they're in.
/// 
/// Alternatively — RoomTracker can auto-detect these at runtime if you set the
/// Room Id field to match the RoomAudioZone's GameObject name.
/// </summary>
public class RoomZoneTag : MonoBehaviour
{
    [Tooltip("Must match the roomId in your RoomMapData asset.")]
    public string roomId;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomTracker.SetCurrentRoom(roomId);
    }
}
