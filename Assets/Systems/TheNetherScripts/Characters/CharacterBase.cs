using UnityEngine;
using UnityEngine.UI; // for Text (or TMPro for TextMeshPro)

public class CharacterBase : MonoBehaviour
{
    [Header("Stats")]
    public string characterName;
    public int maxHP = 100;
    public int currentHP;
    public int attack = 20;
    public int defense = 10;

    [Header("UI")]
    public Text statsText; // assign in inspector

    protected virtual void Awake()
    {
        currentHP = maxHP;
        UpdateStatsUI();
    }

    public virtual void TakeDamage(int dmg)
    {
        int actualDamage = Mathf.Max(dmg - defense, 0);
        currentHP -= actualDamage;
        Debug.Log($"{characterName} took {actualDamage} damage! HP left: {currentHP}");

        UpdateStatsUI();

        if (currentHP <= 0)
            Die();
    }

    public virtual void UpdateStatsUI()
    {
        if (statsText != null)
            statsText.text = $"{characterName}\nHP: {currentHP}\nATK: {attack}";
    }

    public virtual void Die()
    {
        Debug.Log($"{characterName} has been defeated!");
        gameObject.SetActive(false);
    }
}
