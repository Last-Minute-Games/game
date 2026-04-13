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

        [Tooltip("Fine-tuning offset applied to markers in this room (normalised 0-1). Use to align markers with the visual map.")]
        public Vector2 mapOffset = Vector2.zero;

        [Tooltip("IDs of rooms this room connects to (bidirectional doors).")]
        public List<string> connectedRoomIds = new List<string>();

        [Header("World Bounds (for real-time map tracking)")]
        [Tooltip("Centre of the room trigger zone in world space.")]
        public Vector2 worldCenter;

        [Tooltip("Radius of the room trigger zone in world space.")]
        public float worldRadius = 20f;
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

    /// <summary>
    /// Convert a world-space position to a normalised map position (0-1).
    /// Finds the room whose world circle contains the point, then maps
    /// the position proportionally within that room's rectangle on the map.
    /// Falls back to the closest room if the point is outside all zones.
    /// </summary>
    public Vector2 WorldToMapPosition(Vector3 worldPos)
    {
        Vector2 wp = new Vector2(worldPos.x, worldPos.y);

        // Phase 1: Find rooms that actually contain this point (inside their radius)
        Room bestContainingRoom = null;
        float bestContainedDist = float.MaxValue;

        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room.worldRadius <= 0f) continue;

            float dist = Vector2.Distance(wp, room.worldCenter);
            
            // Is the point inside this room's circle?
            if (dist <= room.worldRadius)
            {
                // Pick the room where the point is most centered
                if (dist < bestContainedDist)
                {
                    bestContainedDist = dist;
                    bestContainingRoom = room;
                }
            }
        }

        // If we found a room that contains the point, use it
        Room targetRoom = bestContainingRoom;

        // Phase 2: Fallback if point is outside all rooms - find closest
        if (targetRoom == null)
        {
            float closestDist = float.MaxValue;
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                if (room.worldRadius <= 0f) continue;

                float dist = Vector2.Distance(wp, room.worldCenter);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetRoom = room;
                }
            }
        }

        if (targetRoom == null) return Vector2.one * 0.5f; // centre fallback

        // Normalise position within the room's world circle → -1..1
        Vector2 localOffset = wp - targetRoom.worldCenter;
        Vector2 normalised = targetRoom.worldRadius > 0f
            ? localOffset / targetRoom.worldRadius
            : Vector2.zero;

        // Clamp to unit circle (only matters for fallback case)
        if (normalised.sqrMagnitude > 1f)
            normalised = normalised.normalized;

        // Map from -1..1 to the room's rectangle on the map
        // Room rectangle spans mapPosition ± mapSize/2
        Vector2 mapPos = targetRoom.mapPosition + new Vector2(
            normalised.x * targetRoom.mapSize.x * 0.45f,  // 0.45 keeps dot inside border
            normalised.y * targetRoom.mapSize.y * 0.45f
        );

        // Apply fine-tuning offset
        mapPos += targetRoom.mapOffset;

        return mapPos;
    }
}
