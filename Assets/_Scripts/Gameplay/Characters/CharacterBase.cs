using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Unnamed";
    public Sprite portrait;

    [Header("Core Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int block;         // temporary damage reduction
    public int strength = 0;  // bonus for attack cards
    public int defense = 0;   // bonus for defense cards

    [Header("Inventory")]
    public Inventory inventory; // optional (hook later)

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        int mitigated = Mathf.Max(amount - block - defense, 0);
        block = Mathf.Max(block - amount, 0); // block consumed first
        currentHealth -= mitigated;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{characterName} took {mitigated} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{characterName} healed {amount}! HP: {currentHealth}/{maxHealth}");
    }

    public virtual void AddBlock(int amount)
    {
        block += amount;
        Debug.Log($"{characterName} gains {amount} block! (Total block: {block})");
    }

    protected virtual void Die()
    {
        Debug.Log($"{characterName} has fallen!");
        gameObject.SetActive(false);
    }
}
