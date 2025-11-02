using UnityEngine;
using TMPro;

/// <summary>
/// Base component for individual journal entries within a tab.
/// Attach this to each entry GameObject in your journal pages.
/// 
/// SIMPLE MODE: Just unlocks/locks the entire entry
/// PROGRESSIVE MODE: Updates text fields as specific flags are set
/// 
/// When locked: entire entry is hidden
/// When unlocked: entire entry is visible (and texts update progressively if configured)
/// </summary>
public class JournalEntry : MonoBehaviour
{
    [System.Serializable]
    public class ProgressiveTextUpdate
    {
        [Tooltip("The flag that triggers this text to appear (e.g., 'character.marco.lied')")]
        public string flag;
        
        [Tooltip("The text field to update when the flag exists")]
        public TMP_Text textField;
        
        [Tooltip("The text to show when the flag exists")]
        [TextArea(3, 10)]
        public string textToShow;
    }
    
    [Header("Entry Configuration")]
    [Tooltip("Unique ID for this entry (e.g., 'character.allistair', 'monster.gargoyle')")]
    public string entryId;
    
    [Header("Progressive Text Updates (Optional)")]
    [Tooltip("Leave empty for simple entries. Add updates to make text change as flags are set.")]
    public ProgressiveTextUpdate[] progressiveUpdates;
    
    private bool isUnlocked = false;
    private bool isProgressiveMode = false;
    
    void Start()
    {
        // Check if this is a progressive entry
        isProgressiveMode = progressiveUpdates != null && progressiveUpdates.Length > 0;
        
        // Subscribe to flag changes if progressive
        if (isProgressiveMode && GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged += OnFlagChanged;
        }
        
        // Initial state is locked (hidden)
        gameObject.SetActive(false);
        
        // If progressive, check initial state
        if (isProgressiveMode)
        {
            RefreshProgressiveTexts();
        }
    }
    
    void OnDestroy()
    {
        if (isProgressiveMode && GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged -= OnFlagChanged;
        }
    }
    
    private void OnFlagChanged(string flag)
    {
        if (isUnlocked && isProgressiveMode)
        {
            RefreshProgressiveTexts();
        }
    }
    
    /// <summary>
    /// Update all progressive text fields based on current flags
    /// </summary>
    private void RefreshProgressiveTexts()
    {
        if (!isProgressiveMode) return;
        
        foreach (var update in progressiveUpdates)
        {
            if (string.IsNullOrEmpty(update.flag)) continue;
            
            // Check if the flag EXISTS
            bool flagExists = GameFlags.HasFlag(update.flag);
            
            if (update.textField != null)
            {
                if (flagExists)
                {
                    // Show and update text
                    update.textField.text = update.textToShow;
                    update.textField.gameObject.SetActive(true);
                }
                else
                {
                    // Hide text field if flag doesn't exist yet
                    update.textField.gameObject.SetActive(false);
                }
            }
        }
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
        
        // If progressive and now unlocked, refresh texts
        if (unlocked && isProgressiveMode)
        {
            RefreshProgressiveTexts();
        }
    }
    
    /// <summary>
    /// Check if this entry is currently unlocked
    /// </summary>
    public bool IsUnlocked => isUnlocked;
}
