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

    [Header("Scaling")]
    public float globalPowerScale = 1.0f;

    private GameObject defensePanelInstance;
    private TMP_Text defenseText;

    protected override void Awake()
    {
        base.Awake();
        characterName = "Player";
        currentEnergy = maxEnergy;

        healthBarInstance = FindObjectOfType<PlayerHealthBar>();
        if (healthBarInstance != null)
            healthBarInstance.Initialize(this);
    }

    // -----------------------------------------------------
    // ENERGY MANAGEMENT
    // -----------------------------------------------------
    public bool UseEnergy(int amount)
    {
        if (EnergySystem.Instance == null)
        {
            Debug.LogWarning("⚠️ No EnergySystem found.");
            return false;
        }

        bool success = EnergySystem.Instance.UseEnergy(amount);
        currentEnergy = EnergySystem.Instance.currentEnergy;
        return success;
    }

    public void RefillEnergy()
    {
        if (EnergySystem.Instance == null) return;

        EnergySystem.Instance.RefillEnergy();
        currentEnergy = EnergySystem.Instance.currentEnergy;
        Debug.Log($"{characterName}'s energy refilled to {currentEnergy}/{maxEnergy}");
    }

    // -----------------------------------------------------
    // CARD EXECUTION
    // -----------------------------------------------------
    public void PlayCard(CardBase card, CharacterBase target)
    {
        if (card == null) return;

        // Spend energy FIRST
        if (UseEnergy(card.energy))
        {
            card.Use(this, target);
        }
    }

    // rest of the class unchanged ...
}
