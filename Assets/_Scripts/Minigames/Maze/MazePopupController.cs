using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MazePopupController : MonoBehaviour
{
    [Header("Camera (used only to place the maze)")]
    public Camera mainCamera;
    [Tooltip("Separate UI Camera for backdrop (optional, will be created if needed).")]
    private Camera uiCamera;

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

    [Header("Instructions")]
    [SerializeField] private MinigameInstructions instructions;   // NEW


    [Header("Sprite Swap (optional but fun)")]
    [Tooltip("Sprite to use for the player while in the maze (little head).")]
    public Sprite mazePlayerSprite;

    [Header("Player teleport (Sokoban-style)")]
    [Tooltip("Main overworld player whose position we save/restore.")]
    public Transform overworldPlayer;

    [Header("Transition")]
    [Tooltip("CanvasGroup used to fade the screen when entering/exiting the minigame.")]
    [SerializeField] CanvasGroup transitionCanvasGroup;
    [Tooltip("Optional text element that displays the current transition message.")]
    [SerializeField] TMP_Text transitionStatusText;
    [Tooltip("How long (in seconds) the screen stays fully faded while we reposition objects.")]
    [SerializeField] float transitionCoveredDuration = 1.2f;
    [Tooltip("Fade-in duration in seconds.")]
    [SerializeField] float transitionFadeInDuration = 0.4f;
    [Tooltip("Fade-out duration in seconds.")]
    [SerializeField] float transitionFadeOutDuration = 0.4f;

    private Coroutine transitionRoutine;
    private bool isTransitionRunning;

    private InitialPositionn playerInitialPosition;

    private Vector3 originalCamPos;
    private float originalCamSize;


    // World-space backdrop sprite (renders behind maze)
    private GameObject worldSpaceBackdrop;

    SpriteRenderer overworldSpriteRenderer;
    Sprite overworldSprite;

    bool isOpen = false;
    private bool mazeGenerated = false;

    void Awake()
    {
        // Hook quit button to use transitions
        if (quitButton != null)
            quitButton.onClick.AddListener(() => EndMaze(false)); // false = quit, not solved


        if (overworldPlayer == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                overworldPlayer = playerGO.transform;
        }

        if (overworldPlayer != null)
        {
            playerInitialPosition = overworldPlayer.GetComponent<InitialPositionn>();
            if (playerInitialPosition == null)
            {
                // Optional: auto-add the component if missing
                playerInitialPosition = overworldPlayer.gameObject.AddComponent<InitialPositionn>();
            }
        }
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

        if (mazePlayer != null)
        {
            mazePlayer.enabled = false;
            mazePlayer.gameObject.SetActive(false); // player head hidden at start
        } // no maze control at start

        // Restore overworld sprite if you ever wire it
        if (overworldSpriteRenderer != null)
        {
            overworldSpriteRenderer.enabled = true;
            if (overworldSprite != null)
                overworldSpriteRenderer.sprite = overworldSprite;
        }

        mazeGenerated = false;

        // Ensure transition canvas starts disabled
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by the MazePopupActivator when the player interacts with the entrance.
    /// Uses transitions similar to Sokoban.
    /// </summary>
    public void StartMaze()
    {
        if (isOpen) return;
        RunTransition("ENTERING MAZE", PerformMazeStart);
    }

    /// <summary>
    /// Called by the Quit Button or when exiting the maze.
    /// Uses transitions similar to Sokoban.
    /// </summary>
    /// <param name="solved">True if the player completed the maze, false if they quit.</param>
    public void EndMaze(bool solved = false)
    {
        if (!isOpen) return;
        RunTransition(solved ? "YOU WIN" : "EXITING MAZE", PerformMazeEnd);
    }

    private void RunTransition(string message, System.Action midAction)
    {
        if (isTransitionRunning)
        {
            return;
        }

        if (transitionCanvasGroup == null)
        {
            midAction?.Invoke();
            return;
        }

        // Ensure the GameObject is active so we can start coroutines
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        transitionRoutine = StartCoroutine(TransitionRoutine(message, midAction));
    }

    private IEnumerator TransitionRoutine(string message, System.Action midAction)
    {
        isTransitionRunning = true;

        if (transitionStatusText != null)
        {
            transitionStatusText.text = message;
        }

        GameObject transitionObject = transitionCanvasGroup.gameObject;
        if (!transitionObject.activeSelf)
        {
            transitionObject.SetActive(true);
        }

        transitionCanvasGroup.blocksRaycasts = true;
        transitionCanvasGroup.interactable = true;

        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 1f, transitionFadeInDuration);

        midAction?.Invoke();

        if (transitionCoveredDuration > 0f)
        {
            yield return new WaitForSeconds(transitionCoveredDuration);
        }

        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 0f, transitionFadeOutDuration);

        transitionCanvasGroup.blocksRaycasts = false;
        transitionCanvasGroup.interactable = false;
        transitionObject.SetActive(false);

        transitionRoutine = null;
        isTransitionRunning = false;
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (duration <= 0f)
        {
            transitionCanvasGroup.alpha = end;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transitionCanvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        transitionCanvasGroup.alpha = end;
    }

    private void PerformMazeStart()
    {
        // Pause NPCs and timer, but NOT player input (using minigame pause)
        GlobalPause.SetMinigamePaused(true);

        Show();
    }

    private void PerformMazeEnd()
    {
        // Resume NPCs and timer (using minigame pause)
        GlobalPause.SetMinigamePaused(false);

        PerformHide();
    }

    private void PerformHide()
    {
        if (!isOpen) return;
        
        isOpen = false;

        if (playerInitialPosition != null)
        {
            playerInitialPosition.RestorePosition();
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCamPos;
            mainCamera.orthographicSize = originalCamSize;
        }

        // Hide popup & maze
        if (backdrop) backdrop.SetActive(false);
        if (window) window.SetActive(false);
        if (mazeRoot) mazeRoot.SetActive(false);
        
        // Hide world-space backdrop
        if (worldSpaceBackdrop != null)
            worldSpaceBackdrop.SetActive(false);
        
        // Clean up UI camera
        if (uiCamera != null)
        {
            Destroy(uiCamera.gameObject);
            uiCamera = null;
        }

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
        if (overworldSpriteRenderer != null)
        {
            overworldSpriteRenderer.enabled = true;
            if (overworldSprite != null)
                overworldSpriteRenderer.sprite = overworldSprite;
        }

        // Timer + flag, same style as Blackjack/Sokoban
        FindObjectOfType<ClockTimer>()?.PauseTimer(false);
        GameFlags.SetFlag("minigame.maze.finish");

        mazeGenerated = false;

        // Make sure transition canvas is disabled
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.gameObject.SetActive(false);
        }

        HideImmediate();
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;
        mazeGenerated = false;


        if (playerInitialPosition != null)
        {
            playerInitialPosition.SaveCurrentPosition();
        }
        //GlobalPause.SetPaused(true);

        if (hudGroup != null) //HUD off
            hudGroup.SetActive(false);

        // Hide overworld player sprite so we only see the maze player head
        if (overworldSpriteRenderer != null)
        {
            overworldSpriteRenderer.enabled = false;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            originalCamPos = mainCamera.transform.position;
            originalCamSize = mainCamera.orthographicSize;
        }

        // Show popup & maze - create world-space backdrop behind maze
        if (mainCamera == null)
            mainCamera = Camera.main;

        // --- TELEPORT PLAYER TO MAZE START (SOKOBAN-STYLE) ---
        if (overworldPlayer != null)
        {
            Vector3 startPos;

            // 1) If you have a specific start Transform, use that
            if (mazeStartPoint != null)
            {
                startPos = mazeStartPoint.position;
            }
            // 2) Otherwise, use cell (0,0) from the maze grid
            else if (mazeGenerator != null)
            {
                startPos = mazeGenerator.GetWorldPosition(new Vector2Int(0, 0));
            }
            // 3) Fallback: just use the maze root position
            else if (mazeRoot != null)
            {
                startPos = mazeRoot.transform.position;
            }
            else
            {
                startPos = overworldPlayer.position;
            }

            // Snap to whole numbers so it lines up with the grid
            overworldPlayer.position = new Vector3(
                Mathf.Round(startPos.x),
                Mathf.Round(startPos.y),
                overworldPlayer.position.z
            );
        }

        // Disable UI backdrop - we'll use world-space instead
        if (backdrop) 
        {
            backdrop.SetActive(false); // Disable UI backdrop
        }
        
        // Create world-space black backdrop sprite (renders behind maze based on Z position)
        if (worldSpaceBackdrop == null && mainCamera != null)
        {
            worldSpaceBackdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            worldSpaceBackdrop.name = "WorldSpaceBackdrop";
            worldSpaceBackdrop.transform.SetParent(mazeRoot != null ? mazeRoot.transform.parent : transform);
            
            // Position behind maze (higher Z = further from camera = renders behind)
            Vector3 camPos = mainCamera.transform.position;
            float backdropZ = camPos.z + (mainCamera.orthographic ? 50f : 50f);
            worldSpaceBackdrop.transform.position = new Vector3(camPos.x, camPos.y, backdropZ);
            
            // Scale to cover entire camera view
            float orthoSize = mainCamera.orthographic ? mainCamera.orthographicSize : 10f;
            float aspect = (float)Screen.width / Screen.height;
            float width = orthoSize * aspect * 2f;
            float height = orthoSize * 2f;
            worldSpaceBackdrop.transform.localScale = new Vector3(width, height, 1f);
            
            // Make it black - use MeshRenderer approach which works better in builds
            Renderer renderer = worldSpaceBackdrop.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Try to find shader, but handle if it's null (common in builds)
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    // Fallback to Sprites/Default which is more reliably included
                    shader = Shader.Find("Sprites/Default");
                }
                if (shader == null)
                {
                    // Last resort: use the material's current shader
                    shader = Shader.Find("Standard");
                }
                
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = Color.black;
                    renderer.material = mat;
                }
                else
                {
                    // If all shader lookups fail, just set the existing material's color
                    if (renderer.material != null)
                    {
                        renderer.material.color = Color.black;
                    }
                    Debug.LogWarning("[MazePopup] Could not find any suitable shader for backdrop. Using default material.");
                }
            }
            
            // Remove collider (we don't need it)
            Collider col = worldSpaceBackdrop.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
        }
        
        if (worldSpaceBackdrop != null)
            worldSpaceBackdrop.SetActive(true);
        
        // Window and maze appear on top
        if (window) 
        {
            window.SetActive(true);
            // Ensure window appears on top
            Canvas windowCanvas = window.GetComponent<Canvas>();
            if (windowCanvas == null)
                windowCanvas = window.GetComponentInParent<Canvas>();
            if (windowCanvas != null)
            {
                windowCanvas.sortingOrder = 1; // Higher sorting order = in front
            }
        }
        
        if (mazeRoot) 
        {
            mazeRoot.SetActive(true);
            // Ensure maze appears on top (it might be in world space or on a different canvas)
            Canvas mazeCanvas = mazeRoot.GetComponent<Canvas>();
            if (mazeCanvas == null)
                mazeCanvas = mazeRoot.GetComponentInParent<Canvas>();
            if (mazeCanvas != null)
            {
                mazeCanvas.sortingOrder = 2; // Highest sorting order = in front
            }
        }
        
        gameObject.SetActive(true);

        if (instructions == null)
        {
            instructions = GetComponentInChildren<MinigameInstructions>(true);
        }
        if (instructions != null)
        {
            instructions.OnPopupOpened();
        }


        // Disable overworld movement scripts
        foreach (var b in overworldControlScripts)
            if (b) b.enabled = false;

        // Enable maze grid movement
        if (mazePlayer != null) { 
            mazePlayer.enabled = false;
            mazePlayer.gameObject.SetActive(false);
        }

        

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

        // --- Move CAMERA to the maze center ---
        Vector3 camPos = mainCamera.transform.position;
        camPos.x = bounds.center.x; //chnage this to up the cam
        camPos.y = bounds.center.y;
        mainCamera.transform.position = camPos;

        // --- Optional: zoom so the whole maze fits on screen ---
        float halfHeight = bounds.size.y * 0.5f;
        float halfWidth = bounds.size.x * 0.5f / mainCamera.aspect;
        mainCamera.orthographicSize = Mathf.Max(halfHeight, halfWidth);
    }

    /// <summary>
    /// Public Hide method. If transitions are available and not already running, uses them.
    /// Otherwise, hides immediately.
    /// </summary>
    public void Hide()
    {
        if (!isOpen) return;
        
        // If we're in a transition, don't call Hide directly - use EndMaze instead
        if (isTransitionRunning)
        {
            return;
        }
        
        // If transitions are available, use them
        if (transitionCanvasGroup != null)
        {
            EndMaze(false); // false = quit, not solved
            return;
        }
        
        // Otherwise, hide immediately
        PerformHide();
    }

    void HideImmediate()
    {
        if (window) window.SetActive(false);
        if (backdrop) backdrop.SetActive(false);
        if (mazeRoot) mazeRoot.SetActive(false);
        gameObject.SetActive(false);
    }

    // Helper method to set layer recursively
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
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
