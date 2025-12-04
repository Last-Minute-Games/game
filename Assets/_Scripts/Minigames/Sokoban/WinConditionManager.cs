using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class WinConditionManager : MonoBehaviour
{
    private int totalGoals;
    private int goalsReached = 0;

    // This variable is no longer used for scene loading but is kept for the GrantReward log.
    public string overworldSceneName = "Overworld";

    // Cache the MinigameController for communication
    private MinigameController controller;

    /// <summary>
    /// new
    /// </summary>


    [Header("Win UI / Delay")]
    [SerializeField] float winDelaySeconds = 1.5f;
    [SerializeField] GameObject winPanel;



    private bool winSequenceStarted = false;


    void Start()
    {
        // 1. Find the MinigameController instance
        controller = FindObjectOfType<MinigameController>(); // Using FindObjectOfType is safer here since it's a persistent object
        if (controller == null)
        {
            Debug.LogError("MinigameController instance not found! Win/Quit functionality will fail.");
        }

        // 2. Count goals
        totalGoals = GameObject.FindGameObjectsWithTag("Goal").Length;
        Debug.Log("Total Goals: " + totalGoals);

        if (totalGoals == 0)
        {
            Debug.LogWarning("No goals found. The puzzle cannot be solved.");
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void GoalReached()
    {
        goalsReached++;
        CheckWinCondition();
    }

    public void GoalUnreached()
    {
        goalsReached--;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        Debug.Log("Goals Reached: " + goalsReached);

        if (goalsReached == totalGoals && totalGoals > 0 && !winSequenceStarted)
        {
            // WIN CONDITION MET!
            winSequenceStarted = true;
            Debug.Log("Puzzle Solved!");
            GrantReward();

            // Notify the controller to disable the minigame and restore Overworld controls
            // The 'true' argument means the puzzle was solved successfully

            //controller?.EndSokoban(true);

            StartCoroutine(WinSequence());

        }
    }

    private void GrantReward()
    {
        // Define the temporary reward item and clue.
        string rewardItemName = "Dungeon Card: The Crypt Key";
        string rewardClue = "The first step is always down. Check the well in the market square.";

        // Log the reward details (This is where you would call your PlayerInventory later)
        Debug.Log("--- REWARD GRANTED ---");
        Debug.Log("Item Received: " + rewardItemName);
        Debug.Log("Clue Unlocked: " + rewardClue);
        Debug.Log("----------------------");
    }

    // --- NEW: Function required for MinigameController Reset ---

    /// <summary>
    /// Forces the goals reached count to zero. Called by MinigameController.ResetPuzzle().
    /// </summary>
    public void ForceResetGoals()
    {
        goalsReached = 0;
        // The Goal.cs scripts will automatically reset their 'isOccupied' flag 
        // once the boxes are moved off them by the MinigameController.
        Debug.Log("Goal count reset to 0.");
    }


    // --- UI Button Logic (Changed to use the Controller) ---

    /// <summary>
    /// Quits the minigame and returns control to the overworld.
    /// This is publicly exposed for the UI Quit Button's OnClick event.
    /// </summary>
    public void QuitMinigame()
    {
        // The 'false' argument means the player quit, not solved
        Debug.Log("Quitting puzzle. Returning to Overworld.");
        controller?.EndSokoban(false);
    }

    private IEnumerator WinSequence()
    {
        // 1) Show a small overlay (optional)
        if (winPanel != null)
            winPanel.SetActive(true);

        // 2) Freeze Sokoban controls during the delay
        if (controller != null && controller.sokobanPlayerScript != null)
            controller.sokobanPlayerScript.enabled = false; // public in MinigameController

        // 3) Wait a bit so the player sees they’ve won
        yield return new WaitForSeconds(winDelaySeconds);

        // 4) Now exit the minigame normally
        controller?.EndSokoban(true);
    }



}
