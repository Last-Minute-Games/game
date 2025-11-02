using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemy/Enemy Data", fileName = "NewEnemyData")]
public class EnemyData : EntityData
{
    [Header("Enemy Info")]
    [Tooltip("Display name for this enemy.")]
    public string enemyName = "New Enemy";

    [Tooltip("Short description of the enemy's behavior or lore.")]
    [TextArea(2, 5)]
    public string description;

    [Header("Enemy Data")]
    [Tooltip("Enemy artwork displayed in battle.")]
    public Sprite artwork;

    [Header("Scaling Multipliers")]
    [Range(0f, 10f)]
    public float minScaleMultiplier = 0.5;

    [Range(0f, 10f)]
    public float maxScaleMultiplier = 2f;

    [Header("FX Data")]
    [Tooltip("Reference to visual/sound FX data for this enemy.")]
    public EnemyFXData enemyFXData;

    [Header("Metadata")]
    [Tooltip("Unique ID for this enemy (used for lookups or saves).")]
    public int uniqueID;

    protected override void OnValidate()
    {
        // verify valid scale multiplier
        if (minScaleMultiplier > maxScaleMultiplier)
            minScaleMultiplier = maxScaleMultiplier;

        // Sanity checks
        baseHealth        = Mathf.Max(1, baseHealth);
        baseShield        = Mathf.Max(0, baseShield);
        basePowerScale    = Mathf.Max(0.01f, basePowerScale);
        minScaleMultiplier = Mathf.Clamp(minScaleMultiplier, 0f, maxScaleMultiplier);
        maxScaleMultiplier = Mathf.Max(minScaleMultiplier, maxScaleMultiplier);
    }
}
