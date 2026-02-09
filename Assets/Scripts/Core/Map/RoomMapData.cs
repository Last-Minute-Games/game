using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that holds the castle room layout for the map overlay.
/// Create via Assets → Create → Castle of Time → Room Map Data.
/// </summary>
[CreateAssetMenu(fileName = "RoomMapData", menuName = "Castle of Time/Room Map Data")]
public class RoomMapData : ScriptableObject
{
    [System.Serializable]
    public class Room
    {
        [Tooltip("Display name shown on the map.")]
        public string roomName;

        [Tooltip("Unique ID (matches the RoomAudioZone GameObject name).")]
        public string roomId;

        [Tooltip("Position on the map UI (normalised 0-1, where 0,0 = bottom-left).")]
        public Vector2 mapPosition;

        [Tooltip("Size of the room rectangle on the map (normalised 0-1).")]
        public Vector2 mapSize = new Vector2(0.12f, 0.10f);

        [Tooltip("IDs of rooms this room connects to (bidirectional doors).")]
        public List<string> connectedRoomIds = new List<string>();
    }

    [Header("Rooms")]
    public List<Room> rooms = new List<Room>();

    /// <summary>Find a room entry by its ID.</summary>
    public Room GetRoom(string id)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].roomId == id)
                return rooms[i];
        }
        return null;
    }
}
