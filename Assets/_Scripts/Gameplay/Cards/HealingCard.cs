using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Card", menuName = "Cards/Healing Card")]
public class HealingCard : CardBase
{
    [Header("Healing Settings")]
    public int healAmount = 12;

    public override void Use(CharacterBase user, CharacterBase target)
    {
        CharacterBase recipient = target != null ? target : user;
        if (recipient == null)
        {
            Debug.LogWarning($"{cardName}: No valid target to heal!");
            return;
        }

        recipient.Heal(healAmount);
        Debug.Log($"{user.characterName} uses {cardName} to heal {recipient.characterName} for {healAmount} HP!");
    }
}
