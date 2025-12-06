using UnityEngine;

/// <summary>
/// Manager class to track the current active save name and provide centralized save operations.
/// This ensures all GameFlags save/load operations use the same save slot.
/// </summary>
public static class GameFlagsManager
{
    private const string CURRENT_SAVE_KEY = "GameFlags_CurrentSave";
    private static string _currentSaveName = null;
    
    /// <summary>
    /// Get the current active save name. Returns "default" if none is set.
    /// </summary>
    public static string GetCurrentSaveName()
    {
        if (_currentSaveName == null)
        {
            _currentSaveName = PlayerPrefs.GetString(CURRENT_SAVE_KEY, "default");
        }
        return _currentSaveName;
    }
    
    /// <summary>
    /// Set the current active save name and persist it
    /// </summary>
    public static void SetCurrentSaveName(string saveName)
    {
        _currentSaveName = saveName;
        PlayerPrefs.SetString(CURRENT_SAVE_KEY, saveName);
        PlayerPrefs.Save();
        Debug.Log($"[GameFlagsManager] Active save set to: {saveName}");
    }
    
    /// <summary>
    /// Clear the current save name (useful for logout or reset)
    /// </summary>
    public static void ClearCurrentSaveName()
    {
        _currentSaveName = null;
        PlayerPrefs.DeleteKey(CURRENT_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[GameFlagsManager] Current save name cleared");
    }
    
    /// <summary>
    /// Save flags to the current active save slot
    /// </summary>
    public static bool SaveCurrentGame()
    {
        string saveName = GetCurrentSaveName();
        bool success = GameFlags.SaveToFile(saveName);
        
        if (success)
        {
            Debug.Log($"[GameFlagsManager] Game saved to: {saveName}");
            SaveGameEvents.OnSaveCreated?.Invoke(saveName);
        }
        else
        {
            Debug.LogError($"[GameFlagsManager] Failed to save game to: {saveName}");
        }
        
        return success;
    }
    
    /// <summary>
    /// Load flags from the current active save slot
    /// </summary>
    public static bool LoadCurrentGame()
    {
        string saveName = GetCurrentSaveName();
        bool success = GameFlags.LoadFromFile(saveName);
        
        if (success)
        {
            Debug.Log($"[GameFlagsManager] Game loaded from: {saveName}");
            SaveGameEvents.OnSaveLoaded?.Invoke(saveName);
        }
        else
        {
            Debug.LogError($"[GameFlagsManager] Failed to load game from: {saveName}");
        }
        
        return success;
    }
    
    /// <summary>
    /// Check if the current save slot has a save file
    /// </summary>
    public static bool HasCurrentSave()
    {
        string saveName = GetCurrentSaveName();
        return GameFlags.HasSaveFile(saveName);
    }
    
    /// <summary>
    /// Delete the current save file
    /// </summary>
    public static bool DeleteCurrentSave()
    {
        string saveName = GetCurrentSaveName();
        bool success = GameFlags.DeleteSaveFile(saveName);
        
        if (success)
        {
            Debug.Log($"[GameFlagsManager] Save deleted: {saveName}");
            SaveGameEvents.OnSaveDeleted?.Invoke(saveName);
            ClearCurrentSaveName();
        }
        
        return success;
    }
    
    /// <summary>
    /// Create a new save with the given name and set it as active
    /// </summary>
    public static bool CreateNewSave(string saveName)
    {
        // Check if save already exists
        if (GameFlags.HasSaveFile(saveName))
        {
            Debug.LogWarning($"[GameFlagsManager] Save already exists: {saveName}");
            return false;
        }
        
        // Set as active save
        SetCurrentSaveName(saveName);
        
        // Reset to defaults
        GameFlags.ResetToDefaults();
        
        // Save the defaults
        bool success = SaveCurrentGame();
        
        if (success)
        {
            Debug.Log($"[GameFlagsManager] New save created: {saveName}");
            SaveGameEvents.OnSaveCreated?.Invoke(saveName);
        }
        
        return success;
    }
}
