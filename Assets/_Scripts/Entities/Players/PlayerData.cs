using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Player/Player Data", fileName = "NewPlayerData")]
public class PlayerData : EntityData
{
    [Header("Player-Specific Settings")]
    [Tooltip("Reference to global configuration settings.")]
    public GameConfig config;

    [Tooltip("Starting energy (overrides config if non-zero).")]
    public int baseEnergy;

    [Tooltip("Maximum energy cap (overrides config if non-zero).")]
    public int maxEnergy;

    [Tooltip("Optional list of starting relics.")]
    public List<RelicData> startingRelics = new List<RelicData>();

    
    protected override void OnValidate()
    {
        base.OnValidate();

        if (config != null)
        {
            if (baseEnergy <= 0)
                baseEnergy = config.defaultBaseEnergy;

            if (maxEnergy <= 0)
                maxEnergy = config.defaultMaxEnergy;
        }

        baseEnergy = Mathf.Max(0, baseEnergy);
        maxEnergy = Mathf.Max(baseEnergy, maxEnergy);
    }
}
