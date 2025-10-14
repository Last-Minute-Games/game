using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : CharacterBase
{
    [Header("Player Settings")]
    public int maxEnergy = 3;
    public int currentEnergy;

    [Header("Deck & Inventory")]
    public GameObject cardPrefab;

    [Header("UI Prefabs")]
    public GameObject healthBarPrefab;

    // --- Defense Panel ---
    private GameObject defensePanelInstance;
    private TMP_Text defenseText;

    // ============================
    //  HEALTH BAR SETUP
    // ============================
    protected override void Awake()
    {
        base.Awake();
        characterName = "Player";
        currentEnergy = maxEnergy;

        // Find existing HealthBar in scene
        healthBarInstance = FindObjectOfType<PlayerHealthBar>();
        if (healthBarInstance != null)
        {
            healthBarInstance.Initialize(this);
            Debug.Log("✅ Player HealthBar linked successfully!");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerHealthBar not found in scene!");
        }
    }

    public bool UseEnergy(int amount)
    {
        if (EnergySystem.Instance == null) return false;

        if (EnergySystem.Instance.UseEnergy(amount))
        {
            currentEnergy = EnergySystem.Instance.currentEnergy;
            Debug.Log($"{characterName} used {amount} energy. Remaining: {currentEnergy}");
            return true;
        }

        Debug.Log($"{characterName} doesn’t have enough energy!");
        return false;
    }

    public void RefillEnergy()
    {
        if (EnergySystem.Instance == null) return;

        EnergySystem.Instance.RefillEnergy();
        currentEnergy = EnergySystem.Instance.currentEnergy;
        Debug.Log($"{characterName}'s energy refilled to {currentEnergy}/{maxEnergy}");
    }

    public void Attack(CharacterBase target)
    {
        if (target == null) return;

        int damage = strength;
        target.TakeDamage(damage);
    }

    public void PlayCard(CardBase card, CharacterBase target)
    {
        if (card == null) return;

        if (UseEnergy(card.energy))
        {
            card.Use(this, target);
        }
    }

    public override void AddBlock(int amount)
    {
        base.AddBlock(amount);

        if (defensePanelInstance != null)
        {
            defensePanelInstance.SetActive(block > 0);
            defenseText.text = block.ToString();
        }
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        if (defensePanelInstance != null)
        {
            defensePanelInstance.SetActive(block > 0);
            defenseText.text = block.ToString();
        }
    }

    public override void ShowBlockFeedback(int amount)
    {
        base.ShowBlockFeedback(amount);

        // Update shield for player
        if (activeShield == null && shieldPrefab != null)
        {
            activeShield = Instantiate(shieldPrefab, Vector3.zero, Quaternion.identity);
            activeShield.transform.localScale = Vector3.one * 5f;
        }
    }
}
