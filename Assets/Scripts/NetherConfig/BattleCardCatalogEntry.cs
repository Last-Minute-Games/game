using GameItems.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One slot in <see cref="BattleCardCatalogPopup"/>: artwork, locked overlay, hover hint when locked.
/// </summary>
[RequireComponent(typeof(Image))]
public class BattleCardCatalogEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Color lockedTint = new Color(0.35f, 0.35f, 0.35f, 1f);

    private CardData _card;
    private BattleCardCatalogPopup _catalog;

    private void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();

        if (cardImage != null)
            cardImage.raycastTarget = true;
    }

    public void Setup(CardData card, BattleCardCatalogPopup catalog)
    {
        _card = card;
        _catalog = catalog;
        Refresh();
    }

    public void Refresh()
    {
        if (_card == null || cardImage == null)
            return;

        cardImage.sprite = _card.artwork;
        bool unlocked = BattleCardCatalogPopup.IsCardUnlockedForCatalog(_card);

        cardImage.color = unlocked ? Color.white : lockedTint;

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!unlocked);
            // Let rays hit the root card Image so hover doesn't flicker between overlay and art.
            foreach (var g in lockedOverlay.GetComponentsInChildren<Graphic>(true))
                g.raycastTarget = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_catalog == null || _card == null)
            return;

        if (BattleCardCatalogPopup.IsCardUnlockedForCatalog(_card))
            return;

        string msg = BuildHintMessage();
        _catalog.ShowUnlockHint(msg, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _catalog?.HideUnlockHint();
    }

    private string BuildHintMessage()
    {
        if (_card == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(_card.unlockHint))
            return _card.unlockHint.Trim();

        if (!string.IsNullOrEmpty(_card.unlockFlag))
            return $"Locked. Progress to earn: {_card.unlockFlag}";

        return "Locked.";
    }
}
