using UnityEditor;
using System.Linq;

public static class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        // Read custom output path from command line if provided
        string customPath = GetArg("-customBuildPath");
        string buildPath = string.IsNullOrEmpty(customPath)
            ? "Builds/Windows/Game.exe"
            : System.IO.Path.Combine(customPath, "Game.exe");

        var report = BuildPipeline.BuildPlayer(
            scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new System.Exception("Build failed: " + report.summary.result);
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
