using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Systems;
using Systems.Overworld.Intro;

/// <summary>
/// Displays a room name popup when entering new areas.
/// Self-contained - creates its own UI at runtime.
/// Just add this component to any active GameObject in your scene.
/// </summary>
public class RoomNamePopup : MonoBehaviour
{
    private enum PopupPhase
    {
        None,
        FadeIn,
        Display,
        FadeOut
    }

    [Header("Room Data")]
    [SerializeField] private RoomMapData roomMapData;

    [Header("Appearance")]
    [SerializeField] private Sprite journalSprite;
    [SerializeField] private float popupWidth = 680f;
    [SerializeField] private float popupHeight = 240f;
    [SerializeField] private float topOffset = 50f;

    [Header("Text Settings")]
    [SerializeField] private float maxFontSize = 52f;
    [SerializeField] private float minFontSize = 24f;
    [SerializeField] private float textHorizontalPadding = 115f;
    [SerializeField] private float textVerticalPadding = 70f;
    [SerializeField] private float textVerticalOffset = 28f;
    [SerializeField] private Color textColor = new Color(0.2f, 0.15f, 0.1f, 1f);

    [Header("Journal Font")]
    [SerializeField] private KeyCode journalToggleKey = KeyCode.Q;
    [SerializeField] private TMP_FontAsset journalFontOverride;

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
    private RoomMapUI cachedRoomMapUI;
    private PopupPhase currentPhase = PopupPhase.None;
    private float currentPhaseElapsed;
    private string activeRoomName;
    private bool hasSuppressedBannerState;
    private PopupPhase suppressedPhase = PopupPhase.None;
    private float suppressedPhaseElapsed;
    private string suppressedRoomName;

    void Awake()
    {
        CreateUI();
        RoomTracker.OnRoomChanged += HandleRoomChanged;
        TeleportSystem.OnAnyTeleportCompleted += HandleTeleportCompleted;
        OverworldWakeUpCutscene.OnWakeUpSequenceCompleted += HandleWakeUpSequenceCompleted;
        RoomMapUI.OnMapVisibilityChanged += HandleMapVisibilityChanged;
    }

    void Start()
    {
        // Check if already in a room
        StartCoroutine(DelayedInitialCheck());
        ApplyJournalFont();
    }

    void Update()
    {
        ForceHidePopupIfTransitionActive();
        TryResumeSuppressedBannerIfVisible();
        TryShowPendingRoomIfVisible();

        if (Input.GetKeyDown(journalToggleKey))
        {
            ApplyJournalFont();
        }
    }

    void OnDestroy()
    {
        RoomTracker.OnRoomChanged -= HandleRoomChanged;
        TeleportSystem.OnAnyTeleportCompleted -= HandleTeleportCompleted;
        OverworldWakeUpCutscene.OnWakeUpSequenceCompleted -= HandleWakeUpSequenceCompleted;
        RoomMapUI.OnMapVisibilityChanged -= HandleMapVisibilityChanged;
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
        containerRect.localScale = new Vector3(0.5f, 0.5f, 1f);

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
        float effectiveVerticalOffset = Mathf.Max(textVerticalOffset, 28f);
        textRect.offsetMin = new Vector2(textHorizontalPadding, textVerticalPadding + effectiveVerticalOffset);
        textRect.offsetMax = new Vector2(-textHorizontalPadding, -textVerticalPadding + effectiveVerticalOffset);

        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableAutoSizing = true;
        textComponent.enableWordWrapping = false;
        textComponent.fontSizeMin = Mathf.Max(minFontSize * 3f, 24f);
        textComponent.fontSizeMax = Mathf.Max(maxFontSize * 3f, 52f);
        textComponent.color = textColor;
        textComponent.fontStyle = FontStyles.Bold;
    }

    private void HandleRoomChanged(string roomId)
    {
        if (string.IsNullOrEmpty(roomId)) return;
        if (roomId == lastRoomId) return;

        if (ShouldDeferBanner())
        {
            pendingRoomId = roomId;
            return;
        }

        ShowRoomById(roomId);
    }

    private void HandleTeleportCompleted()
    {
        if (string.IsNullOrEmpty(pendingRoomId)) return;
        if (ShouldDeferBanner()) return;

        string roomToShow = pendingRoomId;
        pendingRoomId = null;

        if (roomToShow == lastRoomId) return;
        ShowRoomById(roomToShow);
    }

    private void HandleWakeUpSequenceCompleted()
    {
        TryShowPendingRoomIfVisible();
    }

    private bool IsWakeUpSequenceBlockingBanner()
    {
        return OverworldWakeUpCutscene.IsWakeUpSequenceActive;
    }

    private bool ShouldDeferBanner()
    {
        if (RoomMapUI.IsMapVisible) return true;
        if (IsVisualTransitionActive()) return true;
        if (IsWakeUpSequenceBlockingBanner()) return true;

        return false;
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
        if (animationCoroutine != null) return;
        if (string.IsNullOrEmpty(pendingRoomId)) return;
        if (ShouldDeferBanner()) return;

        string roomToShow = pendingRoomId;
        pendingRoomId = null;

        if (roomToShow == lastRoomId) return;
        ShowRoomById(roomToShow);
    }

