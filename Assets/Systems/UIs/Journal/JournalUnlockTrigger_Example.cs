using UnityEngine;

/// <summary>
/// EXAMPLE: This script shows how to unlock journal entries from various game events.
/// Attach this to NPCs, items, or triggers to automatically unlock journal entries.
/// </summary>
public class JournalUnlockTrigger_Example : MonoBehaviour
{
    [Header("Unlock Configuration")]
    [Tooltip("The flag to set when this is triggered")]
    public string flagToSet = "met_npc";
    
    [Tooltip("Optional: Direct entry ID to unlock (bypasses flag system)")]
    public string directEntryId = "";
    
    [Tooltip("Optional: Reference to JournalManager for direct unlock")]
    public JournalManager journalManager;
    
    [Header("Trigger Settings")]
    [Tooltip("Only trigger once?")]
    public bool triggerOnce = true;
    
    [Tooltip("Require player tag?")]
    public bool requirePlayerTag = true;
    
    private bool hasTriggered = false;
    
    // ==========================================
    // EXAMPLE 1: Trigger on Collision
    // ==========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce) return;
        
        if (requirePlayerTag && !other.CompareTag("Player")) return;
        
        UnlockJournalEntry();
    }
    
    // ==========================================
    // EXAMPLE 2: Call from Button/Event
    // ==========================================
    public void OnInteract()
    {
        if (hasTriggered && triggerOnce) return;
        UnlockJournalEntry();
    }
    
    // ==========================================
    // EXAMPLE 3: Unlock on Start (for testing)
    // ==========================================
    [ContextMenu("Test Unlock Now")]
    private void TestUnlock()
    {
        UnlockJournalEntry();
    }
    
    // ==========================================
    // Core Unlock Logic
    // ==========================================
    private void UnlockJournalEntry()
    {
        // Method 1: Use the flag system (recommended)
        if (!string.IsNullOrEmpty(flagToSet))
        {
            GameFlags.SetFlag(flagToSet, true);
            Debug.Log($"[JournalUnlock] Set flag: {flagToSet}");
        }
        
        // Method 2: Direct unlock (bypasses flags)
        if (!string.IsNullOrEmpty(directEntryId) && journalManager != null)
        {
            journalManager.AddEntry(directEntryId);
            Debug.Log($"[JournalUnlock] Direct unlock: {directEntryId}");
        }
        
        hasTriggered = true;
    }
}

// ==========================================
// MORE EXAMPLES
// ==========================================

#region Example Usage in Other Scripts

/*
// EXAMPLE A: Unlock from Dialogue System
public class DialogueNPC : MonoBehaviour
{
    public void OnDialogueComplete()
    {
        // Unlocks journal entry mapped to this flag
        GameFlags.SetFlag("talked_to_knight", true);
    }
}

// EXAMPLE B: Unlock from Combat Victory
public class BossEnemy : Enemy
{
    protected override void Die()
    {
        base.Die();
        // Unlocks journal entry for this boss
        GameFlags.SetFlag("defeated_dragon", true);
    }
}

// EXAMPLE C: Unlock from Item Pickup
public class ItemPickup : MonoBehaviour
{
    public string itemFlagName = "found_sword";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameFlags.SetFlag(itemFlagName, true);
            Destroy(gameObject);
        }
    }
}

// EXAMPLE D: Unlock Multiple Entries at Once
public class QuestComplete : MonoBehaviour
{
    public void OnQuestFinished()
    {
        // Unlock multiple related entries
        GameFlags.SetFlag("quest_forest_complete", true);
        GameFlags.SetFlag("met_forest_spirit", true);
        GameFlags.SetFlag("found_ancient_key", true);
    }
}

// EXAMPLE E: Unlock from Scene Transition
public class SceneController : MonoBehaviour
{
    private void OnSceneLoaded()
    {
        // Unlock tutorial entry when reaching a new area
        if (SceneManager.GetActiveScene().name == "Town")
        {
            GameFlags.SetFlag("reached_town", true);
        }
    }
}
*/

#endregion
