using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Card", menuName = "Cards/Attack Card")]
public class AttackCard : CardBase
{
    [Header("Attack Settings")]
    public int baseDamage = 10;

    public override void Use(CharacterBase user, CharacterBase target)
    {
        if (target == null)
        {
            Debug.LogWarning($"{cardName}: No target selected!");
            return;
        }

        int totalDamage = baseDamage + (user != null ? user.strength : 0);
        target.TakeDamage(totalDamage);
        target.ShowDamageFeedback(totalDamage);

        string userName = user != null ? user.characterName : "Player";
        Debug.Log($"{userName} used {cardName}, dealing {totalDamage} damage to {target.characterName}!");
    }
}
