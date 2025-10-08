using UnityEngine;

[System.Serializable]
public abstract class CardBase : MonoBehaviour
{
    [Header("Card Info")]
    public string cardName = "Unnamed Card";
    [TextArea] public string description = "Card description here";
    public Sprite artwork;
    
    [Header("Gameplay")]
    public int energy = 1; // cost to play
    public CardType cardType; // enum (Attack, Defense, Heal)

    // Called when the card is used (implemented by subclasses)
    public abstract void Use(CharacterBase user, CharacterBase target);

    // Optional helper: tooltip info for hover display
    public virtual string GetTooltipText()
    {
        return $"{cardName}\nCost: {energy}\n\n{description}";
    }
}

public enum CardType
{
    Attack,
    Defense,
    Healing,
    Utility
}


