using UnityEditor;
using System.Linq;
using UnityEngine;

public static class BuildScript
{
    public static void BuildWindows()
    {
        UnityEngine.Debug.Log("[BuildScript] Starting Windows build...");
        UnityEngine.Debug.Log($"[BuildScript] Current BuildTarget: {EditorUserBuildSettings.activeBuildTarget}");
        
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        UnityEngine.Debug.Log($"[BuildScript] Building {scenes.Length} scenes: {string.Join(", ", scenes)}");

        // Read custom output path from command line if provided
        string customPath = GetArg("-customBuildPath");
        string buildPath = string.IsNullOrEmpty(customPath)
            ? "Builds/Windows/Game.exe"
            : System.IO.Path.Combine(customPath, "CastleOfTime.exe");

        UnityEngine.Debug.Log($"[BuildScript] Output path: {buildPath}");

        var report = BuildPipeline.BuildPlayer(
            scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            throw new System.Exception("Build failed: " + report.summary.result);
        }
        
        UnityEngine.Debug.Log("[BuildScript] Windows build completed successfully!");
    }

    public static void BuildLinux()
    {
        UnityEngine.Debug.Log("[BuildScript] Starting Linux build...");
        UnityEngine.Debug.Log($"[BuildScript] Current BuildTarget: {EditorUserBuildSettings.activeBuildTarget}");
        
        // Check if Linux build support is installed
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
        {
            UnityEngine.Debug.LogError("[BuildScript] Linux build support is not installed!");
            throw new System.Exception("Linux build target is not supported. Please install Linux Build Support module in Unity Hub.");
        }
        
        // Force IL2CPP for Linux builds (Mono is deprecated/broken in Unity 6)
        var linuxTarget = NamedBuildTarget.Standalone;
        var originalBackend = PlayerSettings.GetScriptingBackend(linuxTarget);
        UnityEngine.Debug.Log($"[BuildScript] Original scripting backend: {originalBackend}");
        
        PlayerSettings.SetScriptingBackend(linuxTarget, ScriptingImplementation.IL2CPP);
        UnityEngine.Debug.Log("[BuildScript] Set scripting backend to IL2CPP for Linux build");
        
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        UnityEngine.Debug.Log($"[BuildScript] Building {scenes.Length} scenes: {string.Join(", ", scenes)}");

        // Read custom output path from command line if provided
        string customPath = GetArg("-customBuildPath");
        string buildPath = string.IsNullOrEmpty(customPath)
            ? "Builds/Linux/Game.x86_64"
            : System.IO.Path.Combine(customPath, "CastleOfTime.x86_64");

        UnityEngine.Debug.Log($"[BuildScript] Output path: {buildPath}");

        var report = BuildPipeline.BuildPlayer(
            scenes, buildPath, BuildTarget.StandaloneLinux64, BuildOptions.None);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            throw new System.Exception("Build failed: " + report.summary.result);
        }
        
        UnityEngine.Debug.Log("[BuildScript] Linux build completed successfully!");
    }

    private static string GetArg(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
            if (args[i] == name && i + 1 < args.Length)
                return args[i + 1];
        return null;
    }
}
