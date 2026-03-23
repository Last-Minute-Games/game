using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen room-map overlay for the Overworld scene.
/// Press M (configurable) to toggle the map on/off.
/// Displays a castle blueprint image (ColorizedMap.png) with
/// real-time Wizard101-style portrait markers for the player and discovered NPCs.
///
/// Setup:
///   1. Create a RoomMapData asset (Assets → Create → Castle of Time → Room Map Data)
///      and fill in the rooms, positions, sizes, connections, and world bounds.
///   2. Add this script to a GameObject in the Overworld scene.
///   3. Assign the RoomMapData asset.
///   4. Assign ColorizedMap.png from Assets/Sprites/gfx/gfx/ColorizedMap.png.
///   5. Assign the portraitFrame sprite from Assets/UIs/portraitFrame.png.
///   6. Attach NPCMapTracker to every named NPC in the scene.
///   7. Play — press M to open/close the map.
/// </summary>
public class RoomMapUI : MonoBehaviour
{
    // ─────────────────── Inspector ───────────────────

    [Header("Data")]
    [SerializeField] private RoomMapData mapData;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.M;
    [SerializeField] private KeyCode debugRevealKey = KeyCode.R;

    [Header("Journal Background")]
    [Tooltip("The fully-open journal sprite used as the map background. " +
             "Assign JournalOverworld-Sheet_5 from Assets/Sprites/UI/journal/JournalOverworld-Sheet.png.")]
    [SerializeField] private Sprite journalSprite;

    [Header("Castle Blueprint Map")]
    [Tooltip("The castle blueprint image displayed as the map. " +
             "Assign ColorizedMap.png from Assets/Sprites/gfx/gfx/ColorizedMap.png.")]
    [SerializeField] private Sprite castleMapSprite;

    [Header("Overlay Appearance")]
    [Tooltip("Background colour of the overlay (behind the journal).")]
    [SerializeField] private Color overlayColor = new Color(0.04f, 0.03f, 0.06f, 0.92f);

    [Header("Portrait Markers (Wizard101 Style)")]
    [Tooltip("The decorative frame sprite placed around each portrait. " +
             "Assign from Assets/UIs/portraitFrame.png.")]
    [SerializeField] private Sprite portraitFrame;

    [Tooltip("Size of each portrait marker on the map (pixels).")]
    [SerializeField] private float portraitMarkerSize = 40f;

    [Tooltip("How much larger the frame is relative to the portrait (multiplier).")]
    [SerializeField] private float frameScale = 1.35f;

    [Tooltip("Tint colour for the portrait frame on NPC markers.")]
    [SerializeField] private Color npcFrameTint = new Color(0.75f, 0.65f, 0.50f, 1f);

    [Tooltip("Tint colour for the portrait frame on the player marker.")]
    [SerializeField] private Color playerFrameTint = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Player Indicator")]
    [Tooltip("Optional player portrait sprite. If empty, a gold pulsing dot is used.")]
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Color playerDotColor = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float playerDotSize  = 16f;
    [SerializeField] private float pulseSpeed     = 2.5f;
    [SerializeField] private float pulseScale     = 1.35f;

    [Header("NPC Indicators")]
    [SerializeField] private float npcDotSize     = 12f;
    [SerializeField] private int   npcLabelSize   = 12;
    [SerializeField] private Color npcLabelColor   = new Color(0.9f, 0.85f, 0.75f, 0.9f);

    [Header("Labels")]
    [SerializeField] private Color fontColor = new Color(0.9f, 0.85f, 0.75f, 1f);

