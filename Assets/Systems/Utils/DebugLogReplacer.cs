using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Editor tool to batch replace Debug.Log calls with DebugLogger calls
/// </summary>
public class DebugLogReplacer : EditorWindow
{
    [MenuItem("Tools/Debug/Replace Debug.Log with DebugLogger")]
    public static void ReplaceDebugLogs()
    {
        string[] filesToUpdate = new string[]
        {
            "Assets/Systems/UIs/Clock/ClockTimer.cs",
            "Assets/Systems/Overworld/Room Music/RoomAudioZone.cs",
            "Assets/Systems/InteractableItems/InteractionDetector.cs",
            "Assets/Systems/Teleport/TeleportSystem.cs",
            "Assets/Resources/Dialogues/DialogHandler.cs",
            "Assets/Systems/Flags/GameFlags.cs",
            "Assets/Systems/UIs/Journal/JournalManager.cs",
            "Assets/Systems/UIs/Journal/JournalButtonController.cs",
            "Assets/Systems/GlobalPause.cs",
            "Assets/Systems/Overworld/Intro/TutorialTrigger.cs",
            "Assets/Systems/UIs/Settings/Settings.cs"
        };

        int totalReplacements = 0;

        foreach (string filePath in filesToUpdate)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"File not found: {filePath}");
                continue;
            }

            string content = File.ReadAllText(filePath);
            string original = content;
            int fileReplacements = 0;

            // Determine the log category based on file path
            string logMethod = DetermineLogMethod(filePath);

            // Replace Debug.Log patterns
            // Pattern 1: Debug.Log($"[Tag] message")
            content = Regex.Replace(content, @"Debug\.Log\(\$""(\[[\w\s]+\])\s*(.+?)""\)", match =>
            {
                fileReplacements++;
                return $"{logMethod}($\"{match.Groups[2].Value}\")";
            });

            // Pattern 2: Debug.Log("[Tag] " + variable)
            content = Regex.Replace(content, @"Debug\.Log\(""(\[[\w\s]+\])\s*""\s*\+\s*(.+?)\)", match =>
            {
                fileReplacements++;
                return $"{logMethod}(\"{match.Groups[2].Value.Trim()}\")";
            });

            // Pattern 3: Simple Debug.Log with interpolated string
            content = Regex.Replace(content, @"Debug\.Log\(\$""([^""]+)""\)", match =>
            {
                string msg = match.Groups[1].Value;
                // Remove tag if present
                msg = Regex.Replace(msg, @"^\[[\w\s]+\]\s*", "");
                fileReplacements++;
                return $"{logMethod}($\"{msg}\")";
            });

            // Pattern 4: Simple Debug.Log with regular string
            content = Regex.Replace(content, @"Debug\.Log\(""([^""]+)""\)", match =>
            {
                string msg = match.Groups[1].Value;
                // Remove tag if present
                msg = Regex.Replace(msg, @"^\[[\w\s]+\]\s*", "");
                fileReplacements++;
                return $"{logMethod}(\"{msg}\")";
            });

            // Don't replace Debug.LogError or Debug.LogWarning - they should always show
            // Only replace if we actually made changes
            if (content != original)
            {
                File.WriteAllText(filePath, content);
                Debug.Log($"? Updated {filePath}: {fileReplacements} replacements");
                totalReplacements += fileReplacements;
            }
        }

        Debug.Log($"<b>Debug Log Replacement Complete!</b> Total replacements: {totalReplacements}");
        AssetDatabase.Refresh();
    }

    private static string DetermineLogMethod(string filePath)
    {
        if (filePath.Contains("ClockTimer")) return "DebugLogger.LogClockTimer";
        if (filePath.Contains("Interaction")) return "DebugLogger.LogInteraction";
        if (filePath.Contains("Teleport")) return "DebugLogger.LogTeleport";
        if (filePath.Contains("RoomAudio")) return "DebugLogger.LogRoomAudio";
        if (filePath.Contains("Dialog")) return "DebugLogger.LogDialogue";
        if (filePath.Contains("GameFlags")) return "DebugLogger.LogGameFlags";
        if (filePath.Contains("Journal")) return "DebugLogger.LogJournal";
        if (filePath.Contains("GlobalPause")) return "DebugLogger.LogGlobalPause";
        if (filePath.Contains("Tutorial")) return "DebugLogger.LogTutorial";
        if (filePath.Contains("Settings")) return "DebugLogger.LogSettings";
        return "DebugLogger.Log";
    }
}
