#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class FindDuplicateGlobalLights2D
{
    // Store light info as data before scene closes
    private class LightInfo
    {
        public string GameObjectPath;
        public string ScenePath;
        public int BlendStyleIndex;
        public int[] TargetSortingLayers;
        
        public LightInfo(Light2D light, string scenePath)
        {
            GameObjectPath = GetFullPath(light.gameObject);
            ScenePath = scenePath;
            BlendStyleIndex = light.blendStyleIndex;
            TargetSortingLayers = light.targetSortingLayers.ToArray();
        }
    }

    [MenuItem("Tools/2D Lighting/Find Duplicate Global Lights")]
    public static void FindDuplicates()
    {
        FindDuplicatesInAllScenes();
    }

    [MenuItem("Tools/2D Lighting/Auto-Fix Duplicates (Assign Unique Blend Styles)")]
    public static void AutoFixDuplicates()
    {
        if (!EditorUtility.DisplayDialog(
            "Auto-Fix Global Light Duplicates",
            "This will assign unique blend style indices to each scene's Global Lights.\n\n" +
            "⚠️ WARNING: This may change the visual appearance of your scenes if you've customized blend styles in your 2D Renderer!\n\n" +
            "Scenes will be assigned:\n" +
            "- MainMenu: Blend Style 0\n" +
            "- Overworld: Blend Style 1\n" +
            "- BattleScene: Blend Style 2\n" +
            "- NewTutorial: Blend Style 3\n\n" +
            "Continue?",
            "Yes, Fix It",
            "Cancel"))
        {
            return;
        }

        AutoAssignBlendStyles();
    }

    private static void AutoAssignBlendStyles()
    {
        // Save currently open scenes
        var currentSetup = EditorSceneManager.GetSceneManagerSetup();

        // Get all scenes in build settings
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();

        // Assign blend styles based on scene order
        int blendStyleIndex = 0;
        int fixedCount = 0;

        try
        {
            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                var lights = Object.FindObjectsByType<Light2D>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(l => l.lightType == Light2D.LightType.Global)
                .ToList();

                if (lights.Count > 0)
                {
                    foreach (var light in lights)
                    {
                        light.blendStyleIndex = blendStyleIndex;
                        EditorUtility.SetDirty(light);
                        fixedCount++;
                    }

                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[2D Lighting] Scene '{scene.name}' - Assigned blend style {blendStyleIndex} to {lights.Count} light(s)");
                    
                    blendStyleIndex++;
                    
                    // Unity only supports 4 blend styles (0-3)
                    if (blendStyleIndex > 3)
                    {
                        Debug.LogWarning("[2D Lighting] More than 4 scenes with Global Lights! Wrapping back to blend style 0.");
                        blendStyleIndex = 0;
                    }
                }
            }
        }
        finally
        {
            // Restore original scene setup
            if (currentSetup != null && currentSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(currentSetup);
            }
        }

        Debug.Log($"[2D Lighting] ✅ Fixed {fixedCount} Global Lights across {scenePaths.Count} scenes!");
        EditorUtility.DisplayDialog("Success", $"Fixed {fixedCount} Global Lights!\n\nYour build should now succeed.", "OK");
    }

    /// <summary>
    /// Search ALL scenes in build settings (like Unity's build process does)
    /// </summary>
    private static void FindDuplicatesInAllScenes()
    {
        // Save currently open scenes
        var currentSetup = EditorSceneManager.GetSceneManagerSetup();

        // Get all scenes in build settings
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();

        if (scenePaths.Count == 0)
        {
            Debug.LogWarning("[2D Lighting] No enabled scenes found in Build Settings!");
            return;
        }

        Debug.Log($"[2D Lighting] Scanning {scenePaths.Count} scenes from Build Settings...");

        // Dictionary: (blendStyleIndex, sortingLayerId) -> List of LightInfo
        var map = new Dictionary<(int, int), List<LightInfo>>();

        try
        {
            // Iterate through all scenes
            foreach (var scenePath in scenePaths)
            {
                // Open the scene additively without saving current scenes
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                // Find all global lights in this scene
                var lights = Object.FindObjectsByType<Light2D>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(l => l.lightType == Light2D.LightType.Global)
                .Where(l => l.gameObject.scene == scene) // Only lights from this scene
                .ToList();

                Debug.Log($"[2D Lighting] Scene: {scene.name} - Found {lights.Count} Global Light2D(s)");

                // IMPORTANT: Capture light info BEFORE closing the scene
                foreach (var light in lights)
                {
                    var lightInfo = new LightInfo(light, scenePath);
                    
                    // Debug: show what layers this light targets
                    string layerNames = string.Join(", ", lightInfo.TargetSortingLayers.Select(id => SortingLayer.IDToName(id)));
                    Debug.Log($"    Light '{lightInfo.GameObjectPath}' (Blend Style {lightInfo.BlendStyleIndex}) targets layers: [{layerNames}]");
                    
                    foreach (int layerId in lightInfo.TargetSortingLayers)
                    {
                        var key = (lightInfo.BlendStyleIndex, layerId);

                        if (!map.TryGetValue(key, out var list))
                        {
                            list = new List<LightInfo>();
                            map[key] = list;
                        }

                        list.Add(lightInfo);
                    }
                }

                // Close this scene (don't save changes)
                EditorSceneManager.CloseScene(scene, false);
            }
        }
        finally
        {
            // Restore original scene setup
            if (currentSetup != null && currentSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(currentSetup);
            }
        }

        // Check for duplicates
        bool found = false;

        foreach (var kv in map.Where(kv => kv.Value.Count > 1))
        {
            found = true;
            string layerName = SortingLayer.IDToName(kv.Key.Item2);

            Debug.LogWarning(
                $"[2D Lighting] ⚠️ DUPLICATE Global Lights found\n" +
                $"- Blend Style: {kv.Key.Item1}\n" +
                $"- Sorting Layer: {layerName}\n" +
                $"- Found in {kv.Value.Count} location(s)"
            );

            // Group by scene to make it clearer
            var byScene = kv.Value.GroupBy(x => x.ScenePath);

            foreach (var sceneGroup in byScene)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneGroup.Key);
                Debug.LogWarning($"   📁 Scene: {sceneName}");

                foreach (var lightInfo in sceneGroup)
                {
                    Debug.LogWarning($"     • {lightInfo.GameObjectPath}");
                }
            }
        }

        if (!found)
        {
            Debug.Log("[2D Lighting] ✅ No duplicate Global Light2D found 🎉");
        }
        else
        {
            Debug.LogError(
                "[2D Lighting] ❌ Found duplicate lights! These will cause build errors.\n\n" +
                "Fix options:\n" +
                "1. Use 'Tools > 2D Lighting > Auto-Fix Duplicates' to automatically assign unique blend styles\n" +
                "2. Manually change blend style indices in each scene's Global Lights\n" +
                "3. Remove duplicate lights if not needed"
            );
        }
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}
#endif