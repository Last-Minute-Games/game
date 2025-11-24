using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data", fileName = "NewCardData")]
public class CardData : GameItemData
{
    // --------------------------------------------------
    // CARD INFO
    // --------------------------------------------------

    [Header("Card Info")]
    [Tooltip("Displayed text for the card's intention (e.g., 'Deal damage', 'Block', etc.).")]
    [TextArea(1, 3)]
    public string intentionText;

    [Tooltip("Sound cue played when this card is used.")]
    public SFXCueData soundCue;

    [Tooltip("Energy cost to play this card.")]
    public int energyCost;

    // --------------------------------------------------
    // ANIMATION SETTINGS
    // --------------------------------------------------

    [Header("Animation Settings")]
    [Tooltip("Animation played by the source entity when this card is used (e.g., Attack).")]
    public EnemyAnim sourceAnim = EnemyAnim.Attack;

    [Tooltip("Animation played by the target entity when this card is used (e.g., Hurt).")]
    public EnemyAnim targetAnim = EnemyAnim.Hurt;

    // --------------------------------------------------
    // VARIABILITY SETTINGS
    // --------------------------------------------------

    [Header("Variability Settings")]
    [Tooltip("If true, this card can roll variable potency (Poor, Normal, Potent) based on its multiplier range.")]
    public bool isVariableCard = false;

    [Tooltip("Artwork shown when the card rolls a Poor outcome (optional).")]
    public Sprite poorArtwork;

    [Tooltip("Artwork shown when the card rolls a Potent outcome (optional).")]
    public Sprite potentArtwork;

    [Header("Variability Threshold")]

    [Tooltip("The upper bound range of the variability threshold.")]
    [Range(0f, 1f)] public float maxMultiplierThreshold = 0.66f;

    [Tooltip("The lower bound range of the variability threshold.")]
    [Range(0f, 1f)] public float minMultiplierThreshold = 0.33f;

    // naming prefixes
    [Header("Name Prefixes")]
    public string weakPrefix = "Poor";
    public string strongPrefix = "Potent";

    [Tooltip("Color for weak prefix text.")]
    public Color weakPrefixColor = Color.gray;

    [Tooltip("Color for strong prefix text.")]
    public Color strongPrefixColor = new Color(1f, 0.4f, 0.4f);

    // --------------------------------------------------
    // VALIDATION HELPERS
    // --------------------------------------------------

    // checks if card variability is valid
    public bool IsCardVariabilityValid()
    {
        return isVariableCard && effects != null && effects.Count == 1;
    }

    // postcopy rolled effect
    public CardVariationTier GetVariationTier(Effect rolledEffect)
    {
        float min = rolledEffect.minMultiplier;
        float max = rolledEffect.maxMultiplier;
        float baseVal = rolledEffect.baseValue;
        float post = rolledEffect.postCopyValue;
        float lowBound = baseVal * min;
        float highBound = baseVal * max;

        if (post == baseVal) // for cases where there is no gap between minT, maxT and multiplier isn't on
            return CardVariationTier.NormalModifier;

        // Normalize
        float t = Mathf.InverseLerp(lowBound, highBound, post);

        if (t <= minMultiplierThreshold)
            return CardVariationTier.WeakModifier;
        if (t >= maxMultiplierThreshold)
            return CardVariationTier.StrongModifier;
        return CardVariationTier.NormalModifier; // ultimate fallback
    }
    public string GetWeakPrefixColorTag() =>
        ColorUtility.ToHtmlStringRGB(weakPrefixColor);

    public string GetStrongPrefixColorTag() =>
        ColorUtility.ToHtmlStringRGB(strongPrefixColor);

    public string GetColoredPrefix(CardVariationTier tier)
    {
        return tier switch
        {
            CardVariationTier.WeakModifier =>
                $"<color=#{GetWeakPrefixColorTag()}>{weakPrefix}</color>",
            CardVariationTier.StrongModifier =>
                $"<color=#{GetStrongPrefixColorTag()}>{strongPrefix}</color>",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the appropriate artwork sprite based on the variation tier.
    /// Returns poorArtwork for weak tier, potentArtwork for strong tier, or default artwork otherwise.
    /// </summary>
    public Sprite GetArtworkForTier(CardVariationTier tier)
    {
        return tier switch
        {
            CardVariationTier.WeakModifier => poorArtwork != null ? poorArtwork : artwork,
            CardVariationTier.StrongModifier => potentArtwork != null ? potentArtwork : artwork,
            _ => artwork
        };
    }

    protected override void OnValidate()
    {
          
        base.OnValidate();

        // Ensure max ≥ min
        if (maxMultiplierThreshold < minMultiplierThreshold)
            maxMultiplierThreshold = minMultiplierThreshold;

        // Ensure min ≤ max
        if (minMultiplierThreshold > maxMultiplierThreshold)
            minMultiplierThreshold = maxMultiplierThreshold;

        // Optional: clamp inside 0–1 range (even though Range attribute does this in inspector)
        minMultiplierThreshold = Mathf.Clamp01(minMultiplierThreshold);
        maxMultiplierThreshold = Mathf.Clamp01(maxMultiplierThreshold);
    }
}
