using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor helper that creates a pre-filled RoomMapData asset with all
/// Castle of Time overworld rooms already laid out and connected.
/// Use:  menu bar → Castle of Time → Create Default Room Map Data
/// </summary>
public static class RoomMapDataFactory
{
#if UNITY_EDITOR
    [MenuItem("Castle of Time/Create Default Room Map Data")]
    public static void CreateDefaultAsset()
    {
        var data = ScriptableObject.CreateInstance<RoomMapData>();

        // ──────────── Room layout (normalised 0-1 coordinates) ────────────
        //
        //  The castle is roughly:
        //
        //       Armory        Classroom
        //         |               |
        //    Throne Room    Kitchen ── Dining Room
        //         |           |
        //       BALLROOM ─────┘
        //       / |    \
        //   Patio Study  Library
        //          |
        //       Bedroom
        //

        data.rooms = new System.Collections.Generic.List<RoomMapData.Room>
        {
            MakeRoom("Ballroom",    "ballroom",    new Vector2(0.50f, 0.50f), new Vector2(0.16f, 0.12f),
                "throne", "library", "patio", "kitchen", "study", "dining"),

            MakeRoom("Throne Room", "throne",      new Vector2(0.30f, 0.72f), new Vector2(0.13f, 0.10f),
                "ballroom", "armory"),

            MakeRoom("Armory",      "armory",      new Vector2(0.14f, 0.88f), new Vector2(0.12f, 0.09f),
                "throne"),

            MakeRoom("Kitchen",     "kitchen",     new Vector2(0.70f, 0.72f), new Vector2(0.13f, 0.10f),
                "ballroom", "classroom", "dining"),

            MakeRoom("Classroom",   "classroom",   new Vector2(0.70f, 0.90f), new Vector2(0.12f, 0.09f),
                "kitchen"),

            MakeRoom("Dining Room", "dining",      new Vector2(0.88f, 0.60f), new Vector2(0.13f, 0.10f),
                "ballroom", "kitchen"),

            MakeRoom("Study Room",  "study",       new Vector2(0.35f, 0.30f), new Vector2(0.13f, 0.10f),
                "ballroom", "bedroom", "library"),

            MakeRoom("Library",     "library",     new Vector2(0.60f, 0.25f), new Vector2(0.13f, 0.10f),
                "ballroom", "study"),

            MakeRoom("Patio",       "patio",       new Vector2(0.25f, 0.15f), new Vector2(0.13f, 0.10f),
                "ballroom"),

            MakeRoom("Bedroom",     "bedroom",     new Vector2(0.20f, 0.10f), new Vector2(0.12f, 0.09f),
                "study"),
        };

        const string path = "Assets/_Data/RoomMapData.asset";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            AssetDatabase.CreateFolder("Assets", "_Data");

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;

        Debug.Log($"✔ Created RoomMapData at {path}. " +
                  "Adjust room positions in the Inspector, then wire it into RoomMapUI.");
    }

    private static RoomMapData.Room MakeRoom(
        string name, string id, Vector2 pos, Vector2 size, params string[] connections)
    {
        var room = new RoomMapData.Room
        {
            roomName = name,
            roomId = id,
            mapPosition = pos,
            mapSize = size,
            connectedRoomIds = new System.Collections.Generic.List<string>(connections)
        };
        return room;
    }
#endif
}
