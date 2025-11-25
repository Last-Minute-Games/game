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

public enum EnemyIntent
{
    Attack,
    Block,
    Heal,
    Buff
}

[Serializable]
public class IntentIconMapping
{
    [Header("Intent Icons")]
    [Tooltip("Icon shown when enemy intends to attack")]
    public Sprite attackIcon;
    
    [Tooltip("Icon shown when enemy intends to block/defend")]
    public Sprite blockIcon;
    
    [Tooltip("Icon shown when enemy intends to heal")]
    public Sprite healIcon;
    
    [Tooltip("Icon shown when enemy intends to buff")]
    public Sprite buffIcon;

    /// <summary>
    /// Get the appropriate icon sprite for the given intent.
    /// </summary>
    public Sprite GetIconForIntent(EnemyIntent intent)
    {
        return intent switch
        {
            EnemyIntent.Attack => attackIcon,
            EnemyIntent.Block => blockIcon,
            EnemyIntent.Heal => healIcon,
            EnemyIntent.Buff => buffIcon,
            _ => null
        };
    }
}

[Serializable]
public class EnemyData : EntityData
{
    [Header("Core Stats")]
    public int attackPower;           // Base attack power for intents
    public int defensePower;          // Optional, if you have defensive actions

    [Header("Intent System")]
    public EnemyIntent currentIntent; // What the enemy plans to do this turn
    public EnemyAction currentAction; // The full action being performed (includes custom name)
    public string intentText;         // Text like "Attack" or "Buff Self"
    public int intentValue;           // How much damage or block that intent will do


    [Header("Behavior")]
    public List<EnemyAction> actionPattern; // Optional list of possible actions

    [Header("Metadata")]
    public int enemyID;
    public string enemyName;
    public Sprite artwork;                  // Optional portrait or sprite
    public AudioClip attackSFX;             // Optional sound for attack
    public EnemyConfig sourceConfig;

    [Header("Animator (Optional)")]
    public RuntimeAnimatorController animatorController; // If set, animator-driven states are used

    [Header("Sprite Animations (Lightweight alternative to AnimationClips)")]
    public SpriteAnimation idleAnim;
    public SpriteAnimation attackAnim;
    public SpriteAnimation hurtAnim;
    public SpriteAnimation deathAnim;

