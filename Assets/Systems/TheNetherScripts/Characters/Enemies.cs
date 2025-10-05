using UnityEngine;

public class Enemy : CharacterBase
{
    protected override void Awake()
    {
        base.Awake();
        characterName = "Enemy";
        maxHP = 100;
        strength = 15;
        dexterity = 10;
        vulnerability = 0;
        energy = 0; // enemy may not use energy
        UpdateStatsUI();
    }

    // Optional: Enemy attack method
    public void Attack(CharacterBase target)
    {
        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
        target.TakeDamage(damage);
    }
}
