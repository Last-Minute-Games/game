using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Player/Player Data", fileName = "NewPlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("References")]
    [Tooltip("Reference to global configuration settings.")]
    public GameConfig config;

    [Header("Core Stats (overrides config if set)")]
    public string playerName = "Player";
    public int baseHealth;
    public int baseShield;

    [Header("Energy Settings")]
    [Tooltip("Starting energy")]
    public int baseEnergy;

    [Tooltip("Maximum energy cap")]
    public int maxEnergy;

    [Header("Collections")]
    [Tooltip("Cards the player can use (initial deck and pool)")]
    public List<CardData> usableCards = new List<CardData>();

    // Runtime entity state (not saved as defaults)
    [System.NonSerialized]
    public EntityData entity;

    private void OnValidate()
    {
        if (config != null)
        {
            // Pull defaults from GameConfig
            if (baseHealth <= 0) baseHealth = config.defaultHealth;
            if (baseShield < 0) baseShield = config.defaultShield;
            if (baseEnergy <= 0) baseEnergy = config.defaultBaseEnergy;
            if (maxEnergy <= 0) maxEnergy = config.defaultMaxEnergy;

            // Populate cards from config if none assigned
            if ((usableCards == null || usableCards.Count == 0) && config.defaultCards != null)
            {
                usableCards = new List<CardData>(config.defaultCards);
            }
            // Note: default relics are not defined in GameConfig currently.
        }

        // Basic sanity
        if (maxEnergy < baseEnergy) maxEnergy = baseEnergy;
        if (usableCards == null) usableCards = new List<CardData>();
        if (usableCards.Count == 0)
            Debug.LogWarning("PlayerData: No usable cards assigned.");
    }

    public void InitializeRuntime()
    {
        entity = new EntityData();
        entity.Initialize(string.IsNullOrEmpty(playerName) ? "Player" : playerName, Mathf.Max(1, baseHealth));
        if (baseShield > 0) entity.GainBlock(baseShield);
    }
}
