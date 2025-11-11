#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class URPRefresh
{
    // This runs automatically right before Play Mode starts
    [InitializeOnEnterPlayMode]
    static void OnEnterPlayMode(EnterPlayModeOptions options)
    {
        var rp = GraphicsSettings.defaultRenderPipeline;
        GraphicsSettings.defaultRenderPipeline = null;
        GraphicsSettings.defaultRenderPipeline = rp;
        Debug.Log("[URPRefresh] URP renderer asset reloaded to fix 2D shadow cache.");
    }
}
#endif
