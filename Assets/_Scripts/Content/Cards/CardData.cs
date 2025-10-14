using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName = "Unnamed Card";
    [TextArea] public string description;
    public Sprite artwork;
    public CardType cardType;
    public int energy = 1;

    [Header("Targeting Rule")]
    [Tooltip("Defines how this card selects its target (e.g., self, enemy, all enemies, etc.).")]
    public TargetingRule targetingRule;

    [Header("Intention Display (for Enemies)")]
    public Sprite intentionIcon;
    [TextArea] public string intentionText;

    [Header("Scaling Range (Randomized Per Use)")]
    [Tooltip("Random power range: e.g. 0.8x–1.2x base.")]
    public float minMultiplier = 0.9f;
    public float maxMultiplier = 1.1f;

    [Header("Effects")]
    public CardEffect[] effects;

    [Header("Player Draw Weighting")]
    [Tooltip("Relative chance (0–1) that this card appears when the player draws. 1 = most common, 0 = never.")]
    [Range(0f, 1f)] public float playerDrawWeight = 1f;

    [Header("Metadata")]
    [Tooltip("If false, the player cannot obtain or draw this card (enemy-exclusive).")]
    public bool isPlayerUsable = true;

    [Header("Identification")]
    [Tooltip("A unique ID to directly pull this card from code.")]
    public int uniqueId = 0;
}
