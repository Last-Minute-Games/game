using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple test script to manually unlock journal entries during development.
/// Attach this to a UI button or GameObject in your scene.
/// </summary>
public class JournalTestUnlocker : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("The flag to set when testing")]
    public string testFlag = "defeated_gargoyle";
    
    [Header("Optional: Direct Entry Unlock")]
    [Tooltip("Or unlock a specific entry directly (bypasses flags)")]
    public string testEntryId = "monster.gargoyle";
    
    [Tooltip("Reference to JournalManager")]
    public JournalManager journalManager;
    
    void Start()
    {
        // Auto-hook to button if this is on a Button GameObject
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(TestUnlock);
            Debug.Log("[JournalTestUnlocker] Hooked to button. Click to unlock!");
        }
    }
    
    void Update()
    {
        // Press T key to test unlock
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestUnlock();
        }
    }
    
    /// <summary>
    /// Call this from anywhere to test unlocking
    /// </summary>
    [ContextMenu("Test Unlock Now")]
    public void TestUnlock()
    {
        // Method 1: Use flag system
        if (!string.IsNullOrEmpty(testFlag))
        {
            GameFlags.SetFlag(testFlag);
            Debug.Log($"[TEST] Set flag: {testFlag}");
        }
        
        // Method 2: Direct unlock
        if (!string.IsNullOrEmpty(testEntryId) && journalManager != null)
        {
            journalManager.AddEntry(testEntryId);
            Debug.Log($"[TEST] Direct unlock: {testEntryId}");
        }
    }
    
    /// <summary>
    /// Test unlock for Allistair character
    /// </summary>
    [ContextMenu("Unlock Allistair")]
    public void UnlockAllistair()
    {
        GameFlags.SetFlag("met_allistair");
        Debug.Log("[TEST] Unlocked Allistair!");
    }
    
    /// <summary>
    /// Test unlock for Gargoyle monster
    /// </summary>
    [ContextMenu("Unlock Gargoyle")]
    public void UnlockGargoyle()
    {
        GameFlags.SetFlag("defeated_gargoyle");
        Debug.Log("[TEST] Unlocked Gargoyle!");
    }
    
    /// <summary>
    /// Unlock all test entries at once
    /// </summary>
    [ContextMenu("Unlock All Test Entries")]
    public void UnlockAllTestEntries()
    {
        GameFlags.SetFlag("met_allistair");
        GameFlags.SetFlag("defeated_gargoyle");
        GameFlags.SetFlag("found_blade");
        Debug.Log("[TEST] Unlocked all test entries!");
    }
}
