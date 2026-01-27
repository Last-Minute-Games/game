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
        public bool interactiveItems = false; // For InteractiveItem.cs
        public bool pagination = false; // For JournalPaginationController
        public bool settingsManager = false; // For SettingsManager
        public bool cutscenes = false; // For OverworldWakeUpCutscene, EndingCutsceneManager
        public bool dialogBehaviour = false; // For DialogBehaviour plugin
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
    
    // Journal UI logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogJournalUI(string message)
    {
        if (_settings.journal)
            Debug.Log($"[JournalUI] {message}");
    }
    
    // Journal Named UI logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogJournalNamed(string message)
    {
        if (_settings.journal)
            Debug.Log($"[JournalUI_Named] {message}");
    }
    
    // Pagination logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogPagination(string message)
    {
        if (_settings.pagination)
            Debug.Log($"[Pagination] {message}");
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
    
    // Settings Manager logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogSettingsManager(string message)
    {
        if (_settings.settingsManager)
            Debug.Log($"[SettingsManager] {message}");
    }
    
    // Interactive Items logs (for InteractiveItem.cs)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogInteractiveItem(string message, string itemName = "")
    {
        if (_settings.interactiveItems)
        {
            if (!string.IsNullOrEmpty(itemName))
                Debug.Log($"[InteractiveItem] {itemName}: {message}");
            else
                Debug.Log($"[InteractiveItem] {message}");
        }
    }
    
    // Cutscene logs (for OverworldWakeUpCutscene, EndingCutsceneManager)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogCutscene(string message)
    {
        if (_settings.cutscenes)
            Debug.Log($"[Cutscene] {message}");
    }
    
    // OverworldWakeUpCutscene logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogWakeUpCutscene(string message)
    {
        if (_settings.cutscenes)
            Debug.Log($"[OverworldWakeUpCutscene] {message}");
    }
    
    // DialogBehaviour logs (plugin)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogDialogBehaviour(string message)
    {
        if (_settings.dialogBehaviour)
            Debug.Log($"[DialogBehaviour] {message}");
    }
    
    // Game Bootstrapper logs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogBootstrapper(string message)
    {
        if (_settings.general) // Use general toggle for bootstrapper
            Debug.Log($"[GameBootstrapper] {message}");
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
