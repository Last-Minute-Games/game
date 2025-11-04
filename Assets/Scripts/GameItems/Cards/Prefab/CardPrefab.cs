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

    // hierarchial target rule
    [HideInInspector] public TargetRule targetRule;

    [HideInInspector] public float ownerPowerScale = 1f;

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
        UpdateTargetRule();

        Debug.Log($"[CardPrefab] Initialized card: {data.itemName}");
    }

    // refreshes and re-applies visuals for card
    public void RefreshVisuals()
    {
        if (cardData == null) return;

        // --- Determine variation tier (first effect assumed primary) ---
        CardVariationTier tier = CardVariationTier.NormalModifier;
        if (cardData.IsCardVariabilityValid())
            tier = cardData.GetVariationTier(cardData.effectData[0], ownerPowerScale);

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


    /// <summary>
    /// Determines the highest hierarchical TargetRule from this card's CardData effects.
    /// Stored at runtime as this card's current target rule.
    /// </summary>
    public void UpdateTargetRule()
    {
        if (cardData == null)
        {
            Debug.LogWarning($"[CardPrefab] '{name}' has no CardData assigned when updating target rule.");
            targetRule = TargetRule.None;
            return;
        }

        targetRule = cardData.GetDominatingTargetRule();
        Debug.Log($"[CardPrefab] '{cardData.itemName}' target rule set to: {targetRule}");
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
