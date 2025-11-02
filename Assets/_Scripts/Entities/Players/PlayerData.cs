using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Player/Player Data", fileName = "NewPlayerData")]
public class PlayerData : EntityData
{
    [Header("Player-Specific Settings")]
    [Tooltip("Reference to global configuration settings.")]
    public GameConfig config;

    [Tooltip("Starting energy (will be replaced by config if assigned).")]
    public int baseEnergy = 3;

    [Tooltip("Maximum energy cap (will be replaced by config if assigned).")]
    public int maxEnergy = 3;

    [Tooltip("Optional list of starting relics.")]
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
