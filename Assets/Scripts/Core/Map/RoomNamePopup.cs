using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Systems;

/// <summary>
/// Displays a room name popup when entering new areas.
/// Self-contained - creates its own UI at runtime.
/// Just add this component to any active GameObject in your scene.
/// </summary>
public class RoomNamePopup : MonoBehaviour
{
    [Header("Room Data")]
    [SerializeField] private RoomMapData roomMapData;

    [Header("Appearance")]
    [SerializeField] private Sprite journalSprite;
    [SerializeField] private float popupWidth = 500f;
    [SerializeField] private float popupHeight = 150f;
    [SerializeField] private float topOffset = 50f;

    [Header("Text Settings")]
    [SerializeField] private float maxFontSize = 42f;
    [SerializeField] private float minFontSize = 28f;
    [SerializeField] private Color textColor = new Color(0.2f, 0.15f, 0.1f, 1f);

    [Header("Animation")]
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private float fadeOutTime = 0.5f;

    // Runtime UI references
    private Canvas popupCanvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI textComponent;
    private string lastRoomId;
    private string pendingRoomId;
    private CharacterMotor2D characterMotor;
    private CanvasGroup fadeCanvasGroup;
    private ScreenFader screenFader;
    private Coroutine animationCoroutine;

    void Awake()
    {
        CreateUI();
        RoomTracker.OnRoomChanged += HandleRoomChanged;
        TeleportSystem.OnAnyTeleportCompleted += HandleTeleportCompleted;
    }

    void Start()
    {
        // Check if already in a room
        StartCoroutine(DelayedInitialCheck());
    }

    void Update()
    {
        ForceHidePopupIfTransitionActive();
        TryShowPendingRoomIfVisible();
    }

    void OnDestroy()
    {
        RoomTracker.OnRoomChanged -= HandleRoomChanged;
        TeleportSystem.OnAnyTeleportCompleted -= HandleTeleportCompleted;
    }

    private IEnumerator DelayedInitialCheck()
    {
        yield return null;
        yield return null;

        string currentRoom = RoomTracker.CurrentRoomId;
        if (!string.IsNullOrEmpty(currentRoom) && currentRoom != lastRoomId)
        {
            HandleRoomChanged(currentRoom);
        }
    }

    private void CreateUI()
    {
        // Create a dedicated Canvas for the popup
        GameObject canvasObj = new GameObject("RoomNamePopup_Canvas");
        canvasObj.transform.SetParent(transform);

        popupCanvas = canvasObj.AddComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = 100; // Render on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create popup container
        GameObject containerObj = new GameObject("PopupContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0f, -topOffset);
        containerRect.sizeDelta = new Vector2(popupWidth, popupHeight);

        canvasGroup = containerObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Create background image
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(containerObj.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        if (journalSprite != null)
        {
            bgImage.sprite = journalSprite;
            bgImage.preserveAspect = true;
        }
        else
        {
            bgImage.color = new Color(0.85f, 0.75f, 0.55f, 1f);
        }

        // Create text
        GameObject textObj = new GameObject("RoomNameText");
        textObj.transform.SetParent(containerObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(50f, 40f);
        textRect.offsetMax = new Vector2(-50f, -40f);

        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = minFontSize;
        textComponent.fontSizeMax = maxFontSize;
        textComponent.color = textColor;
        textComponent.fontStyle = FontStyles.Bold;
    }

    private void HandleRoomChanged(string roomId)
    {
        if (string.IsNullOrEmpty(roomId)) return;
        if (roomId == lastRoomId) return;

        if (IsVisualTransitionActive())
        {
            pendingRoomId = roomId;
            return;
        }

        ShowRoomById(roomId);
    }

    private void HandleTeleportCompleted()
    {
        if (string.IsNullOrEmpty(pendingRoomId)) return;
        if (IsVisualTransitionActive()) return;

        string roomToShow = pendingRoomId;
        pendingRoomId = null;

        if (roomToShow == lastRoomId) return;
        ShowRoomById(roomToShow);
    }

    private bool IsTeleportInProgress()
    {
        if (characterMotor == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                characterMotor = player.GetComponent<CharacterMotor2D>();
            }
        }

        return characterMotor != null && characterMotor.IsTeleporting;
    }

    private bool IsVisualTransitionActive()
    {
        if (IsTeleportInProgress()) return true;

        if (fadeCanvasGroup == null)
        {
            var fadeObj = GameObject.Find("FadeCanvasGroup");
            if (fadeObj != null)
            {
                fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
            }
        }

        if (fadeCanvasGroup != null && fadeCanvasGroup.alpha > 0.01f)
        {
            return true;
        }

        if (screenFader == null)
        {
            screenFader = FindFirstObjectByType<ScreenFader>();
        }

        if (screenFader != null &&
            screenFader.fadePanel != null &&
            screenFader.fadePanel.gameObject.activeInHierarchy &&
            screenFader.fadePanel.color.a > 0.01f)
        {
            return true;
        }

        return false;
    }

    private void TryShowPendingRoomIfVisible()
    {
        if (string.IsNullOrEmpty(pendingRoomId)) return;
        if (IsVisualTransitionActive()) return;

        string roomToShow = pendingRoomId;
        pendingRoomId = null;

        if (roomToShow == lastRoomId) return;
        ShowRoomById(roomToShow);
    }

    private void ForceHidePopupIfTransitionActive()
    {
        if (!IsVisualTransitionActive()) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void ShowRoomById(string roomId)
    {
        if (IsVisualTransitionActive())
        {
            pendingRoomId = roomId;
            return;
        }

        lastRoomId = roomId;

        string displayName = GetRoomDisplayName(roomId);
        if (!string.IsNullOrEmpty(displayName))
        {
            ShowPopup(displayName);
        }
    }

    private string GetRoomDisplayName(string roomId)
    {
        if (roomMapData != null)
        {
            var room = roomMapData.GetRoom(roomId);
            if (room != null && !string.IsNullOrEmpty(room.roomName))
            {
                return room.roomName;
            }
        }

        // Fallback: capitalize first letter
        if (!string.IsNullOrEmpty(roomId))
        {
            return char.ToUpper(roomId[0]) + roomId.Substring(1);
        }
        return roomId;
    }

    private void ShowPopup(string roomName)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimatePopup(roomName));
    }

    private IEnumerator AnimatePopup(string roomName)
    {
        textComponent.text = roomName;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Display
        yield return new WaitForSeconds(displayTime);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        animationCoroutine = null;
    }
}
