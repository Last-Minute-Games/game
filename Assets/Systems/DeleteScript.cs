#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

public static class SL2D_ProjectStrip
{
    [MenuItem("Tools/SmartLighting2D/Strip From All Scenes")]
    public static void StripScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        int removed = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.scene == scene))
            {
                // kill known objects by name
                if (go.name.Contains("Lighting Manager 2D") || go.name.Contains("Camera Buffer"))
                {
                    Object.DestroyImmediate(go);
                    removed++;
                    continue;
                }
                // remove any FunkyCode components by type
                foreach (var mb in go.GetComponents<MonoBehaviour>())
                {
                    if (mb == null) continue;
                    var tn = mb.GetType().FullName ?? "";
                    if (tn.Contains("FunkyCode"))
                    {
                        Object.DestroyImmediate(mb);
                        removed++;
                    }
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"SmartLighting2D strip complete. Removed items: {removed}");
    }
}
#endif
