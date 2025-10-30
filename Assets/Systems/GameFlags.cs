using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple game flags system for tracking boolean game states.
/// Flags are persisted using PlayerPrefs and automatically loaded on first access.
/// </summary>
public class GameFlags : PersistentSingleton<GameFlags>
{
    private HashSet<string> _activeFlags = new HashSet<string>();
    private bool _isLoaded = false;
    private const string SAVE_KEY = "GameFlags_Data";

    /// <summary>
    /// Event fired when a flag value changes
    /// </summary>
    public event Action<string, bool> OnFlagChanged;

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
    /// Set a flag to the specified value. Automatically saves to PlayerPrefs.
    /// </summary>
    /// <param name="flagName">The name of the flag to set</param>
    /// <param name="value">The value to set (true or false)</param>
    public static void SetFlag(string flagName, bool value = true)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning("[GameFlags] Attempted to set flag with null or empty name");
            return;
        }

        bool previousValue = Instance._activeFlags.Contains(flagName);
        bool changed = false;

        if (value)
        {
            if (!Instance._activeFlags.Contains(flagName))
            {
                Instance._activeFlags.Add(flagName);
                changed = true;
                Debug.Log($"[GameFlags] Set flag: {flagName}");
            }
        }
        else
        {
            if (Instance._activeFlags.Remove(flagName))
            {
                changed = true;
                Debug.Log($"[GameFlags] Cleared flag: {flagName}");
            }
        }

        if (changed)
        {
            Instance.SaveFlags();
            Instance.OnFlagChanged?.Invoke(flagName, value);
        }
    }

    /// <summary>
    /// Check if a flag exists (is set to true)
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
    public static void ClearFlag(string flagName)
    {
        SetFlag(flagName, false);
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
    /// Instance method: Get the value of a flag
    /// </summary>
    public bool Get(string flagName)
    {
        return HasFlag(flagName);
    }

    /// <summary>
    /// Instance method: Set a flag value
    /// </summary>
    public void Set(string flagName, bool value)
    {
        SetFlag(flagName, value);
    }
}
