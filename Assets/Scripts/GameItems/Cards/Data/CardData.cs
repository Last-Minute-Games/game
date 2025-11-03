using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data", fileName = "NewCardData")]
public class CardData : GameItemData
{
    [Header("Card Info")]
    [Tooltip("Displayed text for the card's intention (e.g., 'Deal damage', 'Block', etc.).")]
    [TextArea(1, 3)]
    public string intentionText;

    [Tooltip("Sound cue played when this card is used.")]
    public SFXCueData soundCue;

    [Tooltip("Energy cost to play this card.")]
    public int energyCost;

    [Header("Variability Settings")]
    [Tooltip("If true, this card can roll variable potency (Poor, Normal, Potent) based on its multiplier range.")]
    public bool isVariableCard = false;

    [Tooltip("Artwork shown when the card rolls a Poor outcome (optional).")]
    public Sprite poorArtwork;

    [Tooltip("Artwork shown when the card rolls a Potent outcome (optional).")]
    public Sprite potentArtwork;

    /// <summary>
    /// Determines if this card meets the logical criteria for variability:
    ///  - It has exactly one effect.
    ///  - That effect uses a multiplier.
    /// </summary>
    public bool HasVariableEffect()
    {
        return effectData != null
            && effectData.Count == 1
            && effectData[0] != null
            && effectData[0].usesMultiplier;
    }
}
