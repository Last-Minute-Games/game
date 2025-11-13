using UnityEngine;
using UnityEngine.UI;

public class MazePopupController : MonoBehaviour
{
    [Header("Popup Wiring")]
    [Tooltip("Root window panel that contains the maze (like Blackjack window).")]
    public GameObject window;
    [Tooltip("Semi-opaque full-screen image behind the window.")]
    public GameObject backdrop;
    [Tooltip("Quit/Close button inside the maze UI.")]
    public Button quitButton;

    [Header("Maze Content")]
    [Tooltip("Parent that contains the generated maze and the maze player head.")]
    public GameObject mazeRoot;
    [Tooltip("Grid movement script used inside the maze (your PlayerMovementScript).")]
    public PlayerMovementScript mazePlayerMovement;
    [Tooltip("Where the maze player head should start each time.")]
    public Transform mazeStartPoint;
    [Tooltip("Optional: maze generator to call when opening.")]
    public GenerateMaze mazeGenerator;

    [Header("Overworld Control")]
    [Tooltip("All movement scripts that should be disabled while maze is open (e.g. your main Player movement).")]
    public Behaviour[] overworldControlScripts;
    [SerializeField] private GameObject hudGroup; // same idea as Blackjack

    [Header("Sprite Swap (optional but fun)")]
    [Tooltip("Sprite to use for the player while in the maze (little head).")]
    public Sprite mazePlayerSprite;

    SpriteRenderer overworldSpriteRenderer;
    Sprite overworldSprite;

    bool isOpen = false;

    void Awake()
    {
        // Hook quit button
        if (quitButton != null)
            quitButton.onClick.AddListener(Hide);

        // Grab overworld player sprite once (same idea as MinigameController)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            overworldSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (overworldSpriteRenderer != null)
                overworldSprite = overworldSpriteRenderer.sprite;
        }

        HideImmediate();                // make sure popup is off
        if (mazeRoot != null)
            mazeRoot.SetActive(false);  // maze itself hidden
        if (mazePlayerMovement != null)
            mazePlayerMovement.enabled = false; // no maze control at start
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;

        // Optional: regenerate the maze each time the popup opens
        if (mazeGenerator != null)
        {
            mazeGenerator.CreateMaze();   // uses your existing CreateMaze logic :contentReference[oaicite:2]{index=2}
        }

        // Place maze player at start
        if (mazePlayerMovement != null && mazeStartPoint != null)
        {
            mazePlayerMovement.transform.position = mazeStartPoint.position;
        }

        // HUD off (like Blackjack)
        if (hudGroup != null)
            hudGroup.SetActive(false);

        // Show popup & maze
        if (backdrop) backdrop.SetActive(true);
        if (window) window.SetActive(true);
        if (mazeRoot) mazeRoot.SetActive(true);
        gameObject.SetActive(true);

        // Disable overworld movement scripts
        foreach (var b in overworldControlScripts)
            if (b) b.enabled = false;

        // Enable maze grid movement
        if (mazePlayerMovement != null)
            mazePlayerMovement.enabled = true;

        // Swap overworld sprite to maze sprite (optional; you mostly see the head in the maze)
        if (overworldSpriteRenderer != null && mazePlayerSprite != null)
            overworldSpriteRenderer.sprite = mazePlayerSprite;
    }

    public void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        // Hide popup & maze
        if (backdrop) backdrop.SetActive(false);
        if (window) window.SetActive(false);
        if (mazeRoot) mazeRoot.SetActive(false);

        // HUD back on
        if (hudGroup != null)
            hudGroup.SetActive(true);

        // Re-enable overworld movement
        foreach (var b in overworldControlScripts)
            if (b) b.enabled = true;

        // Turn off maze controls
        if (mazePlayerMovement != null)
            mazePlayerMovement.enabled = false;

        // Restore overworld sprite
        if (overworldSpriteRenderer != null && overworldSprite != null)
            overworldSpriteRenderer.sprite = overworldSprite;

        // Timer + flag, same style as Blackjack/Sokoban
        FindObjectOfType<ClockTimer>()?.PauseTimer(false);
        GameFlags.SetFlag("minigame.maze.finish");

        HideImmediate();
    }

    void HideImmediate()
    {
        if (window) window.SetActive(false);
        if (backdrop) backdrop.SetActive(false);
        if (mazeRoot) mazeRoot.SetActive(false);
        gameObject.SetActive(false);
    }
}
