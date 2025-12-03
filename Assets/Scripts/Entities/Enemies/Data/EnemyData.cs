using System;
using System.Collections.Generic;
using UnityEngine;
using Entities.Players.Data;
using Entities.Enemies.Helpers;

// TODO: Buff balancing needs to be done before being considered back on the enemies

[Serializable]
public struct EnemyAction
{
    public EnemyIntent intent;

    [Tooltip("Value of this action. For Attack/Block/Heal, rounded to int. For Buff, float multiplier.")]
    public float value;

    [Range(0.1f, 1f)]
    public float actionChance; // chance of action being played

    [Tooltip("Optional custom name for this action (e.g., 'Crushing Blow' instead of 'Attack'). Leave empty to use default.")]
    public string customName;
}

public enum EnemyIntent { Attack, Block, Heal, Buff }

[Serializable]
public class IntentIconMapping
{
    [Header("Intent Icons")]
    public Sprite attackIcon;
    public Sprite blockIcon;
    public Sprite healIcon;
    public Sprite buffIcon;

    public Sprite GetIconForIntent(EnemyIntent intent)
    {
        return intent switch
        {
            EnemyIntent.Attack => attackIcon,
            EnemyIntent.Block  => blockIcon,
            EnemyIntent.Heal   => healIcon,
            EnemyIntent.Buff   => buffIcon,
            _ => null
        };
    }
}

[Serializable]
public class EnemyData : EntityData
{
    [Header("Core Stats (fallbacks when pattern value is 0)")]
    public int attackPower;
    public int defensePower;
    public int healPower;
    public int buffPower;

    [Header("Intent System")]
    public EnemyIntent currentIntent;
    public EnemyAction currentAction;
    public string intentText;
    public int intentValue;

    [Header("Behavior")]
    public List<EnemyAction> actionPattern;

    [Header("Metadata")]
    public int enemyID;
    public string enemyName;
    public Sprite artwork;
    public AudioClip attackSFX;
    public EnemyConfig sourceConfig;

    [Header("Animator (Optional)")]
    public RuntimeAnimatorController animatorController;

    [Header("Sprite Animations")]
    public SpriteAnimation idleAnim;
    public SpriteAnimation attackAnim;
    public SpriteAnimation hurtAnim;
    public SpriteAnimation deathAnim;

