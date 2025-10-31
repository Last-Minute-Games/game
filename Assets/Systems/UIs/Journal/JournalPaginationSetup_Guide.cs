using UnityEngine;

/// <summary>
/// SETUP GUIDE: How to use the Journal Pagination System
/// 
/// This is a reference guide - you don't need to attach this script anywhere.
/// Follow the steps below to set up pagination in your journal.
/// </summary>
public class JournalPaginationSetup_Guide : MonoBehaviour
{
    /*
     * ========================================
     * STEP 1: UNLOCK ENTRIES FROM ANYWHERE
     * ========================================
     * 
     * You DON'T need JournalUnlockTrigger_Example.
     * Just call GameFlags.SetFlag() from ANY script:
     */
    
    void Example_UnlockFromEnemyDeath()
    {
        // When an enemy dies:
        GameFlags.SetFlag("defeated_dragon", true);
        
        // JournalManager automatically listens and unlocks the entry
        // if you've mapped "defeated_dragon" -> "monster.dragon" in the Inspector
    }
    
    void Example_UnlockFromDialogue()
    {
        // When dialogue completes:
        GameFlags.SetFlag("talked_to_knight", true);
        // Automatically unlocks the journal entry
    }
    
    void Example_UnlockMultiple()
    {
        // Unlock multiple entries at once:
        GameFlags.SetFlag("quest_complete", true);
        GameFlags.SetFlag("found_evidence", true);
        GameFlags.SetFlag("met_suspect", true);
    }
    
    /*
     * ========================================
     * STEP 2: HIERARCHY SETUP FOR PAGINATION
     * ========================================
     * 
     * Your journal hierarchy should look like this:
     * 
     * JournalPanel
     * ??? Pages
     *     ??? MonstersPage              <- Add JournalPaginationController here
     *         ??? PreviousButton         <- Drag to "previousButton"
     *         ??? NextButton             <- Drag to "nextButton"
     *         ??? PageCounter (Text)     <- Drag to "pageCounterText" (optional)
     *         ??? EntriesContainer       <- Drag to "entriesContainer"
     *             ??? MonsterEntry1      <- Has JournalEntry component (entryId = "monster.dragon")
     *             ??? MonsterEntry2      <- Has JournalEntry component (entryId = "monster.goblin")
     *             ??? MonsterEntry3      <- Has JournalEntry component (entryId = "monster.slime")
     * 
     * Each MonsterEntry GameObject should have:
     * - JournalEntry component with unique entryId
     * - lockedView GameObject (shows "???" when locked)
     * - unlockedView GameObject (shows actual content when unlocked)
     */
    
    /*
     * ========================================
     * STEP 3: INSPECTOR CONFIGURATION
     * ========================================
     * 
     * On MonstersPage GameObject:
     * 1. Add JournalPaginationController component
     * 2. Set "Entries Per Page" = 1 (or however many you want visible at once)
     * 3. Drag the Previous button to "previousButton"
     * 4. Drag the Next button to "nextButton"
     * 5. Drag the page counter text to "pageCounterText" (optional)
     * 6. Drag the EntriesContainer GameObject to "entriesContainer"
     * 7. Enable "Auto Detect Entries" = true
     * 8. Drag your JournalManager ScriptableObject to "journalManager"
     * 
     * On JournalManager ScriptableObject (in Project window):
     * 1. Open the Inspector
     * 2. Add mappings like:
     *    - flag: "defeated_dragon"  -> entryId: "monster.dragon"
     *    - flag: "defeated_goblin"  -> entryId: "monster.goblin"
     *    - flag: "defeated_slime"   -> entryId: "monster.slime"
     * 3. Check "Only When True" = true
     */
    
    /*
     * ========================================
     * STEP 4: HOW IT WORKS AT RUNTIME
     * ========================================
     * 
     * 1. All entries start hidden (locked)
     * 2. When GameFlags.SetFlag("defeated_dragon", true) is called:
     *    - JournalManager automatically unlocks "monster.dragon"
     *    - JournalPaginationController refreshes
     *    - The dragon entry becomes visible
     * 3. Player can use Previous/Next buttons to scroll through unlocked entries
     * 4. Locked entries are never shown in the pagination
     * 5. Page counter updates (e.g., "1 / 3" if 3 monsters are unlocked)
     */
    
    /*
     * ========================================
     * EXAMPLE: ENEMY INTEGRATION
     * ========================================
     * 
     * In your Enemy.cs script, add this to the Die() method:
     */
    
    // protected override void Die()
    // {
    //     base.Die();
    //     
    //     // Unlock journal entry for this enemy
    //     if (!string.IsNullOrEmpty(data.journalFlagName))
    //     {
    //         GameFlags.SetFlag(data.journalFlagName, true);
    //         Debug.Log($"Unlocked journal entry: {data.journalFlagName}");
    //     }
    // }
    
    /*
     * Then in your EnemyData ScriptableObject, add:
     * 
     * [Header("Journal")]
     * public string journalFlagName = "defeated_dragon";
     * 
     * And map it in JournalManager:
     * defeated_dragon -> monster.dragon
     */
    
    /*
     * ========================================
     * ADVANCED: CUSTOM PAGINATION
     * ========================================
     * 
     * If you want to show multiple entries per page (e.g., a grid):
     * 1. Set "Entries Per Page" = 3 (or whatever number)
     * 2. Make sure your EntriesContainer has a proper layout (e.g., GridLayoutGroup)
     * 3. The pagination will automatically show 3 entries at a time
     * 
     * If you want different layouts per tab:
     * - Each tab (Characters, Evidence, Monsters, etc.) can have its own JournalPaginationController
     * - They work independently
     * - Configure entriesPerPage differently for each
     */
    
    /*
     * ========================================
     * FAQ
     * ========================================
     * 
     * Q: Do I need JournalUnlockTrigger_Example?
     * A: No! Just use GameFlags.SetFlag() directly in your code.
     * 
     * Q: Can I unlock entries without flags?
     * A: Yes! Call journalManager.AddEntry("monster.dragon") directly.
     * 
     * Q: How do I unlock multiple entries at once?
     * A: Just call GameFlags.SetFlag() multiple times, or loop through a list.
     * 
     * Q: Can different tabs have different layouts?
     * A: Yes! Each tab gets its own JournalPaginationController with independent settings.
     * 
     * Q: What if I want no pagination (show all entries at once)?
     * A: Don't use JournalPaginationController. Just use JournalPageController instead.
     *    It will show all unlocked entries without pagination buttons.
     */
}
