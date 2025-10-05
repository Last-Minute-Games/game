using UnityEngine;

public class Enemy : CharacterBase
{
    private void Start()
    {
        characterName = "Enemy";
        maxHP = 80;
        currentHP = maxHP;
        attack = 15;
        defense = 3;
    }

    public virtual void AttackTarget(CharacterBase target)
    {
        Debug.Log($"{characterName} attacks {target.characterName}!");
        target.TakeDamage(attack);
    }

}
