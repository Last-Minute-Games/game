using UnityEngine;

/// <summary>
/// Base component for individual journal entries within a tab.
/// Attach this to each entry GameObject in your journal pages.
/// When locked: entire entry is hidden
/// When unlocked: entire entry is visible
/// </summary>
public class JournalEntry : MonoBehaviour
{
    [Header("Entry Configuration")]
    [Tooltip("Unique ID for this entry (e.g., 'character.allistair', 'monster.gargoyle')")]
    public string entryId;
    
    private bool isUnlocked = false;
    
    void Start()
    {
        // Initial state is locked (hidden)
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Set whether this entry is unlocked or not
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        if (isUnlocked == unlocked) return;
        
        isUnlocked = unlocked;
        
        // Simple: just show or hide the entire entry
        gameObject.SetActive(unlocked);
    }
    
    /// <summary>
    /// Check if this entry is currently unlocked
    /// </summary>
    public bool IsUnlocked => isUnlocked;
}
