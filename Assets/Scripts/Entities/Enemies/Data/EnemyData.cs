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
    public float minScaleMultiplier = 0.5f;

    [Range(0f, 10f)]
    public float maxScaleMultiplier = 2f;

    [Tooltip("If true, this enemy can spawn with random strength (min–max scaling).")]
    public bool isVariableEnemy = false;

    [Header("Variable Naming")]
    [Tooltip("Name prefix when the enemy rolls near its minimum scale (weaker variant).")]
    public string weakPrefix = "Weak";

    [Tooltip("Name prefix when the enemy rolls near its maximum scale (stronger variant).")]
    public string strongPrefix = "Insane";

    [Header("Variant Visual Overrides (Optional)")]
    [Tooltip("Visual overrides when this enemy spawns as its weaker variant.")]
    public VariantVisualOptions weakOptions;

    [Tooltip("Visual overrides when this enemy spawns as its stronger variant.")]
    public VariantVisualOptions strongOptions;

    [Header("FX Data")]
    [Tooltip("Reference to visual/sound FX data for this enemy.")]
    public EnemyFXData enemyFXData;

    [Header("Metadata")]
    [Tooltip("Unique ID for this enemy (used for lookups or saves).")]
    public int uniqueID;

    // struct for per-variant visual customization
    [System.Serializable]
    public struct VariantVisualOptions
    {
        [Range(0f, 2f)]
        [Tooltip("Optional size multiplier for this variant (if 0, EnemyManager uses global default).")]
        public float sizeMultiplier;

        [Tooltip("Optional tint color for this variant (alpha = 0 means none).")]
        public Color tintColor;
    }

    protected override void OnValidate()
    {
        // verify valid scale multiplier
        if (minScaleMultiplier > maxScaleMultiplier)
            minScaleMultiplier = maxScaleMultiplier;

        // Sanity checks
        baseHealth         = Mathf.Max(1, baseHealth);
        baseShield         = Mathf.Max(0, baseShield);
        basePowerScale     = Mathf.Max(0.01f, basePowerScale);
        minScaleMultiplier = Mathf.Clamp(minScaleMultiplier, 0f, maxScaleMultiplier);
        maxScaleMultiplier = Mathf.Max(minScaleMultiplier, maxScaleMultiplier);
    }
}
