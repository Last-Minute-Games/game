using UnityEngine;

/// <summary>
/// The central manager for the Sokoban minigame within the Overworld scene.
/// Handles the activation/deactivation of the puzzle components and swaps player control.
/// </summary>
public class MinigameController : MonoBehaviour
{
    // Make this class accessible globally for scripts like WinConditionManager and Activator
    public static MinigameController Instance { get; private set; }

    [Header("Puzzle Components")]
    [Tooltip("The parent GameObject containing all walls, boxes, and goals.")]
    public GameObject sokobanRoot;
    [Tooltip("The starting X and Y offset for the player when the minigame starts.")]
    public Vector2 playerStartPositionOffset = new Vector2(0f, 0f);

    [Header("Player Control")]
    [Tooltip("The movement script for the Sokoban game (enable this, disable Overworld).")]
    public MonoBehaviour sokobanPlayerScript;
    [Tooltip("The main movement script for the Overworld (disable this, enable Overworld).")]
    public MonoBehaviour overworldPlayerScript;

    [Header("Visual Swap")]
    [Tooltip("The sprite to use when the player is inside the Sokoban minigame.")]
    public Sprite sokobanPlayerSprite;
    private SpriteRenderer playerSpriteRenderer;
    private Sprite overworldPlayerSprite; // Stores the original sprite

    [Header("Return Position")]
    [Tooltip("The position where the player should return to in the Overworld. Set dynamically by SokobanActivator.")]
    // This is now set by the Activator, but we keep it public for access.
    public Vector3 overworldExitPosition;

    // References for internal logic
    private GameObject player;
    private WinConditionManager winManager;

    void Awake()
    {
        // Singleton pattern: Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("Player GameObject not found! Check 'Player' tag."); }

        // Get Player SpriteRenderer and store original sprite
        if (player != null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                overworldPlayerSprite = playerSpriteRenderer.sprite;
            }
        }

        winManager = sokobanRoot.GetComponentInChildren<WinConditionManager>();
        if (winManager == null) { Debug.LogError("WinConditionManager not found inside the Sokoban Root."); }

        // --- FIX: SET INITIAL STATE (REQUIRED FOR IN-SCENE MINIGAMES) ---
        if (sokobanRoot != null)
        {
            sokobanRoot.SetActive(false); // Hide the puzzle
        }

        if (overworldPlayerScript != null && sokobanPlayerScript != null)
        {
            overworldPlayerScript.enabled = true; // Start with Overworld movement ON
            sokobanPlayerScript.enabled = false;  // Start with Sokoban movement OFF
        }
    }

    /// <summary>
    /// Called by the InteractiveSokobanActivator when the player interacts with the entrance.
    /// </summary>
    public void StartSokoban()
    {
        if (player == null || sokobanRoot == null) return;

        // 1. Swap Player Controls: Disable Overworld, Enable Sokoban
        overworldPlayerScript.enabled = false;
        sokobanPlayerScript.enabled = true;

        // 2. Teleport Player to the Puzzle Start Position (with rounding for grid alignment)
        Vector3 targetPos = sokobanRoot.transform.position + new Vector3(playerStartPositionOffset.x, playerStartPositionOffset.y, 0f);
        player.transform.position = new Vector3(
            Mathf.Round(targetPos.x),
            Mathf.Round(targetPos.y),
            targetPos.z
        );

        // 3. ACTIVATE SPRITE SWAP
        if (playerSpriteRenderer != null && sokobanPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = sokobanPlayerSprite;
        }

        // 4. Activate the Puzzle: Make all walls/boxes/goals visible
        sokobanRoot.SetActive(true);

        Debug.Log("Sokoban Minigame started.");
    }

    /// <summary>
    /// Called by the WinConditionManager or the Quit Button.
    /// </summary>
    /// <param name="solved">True if the player solved the puzzle, false if they quit.</param>
    public void EndSokoban(bool solved)
    {
        if (player == null || sokobanRoot == null) return;

        // 1. Deactivate the Puzzle: Hide all walls/boxes/goals
        sokobanRoot.SetActive(false);

        // 2. Swap Player Controls: Enable Overworld, Disable Sokoban
        sokobanPlayerScript.enabled = false;
        overworldPlayerScript.enabled = true;

        // 3. RETURN SPRITE SWAP
        if (playerSpriteRenderer != null && overworldPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = overworldPlayerSprite;
        }

        // 4. Teleport Player back to the Overworld Exit Position (set dynamically by Activator)
        player.transform.position = overworldExitPosition;

        // --- FIX: SNAP CAMERA TO PLAYER'S NEW POSITION ---
        if (Camera.main != null)
        {
            // Instantly moves the camera to follow the player, preventing the black screen/lag.
            Camera.main.transform.position = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                Camera.main.transform.position.z // Keep the original Z depth
            );
        }

        Debug.Log($"Sokoban Minigame finished. Solved: {solved}");
    }

    /// <summary>
    /// Resets the positions of the player and all boxes to their starting locations.
    /// Called by the Reset UI Button.
    /// </summary>
    public void ResetPuzzle()
    {
        // 1. Reset all boxes and the player (if they have the InitialPosition script)
        InitialPosition[] allResettableObjects = FindObjectsOfType<InitialPosition>();
        foreach (InitialPosition resettable in allResettableObjects)
        {
            resettable.ResetPosition();
        }

        // 2. Reset the goal counter on the WinConditionManager
        winManager?.ForceResetGoals();

        Debug.Log("Puzzle reset complete.");
    }
}
