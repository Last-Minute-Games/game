using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple game flags system for tracking game states.
/// Flags either EXIST or DON'T EXIST - no true/false confusion.
/// Flags are persisted using PlayerPrefs and automatically loaded on first access.
/// </summary>
public class GameFlags : PersistentSingleton<GameFlags>
{
    private HashSet<string> _activeFlags = new HashSet<string>();
    private bool _isLoaded = false;
    private const string SAVE_KEY = "GameFlags_Data";

    /// <summary>
    /// Event fired when a flag is set (added) or removed
    /// </summary>
    public event Action<string> OnFlagChanged;

    protected override void Awake()
    {
        base.Awake();
        LoadFlags();
    }

    /// <summary>
    /// Load all flags from PlayerPrefs
    /// </summary>
    private void LoadFlags()
    {
        if (_isLoaded) return;

        _activeFlags.Clear();
        string savedData = PlayerPrefs.GetString(SAVE_KEY, "");
        
        if (!string.IsNullOrEmpty(savedData))
        {
            string[] flags = savedData.Split('|');
            foreach (string flag in flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _activeFlags.Add(flag);
                }
            }
        }

        _isLoaded = true;
        Debug.Log($"[GameFlags] Loaded {_activeFlags.Count} flags");
    }

    /// <summary>
    /// Save all flags to PlayerPrefs
    /// </summary>
    private void SaveFlags()
    {
        string data = string.Join("|", _activeFlags);
        PlayerPrefs.SetString(SAVE_KEY, data);
        PlayerPrefs.Save();
    }

    // ========== STATIC API (Singleton Pattern) ==========

    /// <summary>
    /// Set a flag (adds it to the active flags). Automatically saves to PlayerPrefs.
    /// </summary>
    /// <param name="flagName">The name of the flag to set</param>
    public static void SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning("[GameFlags] Attempted to set flag with null or empty name");
            return;
        }

        if (!Instance._activeFlags.Contains(flagName))
        {
            Instance._activeFlags.Add(flagName);
            Instance.SaveFlags();
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

        return Instance._activeFlags.Contains(flagName);
    }

    /// <summary>
    /// Remove a specific flag. Automatically saves to PlayerPrefs.
    /// </summary>
    /// <param name="flagName">The name of the flag to remove</param>
    public static void RemoveFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
            return;

        if (Instance._activeFlags.Remove(flagName))
        {
            Instance.SaveFlags();
            Instance.OnFlagChanged?.Invoke(flagName);
            Debug.Log($"[GameFlags] Removed flag: {flagName}");
        }
    }

    /// <summary>
    /// Clear all flags. Automatically saves to PlayerPrefs.
    /// </summary>
    public static void ClearAllFlags()
    {
        Instance._activeFlags.Clear();
        Instance.SaveFlags();
        Debug.Log("[GameFlags] Cleared all flags");
    }

    /// <summary>
    /// Get the count of active flags
    /// </summary>
    public static int GetFlagCount()
    {
        return Instance._activeFlags.Count;
    }

    /// <summary>
    /// Get all active flag names
    /// </summary>
    public static HashSet<string> GetAllFlags()
    {
        return new HashSet<string>(Instance._activeFlags);
    }

    /// <summary>
    /// Debug: Print all active flags to the console
    /// </summary>
    public static void PrintAllFlags()
    {
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

    // ========== DEPRECATED (for backwards compatibility) =========
    // These are marked obsolete to guide developers to the new API

    /// <summary>
    /// DEPRECATED: Use HasFlag(flagName) instead.
    /// This method exists for backwards compatibility only.
    /// </summary>
    [System.Obsolete("Use HasFlag(flagName) instead. Flags no longer use true/false values.")]
    public bool Get(string flagName)
    {
        return HasFlag(flagName);
    }

    /// <summary>
    /// DEPRECATED: Use SetFlag(flagName) or RemoveFlag(flagName) instead.
    /// This method exists for backwards compatibility only.
    /// </summary>
    [System.Obsolete("Use SetFlag(flagName) to add a flag, or RemoveFlag(flagName) to remove it. Flags no longer use true/false values.")]
    public void Set(string flagName, bool value)
    {
        if (value)
            SetFlag(flagName);
        else
            RemoveFlag(flagName);
    }

    /// <summary>
    /// DEPRECATED: Use SetFlag(flagName) or RemoveFlag(flagName) instead.
    /// </summary>
    [System.Obsolete("Use SetFlag(flagName) to add a flag, or RemoveFlag(flagName) to remove it.")]
    public static void SetFlag(string flagName, bool value)
    {
        if (value)
            SetFlag(flagName);
        else
            RemoveFlag(flagName);
    }

    /// <summary>
    /// DEPRECATED: Use RemoveFlag(flagName) instead.
    /// </summary>
    [System.Obsolete("Use RemoveFlag(flagName) instead.")]
    public static void ClearFlag(string flagName)
    {
        RemoveFlag(flagName);
    }
}
