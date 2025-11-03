using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// Base component for individual journal entries within a tab.
/// Attach this to each entry GameObject in your journal pages.
/// 
/// SIMPLE MODE: Just unlocks/locks the entire entry
/// PROGRESSIVE MODE: Updates a single text field as flags are set, showing the newest flag's text
/// 
/// When locked: entire entry is hidden
/// When unlocked: entire entry is visible (and text updates progressively if configured)
/// </summary>
public class JournalEntry : MonoBehaviour
{
    [System.Serializable]
    public class ProgressiveTextUpdate
    {
        [Tooltip("The flag that triggers this text to appear (e.g., 'character.allistair.lies')")]
        public string flag;
        
        [Tooltip("The text to show when this flag exists (replaces previous text)")]
        [TextArea(3, 10)]
        public string textToShow;
        
        [Tooltip("Priority - higher numbers override lower numbers. Use order in list if 0.")]
        public int priority = 0;
        
        [HideInInspector]
        public int orderIndex; // Set automatically
    }
    
    [Header("Entry Configuration")]
    [Tooltip("Unique ID for this entry (e.g., 'character.allistair', 'monster.gargoyle')")]
    public string entryId;
    
    [Header("Progressive Text Updates (Optional)")]
    [Tooltip("The text field to update (e.g., CharStuff)")]
    public TMP_Text progressiveTextField;
    
    [Tooltip("Text updates - will show the NEWEST/HIGHEST PRIORITY flag's text")]
    public ProgressiveTextUpdate[] progressiveUpdates;
    
    [Header("Default Text")]
    [Tooltip("Text to show if no progressive flags are set")]
    [TextArea(3, 10)]
    public string defaultText = "";
    
    private bool isUnlocked = false;
    private bool isProgressiveMode = false;
    private bool isInitialized = false;
    
    void Awake()
    {
        // Set order indices for priority fallback
        if (progressiveUpdates != null)
        {
            for (int i = 0; i < progressiveUpdates.Length; i++)
            {
                progressiveUpdates[i].orderIndex = i;
            }
        }
    }
    
    void Start()
    {
        // Check if this is a progressive entry
        isProgressiveMode = progressiveUpdates != null && progressiveUpdates.Length > 0 && progressiveTextField != null;
        
        // Subscribe to flag changes if progressive
        if (isProgressiveMode && GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged += OnFlagChanged;
        }
        
        isInitialized = true;
        
        // If progressive and unlocked, refresh text immediately
        if (isProgressiveMode && isUnlocked)
        {
            RefreshProgressiveText();
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
            RefreshProgressiveText();
        }
    }
    
    /// <summary>
    /// Update the text field to show the newest/highest priority active flag's text
    /// </summary>
    private void RefreshProgressiveText()
    {
        if (!isProgressiveMode || progressiveTextField == null) return;
        
        // Find all active flag updates
        var activeUpdates = progressiveUpdates
            .Where(update => !string.IsNullOrEmpty(update.flag) && GameFlags.HasFlag(update.flag))
            .ToList();
        
        if (activeUpdates.Count > 0)
        {
            // Get the highest priority update, or if tied, the one that appears last in the list
            var selectedUpdate = activeUpdates
                .OrderByDescending(u => u.priority)
                .ThenByDescending(u => u.orderIndex)
                .First();
            
            progressiveTextField.text = selectedUpdate.textToShow;
        }
        else
        {
            // No flags set, show default text
            progressiveTextField.text = defaultText;
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
        
        // If progressive and now unlocked, refresh text (only if initialized)
        if (unlocked && isProgressiveMode && isInitialized)
        {
            RefreshProgressiveText();
        }
    }
    
    /// <summary>
    /// Check if this entry is currently unlocked
    /// </summary>
    public bool IsUnlocked => isUnlocked;

#if UNITY_EDITOR
    [ContextMenu("Test: Toggle First Progressive Flag")]
    private void TestToggleFirstFlag()
    {
        if (progressiveUpdates == null || progressiveUpdates.Length == 0)
        {
            Debug.LogWarning("[JournalEntry] No progressive updates configured!");
            return;
        }
        
        string testFlag = progressiveUpdates[0].flag;
        if (string.IsNullOrEmpty(testFlag))
        {
            Debug.LogWarning("[JournalEntry] First progressive update has no flag set!");
            return;
        }
        
        if (GameFlags.HasFlag(testFlag))
        {
            GameFlags.RemoveFlag(testFlag);
            Debug.Log($"[JournalEntry] Test: Removed flag '{testFlag}'");
        }
        else
        {
            GameFlags.SetFlag(testFlag);
            Debug.Log($"[JournalEntry] Test: Set flag '{testFlag}'");
        }
        
        // Manually refresh to see immediate results
        if (isProgressiveMode && isInitialized)
        {
            RefreshProgressiveText();
        }
    }
    
    [ContextMenu("Test: Refresh Progressive Text Now")]
    private void TestRefreshText()
    {
        if (!isProgressiveMode)
        {
            Debug.LogWarning("[JournalEntry] Not in progressive mode!");
            return;
        }
        
        RefreshProgressiveText();
        Debug.Log($"[JournalEntry] Refreshed progressive text for '{entryId}'");
    }
#endif
}
