using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen room-map overlay for the Overworld scene.
/// Press M (configurable) to toggle the map on/off.
/// Shows every room as a labelled rectangle with connection lines
/// and highlights the room the player is currently in.
///
/// Setup:
///   1. Create a RoomMapData asset (Assets → Create → Castle of Time → Room Map Data)
///      and fill in the rooms, positions, sizes, and connections.
///   2. Add this script to a GameObject in the Overworld scene.
///   3. Assign the RoomMapData asset.
///   4. Play — press M to open/close the map.
/// </summary>
public class RoomMapUI : MonoBehaviour
{
    // ─────────────────── Inspector ───────────────────

    [Header("Data")]
    [SerializeField] private RoomMapData mapData;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.M;

    [Header("Overlay Appearance")]
    [Tooltip("Background colour of the overlay.")]
    [SerializeField] private Color overlayColor = new Color(0.04f, 0.03f, 0.06f, 0.92f);

    [Header("Room Appearance")]
    [SerializeField] private Color roomColor        = new Color(0.18f, 0.16f, 0.24f, 1f);
    [SerializeField] private Color roomBorderColor   = new Color(0.55f, 0.48f, 0.38f, 1f);
    [SerializeField] private Color currentRoomColor  = new Color(0.35f, 0.22f, 0.12f, 1f);
    [SerializeField] private Color currentBorderColor = new Color(1f, 0.82f, 0.45f, 1f);
    [SerializeField] private float borderWidth = 3f;

    [Header("Connection Lines")]
    [SerializeField] private Color lineColor = new Color(0.55f, 0.48f, 0.38f, 0.6f);
    [SerializeField] private float lineWidth = 3f;

    [Header("Player Indicator")]
    [SerializeField] private Color playerDotColor = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float playerDotSize  = 16f;
    [SerializeField] private float pulseSpeed     = 2.5f;
    [SerializeField] private float pulseScale     = 1.35f;

    [Header("Labels")]
    [SerializeField] private int   fontSize  = 18;
    [SerializeField] private Color fontColor = new Color(0.9f, 0.85f, 0.75f, 1f);

    [Header("Title")]
    [SerializeField] private string mapTitle = "Castle Map";
    [SerializeField] private int titleFontSize = 32;

    // ─────────────────── Runtime ───────────────────

    private Canvas _canvas;
    private GameObject _root;
    private RectTransform _mapArea;
    private bool _isOpen;

    // Caches
    private readonly Dictionary<string, Image>   _roomBgs     = new();
    private readonly Dictionary<string, Image>   _roomBorders = new();
    private readonly Dictionary<string, Text>    _roomLabels  = new();
    private RectTransform _playerDot;
    private Image _playerDotImage;

    // ─────────────────── Lifecycle ───────────────────

    void Start()
    {
        if (mapData == null)
        {
            Debug.LogError("RoomMapUI: No RoomMapData assigned.");
            enabled = false;
            return;
        }

        BuildCanvas();
        BuildOverlay();
        _root.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            // Don't open the map if another pause-level UI is open (pause menu, etc.)
            if (!_isOpen && GlobalPause.IsPaused) return;
            ToggleMap();
        }

