using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple game flags system for tracking game states.
/// Flags either EXIST or DON'T EXIST - no true/false confusion.
/// Flags persist across scenes during runtime but reset when the game restarts.
/// </summary>
public class GameFlags : PersistentSingleton<GameFlags>
{
    private HashSet<string> _activeFlags = new HashSet<string>();

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
        InitializeDefaultFlags();
        
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

    // ========== STATIC API (Singleton Pattern) ==========

    /// <summary>
    /// Set a flag (adds it to the active flags). Persists across scenes but not between game sessions.
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
        }
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
