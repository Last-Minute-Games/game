using UnityEngine;

public class Protagonist : CharacterBase
{
    private void Start()
    {
        characterName = "Player";
        maxHP = 100;
        currentHP = maxHP;
        attack = 25;
        defense = 5;
    }

    public virtual void AttackTarget(CharacterBase target)
    {
        Debug.Log($"{characterName} attacks {target.characterName}!");
        target.TakeDamage(attack);
    }

}
