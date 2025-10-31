using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Base component for individual journal entries within a tab.
/// Attach this to each entry GameObject in your journal pages.
/// </summary>
public class JournalEntry : MonoBehaviour
{
    [Header("Entry Configuration")]
    [Tooltip("Unique ID for this entry (e.g., 'character.knight', 'evidence.blade')")]
    public string entryId;
    
    [Header("UI Elements (Optional)")]
    [Tooltip("GameObject to show when locked (e.g., '???')")]
    public GameObject lockedView;
    
    [Tooltip("GameObject to show when unlocked (the actual content)")]
    public GameObject unlockedView;
    
    [Tooltip("Optional: Text that shows when locked")]
    public TMP_Text lockedText;
    
    private bool isUnlocked = false;
    
    void Start()
    {
        // Initial state is locked
        UpdateVisuals(false);
    }
    
    /// <summary>
    /// Set whether this entry is unlocked or not
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        if (isUnlocked == unlocked) return;
        
        isUnlocked = unlocked;
        UpdateVisuals(unlocked);
    }
    
    /// <summary>
    /// Check if this entry is currently unlocked
    /// </summary>
    public bool IsUnlocked => isUnlocked;
    
    private void UpdateVisuals(bool unlocked)
    {
        if (lockedView != null)
            lockedView.SetActive(!unlocked);
        
        if (unlockedView != null)
            unlockedView.SetActive(unlocked);
        
        if (lockedText != null && !unlocked)
            lockedText.text = "???";
    }
}
