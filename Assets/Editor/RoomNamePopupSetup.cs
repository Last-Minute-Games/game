using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to add RoomNamePopup to the scene.
/// Menu: Tools > Castle of Time > Create Room Name Popup
/// </summary>
public class RoomNamePopupSetup : Editor
{
    private const string RoomBannerPath = "Assets/Sprites/roomBanner.png";
    private const string RoomBannerSpriteName = "roomBanner_0";

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

        // Find and assign room banner sprite
        Sprite roomBannerSprite = LoadRoomBannerSprite();

        if (roomBannerSprite != null)
        {
            SerializedObject so = new SerializedObject(popup);
            so.FindProperty("journalSprite").objectReferenceValue = roomBannerSprite;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[RoomNamePopupSetup] roomBanner.png not found at expected path. Please assign manually.");
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

        Debug.Log("[RoomNamePopupSetup] Room Name Popup created! Make sure to assign the room banner sprite and RoomMapData in the Inspector if not auto-detected.");
    }

    private static Sprite LoadRoomBannerSprite()
    {
        Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoomBannerPath);
        if (directSprite != null)
        {
            return directSprite;
        }

        Sprite fallbackSprite = null;
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(RoomBannerPath);
        foreach (Object asset in allAssets)
        {
            if (asset is not Sprite sprite)
            {
                continue;
            }

            if (sprite.name == RoomBannerSpriteName)
            {
                return sprite;
            }

            if (fallbackSprite == null)
            {
                fallbackSprite = sprite;
            }
        }

        return fallbackSprite;
    }
}
