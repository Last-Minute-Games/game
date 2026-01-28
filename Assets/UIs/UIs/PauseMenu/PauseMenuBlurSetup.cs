using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor helper to automatically create a blur overlay for the SimplePauseMenu.
/// This runs once in the editor to set up the blur effect properly.
/// </summary>
[InitializeOnLoad]
public class PauseMenuBlurSetup
{
    static PauseMenuBlurSetup()
    {
        // This will be called when Unity loads
        EditorApplication.delayCall += CheckAndSetupBlurOverlays;
    }

    private static void CheckAndSetupBlurOverlays()
    {
        if (!Application.isPlaying)
        {
            // Only run in edit mode, not play mode
            SimplePauseMenu[] pauseMenus = Object.FindObjectsOfType<SimplePauseMenu>(true);
            
            foreach (var pauseMenu in pauseMenus)
            {
                SetupBlurOverlayIfNeeded(pauseMenu);
            }
        }
    }

    private static void SetupBlurOverlayIfNeeded(SimplePauseMenu pauseMenu)
    {
        if (pauseMenu == null) return;

        // Check if blur overlay is already assigned
        SerializedObject so = new SerializedObject(pauseMenu);
        SerializedProperty blurOverlayProp = so.FindProperty("blurOverlay");
        
        if (blurOverlayProp.objectReferenceValue != null)
        {
            // Already set up
            return;
        }

        // Check if there's already a BlurOverlay child
        Transform existingBlur = pauseMenu.transform.Find("BlurOverlay");
        if (existingBlur != null)
        {
            Image img = existingBlur.GetComponent<Image>();
            if (img != null)
            {
                blurOverlayProp.objectReferenceValue = img;
                so.ApplyModifiedProperties();
                Debug.Log($"[PauseMenuBlurSetup] Linked existing blur overlay to {pauseMenu.gameObject.name}");
                return;
            }
        }

        Debug.Log($"[PauseMenuBlurSetup] No blur overlay found for {pauseMenu.gameObject.name}. You can manually create one or use the context menu.");
    }

    [MenuItem("GameObject/UI/Setup Pause Menu Blur", false, 10)]
    private static void SetupBlurOverlayMenuItem()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Please select a GameObject with SimplePauseMenu component first.");
            return;
        }

        SimplePauseMenu pauseMenu = Selection.activeGameObject.GetComponent<SimplePauseMenu>();
        if (pauseMenu == null)
        {
            Debug.LogWarning("Selected GameObject doesn't have a SimplePauseMenu component.");
            return;
        }

        CreateBlurOverlay(pauseMenu);
    }

    private static void CreateBlurOverlay(SimplePauseMenu pauseMenu)
    {
        // Create blur overlay GameObject
        GameObject blurObj = new GameObject("BlurOverlay");
        blurObj.transform.SetParent(pauseMenu.transform, false);
        
        // Add RectTransform and stretch to fill parent
        RectTransform rt = blurObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        
        // Add Image component
        Image img = blurObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        img.raycastTarget = false; // Don't block clicks
        
        // Set as first child so it renders behind other UI
        blurObj.transform.SetAsFirstSibling();
        
        // Assign to SimplePauseMenu
        SerializedObject so = new SerializedObject(pauseMenu);
        SerializedProperty blurOverlayProp = so.FindProperty("blurOverlay");
        blurOverlayProp.objectReferenceValue = img;
        so.ApplyModifiedProperties();
        
        // Mark scene as dirty
        EditorUtility.SetDirty(pauseMenu);
        EditorUtility.SetDirty(pauseMenu.gameObject);
        
        Debug.Log($"[PauseMenuBlurSetup] Created and assigned blur overlay for {pauseMenu.gameObject.name}");
        
        // Select the blur overlay to show it was created
        Selection.activeGameObject = blurObj;
    }
}
#endif
