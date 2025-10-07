using UnityEngine;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour
{
    [Header("Stats")]
    public string characterName;
    public int maxHP = 100;
    public int currentHP;
    public int vulnerability = 0; // increases damage taken
    public int dexterity = 10;    // could affect critical or hit
    public int strength = 20;     // adds to attack
    public int energy = 3;        // cards cost energy to play

    [Header("UI")]
    public Text statsText; // assign in inspector

    protected virtual void Awake()
    {
        currentHP = maxHP;
        UpdateStatsUI();
    }

    public virtual void TakeDamage(int dmg)
    {
        // Increase damage by vulnerability
        int actualDamage = Mathf.Max(dmg + vulnerability, 0);
        currentHP -= actualDamage;

        Debug.Log($"{characterName} took {actualDamage} damage! HP left: {currentHP}");

        UpdateStatsUI();

        if (currentHP <= 0)
            Die();
    }

    public virtual void UpdateStatsUI()
    {
        if (statsText != null)
        {
            statsText.text = $"{characterName}\nHP: {currentHP}\nSTR: {strength}\nDEX: {dexterity}\nVUL: {vulnerability}\nEN: {energy}";
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{characterName} has been defeated!");
        gameObject.SetActive(false);
    }

    // Optional: basic attack method
    public virtual void AttackTarget(CharacterBase target)
    {
        int damage = strength; // base damage
        target.TakeDamage(damage);
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
    }
}
