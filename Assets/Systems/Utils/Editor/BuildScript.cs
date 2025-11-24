using UnityEditor;
using UnityEditor.Build;
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
        UnityEngine.Debug.Log("[BuildScript] Starting Linux build with Mono backend...");
        UnityEngine.Debug.Log($"[BuildScript] Current BuildTarget: {EditorUserBuildSettings.activeBuildTarget}");
        
        // Check if Linux build support is installed
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
        {
            UnityEngine.Debug.LogError("[BuildScript] Linux build support is not installed!");
            throw new System.Exception("Linux build target is not supported. Please install Linux Build Support module in Unity Hub.");
        }
        
        // NOTE: We're using Mono for now because switching to IL2CPP at build time doesn't work.
        // The scripting backend must be set in Project Settings BEFORE the build starts.
        // To use IL2CPP, manually set it in Edit > Project Settings > Player > Other Settings > Scripting Backend
        
        var linuxTarget = NamedBuildTarget.Standalone;
        var currentBackend = PlayerSettings.GetScriptingBackend(linuxTarget);
        UnityEngine.Debug.Log($"[BuildScript] Current scripting backend: {currentBackend}");
        
        if (currentBackend == ScriptingImplementation.Mono2x)
        {
            UnityEngine.Debug.LogWarning("[BuildScript] Building with Mono backend. For Unity 6 on Linux, IL2CPP is recommended.");
            UnityEngine.Debug.LogWarning("[BuildScript] To use IL2CPP: Set it in Project Settings, commit the change, then rebuild.");
        }
        
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

    public static void BuildMacOS()
    {
        UnityEngine.Debug.Log("[BuildScript] Starting macOS build...");
        UnityEngine.Debug.Log($"[BuildScript] Current BuildTarget: {EditorUserBuildSettings.activeBuildTarget}");
        
        // Check if macOS build support is installed
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
        {
            UnityEngine.Debug.LogError("[BuildScript] macOS build support is not installed!");
            throw new System.Exception("macOS build target is not supported. Please install macOS Build Support module in Unity Hub.");
        }
        
        // CRITICAL: macOS MUST use IL2CPP or Mono with proper runtime included
        // Setting IL2CPP explicitly ensures the runtime is properly bundled
        var standaloneTarget = NamedBuildTarget.Standalone;
        var currentBackend = PlayerSettings.GetScriptingBackend(standaloneTarget);
        UnityEngine.Debug.Log($"[BuildScript] Current scripting backend: {currentBackend}");
        
        // Force IL2CPP for macOS builds to ensure runtime is included
        PlayerSettings.SetScriptingBackend(standaloneTarget, ScriptingImplementation.IL2CPP);
        UnityEngine.Debug.Log("[BuildScript] Set scripting backend to IL2CPP for macOS build");
        
        // Build universal binary (Intel 64-bit + Apple Silicon)
        // According to Unity docs: 0 = None, 1 = ARM64, 2 = Universal
        UnityEngine.Debug.Log("[BuildScript] Building universal binary for Intel 64-bit + Apple Silicon");
        
        // Set the architecture to universal (2) before building
        PlayerSettings.SetArchitecture(standaloneTarget, 2);
        UnityEngine.Debug.Log("[BuildScript] Set architecture to Universal (2) - Intel 64-bit + Apple Silicon");
        
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        UnityEngine.Debug.Log($"[BuildScript] Building {scenes.Length} scenes: {string.Join(", ", scenes)}");

        // Read custom output path from command line if provided
        string customPath = GetArg("-customBuildPath");
        string buildPath = string.IsNullOrEmpty(customPath)
            ? "Builds/macOS/CastleOfTime.app"
            : System.IO.Path.Combine(customPath, "CastleOfTime.app");

        UnityEngine.Debug.Log($"[BuildScript] Output path: {buildPath}");

        var report = BuildPipeline.BuildPlayer(
            scenes, buildPath, BuildTarget.StandaloneOSX, BuildOptions.None);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            UnityEngine.Debug.LogError($"[BuildScript] Build errors: {string.Join("\n", report.summary.totalErrors)}");
            throw new System.Exception($"Build failed: {report.summary.result}");
        }
        
        UnityEngine.Debug.Log("[BuildScript] macOS universal binary build completed successfully!");
        UnityEngine.Debug.Log($"[BuildScript] Build size: {report.summary.totalSize} bytes");
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