        if (_isOpen)
            AnimatePlayerDot();
    }

    void OnDestroy()
    {
        if (_root != null) Destroy(_root);
    }

    // ─────────────────── Toggle ───────────────────

    private void ToggleMap()
    {
        _isOpen = !_isOpen;
        _root.SetActive(_isOpen);

        if (_isOpen)
        {
            GlobalPause.SetPaused(true);
            RefreshHighlight();
        }
        else
        {
            GlobalPause.SetPaused(false);
        }
    }

    // ─────────────────── Build UI ───────────────────

    private void BuildCanvas()
    {
        var go = new GameObject("RoomMapCanvas");
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100; // above most UI

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

        var bg = _root.AddComponent<Image>();
        bg.color = overlayColor;
        bg.raycastTarget = true; // blocks clicks through

        // ── Title ──
        var titleGO = MakeUIObject("MapTitle", _root.transform);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot     = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(500, 50);

        var titleText = titleGO.AddComponent<Text>();
        titleText.text = mapTitle;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = titleFontSize;
        titleText.color = fontColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;

        // ── Hint ──
        var hintGO = MakeUIObject("MapHint", _root.transform);
        var hintRect = hintGO.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot     = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0, 20);
        hintRect.sizeDelta = new Vector2(400, 30);

        var hintText = hintGO.AddComponent<Text>();
        hintText.text = $"Press {toggleKey} to close";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 16;
        hintText.color = new Color(fontColor.r, fontColor.g, fontColor.b, 0.5f);
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.raycastTarget = false;

        // ── Map area (padded region where rooms are placed) ──
        var mapGO = MakeUIObject("MapArea", _root.transform);
        _mapArea = mapGO.GetComponent<RectTransform>();
        _mapArea.anchorMin = new Vector2(0.08f, 0.10f);
        _mapArea.anchorMax = new Vector2(0.92f, 0.88f);
        _mapArea.offsetMin = Vector2.zero;
        _mapArea.offsetMax = Vector2.zero;

        // ── Draw connection lines first (behind rooms) ──
        DrawConnections();

        // ── Draw rooms ──
        foreach (var room in mapData.rooms)
            DrawRoom(room);

        // ── Player dot (on top) ──
        var dotGO = MakeUIObject("PlayerDot", _mapArea);
        _playerDot = dotGO.GetComponent<RectTransform>();
        _playerDot.sizeDelta = new Vector2(playerDotSize, playerDotSize);

        _playerDotImage = dotGO.AddComponent<Image>();
        _playerDotImage.sprite = MakeCircleSprite(64);
        _playerDotImage.color = playerDotColor;
        _playerDotImage.raycastTarget = false;
    }

    // ─────────────────── Draw Room ───────────────────

    private void DrawRoom(RoomMapData.Room room)
    {
        // Border (slightly larger rectangle behind the room fill)
        var borderGO = MakeUIObject(room.roomId + "_border", _mapArea);
        var borderRect = borderGO.GetComponent<RectTransform>();
        SetRoomRect(borderRect, room.mapPosition, room.mapSize, borderWidth);

        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = roomBorderColor;
        borderImg.raycastTarget = false;
        _roomBorders[room.roomId] = borderImg;

        // Fill
        var fillGO = MakeUIObject(room.roomId + "_fill", _mapArea);
        var fillRect = fillGO.GetComponent<RectTransform>();
        SetRoomRect(fillRect, room.mapPosition, room.mapSize, 0);

        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = roomColor;
        fillImg.raycastTarget = false;
        _roomBgs[room.roomId] = fillImg;

        // Label
        var labelGO = MakeUIObject(room.roomId + "_label", _mapArea);
        var labelRect = labelGO.GetComponent<RectTransform>();
        SetRoomRect(labelRect, room.mapPosition, room.mapSize, 0);

        var label = labelGO.AddComponent<Text>();
        label.text = room.roomName;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = fontColor;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        _roomLabels[room.roomId] = label;
    }

    private void SetRoomRect(RectTransform rt, Vector2 pos, Vector2 size, float expand)
    {
        // pos & size are in normalised 0-1 coords relative to _mapArea
        rt.anchorMin = pos - size * 0.5f;
        rt.anchorMax = pos + size * 0.5f;
        rt.offsetMin = new Vector2(-expand, -expand);
        rt.offsetMax = new Vector2(expand, expand);
    }

    // ─────────────────── Connections ───────────────────

    private void DrawConnections()
    {
        // Track drawn pairs so we don't double-draw A→B and B→A
        var drawn = new HashSet<string>();

        foreach (var room in mapData.rooms)
        {
            foreach (var otherId in room.connectedRoomIds)
            {
                string key = room.roomId.CompareTo(otherId) < 0
                    ? room.roomId + "|" + otherId
                    : otherId + "|" + room.roomId;

                if (drawn.Contains(key)) continue;
                drawn.Add(key);

                var other = mapData.GetRoom(otherId);
                if (other == null) continue;

                DrawLine(room.mapPosition, other.mapPosition);
            }
        }
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        var lineGO = MakeUIObject("Line", _mapArea);
        var lineRect = lineGO.GetComponent<RectTransform>();

        var img = lineGO.AddComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false;

        // Compute pixel-independent line using anchors
        Vector2 mid = (from + to) * 0.5f;
        Vector2 diff = to - from;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        // We'll place the line at the midpoint, rotated, with length = distance
        lineRect.anchorMin = lineRect.anchorMax = mid;
        lineRect.pivot = new Vector2(0.5f, 0.5f);

        // Length needs to be in the mapArea's local space — approximate with a helper
        // We use a LayoutRebuilder callback-free approach: set width via sizeDelta later in a helper
        // For now, just store and fix in a coroutine after layout settles
        lineRect.sizeDelta = new Vector2(0, lineWidth);
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(SetLineLength(lineRect, from, to));
    }

    private System.Collections.IEnumerator SetLineLength(RectTransform rt, Vector2 fromNorm, Vector2 toNorm)
    {
        // Wait one frame for layout to settle
        yield return null;

        // Compute length in mapArea pixels
        var mapRect = _mapArea.rect;
        Vector2 fromPx = new Vector2(fromNorm.x * mapRect.width, fromNorm.y * mapRect.height);
        Vector2 toPx   = new Vector2(toNorm.x * mapRect.width, toNorm.y * mapRect.height);
        float length = Vector2.Distance(fromPx, toPx);

        rt.sizeDelta = new Vector2(length, lineWidth);
    }

    // ─────────────────── Highlight Current Room ───────────────────

    private void RefreshHighlight()
    {
        string current = RoomTracker.CurrentRoomId;

        foreach (var room in mapData.rooms)
        {
            bool isCurrent = room.roomId == current;
            _roomBgs[room.roomId].color     = isCurrent ? currentRoomColor  : roomColor;
            _roomBorders[room.roomId].color  = isCurrent ? currentBorderColor : roomBorderColor;
        }

        // Move player dot
        var roomEntry = mapData.GetRoom(current);
        if (roomEntry != null)
        {
            _playerDot.gameObject.SetActive(true);
            _playerDot.anchorMin = _playerDot.anchorMax = roomEntry.mapPosition;
            _playerDot.anchoredPosition = Vector2.zero;
        }
        else
        {
            _playerDot.gameObject.SetActive(false);
        }
    }

    // ─────────────────── Player Dot Pulse ───────────────────

    private void AnimatePlayerDot()
    {
        if (_playerDot == null || !_playerDot.gameObject.activeSelf) return;

        // Use unscaled time because the game is paused while the map is open
        float t = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
        float s = Mathf.Lerp(1f, pulseScale, t);
        _playerDot.localScale = new Vector3(s, s, 1f);
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
