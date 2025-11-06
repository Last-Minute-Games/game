using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Cards/Card Data", fileName = "NewCardData")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    [Tooltip("Display name of the card.")]
    public string Name;

    [TextArea(2, 4)]
    [Tooltip("In-game description of what the card does.")]
    public string Description;

    [Tooltip("Short phrase shown above enemies (e.g., 'Attack', 'Block').")]
    public string IntentionText;

    [Tooltip("Main card artwork shown in the card UI.")]
    public Sprite Artwork;

    [Tooltip("Small icon for intent display.")]
    public Sprite IntentionIcon;

    [FormerlySerializedAs("EffectDataList")]
    [Header("Card Data")]
    [Tooltip("List of effects that this card will trigger when played.")]
    public List<EffectData> effectDataList = new();

    // [Header("Audio")]
    // [Tooltip("Optional sound cue to play when the card is used.")]
    // public SFXCueData SoundCue;

    [Header("Metadata")]
    [Tooltip("Unique identifier for this card.")]
    public int UniqueID;
}
