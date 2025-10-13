using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject {
    [Header("Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite artwork;
    public int energyCost = 1;
    public CardType type;

    [Header("Logic")]
    public CardEffect[] effects;           // what happens when played
    public TargetingRule targetingRule;    // how we pick a target
}
