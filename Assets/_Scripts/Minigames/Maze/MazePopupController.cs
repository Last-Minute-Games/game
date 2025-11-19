using UnityEngine;
using UnityEngine.UI;

public class MazePopupController : MonoBehaviour
{
    [Header("Camera (used only to place the maze)")]
    public Camera mainCamera;

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
    [Tooltip("Movement script used inside the maze (MazePlayerController).")]
    public MazePlayerController mazePlayer;
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
    private bool mazeGenerated = false;

    void Awake()
    {
        // Hook quit button
        if (quitButton != null)
            quitButton.onClick.AddListener(Hide);

        // Grab overworld player sprite once (same idea as MinigameController)
        /*
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            overworldSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (overworldSpriteRenderer != null)
                overworldSprite = overworldSpriteRenderer.sprite;
        }
        */
        HideImmediate();                // make sure popup is off
        if (mazeRoot != null)
            mazeRoot.SetActive(false);  // maze itself hidden

        if (mazePlayer != null)
        {
            mazePlayer.enabled = false;
            mazePlayer.gameObject.SetActive(false); // player head hidden at start
        } // no maze control at start

        mazeGenerated = false;
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;
        mazeGenerated = false;

        if (hudGroup != null) //HUD off
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
        if (mazePlayer != null) { 
            mazePlayer.enabled = false;
            mazePlayer.gameObject.SetActive(false);
        }

        // Swap overworld sprite to maze sprite (optional; you mostly see the head in the maze)
        if (overworldSpriteRenderer != null && mazePlayerSprite != null)
            overworldSpriteRenderer.sprite = mazePlayerSprite;

        if (mazeGenerator != null)
        {
            mazeGenerator.CreateMaze();   // builds rooms (if needed) and carves maze
        }

        CenterMazeOnCamera();

        if (mazePlayer != null)
        {
            mazePlayer.gameObject.SetActive(true);
            mazePlayer.enabled = true;

            if (mazeStartPoint != null)
            {
                mazePlayer.transform.position = mazeStartPoint.position;
            }
            else
            {
                // Use MazePlayerController's helper to go to (0,0)
                mazePlayer.ResetToStart();
            }
        }

        mazeGenerated = true;

    }
    private void CenterMazeOnCamera()
    {
        if (mazeRoot == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        // Collect all renderers in the maze (rooms, walls, etc.)
        var renderers = mazeRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        // Combine their bounds to get the whole maze size & center
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Camera center in world space (x,y) – we don't move camera, just read it
        Vector3 camCenter = mainCamera.transform.position;

        // We only care about x,y; keep maze's existing Z
        Vector3 targetCenter = new Vector3(camCenter.x, camCenter.y, bounds.center.z);

        // How much to move MazeRoot so its center matches camera center
        Vector3 delta = targetCenter - bounds.center;

        // Move the whole maze
        mazeRoot.transform.position += new Vector3(delta.x, delta.y, 0f);
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

        // Turn off maze controls & hide player head
        if (mazePlayer != null)
        {
            mazePlayer.enabled = false;
            mazePlayer.gameObject.SetActive(false);
        }

        // Restore overworld sprite if you ever wire it
        if (overworldSpriteRenderer != null && overworldSprite != null)
            overworldSpriteRenderer.sprite = overworldSprite;

        // Timer + flag, same style as Blackjack/Sokoban
        FindObjectOfType<ClockTimer>()?.PauseTimer(false);
        GameFlags.SetFlag("minigame.maze.finish");

        mazeGenerated = false;   // next time we open, Space is allowed again

        HideImmediate();
    }

    void HideImmediate()
    {
        if (window) window.SetActive(false);
        if (backdrop) backdrop.SetActive(false);
        if (mazeRoot) mazeRoot.SetActive(false);
        gameObject.SetActive(false);
    }

    /*
    void Update()
    {
        if (!isOpen) return;

        // One maze per popup open
        if (!mazeGenerated && Input.GetKeyDown(KeyCode.Space))
        {
            if (mazeGenerator != null)
            {
                mazeGenerator.CreateMaze();   // builds rooms (first time) and carves maze :contentReference[oaicite:4]{index=4}
            }

            //roomsjust 
            CenterMazeOnCamera();

            if (mazePlayer != null)
            {
                // show & enable maze player
                mazePlayer.gameObject.SetActive(true);
                mazePlayer.enabled = true;

                // place at start
                if (mazeStartPoint != null)
                {
                    mazePlayer.transform.position = mazeStartPoint.position;
                }
                else
                {
                    // fallback to (0,0) cell
                    //help here
                    mazePlayer.ResetToStart();
                }
            }

            mazeGenerated = true;
        }

    }

    */

    /*
    public void PlacePlayerAtStart()
    {
        currentIndex = new Vector2Int(0, 0); // or your start cell
        transform.position = maze.GetWorldPosition(currentIndex);
    }
    */
}
