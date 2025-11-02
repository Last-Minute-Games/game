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
}
