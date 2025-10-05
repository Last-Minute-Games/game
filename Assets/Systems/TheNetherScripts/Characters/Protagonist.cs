using UnityEngine;

public class Protagonist : CharacterBase
{
    protected override void Awake()
    {
        base.Awake();
        characterName = "Player";
        maxHP = 120;
        strength = 20;
        dexterity = 12;
        vulnerability = 0;
        energy = 3; // starting energy
        UpdateStatsUI();
    }

    // Optional: Player attack method
    public void Attack(CharacterBase target)
    {
        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
        target.TakeDamage(damage);
    }
}
