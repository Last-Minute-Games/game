using System;
using System.Collections.Generic;
using UnityEngine;
using Entities.Players.Data;
using Entities.Enemies.Helpers;

[Serializable]
public struct EnemyAction
{
    public EnemyIntent intent;
    public int value;
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
    public int healPower;  // NEW
    public int buffPower;  // NEW

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

        // default ranges if zeroed
        attackRange.min = attackRange.min == 0 ? 1f : attackRange.min;
        attackRange.max = attackRange.max == 0 ? 1f : attackRange.max;
        blockRange.min  = blockRange .min == 0 ? 1f : blockRange .min;
        blockRange.max  = blockRange .max == 0 ? 1f : blockRange .max;
        healRange.min   = healRange  .min == 0 ? 1f : healRange  .min;
        healRange.max   = healRange  .max == 0 ? 1f : healRange  .max;
        buffRange.min   = buffRange  .min == 0 ? 1f : buffRange  .min;
        buffRange.max   = buffRange  .max == 0 ? 1f : buffRange  .max;
    }

    /// <summary>Ensures any action with value ≤ 0 is set from fallbacks based on intent.</summary>
    public void NormalizeActionPatternWithFallbacks()
    {
        if (actionPattern == null) return;
        for (int i = 0; i < actionPattern.Count; i++)
        {
            var a = actionPattern[i];
            if (a.value <= 0)
            {
                a.value = a.intent switch
                {
                    EnemyIntent.Attack => Mathf.Max(0, attackPower),
                    EnemyIntent.Block  => Mathf.Max(0, defensePower),
                    EnemyIntent.Heal   => Mathf.Max(0, healPower),
                    EnemyIntent.Buff   => Mathf.Max(0, buffPower),
                    _ => a.value
                };
                actionPattern[i] = a;
            }
        }
    }

    // -------------------------------------------------------
    // Enemy chooses what to do next
    // -------------------------------------------------------
    public void DecideNextIntent()
    {
        // Ensure fallbacks are applied at runtime too
        NormalizeActionPatternWithFallbacks();

        if (actionPattern == null || actionPattern.Count == 0)
        {
            // Fallback: simple attack using attackRange
            currentIntent = EnemyIntent.Attack;
            currentAction = new EnemyAction { intent = EnemyIntent.Attack, value = attackPower, customName = "" };
            float mul = SampleMiddleBiased(attackRange.min, attackRange.max);
            intentValue = Mathf.Max(0, Mathf.RoundToInt(currentAction.value * mul));
            intentText = "Attack";
            return;
        }

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

        intentValue = Mathf.Max(0, Mathf.RoundToInt(chosenAction.value * mult));
        intentText  = string.IsNullOrWhiteSpace(chosenAction.customName)
                        ? chosenAction.intent.ToString()
                        : chosenAction.customName;
    }

    private EnemyAction ChooseStrategicAction()
    {
        float healthPercent = currentHealth / (float)maxHealth;

        var attackActions = new List<EnemyAction>();
        var blockActions  = new List<EnemyAction>();
        var healActions   = new List<EnemyAction>();
        var buffActions   = new List<EnemyAction>();

        foreach (var action in actionPattern)
        {
            switch (action.intent)
            {
                case EnemyIntent.Attack: attackActions.Add(action); break;
                case EnemyIntent.Block:  blockActions .Add(action); break;
                case EnemyIntent.Heal:   healActions  .Add(action); break;
                case EnemyIntent.Buff:   buffActions  .Add(action); break;
            }
        }

        int averageBlockValue = 0;
        if (blockActions.Count > 0)
        {
            int totalBlockValue = 0;
            foreach (var a in blockActions) totalBlockValue += a.value;
            averageBlockValue = totalBlockValue / blockActions.Count;
        }

        int lowBlockThreshold = averageBlockValue / 2;
        int criticalBlockThreshold = Mathf.Max(3, averageBlockValue / 4);
        bool hasLowBlock = block > 0 && block <= lowBlockThreshold;
        bool hasCriticalBlock = block > 0 && block <= criticalBlockThreshold;
        bool hasNoBlock = block == 0;
        bool hasGoodBlock = block > lowBlockThreshold;

        if (healthPercent < 0.2f)
        {
            if (healActions.Count > 0) return GetRandomAction(healActions);
            if (blockActions.Count > 0 && (hasNoBlock || hasCriticalBlock)) return GetRandomAction(blockActions);
        }

        if (healthPercent < 0.5f)
        {
            if (healActions.Count > 0 && UnityEngine.Random.value < 0.7f) return GetRandomAction(healActions);
            if (blockActions.Count > 0 && (hasNoBlock || hasLowBlock))
                if (UnityEngine.Random.value < 0.7f) return GetRandomAction(blockActions);
        }

        if (hasNoBlock && blockActions.Count > 0)
        {
            if (UnityEngine.Random.value < 0.5f) return GetRandomAction(blockActions);
        }
        else if (hasCriticalBlock && blockActions.Count > 0)
        {
            if (UnityEngine.Random.value < 0.4f) return GetRandomAction(blockActions);
        }
        else if (hasLowBlock && blockActions.Count > 0)
        {
            if (UnityEngine.Random.value < 0.25f) return GetRandomAction(blockActions);
        }

        if (healthPercent > 0.8f && hasGoodBlock && buffActions.Count > 0)
        {
            if (UnityEngine.Random.value < 0.3f) return GetRandomAction(buffActions);
        }

        if (attackActions.Count > 0) return GetRandomAction(attackActions);

        Debug.LogWarning($"[AI] {enemyName} has no attack actions - picking random");
        return actionPattern[UnityEngine.Random.Range(0, actionPattern.Count)];
    }

    private EnemyAction GetRandomAction(List<EnemyAction> actions)
    {
        if (actions == null || actions.Count == 0)
        {
            Debug.LogError("[AI] GetRandomAction called with empty list!");
            return new EnemyAction { intent = EnemyIntent.Attack, value = attackPower, customName = "" };
        }
        return actions[UnityEngine.Random.Range(0, actions.Count)];
    }

    // -------------------------------------------------------
    // Execute current intent
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
                // Sample from buff range, then scale ONLY Attack/Block/Heal ranges (exclude buff self-scaling)
                float m = SampleMiddleBiased(buffRange.min, buffRange.max);
                ApplyBuffToRanges(m, includeBuff: false);
                break;
        }
    }

    public bool IsAlive() => isAlive;

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────
    private static float SampleMiddleBiased(float min, float max)
    {
        float a = UnityEngine.Random.Range(min, max);
        float b = UnityEngine.Random.Range(min, max);
        return (a + b) * 0.5f;
    }

    private void ApplyBuffToRanges(float mul, bool includeBuff)
    {
        attackRange = ScaleClamped(attackRange, mul);
        blockRange  = ScaleClamped(blockRange , mul);
        healRange   = ScaleClamped(healRange  , mul);
        if (includeBuff) buffRange = ScaleClamped(buffRange, mul);

        static EnemyConfig.IntentMultiplierRange ScaleClamped(EnemyConfig.IntentMultiplierRange r, float m)
        {
            var rr = new EnemyConfig.IntentMultiplierRange { min = r.min * m, max = r.max * m };
            rr.Normalize();
            return rr;
        }
    }
}
