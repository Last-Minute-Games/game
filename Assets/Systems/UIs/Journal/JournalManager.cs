using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Journal Manager", fileName = "JournalManager")]
public class JournalManager : ScriptableObject
{
    [Serializable]
    public struct Mapping
    {
        [Tooltip("When this flag changes...")]
        public string flag;

        [Tooltip("...unlock this journal entry id.")]
        public string entryId;

        [Tooltip("Only when flag becomes TRUE? If false, unlock on any change.")]
        public bool onlyWhenTrue;
    }

    [Header("Journal Settings")]
    [SerializeField] private List<Mapping> mappings = new();

    private readonly HashSet<string> unlockedEntries = new();

    public event Action<string> OnEntryUnlocked;

    private GameFlags currentFlags;
    private bool isInitialized = false;

    private void OnEnable()
    {
        // Auto-hook when this ScriptableObject is loaded
        // (usually when the game starts or scene loads)
        if (GameFlags.Instance != null)
        {
            Hook(GameFlags.Instance);
        }
    }

    /// <summary>
    /// Initialize and hook up to GameFlags. Call this at game start.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        if (GameFlags.Instance != null)
        {
            Hook(GameFlags.Instance);
        }
        else
        {
            Debug.LogWarning("[Journal] GameFlags singleton not found during initialization.");
        }
        
        isInitialized = true;
    }

    public void Hook(GameFlags flags)
    {
        if (flags == null)
        {
            Debug.LogWarning("[Journal] Tried to hook with null GameFlags.");
            return;
        }

        if (currentFlags != null)
            currentFlags.OnFlagChanged -= HandleFlagChanged;

        currentFlags = flags;
        currentFlags.OnFlagChanged += HandleFlagChanged;

        Debug.Log("[Journal] Hooked into GameFlags.");
        
        // Check all existing flags to see if any entries should be unlocked
        CheckExistingFlags();
    }

    private void CheckExistingFlags()
    {
        if (currentFlags == null) return;
        
        foreach (var m in mappings)
        {
            if (string.IsNullOrWhiteSpace(m.flag)) continue;
            
            bool flagValue = currentFlags.Get(m.flag);
            
            if (m.onlyWhenTrue && flagValue)
            {
                AddEntry(m.entryId);
            }
            else if (!m.onlyWhenTrue && flagValue)
            {
                AddEntry(m.entryId);
            }
        }
    }

    private void HandleFlagChanged(string flag, bool value)
    {
        foreach (var m in mappings)
        {
            if (!string.Equals(m.flag, flag, StringComparison.OrdinalIgnoreCase))
                continue;

            if (m.onlyWhenTrue && !value)
                continue;

            AddEntry(m.entryId);
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
    [ContextMenu("Test Unlock 'found.blade'")]
    private void TestUnlock()
    {
        AddEntry("found.blade");
    }
    
    [ContextMenu("Test Unlock 'character.knight'")]
    private void TestUnlockCharacter()
    {
        AddEntry("character.knight");
    }
#endif
}
