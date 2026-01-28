using UnityEngine;

/// <summary>
/// Centralized debug logging system with toggleable categories.
/// All logs are OFF by default in builds, can be toggled per-category in Editor.
/// </summary>
public static class DebugLogger
{
    // Toggle these in Inspector via DebugLoggerSettings ScriptableObject
    // Or set them directly here for testing
    
    [System.Serializable]
    public class Settings
    {
        public bool clockTimer = false;
        public bool interaction = false;
        public bool teleport = false;
        public bool roomAudio = false;
        public bool dialogue = false;
        public bool gameFlags = false;
        public bool journal = false;
        public bool globalPause = false;
        public bool npcBrain = false;
        public bool tutorials = false;
        public bool settingsUI = false;
        public bool general = false;
    }
    
    private static Settings _settings;
    
    static DebugLogger()
    {
        // Try to load settings from ScriptableObject
        _settings = new Settings();
        
#if UNITY_EDITOR
        // In editor, try to find settings asset
        var settingsAsset = Resources.Load<DebugLoggerSettings>("DebugLoggerSettings");
        if (settingsAsset != null)
        {
            _settings = settingsAsset.settings;
        }
#endif
    }
    
    // Clock Timer logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogClockTimer(string message)
    {
        if (_settings.clockTimer)
            Debug.Log($"[ClockTimer] {message}");
    }
    
    // Interaction logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogInteraction(string message)
    {
        if (_settings.interaction)
            Debug.Log($"[InteractionDetector] {message}");
    }
    
    // Teleport logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogTeleport(string message, string teleportName = "")
    {
        if (_settings.teleport)
        {
            if (!string.IsNullOrEmpty(teleportName))
                Debug.Log($"[TeleportSystem] {teleportName}: {message}");
            else
                Debug.Log($"[TeleportSystem] {message}");
        }
    }
    
    // Room Audio logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogRoomAudio(string message)
    {
        if (_settings.roomAudio)
            Debug.Log(message);
    }
    
    // Dialogue logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogDialogue(string message, string dialogName = "")
    {
        if (_settings.dialogue)
        {
            if (!string.IsNullOrEmpty(dialogName))
                Debug.Log($"[DialogTrigger] {dialogName}: {message}");
            else
                Debug.Log($"[DialogTrigger] {message}");
        }
    }
    
    // Game Flags logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogGameFlags(string message)
    {
        if (_settings.gameFlags)
            Debug.Log($"[GameFlags] {message}");
    }
    
    // Journal logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogJournal(string message)
    {
        if (_settings.journal)
            Debug.Log($"[Journal] {message}");
    }
    
    // Global Pause logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogGlobalPause(string message)
    {
        if (_settings.globalPause)
            Debug.Log($"[GlobalPause] {message}");
    }
    
    // NPC Brain logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogNpcBrain(string message, string npcName = "")
    {
        if (_settings.npcBrain)
        {
            if (!string.IsNullOrEmpty(npcName))
                Debug.Log($"[NpcBrain2D] {npcName}: {message}");
            else
                Debug.Log($"[NpcBrain2D] {message}");
        }
    }
    
    // Tutorial logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogTutorial(string message)
    {
        if (_settings.tutorials)
            Debug.Log($"[TutorialTrigger] {message}");
    }
    
    // Settings logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogSettings(string message)
    {
        if (_settings.settingsUI)
            Debug.Log($"[Settings] {message}");
    }
    
    // General logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(string message)
    {
        if (_settings.general)
            Debug.Log(message);
    }
    
    // Errors and warnings always show
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
    
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
}
