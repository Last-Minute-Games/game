using UnityEngine;

/// <summary>
/// Tiny static tracker that remembers which room the player is currently in.
/// Updated by RoomZoneTag triggers. Read by RoomMapUI to highlight the player's room.
/// </summary>
public static class RoomTracker
{
    /// <summary>The roomId the player is currently inside (null if unknown).</summary>
    public static string CurrentRoomId { get; private set; }

    public static event System.Action<string> OnRoomChanged;

    public static void SetCurrentRoom(string roomId)
    {
        if (CurrentRoomId == roomId) return;
        CurrentRoomId = roomId;
        OnRoomChanged?.Invoke(roomId);
    }
}
