using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to add RoomNamePopup to the scene.
/// Menu: Tools > Castle of Time > Create Room Name Popup
/// </summary>
public class RoomNamePopupSetup : Editor
{
    [MenuItem("Tools/Castle of Time/Create Room Name Popup")]
    public static void CreateRoomNamePopup()
    {
        // Check if already exists
        if (Object.FindObjectOfType<RoomNamePopup>() != null)
        {
            EditorUtility.DisplayDialog("Already Exists",
                "RoomNamePopup already exists in this scene.", "OK");
            Selection.activeGameObject = Object.FindObjectOfType<RoomNamePopup>().gameObject;
            return;
        }

        // Create GameObject
        GameObject popupObj = new GameObject("RoomNamePopup");
        Undo.RegisterCreatedObjectUndo(popupObj, "Create Room Name Popup");

        // Add component
        RoomNamePopup popup = popupObj.AddComponent<RoomNamePopup>();

        // Find and assign journal sprite
        Sprite journalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Sprites/UI/journal/journal.png");

        if (journalSprite != null)
        {
            SerializedObject so = new SerializedObject(popup);
            so.FindProperty("journalSprite").objectReferenceValue = journalSprite;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[RoomNamePopupSetup] journal.png not found at expected path. Please assign manually.");
        }

        // Find and assign RoomMapData
        string[] guids = AssetDatabase.FindAssets("t:RoomMapData");
        if (guids.Length > 0)
        {
            RoomMapData mapData = AssetDatabase.LoadAssetAtPath<RoomMapData>(
                AssetDatabase.GUIDToAssetPath(guids[0]));

            SerializedObject so = new SerializedObject(popup);
            so.FindProperty("roomMapData").objectReferenceValue = mapData;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[RoomNamePopupSetup] RoomMapData not found. Please assign manually.");
        }

        Selection.activeGameObject = popupObj;
        EditorUtility.SetDirty(popupObj);

        Debug.Log("[RoomNamePopupSetup] Room Name Popup created! Make sure to assign the Journal Sprite and RoomMapData in the Inspector if not auto-detected.");
    }
}
