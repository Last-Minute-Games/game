using System;
using System.Collections.Generic;
using UnityEngine;
using Entities.Enemies.Helpers;

[CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "NewEnemy")]
public class EnemyConfig : ScriptableObject
{
    [Header("Core")] public string enemyName;
    public int maxHealth;
    public int attackPower;
    public int defensePower;
    public Sprite artwork;
    public List<EnemyAction> actionPattern;

    [Header("Animator (Optional)")]
    [Tooltip(
        "If assigned, this RuntimeAnimatorController will drive the enemy's animation states (Idle/Attack/Hurt/Death). Drag & drop here.")]
    public RuntimeAnimatorController animatorController;

    [Header("Sprite Animations (Lightweight, used if no Animator Controller)")]
    [Tooltip("Looping idle sprite animation for this enemy (optional).")]
    public SpriteAnimation idleAnim;

    [Tooltip("Attack sprite animation for this enemy (optional).")]
    public SpriteAnimation attackAnim;

    [Tooltip("Hurt sprite animation for this enemy (optional).")]
    public SpriteAnimation hurtAnim;

    [Tooltip("Death sprite animation for this enemy (optional).")]
    public SpriteAnimation deathAnim;

    [Header("Visual Adjustments")] [Tooltip("Position offset to adjust where the enemy sprite appears in the scene.")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Scale multiplier to adjust the size of the enemy sprite (1 = normal size).")]
    public Vector3 scaleOffset = Vector3.one;


    [Header("Enemy variability multiplier to be applied to enemy stats.")]
    [Tooltip("Minimum possible multiplier applied to enemy stats.")]
    [Range(0f, 2f)]
    public float minMultiplier = 1f;

    [Tooltip("Maximum possible multiplier applied to enemy stats.")] [Range(0f, 2f)]
    public float maxMultiplier = 1f;

    [Header("Identity")] [Tooltip("Automatically assigned unique ID for flag tracking.")]
    public string uniqueID;

    public EnemyData CreateRuntimeInstance()
    {
        var data = new EnemyData();
        data.sourceConfig = this; // this = EnemyConfig
        float enemyStatMultiplier = GetMiddleBiasedMultiplier();
        Debug.Log($"{enemyStatMultiplier} enemy multiplier applied");

        data.Initialize(enemyName,
            (int)(maxHealth * enemyStatMultiplier),
            (int)(attackPower * enemyStatMultiplier),
            (int)(defensePower * enemyStatMultiplier));
        data.actionPattern = new List<EnemyAction>(actionPattern);
        data.artwork = artwork;
        // Propagate animator controller and sprite animations
        data.animatorController = animatorController;
        data.idleAnim = idleAnim;
        data.attackAnim = attackAnim;
        data.hurtAnim = hurtAnim;
        data.deathAnim = deathAnim;
        // Propagate visual adjustments
        data.positionOffset = positionOffset;
        data.scaleOffset = scaleOffset;
        return data;
    }

    public float GetMiddleBiasedMultiplier()
    {
        // generates two uniform random values, averages them → triangle distribution
        float a = UnityEngine.Random.Range(minMultiplier, maxMultiplier);
        float b = UnityEngine.Random.Range(minMultiplier, maxMultiplier);

        return (a + b) * 0.5f;
    }

    protected void OnValidate()
    {
        // ensure multipliers are valid
        if (minMultiplier > maxMultiplier)
            minMultiplier = maxMultiplier;

        // clamp
        minMultiplier = Mathf.Max(0f, minMultiplier);
        maxMultiplier = Mathf.Max(minMultiplier, maxMultiplier);

        if (string.IsNullOrWhiteSpace(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }
}