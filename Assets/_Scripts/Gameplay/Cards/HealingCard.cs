using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Card", menuName = "Cards/Healing Card")]
public class HealingCard : CardBase
{
    [Header("Healing Settings")]
    public int healAmount = 12;

    public override void Use(CharacterBase user, CharacterBase target)
    {
        // Healing cards should *always* heal the player who used it
        if (user == null)
        {
            Debug.LogWarning($"{cardName}: No valid player to heal!");
            return;
        }

        user.Heal(healAmount);
        user.ShowHealFeedback(healAmount);
        Debug.Log($"{user.characterName} uses {cardName} and heals for {healAmount} HP!");
    }
}