    private void ForceHidePopupIfTransitionActive()
    {
        if (!ShouldDeferBanner()) return;

        if (animationCoroutine != null)
        {
            if (RoomMapUI.IsMapVisible)
            {
                SaveActiveBannerStateForMap();
            }

            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            ClearActiveAnimationState();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void ApplyJournalFont()
    {
        if (textComponent == null) return;

        TMP_FontAsset fontToApply = ResolveJournalFont();
        if (fontToApply == null || textComponent.font == fontToApply)
        {
            return;
        }

        textComponent.font = fontToApply;
    }

    private TMP_FontAsset ResolveJournalFont()
    {
        if (journalFontOverride != null)
        {
            return journalFontOverride;
        }

        if (cachedRoomMapUI == null)
        {
            cachedRoomMapUI = FindFirstObjectByType<RoomMapUI>();
        }

        if (cachedRoomMapUI != null)
        {
            return cachedRoomMapUI.DialogueLabelFontAsset;
        }

        return null;
    }

    private void ShowRoomById(string roomId)
    {
        if (ShouldDeferBanner())
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
        ClearSuppressedBannerState();
        StartPopupAnimation(roomName, PopupPhase.FadeIn, 0f);
    }

    private void HandleMapVisibilityChanged(bool isMapVisible)
    {
        if (isMapVisible)
        {
            SuppressBannerForMap();
            return;
        }

        TryResumeSuppressedBannerIfVisible();
    }

    private void SuppressBannerForMap()
    {
        SaveActiveBannerStateForMap();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            ClearActiveAnimationState();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void SaveActiveBannerStateForMap()
    {
        if (animationCoroutine == null) return;
        if (string.IsNullOrEmpty(activeRoomName)) return;
        if (currentPhase == PopupPhase.None) return;

        hasSuppressedBannerState = true;
        suppressedRoomName = activeRoomName;
        suppressedPhase = currentPhase;
        suppressedPhaseElapsed = currentPhaseElapsed;
    }

    private void TryResumeSuppressedBannerIfVisible()
    {
        if (!hasSuppressedBannerState) return;
        if (animationCoroutine != null) return;
        if (ShouldDeferBanner()) return;

        StartPopupAnimation(suppressedRoomName, suppressedPhase, suppressedPhaseElapsed);
        ClearSuppressedBannerState();
    }

    private void StartPopupAnimation(string roomName, PopupPhase startPhase, float startElapsed)
    {
        if (string.IsNullOrEmpty(roomName)) return;

        if (startPhase == PopupPhase.None)
        {
            startPhase = PopupPhase.FadeIn;
            startElapsed = 0f;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        ApplyJournalFont();

        activeRoomName = roomName;
        currentPhase = startPhase;
        currentPhaseElapsed = Mathf.Max(0f, startElapsed);
        animationCoroutine = StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        if (textComponent == null || canvasGroup == null)
        {
            animationCoroutine = null;
            ClearActiveAnimationState();
            yield break;
        }

        textComponent.text = activeRoomName;

        while (true)
        {
            switch (currentPhase)
            {
                case PopupPhase.FadeIn:
                    if (fadeInTime <= 0f)
                    {
                        canvasGroup.alpha = 1f;
                        currentPhase = PopupPhase.Display;
                        currentPhaseElapsed = 0f;
                        break;
                    }

                    while (currentPhaseElapsed < fadeInTime)
                    {
                        currentPhaseElapsed += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Clamp01(currentPhaseElapsed / fadeInTime);
                        yield return null;
                    }

                    canvasGroup.alpha = 1f;
                    currentPhase = PopupPhase.Display;
                    currentPhaseElapsed = 0f;
                    break;

                case PopupPhase.Display:
                    if (displayTime <= 0f)
                    {
                        currentPhase = PopupPhase.FadeOut;
                        currentPhaseElapsed = 0f;
                        break;
                    }

                    while (currentPhaseElapsed < displayTime)
                    {
                        currentPhaseElapsed += Time.deltaTime;
                        yield return null;
                    }

                    currentPhase = PopupPhase.FadeOut;
                    currentPhaseElapsed = 0f;
                    break;

                case PopupPhase.FadeOut:
                    if (fadeOutTime <= 0f)
                    {
                        canvasGroup.alpha = 0f;
                        animationCoroutine = null;
                        ClearActiveAnimationState();
                        yield break;
                    }

                    while (currentPhaseElapsed < fadeOutTime)
                    {
                        currentPhaseElapsed += Time.deltaTime;
                        canvasGroup.alpha = 1f - Mathf.Clamp01(currentPhaseElapsed / fadeOutTime);
                        yield return null;
                    }

                    canvasGroup.alpha = 0f;
                    animationCoroutine = null;
                    ClearActiveAnimationState();
                    yield break;

                default:
                    canvasGroup.alpha = 0f;
                    animationCoroutine = null;
                    ClearActiveAnimationState();
                    yield break;
            }
        }
    }

    private void ClearActiveAnimationState()
    {
        currentPhase = PopupPhase.None;
        currentPhaseElapsed = 0f;
        activeRoomName = null;
    }

    private void ClearSuppressedBannerState()
    {
        hasSuppressedBannerState = false;
        suppressedPhase = PopupPhase.None;
        suppressedPhaseElapsed = 0f;
        suppressedRoomName = null;
    }
}
