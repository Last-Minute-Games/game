using UnityEngine;

/// <summary>
/// Automatically maps character flags to journal entries.
/// Add this to your scene to auto-configure journal mappings.
/// </summary>
public class JournalAutoMapper : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The JournalManager ScriptableObject asset")]
    public JournalManager journalManager;
    
    [Header("Auto-Mapping")]
    [Tooltip("Automatically create mappings for character flags on Start")]
    public bool autoMapOnStart = true;
    
    private void Start()
    {
        if (autoMapOnStart && journalManager != null)
        {
            AutoMapCharacterFlags();
        }
    }
    
    /// <summary>
    /// Automatically unlock all character entries based on default flags.
    /// This simulates what the mappings should do.
    /// </summary>
    private void AutoMapCharacterFlags()
    {
        Debug.Log("[JournalAutoMapper] Auto-mapping character flags to entries...");
        
        // Map each character flag to its entry
        string[] characters = new string[]
        {
            "marco",
            "allistair",
            "adrianne",
            "avant",
            "charles",
            "sebastian",
            "elias"
        };
        
        foreach (string character in characters)
        {
            string flag = $"character.{character}";
            string entryId = $"character.{character}";
            
            if (GameFlags.HasFlag(flag))
            {
                journalManager.AddEntry(entryId);
                Debug.Log($"[JournalAutoMapper] Mapped {flag} ? {entryId}");
            }
        }
        
        Debug.Log("[JournalAutoMapper] Auto-mapping complete!");
    }
    
#if UNITY_EDITOR
    [ContextMenu("Force Auto-Map Now")]
    private void ForceAutoMap()
    {
        if (journalManager != null)
        {
            AutoMapCharacterFlags();
        }
        else
        {
            Debug.LogWarning("[JournalAutoMapper] No JournalManager assigned!");
        }
    }
    
    [ContextMenu("Print Current Flags")]
    private void PrintFlags()
    {
        GameFlags.PrintAllFlags();
    }
#endif
}
