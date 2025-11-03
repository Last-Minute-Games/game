using UnityEngine;

/// <summary>
/// GUIDE: Which Interactive Script to Use?
/// 
/// Choose the simplest one that fits your needs!
/// </summary>
public class InteractiveItemGuide : MonoBehaviour
{
    /*
     * ========================================
     * OPTION 1: InteractiveItem (SIMPLE) ?
     * ========================================
     * 
     * Use for: 95% of NPCs and items
     * 
     * Features:
     * - One dialog
     * - Sets one flag when done
     * - Simple and clean
     * 
     * Inspector Setup:
     * 1. Dialog Behaviour: Drag your DialogBehaviour
     * 2. Dialog Graph: Drag the dialog to play
     * 3. Flag To Set: "talked_to_shopkeeper" (or whatever)
     * 
     * Example: Shopkeeper NPC
     * - Player talks to shopkeeper
     * - Dialog plays
     * - Flag "talked_to_shopkeeper" is set
     * - Journal unlocks shopkeeper entry (if mapped)
     */
    
    void Example_SimpleNPC()
    {
        // Just set up in Inspector, that's it!
        // When dialog finishes, flag is automatically set
    }
    
    /*
     * ========================================
     * OPTION 2: ConditionalInteractiveItem (ADVANCED) ??
     * ========================================
     * 
     * Use for: Quest NPCs with multiple states
     * 
     * Features:
     * - Multiple dialog options
     * - Different dialogs based on flags
     * - Sets different flags per state
     * 
     * Inspector Setup:
     * Dialog Options (Array):
     * 
     * Element 0: (Quest Start)
     *   - State Name: "Quest Start"
     *   - Required Flags: (empty)
     *   - Dialog: QuestStartDialog
     *   - Flag To Set: "quest_started"
     * 
     * Element 1: (Quest In Progress)
     *   - State Name: "Quest In Progress"
     *   - Required Flags: ["quest_started"]
     *   - Dialog: QuestProgressDialog
     *   - Flag To Set: (empty)
     * 
     * Element 2: (Quest Complete)
     *   - State Name: "Quest Complete"
     *   - Required Flags: ["quest_started", "quest_objective_done"]
     *   - Dialog: QuestCompleteDialog
     *   - Flag To Set: "quest_finished"
     * 
     * Fallback Dialog: DefaultGreeting
     * 
     * Example: Quest Giver NPC
     * - First visit: Gives quest, sets "quest_started"
     * - During quest: Different dialog
     * - After completion: Reward dialog, sets "quest_finished"
     */
    
    void Example_QuestNPC()
    {
        // Setup in Inspector with multiple dialog options
        // System automatically picks the right dialog based on flags
    }
    
    /*
     * ========================================
     * COMPARISON
     * ========================================
     * 
     * | Feature | InteractiveItem | ConditionalInteractiveItem |
     * |---------|----------------|---------------------------|
     * | Complexity | Simple ? | Advanced ?? |
     * | Dialogs | 1 | Multiple |
     * | Flag Checks | No | Yes |
     * | Setup Time | 1 minute | 5 minutes |
     * | Use Cases | Most NPCs | Quest NPCs |
     */
    
    /*
     * ========================================
     * RECOMMENDATIONS
     * ========================================
     * 
     * Start with InteractiveItem for everything!
     * Only upgrade to ConditionalInteractiveItem if:
     * - NPC needs to remember quest progress
     * - Different dialogs based on game state
     * - Multiple interactions with changing content
     * 
     * Examples:
     * 
     * InteractiveItem:
     * ? Shopkeeper (always same dialog)
     * ? Guard blocking path (simple dialog)
     * ? Tutorial NPC (one-time explanation)
     * ? Background character (flavor text)
     * 
     * ConditionalInteractiveItem:
     * ?? Main quest giver (multiple quest stages)
     * ?? Character with story arc (relationship changes)
     * ?? Mystery NPC (reveals info over time)
     */
    
    /*
     * ========================================
     * JOURNAL INTEGRATION
     * ========================================
     * 
     * Both work the same way with the journal!
     * 
     * 1. NPC sets flag: "talked_to_allistair"
     * 2. JournalManager maps: "talked_to_allistair" ? "character.allistair"
     * 3. Journal entry unlocks automatically!
     * 
     * For progressive entries:
     * - First interaction: GameFlags.SetFlag("met_allistair")
     * - Quest complete: GameFlags.SetFlag("allistair_quest_done")
     * - Secret found: GameFlags.SetFlag("allistair_secret")
     * 
     * JournalSimpleProgressiveEntry automatically shows more text!
     */
}
