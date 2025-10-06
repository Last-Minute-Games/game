using UnityEngine;

public class AttackCard : CardBase
{
    public int damage = 25;

    private void Start()
    {
        cardName = "Strike";
    }

    public override void Use(CharacterBase user, CharacterBase target)
    {
        Debug.Log($"{user.characterName} uses {cardName} on {target.characterName}!");
        target.TakeDamage(damage + user.strength); // include strength bonus
    }
}
