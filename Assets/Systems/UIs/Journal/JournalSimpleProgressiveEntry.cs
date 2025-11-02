using UnityEngine;
using TMPro;

/// <summary>
/// Simple progressive journal entry that updates text fields as flags are set.
/// Checks if flags EXIST (have been set at least once).
/// </summary>
public class JournalSimpleProgressiveEntry : MonoBehaviour
{
    [System.Serializable]
    public class TextUpdate
    {
        [Tooltip("The flag that triggers this text to appear")]
        public string flag;
        
        [Tooltip("The text field to update")]
        public TMP_Text textField;
        
        [Tooltip("The text to show when the flag exists")]
        [TextArea(3, 10)]
        public string textToShow;
    }
    
    [Header("Entry Configuration")]
    [Tooltip("Unique ID for this entry (e.g., 'character.allistair')")]
    public string entryId;
    
    [Header("Text Updates")]
    [Tooltip("Text fields that get updated when flags exist")]
    public TextUpdate[] textUpdates;
    
    private bool isEntryUnlocked = false;
    
    void Start()
    {
        // Initial state: hidden
        gameObject.SetActive(false);
        
        // Subscribe to flag changes
        if (GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged += OnFlagChanged;
        }
        
        // Check initial state
        RefreshTexts();
    }
    
    void OnDestroy()
    {
        if (GameFlags.Instance != null)
        {
            GameFlags.Instance.OnFlagChanged -= OnFlagChanged;
        }
    }
    
    private void OnFlagChanged(string flag)
    {
        RefreshTexts();
    }
    
    /// <summary>
    /// Check all flags and update text fields
    /// </summary>
    public void RefreshTexts()
    {
        if (GameFlags.Instance == null) return;
        
        bool anyFlagExists = false;
        
        foreach (var update in textUpdates)
        {
            if (string.IsNullOrEmpty(update.flag)) continue;
            
            // Check if the flag EXISTS using HasFlag
            bool flagExists = GameFlags.HasFlag(update.flag);
            
            if (flagExists)
            {
                anyFlagExists = true;
                
                // Update the text field
                if (update.textField != null)
                {
                    update.textField.text = update.textToShow;
                    update.textField.gameObject.SetActive(true);
                }
            }
            else
            {
                // Hide the text field if flag doesn't exist yet
                if (update.textField != null)
                {
                    update.textField.gameObject.SetActive(false);
                }
            }
        }
        
        // Show the entire entry if ANY flag exists
        if (anyFlagExists && !isEntryUnlocked)
        {
            isEntryUnlocked = true;
            gameObject.SetActive(true);
            Debug.Log($"[SimpleProgressiveEntry] {entryId}: Entry now visible");
        }
    }
    
    /// <summary>
    /// Set whether this entry is unlocked (for compatibility with pagination)
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isEntryUnlocked = unlocked;
        gameObject.SetActive(unlocked);
        
        if (unlocked)
        {
            RefreshTexts();
        }
    }
    
    /// <summary>
    /// Check if this entry is currently unlocked
    /// </summary>
    public bool IsUnlocked => isEntryUnlocked;
}
