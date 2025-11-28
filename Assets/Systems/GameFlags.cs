using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple game flags system for tracking game states.
/// Flags either EXIST or DON'T EXIST - no true/false confusion.
/// Flags persist across scenes during runtime and can be saved/loaded between game sessions.
/// </summary>
public class GameFlags : PersistentSingleton<GameFlags>
{
    private HashSet<string> _activeFlags = new HashSet<string>();
    
    // PlayerPrefs key for saving flags
    private const string SAVE_KEY = "GameFlags_SaveData";

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
                Debug.Log("[GameFlags] Auto-created instance");
            }
            return PersistentSingleton<GameFlags>.Instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        // Try to load saved flags first
        if (!LoadFlags())
        {
            // If no saved data, initialize with defaults
            InitializeDefaultFlags();
        }
        
        // Notify listeners that initialization is complete
        OnInitialized?.Invoke();
        Debug.Log("[GameFlags] Initialization complete - notifying listeners");
    }

    /// <summary>
    /// Initialize default flags that should always exist from game start.
    /// These flags are always set at the beginning of each game session.
    /// </summary>
    private void InitializeDefaultFlags()
    {
        Debug.Log("[GameFlags] Setting default character flags");
        
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
        
        // these three also have a metadata flag called unlockedByDefault as fallback
        _activeFlags.Add("card.slash");
        _activeFlags.Add("card.block");
        _activeFlags.Add("card.heal_potion");

        Debug.Log($"[GameFlags] Default flags initialized ({_activeFlags.Count} total flags)");
    }

    // ========== SAVE/LOAD SYSTEM ==========

    /// <summary>
    /// Save all active flags to PlayerPrefs using JSON serialization.
    /// Call this when you want to persist the current game state.
    /// </summary>
    public static void SaveFlags()
    {
        if (Instance == null)
        {
            Debug.LogError("[GameFlags] Instance is null! Cannot save flags.");
            return;
        }

        Instance.SaveFlagsInternal();
    }

    /// <summary>
    /// Load flags from PlayerPrefs. Returns true if data was loaded, false if no save data exists.
    /// This is automatically called on Awake, but can be called manually to reload.
    /// </summary>
    public static bool LoadFlags()
    {
        if (Instance == null)
        {
            Debug.LogError("[GameFlags] Instance is null! Cannot load flags.");
            return false;
        }

        return Instance.LoadFlagsInternal();
    }

    /// <summary>
    /// Delete all saved flag data from PlayerPrefs.
    /// This does NOT affect the current runtime flags - use ResetToDefaults() for that.
    /// </summary>
    public static void DeleteSavedFlags()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[GameFlags] Saved flag data deleted from PlayerPrefs");
        }
        else
        {
            Debug.Log("[GameFlags] No saved flag data to delete");
        }
    }

    /// <summary>
    /// Check if saved flag data exists in PlayerPrefs
    /// </summary>
    public static bool HasSavedFlags()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    // ========== INTERNAL SAVE/LOAD METHODS ==========

    private void SaveFlagsInternal()
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

            Debug.Log($"[GameFlags] Saved {_activeFlags.Count} flags to PlayerPrefs");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameFlags] Failed to save flags: {ex.Message}");
        }
    }

    private bool LoadFlagsInternal()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("[GameFlags] No saved flag data found");
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
                Debug.LogWarning("[GameFlags] Save data was invalid or empty");
                return false;
            }

            // Clear current flags and load saved ones
            _activeFlags.Clear();
            foreach (string flag in saveData.flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _activeFlags.Add(flag);
                }
            }

            Debug.Log($"[GameFlags] Loaded {_activeFlags.Count} flags from PlayerPrefs");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameFlags] Failed to load flags: {ex.Message}");
            return false;
        }
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
            Debug.LogWarning("[GameFlags] Attempted to set flag with null or empty name");
            return;
        }

        if (Instance == null)
        {
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (!Instance._activeFlags.Contains(flagName))
        {
            Instance._activeFlags.Add(flagName);
            Instance.OnFlagChanged?.Invoke(flagName);
            Debug.Log($"[GameFlags] Set flag: {flagName}");
            
            // AUTO-SAVE for day progression flags
            if (IsDayFlag(flagName))
            {
                Debug.Log($"[GameFlags] Day flag detected - auto-saving game progress");
                SaveFlags();
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
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
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
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (Instance._activeFlags.Remove(flagName))
        {
            Instance.OnFlagChanged?.Invoke(flagName);
            Debug.Log($"[GameFlags] Removed flag: {flagName}");
        }
    }

    /// <summary>
    /// Clear all flags and reinitialize defaults.
    /// </summary>
    public static void ClearAllFlags()
    {
        if (Instance == null)
        {
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        Instance._activeFlags.Clear();
        Debug.Log("[GameFlags] Cleared all flags");
        
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
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
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
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
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
            Debug.LogError("[GameFlags] Instance is null! Make sure GameFlags exists in the scene.");
            return;
        }

        if (Instance._activeFlags.Count == 0)
        {
            Debug.Log("[GameFlags] No flags are currently set");
            return;
        }

        Debug.Log($"[GameFlags] Active flags ({Instance._activeFlags.Count}):");
        foreach (string flag in Instance._activeFlags)
        {
            Debug.Log($"  - {flag}");
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
}

/// <summary>
/// Serializable wrapper class for saving flags to JSON
/// </summary>
[Serializable]
public class GameFlagsSaveData
{
    public List<string> flags = new List<string>();
}
