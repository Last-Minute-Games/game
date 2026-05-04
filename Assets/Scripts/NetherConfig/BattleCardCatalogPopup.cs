using System.Collections.Generic;
using GameItems.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Battle-scene card codex: opens a panel listing all catalog cards and refreshes
/// locked/unlocked state when <see cref="GameFlags"/> change.
/// </summary>
public class BattleCardCatalogPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform entriesParent;
    [SerializeField] private BattleCardCatalogEntry entryPrefab;

    [Header("Unlock hint (single shared instance)")]
    [SerializeField] private RectTransform unlockHintPanel;
    [SerializeField] private TextMeshProUGUI unlockHintLabel;
    [SerializeField] private Vector2 hintScreenOffset = new Vector2(16f, -16f);

    [Header("Optional")]
    [Tooltip("If unset, uses FindFirstObjectByType at runtime.")]
    [SerializeField] private PlayerManager playerManager;

    private readonly List<BattleCardCatalogEntry> _entries = new List<BattleCardCatalogEntry>();
    /// <summary>True if this catalog called <see cref="GlobalPause.SetPaused"/> while opening.</summary>
    private bool _pausedByCatalog;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        HideUnlockHint();
        ConfigureHintToIgnoreRaycasts();
    }

    /// <summary>
    /// Hint must not block raycasts or it sits under the cursor and steals hover from the card → flicker.
    /// </summary>
    private void ConfigureHintToIgnoreRaycasts()
    {
        if (unlockHintPanel == null)
            return;

        foreach (var graphic in unlockHintPanel.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        foreach (var canvasGroup in unlockHintPanel.GetComponentsInChildren<CanvasGroup>(true))
            canvasGroup.blocksRaycasts = false;

        if (unlockHintLabel != null)
            unlockHintLabel.raycastTarget = false;
    }

    private void OnEnable()
    {
        if (GameFlags.Instance != null)
            GameFlags.Instance.OnFlagChanged += HandleFlagChanged;
    }

    private void OnDisable()
    {
        if (GameFlags.Instance != null)
            GameFlags.Instance.OnFlagChanged -= HandleFlagChanged;
    }

    private void OnDestroy()
    {
        if (GameFlags.Instance != null)
            GameFlags.Instance.OnFlagChanged -= HandleFlagChanged;
        ReleaseCatalogPause();
    }

    private void HandleFlagChanged(string _)
    {
        RefreshAllEntries();
    }

    /// <summary>
    /// Matches catalog display expectations: default-unlocked cards, otherwise flag from <see cref="CardData.unlockFlag"/>.
    /// </summary>
    public static bool IsCardUnlockedForCatalog(CardData card)
    {
        if (card == null)
            return false;
        if (card.unlockedByDefault)
            return true;
        if (string.IsNullOrEmpty(card.unlockFlag))
            return false;
        return GameFlags.HasFlag(card.unlockFlag);
    }

    public void Open()
    {
        bool alreadyOpen = panelRoot != null && panelRoot.activeSelf;
        if (!alreadyOpen)
        {
            // Same path as SimplePauseMenu / settings-over-pause: central pause (time scale, input, clock, journal).
            _pausedByCatalog = !GlobalPause.IsPaused;
            if (_pausedByCatalog)
                GlobalPause.SetPaused(true);
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);
        RebuildEntries();
    }

    public void Close()
    {
        HideUnlockHint();
        if (panelRoot != null)
            panelRoot.SetActive(false);
        ReleaseCatalogPause();
    }

    private void ReleaseCatalogPause()
    {
        if (!_pausedByCatalog)
            return;
        GlobalPause.SetPaused(false);
        _pausedByCatalog = false;
    }

    public void Toggle()
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Close();
        else
            Open();
    }

    private void RebuildEntries()
    {
        HideUnlockHint();

        if (entriesParent == null || entryPrefab == null)
            return;

        for (int i = entriesParent.childCount - 1; i >= 0; i--)
            Destroy(entriesParent.GetChild(i).gameObject);

        _entries.Clear();

        foreach (var card in GetCatalogCardsSorted())
        {
            var entry = Instantiate(entryPrefab, entriesParent);
            entry.Setup(card, this);
            _entries.Add(entry);
        }
    }

    private void RefreshAllEntries()
    {
        if (panelRoot == null || !panelRoot.activeSelf)
            return;

        HideUnlockHint();
        for (int i = 0; i < _entries.Count; i++)
            _entries[i]?.Refresh();
    }

    private List<CardData> GetCatalogCardsSorted()
    {
        var list = new List<CardData>();
        var seen = new HashSet<int>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        if (playerManager != null && playerManager.cardManager.allCardPool != null)
        {
            foreach (var c in playerManager.cardManager.allCardPool)
                TryAddUnique(list, seen, c);
        }

        if (list.Count == 0)
        {
            var loaded = Resources.LoadAll<CardData>("Cards");
            foreach (var c in loaded)
                TryAddUnique(list, seen, c);
        }

        list.Sort((a, b) => a.uniqueID.CompareTo(b.uniqueID));
        return list;
    }

    private static void TryAddUnique(List<CardData> list, HashSet<int> seen, CardData card)
    {
        if (card == null || card.uniqueID <= 0)
            return;
        if (!seen.Add(card.uniqueID))
            return;
        list.Add(card);
    }

    public void ShowUnlockHint(string message, Vector2 screenPosition)
    {
        if (unlockHintPanel == null || unlockHintLabel == null)
            return;

        unlockHintLabel.text = message;
        unlockHintPanel.gameObject.SetActive(true);

        Canvas canvas = unlockHintPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransform parentRect = unlockHintPanel.parent as RectTransform;
        if (parentRect == null)
            return;

        Vector2 pos = screenPosition + hintScreenOffset;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, pos, cam, out Vector2 local))
            unlockHintPanel.anchoredPosition = local;
    }

    public void HideUnlockHint()
    {
        if (unlockHintPanel != null)
            unlockHintPanel.gameObject.SetActive(false);
    }
}
