using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// One-click editor tool that sets every RoomZoneTag.roomId in the active scene
/// to the correct named string that matches the RoomMapData asset.
///
/// The mapping is derived from each RoomZoneTag's **GameObject name** →
/// the corresponding roomId in RoomMapData.  Run once after opening the
/// Overworld scene, then save the scene.
///
/// Menu:  Castle of Time → Fix Room Zone IDs
/// </summary>
public static class RoomZoneIdFixer
{
#if UNITY_EDITOR
    // Maps GameObject name → RoomMapData roomId
    // Add entries here if new rooms are introduced.
    private static readonly Dictionary<string, string> NameToId = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "Ballroom",    "ballroom"  },
        { "Throne Room", "throne"    },
        { "Armory",      "armory"    },
        { "Kitchen",     "kitchen"   },
        { "Classroom",   "classroom" },
        { "Dining Room", "dining"    },
        { "Study Room",  "study"     },
        { "Library",     "library"   },
        { "Patio",       "patio"     },
        { "Bedroom",     "bedroom"   },
    };

    [MenuItem("Castle of Time/Fix Room Zone IDs")]
    public static void FixAllRoomZoneIds()
    {
        var tags = Object.FindObjectsOfType<RoomZoneTag>();

        if (tags.Length == 0)
        {
            EditorUtility.DisplayDialog("No RoomZoneTags",
                "No RoomZoneTag components found in the active scene.\n" +
                "Make sure the Overworld scene is open.", "OK");
            return;
        }

        int fixedCount = 0;
        int alreadyCorrect = 0;
        var warnings = new List<string>();

        foreach (var tag in tags)
        {
            string goName = tag.gameObject.name;

            if (NameToId.TryGetValue(goName, out string correctId))
            {
                if (tag.roomId == correctId)
                {
                    alreadyCorrect++;
                    continue;
                }

                Undo.RecordObject(tag, "Fix RoomZoneTag roomId");
                string oldId = tag.roomId;
                tag.roomId = correctId;
                EditorUtility.SetDirty(tag);
                fixedCount++;
                Debug.Log($"[RoomZoneIdFixer] '{goName}': \"{oldId}\" → \"{correctId}\"");
            }
            else
            {
                warnings.Add($"'{goName}' (roomId: \"{tag.roomId}\") — no mapping found");
            }
        }

        // Mark scene dirty so the user is prompted to save
        if (fixedCount > 0)
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        string msg = $"Scanned {tags.Length} RoomZoneTag(s).\n\n" +
                     $"  Fixed:           {fixedCount}\n" +
                     $"  Already correct: {alreadyCorrect}\n";

        if (warnings.Count > 0)
        {
            msg += $"  Unrecognised:    {warnings.Count}\n\n";
            foreach (var w in warnings)
                msg += $"  ⚠ {w}\n";
        }

        msg += "\nRemember to save the scene (Ctrl+S) to persist the changes.";

        EditorUtility.DisplayDialog("Fix Room Zone IDs", msg, "OK");
    }
#endif
}