    [Header("Visual Adjustments")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;

    // -------------------------------------------------------
    // Initialization
    // -------------------------------------------------------
    public void Initialize(string name, int maxHealth, int atk, int def)
    {
        enemyName = name;
        attackPower = atk;
        defensePower = def;
        
        // Call base EntityData initialization
        base.Initialize(name, maxHealth);
        
        actionPattern = new List<EnemyAction>();
    }

    // -------------------------------------------------------
    // Enemy chooses what to do next - Intelligent AI System
    // -------------------------------------------------------
    public void DecideNextIntent()
    {
        if (actionPattern == null || actionPattern.Count == 0)
        {
            // Default: simple attack
            currentIntent = EnemyIntent.Attack;
            currentAction = new EnemyAction { intent = EnemyIntent.Attack, value = attackPower, customName = "" };
            intentValue = attackPower;
            intentText = "Attack";
            return;
        }

        // Use intelligent AI to decide best move based on current situation
        EnemyAction chosenAction = ChooseStrategicAction();
        currentIntent = chosenAction.intent;
        currentAction = chosenAction; // Store the full action (includes custom name)
        intentValue = chosenAction.value;
        intentText = chosenAction.intent.ToString();
    }

    /// <summary>
    /// Intelligent AI system that chooses the best action based on enemy's current stats and situation.
    /// </summary>
    private EnemyAction ChooseStrategicAction()
    {
        // Calculate health percentage
        float healthPercent = currentHealth / (float)maxHealth;
        
        // Separate actions by type for strategic selection
        List<EnemyAction> attackActions = new List<EnemyAction>();
        List<EnemyAction> blockActions = new List<EnemyAction>();
        List<EnemyAction> healActions = new List<EnemyAction>();
        List<EnemyAction> buffActions = new List<EnemyAction>();
        
        foreach (var action in actionPattern)
        {
            switch (action.intent)
            {
                case EnemyIntent.Attack:
                    attackActions.Add(action);
                    break;
                case EnemyIntent.Block:
                    blockActions.Add(action);
                    break;
                case EnemyIntent.Heal:
                    healActions.Add(action);
                    break;
                case EnemyIntent.Buff:
                    buffActions.Add(action);
                    break;
            }
        }

        // Calculate average block value to set "low block" threshold
        int averageBlockValue = 0;
        if (blockActions.Count > 0)
        {
            int totalBlockValue = 0;
            foreach (var blockAction in blockActions)
            {
                totalBlockValue += blockAction.value;
            }
            averageBlockValue = totalBlockValue / blockActions.Count;
        }
        
        // Define block thresholds based on available block actions
        int lowBlockThreshold = averageBlockValue / 2; // Half of average block is "low"
        int criticalBlockThreshold = Mathf.Max(3, averageBlockValue / 4); // Very low block
        bool hasLowBlock = block > 0 && block <= lowBlockThreshold;
        bool hasCriticalBlock = block > 0 && block <= criticalBlockThreshold;
        bool hasNoBlock = block == 0;
        bool hasGoodBlock = block > lowBlockThreshold;

        // AI Decision Tree based on stats and situation
        
        // CRITICAL HEALTH (< 20%) - Prioritize survival
        if (healthPercent < 0.2f)
        {
            // First priority: Heal if available
            if (healActions.Count > 0)
            {
                Debug.Log($"[AI] {enemyName} is critical ({healthPercent:P0}) - choosing HEAL");
                return GetRandomAction(healActions);
            }
            // Second priority: Block to survive (even if we have some block)
            if (blockActions.Count > 0 && (hasNoBlock || hasCriticalBlock))
            {
                Debug.Log($"[AI] {enemyName} is critical ({healthPercent:P0}) with low/no block ({block}) - choosing BLOCK");
                return GetRandomAction(blockActions);
            }
        }
        
        // LOW HEALTH (20% - 50%) - Defensive play
        if (healthPercent < 0.5f)
        {
            // High priority: Heal if available (70% chance)
            if (healActions.Count > 0 && UnityEngine.Random.value < 0.7f)
            {
                Debug.Log($"[AI] {enemyName} is low health ({healthPercent:P0}) - choosing HEAL");
                return GetRandomAction(healActions);
            }
            
            // Maintain block coverage - refresh if low or none
            if (blockActions.Count > 0 && (hasNoBlock || hasLowBlock))
            {
                // 70% chance to refresh block when vulnerable at low health
                if (UnityEngine.Random.value < 0.7f)
                {
                    Debug.Log($"[AI] {enemyName} is low health ({healthPercent:P0}) with low block ({block}) - refreshing BLOCK");
                    return GetRandomAction(blockActions);
                }
            }
        }
        
        // BLOCK MAINTENANCE (any health) - Keep defensive coverage
        if (hasNoBlock && blockActions.Count > 0)
        {
            // 50% chance to block when completely unprotected
            if (UnityEngine.Random.value < 0.5f)
            {
                Debug.Log($"[AI] {enemyName} has no block - choosing BLOCK for protection");
                return GetRandomAction(blockActions);
            }
        }
        else if (hasCriticalBlock && blockActions.Count > 0)
        {
            // 40% chance to refresh when block is critically low
            if (UnityEngine.Random.value < 0.4f)
            {
                Debug.Log($"[AI] {enemyName} has critically low block ({block}) - refreshing BLOCK");
                return GetRandomAction(blockActions);
            }
        }
        else if (hasLowBlock && blockActions.Count > 0)
        {
            // 25% chance to maintain when block is just low
            if (UnityEngine.Random.value < 0.25f)
            {
                Debug.Log($"[AI] {enemyName} has low block ({block}) - maintaining BLOCK");
                return GetRandomAction(blockActions);
            }
        }
        
        // FULL HEALTH with GOOD BLOCK - Can afford to buff
        if (healthPercent > 0.8f && hasGoodBlock && buffActions.Count > 0)
        {
            // 30% chance to buff when safe
            if (UnityEngine.Random.value < 0.3f)
            {
                Debug.Log($"[AI] {enemyName} is healthy ({healthPercent:P0}) with good block ({block}) - choosing BUFF");
                return GetRandomAction(buffActions);
            }
        }
        
        // DEFAULT - Attack (aggressive AI)
        if (attackActions.Count > 0)
        {
            Debug.Log($"[AI] {enemyName} choosing ATTACK (HP: {healthPercent:P0}, Block: {block})");
            return GetRandomAction(attackActions);
        }
        
        // FALLBACK - Return any random action if attack not available
        Debug.LogWarning($"[AI] {enemyName} has no attack actions - picking random");
        return actionPattern[UnityEngine.Random.Range(0, actionPattern.Count)];
    }

    /// <summary>
    /// Helper method to get a random action from a list
    /// </summary>
    private EnemyAction GetRandomAction(List<EnemyAction> actions)
    {
        if (actions == null || actions.Count == 0)
        {
            Debug.LogError("[AI] GetRandomAction called with empty list!");
            return actionPattern[0]; // Fallback
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
                GainBlock(intentValue);  // Now calls inherited method
                break;

            case EnemyIntent.Heal:
                Heal(intentValue);  // Now calls inherited method
                break;

            case EnemyIntent.Buff:
                ApplyStatus("Strength", intentValue);  // Now calls inherited method
                break;
        }
    }

    public bool IsAlive() => isAlive;  // Now uses inherited field
}


