using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Player/Player Data", fileName = "NewPlayerData")]
public class PlayerData : EntityData
{
    [Tooltip("Reference to global configuration settings.")]
    public GameConfig config;

    [Header("Player-Specific Settings")]

    [Tooltip("Starting energy")]
    public int baseEnergy;

    [Tooltip("Maximum energy cap")]
    public int maxEnergy;

    [Tooltip("Starting relics list")]
    public List<RelicData> startingRelics = new List<RelicData>();

    protected override void OnValidate()
    {
        base.OnValidate();

        if (config != null)
        {
            // Replace with GameConfig definitions straight away
            baseHealth   = config.defaultHealth;
            baseShield   = config.defaultShield;
            baseEnergy   = config.defaultBaseEnergy;
            maxEnergy    = config.defaultMaxEnergy;

            // Get starting definitions of cards and relics from GameConfig
            startingCards.Clear();
            startingRelics.Clear();

            if (config.defaultCards != null && config.defaultCards.Count > 0)
                startingCards.AddRange(config.defaultCards);

            if (config.defaultRelics != null && config.defaultRelics.Count > 0)
                startingRelics.AddRange(config.defaultRelics);
        }
        else
        {
            // Fallback if GameConfig definitions don't exist
            baseHealth   = Mathf.Max(1, baseHealth);
            baseShield   = Mathf.Max(0, baseShield);
            baseEnergy   = Mathf.Max(0, baseEnergy);
            maxEnergy    = Mathf.Max(baseEnergy, maxEnergy);
        }
    }
}
