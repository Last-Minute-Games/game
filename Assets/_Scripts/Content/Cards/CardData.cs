using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite artwork;

    [Header("Gameplay")]
    public int energyCost = 1;

    public CardType cardType;

    [Header("Card Logic")]
    public CardEffect[] effects;
    public TargetingRule targetingRule;
}
