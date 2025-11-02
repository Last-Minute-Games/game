using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemy/Enemy Data", fileName = "NewEnemyData")]
public class EnemyData : EntityData
{
    [Tooltip("Enemy Information")]
    public string name = "";
    public string description = ""

    [Tooltip("List of CardData")]
    public List<CardData> cardData = new List<CardData>();

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
