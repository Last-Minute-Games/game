using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single card instance in battle. Handles
/// only its visuals and data-binding; logic is delegated
/// to CardManager and FeedbackManager.
/// </summary>
public class CardPrefab : MonoBehaviour
{
    [Header("Card Data Reference")]
    [Tooltip("ScriptableObject holding static data for this card.")]
    public CardData cardData;

    [Header("UI References")]
    [Tooltip("Main card background or artwork image.")]
    public Image cardBackground;

    [Tooltip("Icon showing the card's intention or type.")]
    public Image cardIcon;

    [Tooltip("Displayed card name text.")]
    public TMP_Text nameText;

    [Tooltip("Displayed card description text.")]
    public TMP_Text descriptionText;

    [Tooltip("Displayed energy cost number.")]
    public TMP_Text energyCost;

    // ------------------------------------------------------------------
    // LIFECYCLE
    // ------------------------------------------------------------------

    private void Start()
    {
        if (cardData != null)
            Initialize(cardData);
        else
            Debug.LogWarning($"[CardPrefab] '{name}' missing CardData assignment.");
    }

    // ------------------------------------------------------------------
    // INITIALIZATION / VISUALS
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes this card with a CardData reference and applies visuals.
    /// </summary>
    public void Initialize(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("[CardPrefab] Tried to initialize with null CardData!");
            return;
        }

        cardData = data;
        RefreshVisuals();

        Debug.Log($"[CardPrefab] Initialized card: {data.itemName}");
    }

    // refreshes and re-applies visuals for card
    public void RefreshVisuals()
    {
        if (cardData == null) return;

        // --- Determine variation tier (first effect assumed primary) ---
        CardVariationTier tier = CardVariationTier.NormalModifier;
        if (cardData.IsCardVariabilityValid())
            tier = cardData.GetVariationTier(cardData.effectData[0]);

        // --- Apply artwork & colored name ---
        switch (tier)
        {
            case CardVariationTier.WeakModifier:
                if (cardData.poorArtwork) cardBackground.sprite = cardData.poorArtwork;
                nameText.SetText($"{cardData.GetColoredPrefix(tier)} {cardData.itemName}");
                break;

            case CardVariationTier.StrongModifier:
                if (cardData.potentArtwork) cardBackground.sprite = cardData.potentArtwork;
                nameText.SetText($"{cardData.GetColoredPrefix(tier)} {cardData.itemName}");
                break;

            default:
                cardBackground.sprite = cardData.artwork;
                nameText.SetText(cardData.itemName);
                break;
        }

        // --- Core visuals ---
        descriptionText.SetText(cardData.description);
        energyCost.SetText(cardData.energyCost.ToString());
        cardIcon.sprite = cardData.icon;
    }

    // ------------------------------------------------------------------
    // CARD ACTIONS
    // ------------------------------------------------------------------

    /// <summary>
    /// Called when the card is played. Logic handled externally by CardManager.
    /// </summary>
    public void PlayCard()
    {
        Debug.Log($"[CardPrefab] Card '{cardData?.itemName ?? "Unnamed"}' played.");
        // Placeholder: CardManager & FeedbackManager handle actual logic.
    }
}
