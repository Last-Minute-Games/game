using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple game flags system for tracking game states.
/// Flags either EXIST or DON'T EXIST - no true/false confusion.
/// Flags persist across scenes during runtime and can be saved/loaded between game sessions.
/// </summary>
public class GameFlags : PersistentSingleton<GameFlags>
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    private HashSet<string> _activeFlags = new HashSet<string>();
    
    // PlayerPrefs key for saving flags (legacy support)
    private const string SAVE_KEY = "GameFlags_SaveData";
    
    [Header("Debug Controls")]
    [Tooltip("Enable debug flag controls (P to force-add card/minigame unlock flags)")]
    public bool enableDebugControls = true;
    
    // JSON file path for save game system
    private static string GetSaveFilePath(string saveSlot = "default")
    {
        string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        return Path.Combine(saveDirectory, $"GameFlags_{saveSlot}.json");
    }

    /// <summary>
    /// Event fired when a flag is set (added) or removed
    /// </summary>
    public event Action<string> OnFlagChanged;
    
    /// <summary>
    /// Event fired when initialization is complete (after defaults are loaded)
    /// </summary>
    public event Action OnInitialized;

    /// <summary>
    /// Override Instance to auto-create if missing
    /// </summary>
    public static new GameFlags Instance
    {
        get
        {
            if (PersistentSingleton<GameFlags>.Instance == null)
            {
                // Auto-create GameFlags if it doesn't exist
                GameObject go = new GameObject("GameFlags");
                go.AddComponent<GameFlags>();
                DebugLogger.LogGameFlags("Auto-created instance");
            }
            return PersistentSingleton<GameFlags>.Instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        // Always reset to defaults on load - don't auto-load from save
        // Use LoadFromSaveFile() or LoadFromPlayerPrefs() explicitly to load saved data
        InitializeDefaultFlags();
        
        // Notify listeners that initialization is complete
        OnInitialized?.Invoke();
        DebugLogger.LogGameFlags("Initialization complete - reset to defaults (use LoadFromSaveFile() or LoadFromPlayerPrefs() to load saved data)");
    }

    private void Update()
    {
        // Debug controls
        if (enableDebugControls)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                AddAllCardFlags();
            }
        }
    }

    /// <summary>
    /// Debug method: Add all card flags
    /// </summary>
    private void AddAllCardFlags()
    {
        string[] cardFlags = new string[]
        {
            "card.double_slash",
            "card.dramatic_exit",
            "card.exchange",
            "card.tariff_strike",
            "card.energy_drink",
            "card.shield_slash",
            "card.workout",
            "minigame.blackjack.show",
            "minigame.sokoban.show",
            "minigame.coinflip.show",
            "character.avant.heir",
            "minigame.maze.show",
            "minigame.sokoban.finish"
        };

        DebugLogger.LogGameFlags("DEBUG: Force-adding card/minigame unlock flags (P pressed)");
        foreach (string flag in cardFlags)
        {
            SetFlag(flag);
        }
        DebugLogger.LogGameFlags($"DEBUG: Added {cardFlags.Length} card flags");
    }

    /// <summary>
    /// Initialize default flags that should always exist from game start.
    /// These flags are always set at the beginning of each game session.
    /// </summary>
    private void InitializeDefaultFlags()
    {
        DebugLogger.LogGameFlags("Setting default character flags");
        
        // Base character flags - these are ALWAYS available from game start
        _activeFlags.Add("character.marco");
        _activeFlags.Add("character.allistair");
        _activeFlags.Add("character.adrianne");
        _activeFlags.Add("character.avant");
        _activeFlags.Add("character.charles");
        _activeFlags.Add("character.sebastian");
        _activeFlags.Add("character.elias");

        // nether flags
        _activeFlags.Add("day.one");

        // nether insanity meter (max of three insanity levels)
        _activeFlags.Add("insanity.zero");
        
        // these three also have a metadata flag called unlockedByDefault as fallback
        _activeFlags.Add("card.slash");
        _activeFlags.Add("card.block");
        _activeFlags.Add("card.heal_potion");

        DebugLogger.LogGameFlags($"Default flags initialized ({_activeFlags.Count} total flags)");
    }

    // ========== SAVE/LOAD SYSTEM ==========

    /// <summary>
    /// Save all active flags to PlayerPrefs using JSON serialization (legacy method).
    /// For new save system, use SaveToFile() instead.
    /// </summary>
    public static void SaveFlags()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Cannot save flags.");
            return;
        }

        Instance.SaveToPlayerPrefsInternal();
    }

    /// <summary>
    /// Load flags from PlayerPrefs (legacy method). Returns true if data was loaded, false if no save data exists.
    /// This resets to defaults first, then loads saved data.
    /// </summary>
    public static bool LoadFlags()
    {
        return LoadFromPlayerPrefs();
    }

    /// <summary>
    /// Load flags from PlayerPrefs. Returns true if data was loaded, false if no save data exists.
    /// This resets to defaults first, then loads saved data.
    /// </summary>
    public static bool LoadFromPlayerPrefs()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Cannot load flags.");
            return false;
        }

        // Reset to defaults first
        Instance._activeFlags.Clear();
        Instance.InitializeDefaultFlags();
        
        // Then load saved data
        return Instance.LoadFromPlayerPrefsInternal();
    }

    /// <summary>
    /// Save all active flags to a JSON file in the save directory.
    /// </summary>
    /// <param name="saveSlot">Save slot name (default: "default")</param>
    /// <returns>True if save was successful, false otherwise</returns>
    public static bool SaveToFile(string saveSlot = "default")
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Cannot save flags.");
            return false;
        }

        return Instance.SaveToFileInternal(saveSlot);
    }

    /// <summary>
    /// Load flags from a JSON file in the save directory.
    /// This resets to defaults first, then loads saved data.
    /// </summary>
    /// <param name="saveSlot">Save slot name (default: "default")</param>
    /// <returns>True if data was loaded, false if no save file exists</returns>
    public static bool LoadFromFile(string saveSlot = "default")
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Cannot load flags.");
            return false;
        }

        // Reset to defaults first
        Instance._activeFlags.Clear();
        Instance.InitializeDefaultFlags();
        
        // Then load saved data
        return Instance.LoadFromFileInternal(saveSlot);
    }

    /// <summary>
    /// Check if a save file exists for the given slot
    /// </summary>
    /// <param name="saveSlot">Save slot name (default: "default")</param>
    /// <returns>True if save file exists</returns>
    public static bool HasSaveFile(string saveSlot = "default")
    {
        string filePath = GetSaveFilePath(saveSlot);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Delete a save file for the given slot
    /// </summary>
    /// <param name="saveSlot">Save slot name (default: "default")</param>
    /// <returns>True if file was deleted, false if it didn't exist</returns>
    public static bool DeleteSaveFile(string saveSlot = "default")
    {
        string filePath = GetSaveFilePath(saveSlot);
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                DebugLogger.LogGameFlags($"Save file deleted: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"[GameFlags] Failed to delete save file: {ex.Message}");
                return false;
            }
        }
        else
        {
            DebugLogger.LogGameFlags($"Save file does not exist: {filePath}");
            return false;
        }
    }

    /// <summary>
    /// Delete all saved flag data from PlayerPrefs (legacy).
    /// This does NOT affect the current runtime flags - use ResetToDefaults() for that.
    /// </summary>
    public static void DeleteSavedFlags()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            DebugLogger.LogGameFlags("Saved flag data deleted from PlayerPrefs");
        }
        else
        {
            DebugLogger.LogGameFlags("No saved flag data to delete");
        }
    }

    /// <summary>
    /// Check if saved flag data exists in PlayerPrefs (legacy)
    /// </summary>
    public static bool HasSavedFlags()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    // ========== INTERNAL SAVE/LOAD METHODS ==========

    /// <summary>
    /// Save to PlayerPrefs (legacy method)
    /// </summary>
    private void SaveToPlayerPrefsInternal()
    {
        try
        {
            // Create a serializable wrapper for the HashSet
            GameFlagsSaveData saveData = new GameFlagsSaveData
            {
                flags = new List<string>(_activeFlags)
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(saveData, true);
            
            // Save to PlayerPrefs
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            DebugLogger.LogGameFlags($"Saved {_activeFlags.Count} flags to PlayerPrefs");
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"[GameFlags] Failed to save flags to PlayerPrefs: {ex.Message}");
        }
    }

    /// <summary>
    /// Load from PlayerPrefs (legacy method)
    /// </summary>
    private bool LoadFromPlayerPrefsInternal()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            DebugLogger.LogGameFlags("No saved flag data found in PlayerPrefs");
            return false;
        }

        try
        {
            // Load JSON from PlayerPrefs
            string json = PlayerPrefs.GetString(SAVE_KEY);
            
            // Deserialize
            GameFlagsSaveData saveData = JsonUtility.FromJson<GameFlagsSaveData>(json);
            
            if (saveData == null || saveData.flags == null)
            {
                DebugLogger.LogWarning("[GameFlags] Save data from PlayerPrefs was invalid or empty");
                return false;
            }

            // Load saved flags (defaults were already set, so we merge/add them)
            foreach (string flag in saveData.flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _activeFlags.Add(flag);
                }
            }

            DebugLogger.LogGameFlags($"Loaded {saveData.flags.Count} flags from PlayerPrefs (total: {_activeFlags.Count})");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"[GameFlags] Failed to load flags from PlayerPrefs: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Save to JSON file
    /// </summary>
    private bool SaveToFileInternal(string saveSlot)
    {
        try
        {
            // Get current clock time from ClockTimer
            ClockTimer clockTimer = FindObjectOfType<ClockTimer>();
            float clockTimeLeft = clockTimer != null ? clockTimer.GetTimeLeft() : 60f;
            
            // Determine current day
            string currentDay = GetCurrentDay();
            
            // Create a serializable wrapper for the HashSet
            GameFlagsSaveData saveData = new GameFlagsSaveData
            {
                flags = new List<string>(_activeFlags),
                clockTimeLeft = clockTimeLeft,
                currentDay = currentDay
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(saveData, true);
            
            // Get file path
            string filePath = GetSaveFilePath(saveSlot);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write to file
            File.WriteAllText(filePath, json);

            DebugLogger.LogGameFlags($"Saved {_activeFlags.Count} flags to file: {filePath} (clockTime: {clockTimeLeft:F2}s, day: {currentDay})");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"[GameFlags] Failed to save flags to file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load from JSON file
    /// </summary>
    private bool LoadFromFileInternal(string saveSlot)
    {
        string filePath = GetSaveFilePath(saveSlot);
        
        if (!File.Exists(filePath))
        {
            DebugLogger.LogGameFlags($"No save file found at: {filePath}");
            return false;
        }

        try
        {
            // Read JSON from file
            string json = File.ReadAllText(filePath);
            
            // Deserialize
            GameFlagsSaveData saveData = JsonUtility.FromJson<GameFlagsSaveData>(json);
            
            if (saveData == null || saveData.flags == null)
            {
                DebugLogger.LogWarning("[GameFlags] Save file data was invalid or empty");
                return false;
            }

            // Load saved flags (defaults were already set, so we merge/add them)
            foreach (string flag in saveData.flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _activeFlags.Add(flag);
                }
            }

            // Restore clock time if ClockTimer exists
            ClockTimer clockTimer = FindObjectOfType<ClockTimer>();
            if (clockTimer != null)
            {
                if (saveData.clockTimeLeft > 0)
                {
                    clockTimer.RestoreTimeLeft(saveData.clockTimeLeft);
                    DebugLogger.LogGameFlags($"? Restored clock time from save: {saveData.clockTimeLeft:F2}s");
                }
                else
                {
                    DebugLogger.LogWarning($"[GameFlags] Save data has invalid clock time: {saveData.clockTimeLeft}s - using default");
                }
            }
            else
            {
                DebugLogger.LogWarning("[GameFlags] ClockTimer not found in scene - clock time not restored (will be set when ClockTimer initializes)");
            }

            DebugLogger.LogGameFlags($"? Loaded {saveData.flags.Count} flags from file: {filePath} (total: {_activeFlags.Count}, day: {saveData.currentDay})");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"[GameFlags] Failed to load flags from file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the current day string from active flags
    /// </summary>
    private string GetCurrentDay()
    {
        if (_activeFlags.Contains("day.five")) return "day.five";
        if (_activeFlags.Contains("day.four")) return "day.four";
        if (_activeFlags.Contains("day.three")) return "day.three";
        if (_activeFlags.Contains("day.two")) return "day.two";
        if (_activeFlags.Contains("day.one")) return "day.one";
        return "day.one"; // Default
    }

    // ========== STATIC API (Singleton Pattern) ==========

    /// <summary>
    /// Set a flag (adds it to the active flags). Persists across scenes but not between game sessions.
    /// Use SaveFlags() to persist to disk.
    /// 
    /// SPECIAL BEHAVIOR: Day flags (day.one through day.five) automatically trigger a save.
    /// </summary>
    /// <param name="flagName">The name of the flag to set</param>
    public static void SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            DebugLogger.LogWarning("[GameFlags] Attempted to set flag with null or empty name");
            return;
        }

        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (!Instance._activeFlags.Contains(flagName))
        {
            Instance._activeFlags.Add(flagName);
            Instance.OnFlagChanged?.Invoke(flagName);
            DebugLogger.LogGameFlags($"Set flag: {flagName}");
            
            // AUTO-SAVE for day progression flags using current save name
            if (IsDayFlag(flagName))
            {
                DebugLogger.LogGameFlags($"Day flag detected - auto-saving game progress");
                // Use GameFlagsManager to save to current save slot
                GameFlagsManager.SaveCurrentGame();
            }
        }
    }

    /// <summary>
    /// Check if a flag name is a day progression flag (day.one through day.five)
    /// </summary>
    private static bool IsDayFlag(string flagName)
    {
        return flagName == "day.one" || 
               flagName == "day.two" || 
               flagName == "day.three" || 
               flagName == "day.four" || 
               flagName == "day.five";
    }

    /// <summary>
    /// Check if a flag exists (has been set)
    /// </summary>
    /// <param name="flagName">The name of the flag to check</param>
    /// <returns>True if the flag exists, false otherwise</returns>
    public static bool HasFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
            return false;

        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return false;
        }

        return Instance._activeFlags.Contains(flagName);
    }

    /// <summary>
    /// Remove a specific flag.
    /// </summary>
    /// <param name="flagName">The name of the flag to remove</param>
    public static void RemoveFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
            return;

        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (Instance._activeFlags.Remove(flagName))
        {
            Instance.OnFlagChanged?.Invoke(flagName);
            DebugLogger.LogGameFlags($"Removed flag: {flagName}");
        }
    }

    /// <summary>
    /// Clear all flags and reinitialize defaults.
    /// </summary>
    public static void ClearAllFlags()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        Instance._activeFlags.Clear();
        DebugLogger.LogGameFlags("Cleared all flags");
        
        // Reinitialize defaults after clearing
        Instance.InitializeDefaultFlags();
    }

    /// <summary>
    /// Reset to default flags (same as ClearAllFlags).
    /// Useful for "New Game" or reset functionality.
    /// </summary>
    public static void ResetToDefaults()
    {
        ClearAllFlags();
    }

    /// <summary>
    /// Get the count of active flags
    /// </summary>
    public static int GetFlagCount()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return 0;
        }

        return Instance._activeFlags.Count;
    }

    /// <summary>
    /// Get all active flag names
    /// </summary>
    public static HashSet<string> GetAllFlags()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return new HashSet<string>();
        }

        return new HashSet<string>(Instance._activeFlags);
    }

    /// <summary>
    /// Debug: Print all active flags to the console
    /// </summary>
    public static void PrintAllFlags()
    {
        if (Instance == null)
        {
            DebugLogger.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (Instance._activeFlags.Count == 0)
        {
            DebugLogger.LogGameFlags("No flags are currently set");
            return;
        }

        DebugLogger.LogGameFlags($"Active flags ({Instance._activeFlags.Count}):");
        foreach (string flag in Instance._activeFlags)
        {
            DebugLogger.LogGameFlags($"  - {flag}");
        }
    }

    /// <summary>
    /// Get save metadata without loading the entire save
    /// </summary>
    public static GameFlagsSaveData GetSaveMetadata(string saveSlot = "default")
    {
        string filePath = GetSaveFilePath(saveSlot);
        
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            GameFlagsSaveData saveData = JsonUtility.FromJson<GameFlagsSaveData>(json);
            return saveData;
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"[GameFlags] Failed to read save metadata: {ex.Message}");
            return null;
        }
    }

    // ========== INSTANCE API (for ScriptableObject references) ==========

    /// <summary>
    /// Instance method: Check if a flag exists
    /// </summary>
    public bool Has(string flagName)
    {
        return HasFlag(flagName);
    }

    /// <summary>
    /// Instance method: Set a flag
    /// </summary>
    public void Set(string flagName)
    {
        SetFlag(flagName);
    }

    /// <summary>
    /// Instance method: Remove a flag
    /// </summary>
    public void Remove(string flagName)
    {
        RemoveFlag(flagName);
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            DebugLogger.LogGameFlags($"[GameFlags] {message}");
    }
}

/// <summary>
/// Serializable wrapper class for saving flags to JSON
/// </summary>
[Serializable]
public class GameFlagsSaveData
{
    public List<string> flags = new List<string>();
    public float clockTimeLeft = 60f; // NEW: Save clock time remaining
    public string currentDay = "day.one"; // NEW: Save current day for display
}
