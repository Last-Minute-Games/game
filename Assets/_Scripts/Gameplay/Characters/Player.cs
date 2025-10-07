using UnityEngine;

public class Player : CharacterBase
{
    [Header("Player Settings")]
    public int maxEnergy = 3;
    public int currentEnergy;

    [Header("Deck & Inventory (optional for later)")]
    public GameObject cardPrefab;  // for spawning cards later


    [Header("UI")]
    public GameObject healthBarPrefab;

    private HealthBar healthBarInstance;

    protected override void Awake()
    {
        base.Awake();
        characterName = "Player";
        currentEnergy = maxEnergy;
        Debug.Log($"{characterName} initialized with {currentEnergy} energy and {maxHealth} HP.");


        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<HealthBar>();
            healthBarInstance.Initialize(this);
        }
    }

    // --- ENERGY SYSTEM INTEGRATION ---
    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            Debug.Log($"{characterName} used {amount} energy. Remaining: {currentEnergy}");
            return true;
        }

        Debug.Log($"{characterName} doesn’t have enough energy! ({currentEnergy}/{maxEnergy})");
        return false;
    }

    public void RefillEnergy()
    {
        currentEnergy = maxEnergy;
        Debug.Log($"{characterName}'s energy refilled to {currentEnergy}/{maxEnergy}");
    }

    // --- BASIC ACTIONS ---
    public void Attack(CharacterBase target)
    {
        if (target == null)
        {
            Debug.LogWarning("No target to attack!");
            return;
        }

        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
        target.TakeDamage(damage);
    }

    public void PlayCard(CardBase card, CharacterBase target)
    {
        if (card == null) return;

        if (UseEnergy(card.energy))
        {
            card.Use(this, target);
            Debug.Log($"{characterName} successfully played {card.cardName}");
        }
        else
        {
            Debug.Log("Not enough energy to play this card!");
        }
    }

    // --- OPTIONAL ---
    public void EndTurn()
    {
        RefillEnergy();
        Debug.Log("Player turn ended. Energy refilled.");
    }
}
