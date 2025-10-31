using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a single journal page/tab and manages which entries are visible.
/// Attach this to each page GameObject (Characters, Evidence, etc.)
/// </summary>
public class JournalPageController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Reference to the JournalManager ScriptableObject")]
    public JournalManager journalManager;
    
    [Header("Auto-Detection")]
    [Tooltip("Automatically find all JournalEntry components in children")]
    public bool autoDetectEntries = true;
    
    [Tooltip("Or manually assign entries")]
    public List<JournalEntry> entries = new List<JournalEntry>();
    
    private void OnEnable()
    {
        // Refresh whenever this page becomes active
        RefreshEntries();
    }
    
    private void Start()
    {
        // Auto-detect entries if enabled
        if (autoDetectEntries)
        {
            entries.Clear();
            entries.AddRange(GetComponentsInChildren<JournalEntry>(true));
            Debug.Log($"[JournalPage] Auto-detected {entries.Count} entries in {gameObject.name}");
        }
        
        // Subscribe to unlock events
        if (journalManager != null)
        {
            journalManager.OnEntryUnlocked += OnEntryUnlockedHandler;
        }
        else
        {
            Debug.LogWarning($"[JournalPage] {gameObject.name}: No JournalManager assigned!");
        }
        
        // Initial refresh
        RefreshEntries();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (journalManager != null)
        {
            journalManager.OnEntryUnlocked -= OnEntryUnlockedHandler;
        }
    }
    
    private void OnEntryUnlockedHandler(string entryId)
    {
        // When any entry is unlocked, refresh this page
        RefreshEntries();
    }
    
    /// <summary>
    /// Refresh all entries on this page based on unlock status
    /// </summary>
    public void RefreshEntries()
    {
        if (journalManager == null)
        {
            Debug.LogWarning($"[JournalPage] Cannot refresh {gameObject.name}: No JournalManager assigned");
            return;
        }
        
        foreach (var entry in entries)
        {
            if (entry == null) continue;
            
            bool isUnlocked = journalManager.IsEntryUnlocked(entry.entryId);
            entry.SetUnlocked(isUnlocked);
        }
        
        Debug.Log($"[JournalPage] Refreshed {entries.Count} entries in {gameObject.name}");
    }
}