    [Header("Visual Adjustments")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;

    [Header("Runtime Intent Ranges (effective, clamped 0.3–4)")]
    public EnemyConfig.IntentMultiplierRange attackRange;
    public EnemyConfig.IntentMultiplierRange blockRange;
    public EnemyConfig.IntentMultiplierRange healRange;
    public EnemyConfig.IntentMultiplierRange buffRange;

    // ──────────────────────────────────────────────────────────────
    // NEW: Buff multiplier (affects ATK/BLK/HEAL permanently)
    // ──────────────────────────────────────────────────────────────
    [Header("Runtime Buff Multiplier (affects Attack/Block/Heal only)")]
    public float buffMultiplier = 1f;

    // -------------------------------------------------------
    // Initialization
    // -------------------------------------------------------
    public void Initialize(string name, int maxHealth, int atk, int def)
    {
        enemyName = name;
        attackPower = atk;
        defensePower = def;

        base.Initialize(name, maxHealth);

        actionPattern = new List<EnemyAction>();

        attackRange.min = attackRange.min == 0 ? 1f : attackRange.min;
        attackRange.max = attackRange.max == 0 ? 1f : attackRange.max;
        blockRange.min  = blockRange.min == 0 ? 1f : blockRange.min;
        blockRange.max  = blockRange.max == 0 ? 1f : blockRange.max;
        healRange.min   = healRange.min == 0 ? 1f : healRange.min;
        healRange.max   = healRange.max == 0 ? 1f : healRange.max;
        buffRange.min   = buffRange.min == 0 ? 1f : buffRange.min;
        buffRange.max   = buffRange.max == 0 ? 1f : buffRange.max;
    }

    public void NormalizeActionPatternWithFallbacks()
    {
        if (actionPattern == null) return;

        for (int i = 0; i < actionPattern.Count; i++)
        {
            var a = actionPattern[i];
            if (a.value <= 0f)
            {
                a.value = a.intent switch
                {
                    EnemyIntent.Attack => attackPower,
                    EnemyIntent.Block  => defensePower,
                    EnemyIntent.Heal   => healPower,
                    EnemyIntent.Buff   => buffPower,
                    _ => a.value
                };
                actionPattern[i] = a;
            }
        }
    }

    // -------------------------------------------------------
    // Weighted selection only (your system)
    // -------------------------------------------------------
    private EnemyAction ChooseStrategicAction()
    {
        if (actionPattern == null || actionPattern.Count == 0)
        {
            Debug.LogWarning("[AI] Enemy has no actionPattern");
            return new EnemyAction { intent = EnemyIntent.Attack, value = attackPower };
        }

        float totalWeight = 0f;
        foreach (var a in actionPattern)
            totalWeight += Mathf.Max(0.01f, a.actionChance);

        float roll = UnityEngine.Random.Range(0, totalWeight);

        float cumulative = 0f;
        foreach (var a in actionPattern)
        {
            cumulative += Mathf.Max(0.01f, a.actionChance);
            if (roll <= cumulative)
                return a;
        }

        return actionPattern[actionPattern.Count - 1];
    }

    // -------------------------------------------------------
    // Decide next intent
    // -------------------------------------------------------
    public void DecideNextIntent()
    {
        NormalizeActionPatternWithFallbacks();

        EnemyAction chosenAction = ChooseStrategicAction();
        currentIntent = chosenAction.intent;
        currentAction = chosenAction;

        float mult = 1f;
        switch (currentIntent)
        {
            case EnemyIntent.Attack: mult = SampleMiddleBiased(attackRange.min, attackRange.max); break;
            case EnemyIntent.Block:  mult = SampleMiddleBiased(blockRange.min , blockRange.max ); break;
            case EnemyIntent.Heal:   mult = SampleMiddleBiased(healRange.min  , healRange.max  ); break;
            case EnemyIntent.Buff:   mult = SampleMiddleBiased(buffRange.min  , buffRange.max  ); break;
        }

        bool isBuff = chosenAction.intent == EnemyIntent.Buff;

        if (!isBuff)
        {
            intentValue = Mathf.RoundToInt(chosenAction.value * mult * buffMultiplier);
        }
        else
        {
            intentValue = 0;
        }

        intentText = string.IsNullOrWhiteSpace(chosenAction.customName)
            ? chosenAction.intent.ToString()
            : chosenAction.customName;
    }

    // -------------------------------------------------------
    // NEW — Apply buff to ATK/BLK/HEAL actionPattern values
    // -------------------------------------------------------
    private void ApplyBuffToActionPattern(float mult)
    {
        if (actionPattern == null) return;

        for (int i = 0; i < actionPattern.Count; i++)
        {
            EnemyAction a = actionPattern[i];

            // DO NOT buff buff actions
            if (a.intent == EnemyIntent.Buff)
                continue;

            a.value = a.value * mult; // float precision
            actionPattern[i] = a;
        }
    }

    // -------------------------------------------------------
    // Execute current intent (Buff updated)
    // -------------------------------------------------------
    public void ExecuteIntent(PlayerData player)
    {
        if (player == null) return;

        switch (currentIntent)
        {
            case EnemyIntent.Attack:
                player.TakeDamage(intentValue);
                break;

            case EnemyIntent.Block:
                GainBlock(intentValue);
                break;

            case EnemyIntent.Heal:
                Heal(intentValue);
                break;

            case EnemyIntent.Buff:
            {
                float mult = SampleMiddleBiased(buffRange.min, buffRange.max);

                if (currentAction.value > 0f)
                    mult *= currentAction.value;  // float multiplier

                buffMultiplier *= mult;

                ApplyBuffToActionPattern(mult);

                Debug.Log($"[Buff] {enemyName} applied buff! mult={mult:F3}, total={buffMultiplier:F3}");
                break;
            }
        }
    }

    // -------------------------------------------------------
    // Utility
    // -------------------------------------------------------
    public bool IsAlive() => isAlive;

    private static float SampleMiddleBiased(float min, float max)
    {
        float a = UnityEngine.Random.Range(min, max);
        float b = UnityEngine.Random.Range(min, max);
        return (a + b) * 0.5f;
    }
}
