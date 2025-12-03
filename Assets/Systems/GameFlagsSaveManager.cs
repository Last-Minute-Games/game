using UnityEngine;

/// <summary>
/// Helper component for managing GameFlags save/load operations.
/// Can be attached to UI buttons or used for auto-save functionality.
/// NOTE: Manual saving is disabled - the game auto-saves on day progression.
/// </summary>
public class GameFlagsSaveManager : MonoBehaviour
{
    [Header("Auto-Save Settings")]
    [Tooltip("Automatically save flags when they change")]
    [SerializeField] private bool autoSaveOnFlagChange = false;
    
    [Tooltip("Automatically save flags at regular intervals (0 = disabled)")]
    [SerializeField] private float autoSaveInterval = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool logSaveOperations = true;

    private float _autoSaveTimer = 0f;

    private void Start()
    {
        // Subscribe to flag changes if auto-save is enabled
        if (autoSaveOnFlagChange && GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged += OnFlagChanged;
            if (logSaveOperations)
                Debug.Log("[GameFlagsSaveManager] Auto-save on flag change enabled");
        }

        // Reset auto-save timer
        _autoSaveTimer = autoSaveInterval;
    }

    private void OnDestroy()
    {
        // Unsubscribe from flag changes
        if (autoSaveOnFlagChange && GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged -= OnFlagChanged;
        }
    }

    private void Update()
    {
        // Handle interval-based auto-save
        if (autoSaveInterval > 0f)
        {
            _autoSaveTimer -= Time.deltaTime;
            if (_autoSaveTimer <= 0f)
            {
                SaveFlags();
                _autoSaveTimer = autoSaveInterval;
            }
        }
    }

    private void OnFlagChanged(string flagName)
    {
        if (autoSaveOnFlagChange)
        {
            SaveFlags();
        }
    }

    // ========== PUBLIC METHODS (can be called from UI buttons) ==========
    // NOTE: Manual saving is disabled in the main game - auto-save handles everything

    /// <summary>
    /// Save current flags. Used by auto-save system.
    /// Manual save buttons should be disabled in the UI.
    /// </summary>
    private void SaveFlags()
    {
        GameFlagsManager.SaveCurrentGame();
        if (logSaveOperations)
            Debug.Log("[GameFlagsSaveManager] Flags auto-saved");
    }

    /// <summary>
    /// Load flags from save file. Can be called from UI button.
    /// </summary>
    public void LoadFlags()
    {
        bool success = GameFlagsManager.LoadCurrentGame();
        if (logSaveOperations)
        {
            if (success)
                Debug.Log("[GameFlagsSaveManager] Flags loaded successfully");
            else
                Debug.Log("[GameFlagsSaveManager] No saved flags found or load failed");
        }
    }

    /// <summary>
    /// Delete saved flag data. Can be called from UI button.
    /// </summary>
    public void DeleteSavedFlags()
    {
        GameFlagsManager.DeleteCurrentSave();
        if (logSaveOperations)
            Debug.Log("[GameFlagsSaveManager] Saved flags deleted");
    }

    /// <summary>
    /// Reset flags to defaults (doesn't affect saved data). Can be called from UI button.
    /// </summary>
    public void ResetToDefaults()
    {
        GameFlags.ResetToDefaults();
        if (logSaveOperations)
            Debug.Log("[GameFlagsSaveManager] Flags reset to defaults");
    }

    /// <summary>
    /// Start a new game: reset to defaults and save. Can be called from UI button.
    /// </summary>
    public void StartNewGame()
    {
        GameFlags.ResetToDefaults();
        GameFlagsManager.SaveCurrentGame();
        if (logSaveOperations)
            Debug.Log("[GameFlagsSaveManager] New game started (flags reset and saved)");
    }

    /// <summary>
    /// Continue game: load saved flags. Can be called from UI button.
    /// </summary>
    public void ContinueGame()
    {
        bool success = GameFlagsManager.LoadCurrentGame();
        if (logSaveOperations)
        {
            if (success)
                Debug.Log("[GameFlagsSaveManager] Game continued (flags loaded)");
            else
                Debug.Log("[GameFlagsSaveManager] No save data to continue from");
        }
    }

    /// <summary>
    /// Check if a saved game exists. Useful for showing/hiding "Continue" button.
    /// </summary>
    public bool HasSavedGame()
    {
        return GameFlagsManager.HasCurrentSave();
    }

    /// <summary>
    /// Print all active flags to console for debugging
    /// </summary>
    public void PrintAllFlags()
    {
        GameFlags.PrintAllFlags();
    }
}
