using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Journal Manager", fileName = "JournalManager")]
public class JournalManager : ScriptableObject
{
    [Serializable]
    public struct Mapping
    {
        [Tooltip("When this flag is set...")]
        public string flag;

        [Tooltip("...unlock this journal entry id.")]
        public string entryId;
    }

    [Header("Journal Settings")]
    [Tooltip("Leave empty to use 1:1 flag-to-entry mapping (flag name = entry ID)")]
    [SerializeField] private List<Mapping> mappings = new();
    
    [Header("Auto-Mapping")]
    [Tooltip("If true and mappings is empty, flags automatically unlock matching entry IDs")]
    [SerializeField] private bool useAutoMapping = true;

    private readonly HashSet<string> unlockedEntries = new();

    public event Action<string> OnEntryUnlocked;

    private GameFlags currentFlags;
    private bool isInitialized = false;
    private bool hasCheckedInitialFlags = false;

    // Don't use OnEnable for ScriptableObjects - it's too early!
    // Initialization should be done by a MonoBehaviour in the scene
    
    private void OnEnable()
    {
        // Reset initialization state when entering play mode in the editor
        // ScriptableObjects persist their state, so we need to clear it
#if UNITY_EDITOR
        // Reset when entering play mode or when asset is loaded in editor
        isInitialized = false;
        hasCheckedInitialFlags = false;
        currentFlags = null;
        unlockedEntries.Clear();
        Debug.Log("[Journal] OnEnable - Reset state for new play session");
#endif
    }
    
    private void OnDisable()
    {
        if (currentFlags != null)
        {
            currentFlags.OnFlagChanged -= HandleFlagChanged;
            currentFlags.OnInitialized -= OnGameFlagsInitialized;
        }
    }
    
    private void OnGameFlagsInitialized()
    {
        if (hasCheckedInitialFlags) return;
        hasCheckedInitialFlags = true;
        
        Debug.Log("[Journal] GameFlags initialization complete - checking existing flags");
        CheckExistingFlags();
    }

    /// <summary>
    /// Initialize and hook up to GameFlags. Call this at game start from a MonoBehaviour.
    /// </summary>
    public void Initialize()
    {
        // Allow re-initialization if we never successfully hooked to GameFlags
        if (isInitialized && currentFlags != null)
        {
            Debug.Log("[Journal] Already initialized and hooked, skipping");
            return;
        }
        
        Debug.Log($"[Journal] Initialize() called on {name}");
        
        if (GameFlags.Instance != null)
        {
            Debug.Log($"[Journal] GameFlags.Instance found, hooking...");
            Hook(GameFlags.Instance);
            isInitialized = true;
        }
        else
        {
            Debug.LogError("[Journal] GameFlags singleton not found during initialization! This shouldn't happen.");
            isInitialized = false; // Allow retry
        }
        
        Debug.Log($"[Journal] Initialization complete. isInitialized={isInitialized}, hooked={currentFlags != null}");
    }

    public void Hook(GameFlags flags)
    {
        if (flags == null)
        {
            Debug.LogWarning("[Journal] Tried to hook with null GameFlags.");
            return;
        }

        if (currentFlags != null)
        {
            Debug.Log("[Journal] Unhooking from previous GameFlags instance");
            currentFlags.OnFlagChanged -= HandleFlagChanged;
            currentFlags.OnInitialized -= OnGameFlagsInitialized;
        }

        currentFlags = flags;
        currentFlags.OnFlagChanged += HandleFlagChanged;
        currentFlags.OnInitialized += OnGameFlagsInitialized;

        Debug.Log($"[Journal] Hooked into GameFlags. Current flags count: {GameFlags.GetFlagCount()}");
        
        // IMMEDIATE CHECK: Always check flags when hooking
        // GameFlags Awake() runs before this, so flags are already loaded
        if (!hasCheckedInitialFlags)
        {
            hasCheckedInitialFlags = true;
            Debug.Log("[Journal] Immediately checking existing flags after hook");
            CheckExistingFlags();
        }
        else
        {
            Debug.Log("[Journal] Already checked initial flags, skipping");
        }
    }

    private void CheckExistingFlags()
    {
        if (currentFlags == null) return;
        
        Debug.Log($"[Journal] CheckExistingFlags called - useAutoMapping: {useAutoMapping}, mappings.Count: {mappings.Count}");
        
        // If using custom mappings, process them
        if (mappings.Count > 0)
        {
            foreach (var m in mappings)
            {
                if (string.IsNullOrWhiteSpace(m.flag)) continue;
                
                if (GameFlags.HasFlag(m.flag))
                {
                    AddEntry(m.entryId);
                }
            }
        }
        // Otherwise, use auto-mapping: flag name = entry ID
        else if (useAutoMapping)
        {
            var allFlags = GameFlags.GetAllFlags();
            Debug.Log($"[Journal] Auto-mapping {allFlags.Count} flags to entries");
            
            foreach (string flag in allFlags)
            {
                // Automatically unlock entry with matching ID
                AddEntry(flag);
            }
            Debug.Log($"[Journal] Auto-mapped {allFlags.Count} flags to entries");
        }
    }

    private void HandleFlagChanged(string flag)
    {
        // If using custom mappings, check them
        if (mappings.Count > 0)
        {
            foreach (var m in mappings)
            {
                if (!string.Equals(m.flag, flag, StringComparison.OrdinalIgnoreCase))
                    continue;

                AddEntry(m.entryId);
                return;
            }
        }
        // Otherwise, use auto-mapping: flag name = entry ID
        else if (useAutoMapping)
        {
            AddEntry(flag);
        }
    }

    public void AddEntry(string entryId)
    {
        if (string.IsNullOrWhiteSpace(entryId))
            return;

        if (unlockedEntries.Contains(entryId))
            return; // Already unlocked

        unlockedEntries.Add(entryId);
        Debug.Log($"[Journal] Unlocked entry: {entryId}");
        OnEntryUnlocked?.Invoke(entryId);
    }

    /// <summary>
    /// Check if a specific entry is unlocked
    /// </summary>
    public bool IsEntryUnlocked(string entryId)
    {
        return unlockedEntries.Contains(entryId);
    }

    /// <summary>
    /// Get all unlocked entry IDs
    /// </summary>
    public HashSet<string> GetUnlockedEntries()
    {
        return new HashSet<string>(unlockedEntries);
    }

    /// <summary>
    /// Clear all unlocked entries (useful for testing or new game)
    /// </summary>
    public void ClearAllEntries()
    {
        unlockedEntries.Clear();
        Debug.Log("[Journal] Cleared all unlocked entries");
    }

#if UNITY_EDITOR
    [ContextMenu("Test Unlock 'evidence.knife'")]
    private void TestUnlock()
    {
        AddEntry("evidence.knife");
    }
    
    [ContextMenu("Test Unlock 'character.knight'")]
    private void TestUnlockCharacter()
    {
        AddEntry("character.knight");
    }
#endif
}