    [Header("Fade Animation")]
    [Tooltip("How long the map takes to fade in/out (seconds).")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Performance")]
    [Tooltip("Preload portraits and map markers in small batches to avoid first-open stalls.")]
    [SerializeField] private bool enableMapPrewarm = true;
    [SerializeField] private float prewarmStartDelay = 0.20f;
    [SerializeField] private int portraitPrewarmBatchSize = 3;
    [SerializeField] private int markerPrewarmBatchSize = 2;
    [SerializeField] private int runtimeMarkerCreateBudgetPerFrame = 2;

    [Header("Title")]
    [SerializeField] private string mapTitle = "Castle Map";
    [SerializeField] private int titleFontSize = 32;

    // ─────────────────── Runtime ───────────────────

    private Canvas _canvas;
    private GameObject _root;
    private RectTransform _mapArea;
    private Transform _mapContentParent; // journal or root
    private bool _isOpen;

    // Caches
    private RectTransform _playerMarker;     // root of the player portrait marker group
    private Image _playerPortraitImage;       // the portrait image (or gold dot fallback)

    // NPC tracking
    private readonly Dictionary<NPCMapTracker, RectTransform> _npcDots   = new();
    private readonly Dictionary<NPCMapTracker, Text>          _npcLabels = new();
    private RectTransform _characterLegendPanel;
    private readonly Dictionary<NPCMapTracker, GameObject> _legendEntries = new();

    // Minigame tracking
    private readonly Dictionary<MinigameMapTracker, RectTransform> _minigameDots = new();
    private readonly Dictionary<MinigameMapTracker, Text> _minigameLabels = new();
    private RectTransform _minigameLegendPanel;
    private readonly Dictionary<MinigameMapTracker, GameObject> _minigameLegendEntries = new();
    private GameObject _minigameLegendTitle;

    // Player ref
    private Transform _playerTransform;
    private Sprite _circleSprite;
    // Sound
    private EnvironmentSoundHandler _soundHandler;
    // Fade
    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private bool _isFading;
    // Debug
    private bool _debugRevealAllNPCs = false;

    // Prewarm / deferred creation
    private bool _isPrewarming;
    private bool _prewarmComplete;
    private Coroutine _prewarmCoroutine;
    private readonly Queue<NPCMapTracker> _pendingNpcCreates = new();
    private readonly HashSet<NPCMapTracker> _pendingNpcCreateSet = new();
    private readonly Queue<MinigameMapTracker> _pendingMinigameCreates = new();
    private readonly HashSet<MinigameMapTracker> _pendingMinigameCreateSet = new();
    // ─────────────────── Lifecycle ───────────────────

    void Start()
    {
        if (mapData == null)
        {
            Debug.LogError("RoomMapUI: No RoomMapData assigned.");
            enabled = false;
            return;
        }

        _circleSprite = MakeCircleSprite(64);

        // Find sound handler for journal page-flip audio
        var soundHandlerGO = GameObject.Find("EnvironmentSoundHandler");
        if (soundHandlerGO != null)
            _soundHandler = soundHandlerGO.GetComponent<EnvironmentSoundHandler>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        // Auto-load player portrait (Nikolaus) if not assigned
        if (playerPortrait == null)
        {
            var tex = Resources.Load<Texture2D>("Dialogues/Nikolaus/NikolausPortrait");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point; // Crisp rendering, no blur
                playerPortrait = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        // Auto-populate world bounds from the scene's RoomZoneTags
        PopulateWorldBoundsFromScene();

        BuildCanvas();
        BuildOverlay();

        // Add CanvasGroup for fade animation
        _canvasGroup = _root.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _root.SetActive(false);

        if (enableMapPrewarm)
            _prewarmCoroutine = StartCoroutine(PrewarmMapData());
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            // Don't toggle while a fade is in progress
            if (_isFading) return;
            // Don't open the map if another pause-level UI is open (pause menu, etc.)
            if (!_isOpen && GlobalPause.IsPaused) return;
            ToggleMap();
        }
// Debug: Toggle reveal all NPC and minigame markers (works when map is open)
        if (_isOpen && Input.GetKeyDown(debugRevealKey))
        {
            _debugRevealAllNPCs = !_debugRevealAllNPCs;
            Debug.Log($"[RoomMapUI] Debug reveal all NPCs/minigames: {(_debugRevealAllNPCs ? "ON" : "OFF")}");
        }

        
        if (_isOpen)
        {
            ProcessPendingMarkerCreates(runtimeMarkerCreateBudgetPerFrame);
            UpdatePlayerDot();
            UpdateNPCDots();
            UpdateMinigameDots();
            AnimatePlayerDot();
        }
    }

    void OnDestroy()
    {
        if (_prewarmCoroutine != null)
            StopCoroutine(_prewarmCoroutine);

        if (_root != null) Destroy(_root);
    }

    // ─────────────────── Toggle ───────────────────

    private void ToggleMap()
    {
        // Don't open while journal is already open
        if (!_isOpen)
        {
            var journal = FindFirstObjectByType<JournalUI>();
            if (journal != null && journal.IsOpen) return;
        }

        _isOpen = !_isOpen;

        // Play page-flip sound (same as journal open/close)
        if (_soundHandler != null)
        {
            try { _soundHandler.PlayJournalSound(_isOpen); }
            catch (System.Exception ex) { Debug.LogWarning($"[RoomMapUI] Journal sound failed: {ex.Message}"); }
        }

        if (_isOpen)
        {
            GlobalPause.SetPaused(true);

            // Ensure we have the player reference
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }

            _root.SetActive(true);

            // If prewarm is still running, create a small number of markers now.
            if (!_prewarmComplete)
                ProcessPendingMarkerCreates(runtimeMarkerCreateBudgetPerFrame);

            // Immediately position dots
            UpdatePlayerDot();
            UpdateNPCDots();
            UpdateMinigameDots();

            // Fade in
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeMap(0f, 1f));
        }
        else
        {
            // Fade out, then deactivate and unpause
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeMap(1f, 0f, onComplete: () =>
            {
                _root.SetActive(false);
                GlobalPause.SetPaused(false);
            }));
        }
    }

    private IEnumerator PrewarmMapData()
    {
        _isPrewarming = true;

        if (prewarmStartDelay > 0f)
            yield return new WaitForSecondsRealtime(prewarmStartDelay);

        int warmedPortraits = 0;
        int createdMarkers = 0;

        int portraitBatch = Mathf.Max(1, portraitPrewarmBatchSize);
        int markerBatch = Mathf.Max(1, markerPrewarmBatchSize);

        // Phase A: Warm portrait caches in batches.
        int portraitOps = 0;
        foreach (var tracker in NPCMapTracker.AllTrackers)
        {
            if (tracker == null || !tracker.IsDiscovered)
                continue;

            var _ = tracker.Portrait;
            warmedPortraits++;

            portraitOps++;
            if (portraitOps >= portraitBatch)
            {
                portraitOps = 0;
                yield return null;
            }
        }

        foreach (var tracker in MinigameMapTracker.AllTrackers)
        {
            if (tracker == null || !tracker.IsDiscovered)
                continue;

            var _ = tracker.Portrait;
            warmedPortraits++;

            portraitOps++;
            if (portraitOps >= portraitBatch)
            {
                portraitOps = 0;
                yield return null;
            }
        }

        // Phase B: Pre-create discovered markers/legend entries while overlay is hidden.
        int markerOps = 0;
        foreach (var tracker in NPCMapTracker.AllTrackers)
        {
            if (tracker == null || !tracker.IsDiscovered)
                continue;

            if (_npcDots.ContainsKey(tracker))
                continue;

            CreateNPCDot(tracker);
            SetNPCVisualState(tracker, false);
            createdMarkers++;

            markerOps++;
            if (markerOps >= markerBatch)
            {
                markerOps = 0;
                yield return null;
            }
        }

        foreach (var tracker in MinigameMapTracker.AllTrackers)
        {
            if (tracker == null || !tracker.IsDiscovered)
                continue;

            if (_minigameDots.ContainsKey(tracker))
                continue;

            CreateMinigameDot(tracker);
            SetMinigameVisualState(tracker, false);
            createdMarkers++;

            markerOps++;
            if (markerOps >= markerBatch)
            {
                markerOps = 0;
                yield return null;
            }
        }

        _isPrewarming = false;
        _prewarmComplete = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[RoomMapUI] Prewarm complete. Portraits={warmedPortraits}, Markers={createdMarkers}");
#endif
    }

    private void EnqueueNPCMarkerCreate(NPCMapTracker tracker)
    {
        if (tracker == null || _npcDots.ContainsKey(tracker))
            return;

        if (_pendingNpcCreateSet.Add(tracker))
            _pendingNpcCreates.Enqueue(tracker);
    }

    private void EnqueueMinigameMarkerCreate(MinigameMapTracker tracker)
    {
        if (tracker == null || _minigameDots.ContainsKey(tracker))
            return;

        if (_pendingMinigameCreateSet.Add(tracker))
            _pendingMinigameCreates.Enqueue(tracker);
    }

    private void ProcessPendingMarkerCreates(int budgetPerFrame)
    {
        int budget = Mathf.Max(1, budgetPerFrame);
        int consumed = 0;

        while (consumed < budget && _pendingNpcCreates.Count > 0)
        {
            var tracker = _pendingNpcCreates.Dequeue();
            _pendingNpcCreateSet.Remove(tracker);

            if (tracker == null || _npcDots.ContainsKey(tracker))
                continue;

            if (!tracker.IsDiscovered && !_debugRevealAllNPCs)
                continue;

            CreateNPCDot(tracker);
            consumed++;
        }

        while (consumed < budget && _pendingMinigameCreates.Count > 0)
        {
            var tracker = _pendingMinigameCreates.Dequeue();
            _pendingMinigameCreateSet.Remove(tracker);

            if (tracker == null || _minigameDots.ContainsKey(tracker))
                continue;

            if (!tracker.IsDiscovered && !_debugRevealAllNPCs)
                continue;

            CreateMinigameDot(tracker);
            consumed++;
        }
    }

    private void SetNPCVisualState(NPCMapTracker tracker, bool active)
    {
        if (tracker == null)
            return;

        if (_npcDots.TryGetValue(tracker, out var dot) && dot != null)
            dot.gameObject.SetActive(active);

        if (_npcLabels.TryGetValue(tracker, out var label) && label != null)
            label.gameObject.SetActive(active);

        if (_legendEntries.TryGetValue(tracker, out var legend) && legend != null)
            legend.SetActive(active);
    }

    private void SetMinigameVisualState(MinigameMapTracker tracker, bool active)
    {
        if (tracker == null)
            return;

        if (_minigameDots.TryGetValue(tracker, out var dot) && dot != null)
            dot.gameObject.SetActive(active);

        if (_minigameLabels.TryGetValue(tracker, out var label) && label != null)
            label.gameObject.SetActive(active);

        if (_minigameLegendEntries.TryGetValue(tracker, out var legend) && legend != null)
            legend.SetActive(active && tracker.IncludeInLegend);
    }

    private System.Collections.IEnumerator FadeMap(float from, float to, System.Action onComplete = null)
    {
        _isFading = true;
        _canvasGroup.alpha = from;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = to;
        _isFading = false;
        _fadeCoroutine = null;
        onComplete?.Invoke();
    }

    // ─────────────────── Auto-Populate World Bounds ───────────────────

    /// <summary>
    /// Reads every RoomZoneTag in the scene and copies its world-space
    /// centre + radius into the matching RoomMapData.Room entry.
    /// Runs once at Start so the map just works — no editor steps needed.
    /// </summary>
    private void PopulateWorldBoundsFromScene()
    {
        foreach (var zone in RoomZoneTag.AllZones)
        {
            var room = mapData.GetRoom(zone.roomId);
            if (room == null) continue;

            room.worldCenter = zone.WorldCenter;
            room.worldRadius = zone.WorldRadius;
        }
    }

    // ─────────────────── Build UI ───────────────────

    private void BuildCanvas()
    {
        var go = new GameObject("RoomMapCanvas");
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100; // above most UI
        _canvas.pixelPerfect = true; // Crisp rendering

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    private void BuildOverlay()
    {
        // ── Root full-screen panel ──
        _root = MakeUIObject("MapOverlay", _canvas.transform);
        var rootRect = Stretch(_root);

        // Dark scrim behind the journal
        var scrim = _root.AddComponent<Image>();
        scrim.color = overlayColor;
        scrim.raycastTarget = true; // blocks clicks through

        // ── Journal background image (centred, preserving aspect ratio) ──
        Transform mapContentParent = _root.transform; // default parent for map content
        if (journalSprite != null)
        {
            var journalGO = MakeUIObject("JournalBG", _root.transform);
            var journalRT = journalGO.GetComponent<RectTransform>();
            journalRT.anchorMin = new Vector2(0.5f, 0.5f);
            journalRT.anchorMax = new Vector2(0.5f, 0.5f);
            journalRT.pivot = new Vector2(0.5f, 0.5f);
            journalRT.anchoredPosition = Vector2.zero;

            // Size the journal to fill most of the screen while keeping aspect ratio
            float spriteW = journalSprite.rect.width;
            float spriteH = journalSprite.rect.height;
            float aspect = spriteW / spriteH;
            // Target ~85% of the reference resolution height
            float targetH = 1080f * 0.85f;
            float targetW = targetH * aspect;
            // Clamp width to 90% of reference width
            if (targetW > 1920f * 0.90f)
            {
                targetW = 1920f * 0.90f;
                targetH = targetW / aspect;
            }
            journalRT.sizeDelta = new Vector2(targetW, targetH);

            var journalImg = journalGO.AddComponent<Image>();
            journalImg.sprite = journalSprite;
            journalImg.preserveAspect = true;
            journalImg.raycastTarget = false;

            mapContentParent = journalGO.transform;
        }

        _mapContentParent = mapContentParent; // Store for legend panel

        // ── Title (inside the journal) ──
        var titleGO = MakeUIObject("MapTitle", mapContentParent);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot     = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, 10);
        titleRect.sizeDelta = new Vector2(500, 50);

        var titleText = titleGO.AddComponent<Text>();
        titleText.text = mapTitle;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = titleFontSize;
        titleText.color = fontColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;

        // ── Hint (below the journal) ──
        var hintGO = MakeUIObject("MapHint", _root.transform);
        var hintRect = hintGO.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot     = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0, 15);
        hintRect.sizeDelta = new Vector2(400, 30);

        var hintText = hintGO.AddComponent<Text>();
        hintText.text = $"Press {toggleKey} to close";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 16;
        hintText.color = new Color(fontColor.r, fontColor.g, fontColor.b, 0.5f);
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.raycastTarget = false;

        // ── Map area (padded region inside the journal pages) ──
        var mapGO = MakeUIObject("MapArea", mapContentParent);
        _mapArea = mapGO.GetComponent<RectTransform>();
        if (journalSprite != null)
        {
            // Inset the map area with generous padding to stay within the journal pages
            // The open journal has a spine/binding area — offset content slightly right
            _mapArea.anchorMin = new Vector2(0.12f, 0.12f);
            _mapArea.anchorMax = new Vector2(0.88f, 0.80f);
        }
        else
        {
            _mapArea.anchorMin = new Vector2(0.08f, 0.10f);
            _mapArea.anchorMax = new Vector2(0.92f, 0.88f);
        }
        _mapArea.offsetMin = Vector2.zero;
        _mapArea.offsetMax = Vector2.zero;

        // ── Castle blueprint map image (fills the map area) ──
        if (castleMapSprite != null)
        {
            var blueprintGO = MakeUIObject("CastleBlueprint", _mapArea);
            var blueprintRT = Stretch(blueprintGO);

            var blueprintImg = blueprintGO.AddComponent<Image>();
            blueprintImg.sprite = castleMapSprite;
            blueprintImg.preserveAspect = true;
            blueprintImg.raycastTarget = false;

            // Ensure crisp pixel-art rendering
            if (castleMapSprite.texture != null)
                castleMapSprite.texture.filterMode = FilterMode.Point;
        }
        else
        {
            Debug.LogWarning("[RoomMapUI] No castleMapSprite assigned. " +
                "Assign ColorizedMap.png from Assets/Sprites/gfx/gfx/ColorizedMap.png.");
        }

        // ── Player marker (on top of the blueprint) ──
        _playerMarker = CreatePortraitMarker(
            "PlayerMarker", _mapArea,
            playerPortrait, playerFrameTint,
            playerDotColor, "You",
            isPlayer: true);

        // ── Split legend panels in journal whitespace ──
        BuildCharacterLegendPanel();
        BuildMinigameLegendPanel();
    }

    // ─────────────────── Real-Time Player Tracking ───────────────────

    private void UpdatePlayerDot()
    {
        if (_playerTransform == null)
        {
            _playerMarker.gameObject.SetActive(false);
            return;
        }

        _playerMarker.gameObject.SetActive(true);
        Vector2 mapPos = mapData.WorldToMapPosition(_playerTransform.position);
        _playerMarker.anchorMin = _playerMarker.anchorMax = mapPos;
        _playerMarker.anchoredPosition = Vector2.zero;
    }

    // ─────────────────── Real-Time NPC Tracking ───────────────────

    private void UpdateNPCDots()
    {
        // Mark existing dots for potential cleanup
        var staleTrackers = new HashSet<NPCMapTracker>(_npcDots.Keys);

        foreach (var tracker in NPCMapTracker.AllTrackers)
        {
            staleTrackers.Remove(tracker);

            // Only show NPCs the player has interacted with (unless debug reveal is active)
            bool shouldShow = tracker.IsDiscovered || _debugRevealAllNPCs;
            
            if (!shouldShow)
            {
                if (_npcDots.ContainsKey(tracker))
                {
                    _npcDots[tracker].gameObject.SetActive(false);
                    if (_legendEntries.ContainsKey(tracker))
                        _legendEntries[tracker].SetActive(false);
                    if (_npcLabels.ContainsKey(tracker))
                        _npcLabels[tracker].gameObject.SetActive(false);
                }
                continue;
            }

            // Create dot if we haven't yet
            if (!_npcDots.ContainsKey(tracker))
            {
                EnqueueNPCMarkerCreate(tracker);
                continue;
            }

            // Position the marker group
            var dot = _npcDots[tracker];
            dot.gameObject.SetActive(true);
            Vector2 mapPos = mapData.WorldToMapPosition(tracker.WorldPosition);
            dot.anchorMin = dot.anchorMax = mapPos;
            dot.anchoredPosition = Vector2.zero;

            // Label is a child of the marker — just ensure it's active
            if (_npcLabels.TryGetValue(tracker, out var label))
                label.gameObject.SetActive(true);

            // Show legend entry
            if (_legendEntries.ContainsKey(tracker))
                _legendEntries[tracker].SetActive(true);
        }

        // Clean up dots for trackers that are gone (destroyed NPC, etc.)
        foreach (var stale in staleTrackers)
        {
            if (_npcDots.TryGetValue(stale, out var dot))
            {
                Destroy(dot.gameObject);
                _npcDots.Remove(stale);
            }
            if (_npcLabels.TryGetValue(stale, out var label))
            {
                Destroy(label.gameObject);
                _npcLabels.Remove(stale);
            }
            if (_legendEntries.TryGetValue(stale, out var entry))
            {
                Destroy(entry);
                _legendEntries.Remove(stale);
            }
        }
    }

    private void CreateNPCDot(NPCMapTracker tracker)
    {
        // Create a portrait marker group for this NPC
        float markerSize = portraitMarkerSize;
        float totalHeight = markerSize * frameScale + 4f + npcLabelSize; // frame + gap + label

        var markerRT = CreatePortraitMarker(
            "NPC_" + tracker.DisplayName, _mapArea,
            tracker.Portrait, npcFrameTint,
            tracker.MarkerColor, tracker.DisplayName,
            isPlayer: false);

        _npcDots[tracker] = markerRT;

        // The label is the last child of the marker group
        var labelTransform = markerRT.Find("Label_" + tracker.DisplayName);
        if (labelTransform != null)
        {
            var label = labelTransform.GetComponent<Text>();
            _npcLabels[tracker] = label;
        }

        // Add a legend entry
        CreateLegendEntry(tracker);
    }

    private void UpdateMinigameDots()
    {
        var staleTrackers = new HashSet<MinigameMapTracker>(_minigameDots.Keys);

        foreach (var tracker in MinigameMapTracker.AllTrackers)
        {
            staleTrackers.Remove(tracker);

            bool shouldShow = tracker.IsDiscovered || _debugRevealAllNPCs;
            if (!shouldShow)
            {
                if (_minigameDots.TryGetValue(tracker, out var hiddenDot))
                {
                    hiddenDot.gameObject.SetActive(false);
                }
                if (_minigameLabels.TryGetValue(tracker, out var hiddenLabel))
                {
                    hiddenLabel.gameObject.SetActive(false);
                }
                if (_minigameLegendEntries.TryGetValue(tracker, out var hiddenEntry))
                {
                    hiddenEntry.SetActive(false);
                }
                continue;
            }

            if (!_minigameDots.ContainsKey(tracker))
            {
                EnqueueMinigameMarkerCreate(tracker);
                continue;
            }

            var dot = _minigameDots[tracker];
            dot.gameObject.SetActive(true);
            Vector2 mapPos = mapData.WorldToMapPosition(tracker.WorldPosition);
            dot.anchorMin = dot.anchorMax = mapPos;
            dot.anchoredPosition = Vector2.zero;

            if (_minigameLabels.TryGetValue(tracker, out var label))
                label.gameObject.SetActive(true);

            if (_minigameLegendEntries.TryGetValue(tracker, out var legendEntry))
                legendEntry.SetActive(tracker.IncludeInLegend);
        }

        foreach (var stale in staleTrackers)
        {
            if (_minigameDots.TryGetValue(stale, out var dot))
            {
                Destroy(dot.gameObject);
                _minigameDots.Remove(stale);
            }
            if (_minigameLabels.TryGetValue(stale, out var label))
            {
                Destroy(label.gameObject);
                _minigameLabels.Remove(stale);
            }
            if (_minigameLegendEntries.TryGetValue(stale, out var entry))
            {
                Destroy(entry);
                _minigameLegendEntries.Remove(stale);
            }
        }

        UpdateMinigameLegendTitleVisibility();
    }

    private void CreateMinigameDot(MinigameMapTracker tracker)
    {
        var markerRT = CreatePortraitMarker(
            "Minigame_" + tracker.DisplayName,
            _mapArea,
            tracker.Portrait,
            npcFrameTint,
            tracker.MarkerColor,
            tracker.DisplayName,
            isPlayer: false);

        _minigameDots[tracker] = markerRT;

        var labelTransform = markerRT.Find("Label_" + tracker.DisplayName);
        if (labelTransform != null)
        {
            var label = labelTransform.GetComponent<Text>();
            _minigameLabels[tracker] = label;
        }

        if (tracker.IncludeInLegend)
            CreateMinigameLegendEntry(tracker);
    }

    // ─────────────────── Portrait Marker Factory ───────────────────

    /// <summary>
    /// Creates a Wizard101-style portrait marker: a framed character portrait
    /// with a name label underneath. Falls back to a coloured circle if no
    /// portrait sprite is available.
    /// </summary>
    /// <returns>The root RectTransform of the marker group.</returns>
    private RectTransform CreatePortraitMarker(
        string objectName, Transform parent,
        Sprite portrait, Color frameTint,
        Color fallbackColor, string label,
        bool isPlayer)
    {
        float size = isPlayer ? Mathf.Max(playerDotSize, portraitMarkerSize) : portraitMarkerSize;
        float framedSize = size * frameScale;

        // Root container — anchored at 0,0; repositioned each frame via anchors
        var rootGO = MakeUIObject(objectName, parent);
        var rootRT = rootGO.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(framedSize, framedSize + 4f + npcLabelSize);
        rootRT.pivot = new Vector2(0.5f, 0.5f);

        bool hasPortrait = portrait != null;
        bool hasFrame = portraitFrame != null;

        if (hasPortrait)
        {
            // ── Frame behind the portrait ──
            if (hasFrame)
            {
                var frameGO = MakeUIObject("Frame", rootGO.transform);
                var frameRT = frameGO.GetComponent<RectTransform>();
                frameRT.anchorMin = frameRT.anchorMax = new Vector2(0.5f, 1f);
                frameRT.pivot = new Vector2(0.5f, 1f);
                frameRT.sizeDelta = new Vector2(framedSize, framedSize);
                frameRT.anchoredPosition = Vector2.zero;

                var frameImg = frameGO.AddComponent<Image>();
                frameImg.sprite = portraitFrame;
                frameImg.color = frameTint;
                frameImg.type = Image.Type.Sliced;
                frameImg.raycastTarget = false;
            }

            // ── Portrait image (slightly smaller, sits inside the frame) ──
            var portraitGO = MakeUIObject("Portrait", rootGO.transform);
            var portraitRT = portraitGO.GetComponent<RectTransform>();
            portraitRT.anchorMin = portraitRT.anchorMax = new Vector2(0.5f, 1f);
            portraitRT.pivot = new Vector2(0.5f, 1f);
            portraitRT.sizeDelta = new Vector2(size, size);
            // Offset slightly inside the frame
            float inset = (framedSize - size) * 0.5f;
            portraitRT.anchoredPosition = new Vector2(0f, -inset);

            var portraitImg = portraitGO.AddComponent<Image>();
            portraitImg.sprite = portrait;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;

            // Mask the portrait to a circle for a clean look
            var mask = portraitGO.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            if (isPlayer) _playerPortraitImage = portraitImg;
        }
        else
        {
            // ── Fallback: coloured circle dot (original style) ──
            var dotGO = MakeUIObject("Dot", rootGO.transform);
            var dotRT = dotGO.GetComponent<RectTransform>();
            dotRT.anchorMin = dotRT.anchorMax = new Vector2(0.5f, 1f);
            dotRT.pivot = new Vector2(0.5f, 1f);
            dotRT.sizeDelta = new Vector2(size, size);
            dotRT.anchoredPosition = Vector2.zero;

            var dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = _circleSprite;
            dotImg.color = fallbackColor;
            dotImg.raycastTarget = false;

            if (isPlayer) _playerPortraitImage = dotImg;
        }

        // ── Name label below the portrait ──
        var labelGO = MakeUIObject("Label_" + label, rootGO.transform);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = labelRT.anchorMax = new Vector2(0.5f, 1f);
        labelRT.pivot = new Vector2(0.5f, 1f);
        labelRT.sizeDelta = new Vector2(120, npcLabelSize + 4);
        labelRT.anchoredPosition = new Vector2(0f, -(framedSize + 2f));

        var labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = npcLabelSize;
        labelText.color = npcLabelColor;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        labelText.verticalOverflow = VerticalWrapMode.Overflow;
        labelText.raycastTarget = false;

        // Shadow for readability
        var shadow = labelGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);

        return rootRT;
    }

    // ─────────────────── Legend Panel ───────────────────

    private void BuildCharacterLegendPanel()
    {
        // Container anchored to the bottom-left whitespace of the journal
        var panelGO = MakeUIObject("CharacterLegend", _mapContentParent);
        _characterLegendPanel = panelGO.GetComponent<RectTransform>();
        _characterLegendPanel.anchorMin = new Vector2(0.2f, 0.12f);
        _characterLegendPanel.anchorMax = new Vector2(0.2f, 0.12f);
        _characterLegendPanel.pivot = new Vector2(0f, 0f);
        _characterLegendPanel.anchoredPosition = Vector2.zero;
        _characterLegendPanel.sizeDelta = new Vector2(120f, 30f); // will grow

        // Semi-transparent background
        var bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.05f, 0.08f, 0.85f);
        bg.raycastTarget = false;

        // Vertical layout
        var layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 4;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;

        // Content size fitter so it grows with entries
        var fitter = panelGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Legend title
        var titleGO = MakeUIObject("LegendTitle", _characterLegendPanel);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(160, 20);

        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "— Characters —";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 13;
        titleText.color = new Color(fontColor.r, fontColor.g, fontColor.b, 0.6f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;

        // Player entry (always visible)
        CreateLegendRow(_characterLegendPanel, "You", playerDotColor, true, playerPortrait);
    }

    private void BuildMinigameLegendPanel()
    {
        // Container anchored to the bottom-right whitespace of the journal
        var panelGO = MakeUIObject("MinigameLegend", _mapContentParent);
        _minigameLegendPanel = panelGO.GetComponent<RectTransform>();
        _minigameLegendPanel.anchorMin = new Vector2(0.70f, 0.12f);
        _minigameLegendPanel.anchorMax = new Vector2(0.70f, 0.12f);
        _minigameLegendPanel.pivot = new Vector2(1f, 0f);
        _minigameLegendPanel.anchoredPosition = Vector2.zero;
        _minigameLegendPanel.sizeDelta = new Vector2(120f, 30f); // will grow

        // Semi-transparent background
        var bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.05f, 0.08f, 0.85f);
        bg.raycastTarget = false;

        // Vertical layout
        var layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 4;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;

        // Content size fitter so it grows with entries
        var fitter = panelGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Minigames section title (shown only when at least one discovered minigame is visible)
        _minigameLegendTitle = MakeUIObject("MinigameLegendTitle", _minigameLegendPanel).gameObject;
        var minigameTitleRT = _minigameLegendTitle.GetComponent<RectTransform>();
        minigameTitleRT.sizeDelta = new Vector2(160, 20);

        var minigameTitleText = _minigameLegendTitle.AddComponent<Text>();
        minigameTitleText.text = "— Minigames —";
        minigameTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        minigameTitleText.fontSize = 13;
        minigameTitleText.color = new Color(fontColor.r, fontColor.g, fontColor.b, 0.6f);
        minigameTitleText.alignment = TextAnchor.MiddleCenter;
        minigameTitleText.raycastTarget = false;
        _minigameLegendTitle.SetActive(false);
    }

    private void CreateLegendEntry(NPCMapTracker tracker)
    {
        if (_characterLegendPanel == null)
            return;

        var entry = CreateLegendRow(_characterLegendPanel, tracker.DisplayName, tracker.MarkerColor, false, tracker.Portrait);
        _legendEntries[tracker] = entry;
    }

    private void CreateMinigameLegendEntry(MinigameMapTracker tracker)
    {
        if (_minigameLegendPanel == null)
            return;

        var entry = CreateLegendRow(_minigameLegendPanel, tracker.DisplayName, tracker.MarkerColor, false, tracker.Portrait);
        _minigameLegendEntries[tracker] = entry;
        UpdateMinigameLegendTitleVisibility();
    }

    private void UpdateMinigameLegendTitleVisibility()
    {
        if (_minigameLegendTitle == null)
            return;

        bool hasVisibleEntries = false;
        foreach (var kvp in _minigameLegendEntries)
        {
            if (kvp.Value != null && kvp.Value.activeSelf)
            {
                hasVisibleEntries = true;
                break;
            }
        }

        _minigameLegendTitle.SetActive(hasVisibleEntries);
    }

    private GameObject CreateLegendRow(RectTransform parent, string label, Color dotColor, bool alwaysVisible, Sprite portrait = null)
    {
        var rowGO = MakeUIObject("Legend_" + label, parent);
        var rowRT = rowGO.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(180, 22);

        var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 6;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.padding = new RectOffset(2, 2, 0, 0);

        bool hasPortrait = portrait != null;

        if (hasPortrait)
        {
            // Small portrait thumbnail with optional frame
            float thumbSize = 18f;

            // Frame container
            var thumbContainerGO = MakeUIObject("ThumbContainer", rowGO.transform);
            var thumbContainerRT = thumbContainerGO.GetComponent<RectTransform>();
            thumbContainerRT.sizeDelta = new Vector2(thumbSize, thumbSize);
            var thumbContainerLE = thumbContainerGO.AddComponent<LayoutElement>();
            thumbContainerLE.preferredWidth = thumbSize;
            thumbContainerLE.preferredHeight = thumbSize;

            // Portrait image
            var thumbImg = thumbContainerGO.AddComponent<Image>();
            thumbImg.sprite = portrait;
            thumbImg.preserveAspect = true;
            thumbImg.raycastTarget = false;
            
            // Ensure crisp rendering (no bilinear filtering)
            if (portrait != null && portrait.texture != null)
            {
                portrait.texture.filterMode = FilterMode.Point;
            }

            // Frame overlay (if available)
            if (portraitFrame != null)
            {
                var frameOverlayGO = MakeUIObject("FrameOverlay", thumbContainerGO.transform);
                var frameOverlayRT = frameOverlayGO.GetComponent<RectTransform>();
                frameOverlayRT.anchorMin = Vector2.zero;
                frameOverlayRT.anchorMax = Vector2.one;
                float expand = 2f;
                frameOverlayRT.offsetMin = new Vector2(-expand, -expand);
                frameOverlayRT.offsetMax = new Vector2(expand, expand);

                var frameOverlayImg = frameOverlayGO.AddComponent<Image>();
                frameOverlayImg.sprite = portraitFrame;
                frameOverlayImg.color = alwaysVisible ? playerFrameTint : npcFrameTint;
                frameOverlayImg.type = Image.Type.Sliced;
                frameOverlayImg.raycastTarget = false;
            }
        }
        else
        {
            // Fallback: colour dot
            var colorGO = MakeUIObject("Dot", rowGO.transform);
            var colorRT = colorGO.GetComponent<RectTransform>();
            colorRT.sizeDelta = new Vector2(10, 10);
            var colorImg = colorGO.AddComponent<Image>();
            colorImg.sprite = _circleSprite;
            colorImg.color = dotColor;
            colorImg.raycastTarget = false;
            var colorLE = colorGO.AddComponent<LayoutElement>();
            colorLE.preferredWidth = 10;
            colorLE.preferredHeight = 10;
        }

        // Name label
        var nameGO = MakeUIObject("Name", rowGO.transform);
        var nameText = nameGO.AddComponent<Text>();
        nameText.text = label;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 12;
        nameText.color = npcLabelColor;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.raycastTarget = false;
        var nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.preferredWidth = 140;
        nameLE.preferredHeight = 22;

        return rowGO;
    }

    // ─────────────────── Player Dot Pulse ───────────────────

    private void AnimatePlayerDot()
    {
        if (_playerMarker == null || !_playerMarker.gameObject.activeSelf) return;

        // Use unscaled time because the game is paused while the map is open
        float t = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
        float s = Mathf.Lerp(1f, pulseScale, t);
        _playerMarker.localScale = new Vector3(s, s, 1f);
    }

    // ─────────────────── Helpers ───────────────────

    private static GameObject MakeUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static RectTransform Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static Sprite MakeCircleSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float center = res * 0.5f;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float dx = x - center + 0.5f;
            float dy = y - center + 0.5f;
            float alpha = Mathf.Clamp01(center - Mathf.Sqrt(dx * dx + dy * dy));
            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }
}
