using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyAction
{
    public EnemyIntent intent;
    public int value;
}

public enum EnemyIntent
{
    Attack,
    Block,
    Heal,
    Buff
}

[Serializable]
public class EnemyData
{
    [Header("Core Stats")]
    public EntityData entity;         // Shared health, block, status data
    public int attackPower;           // Base attack power for intents
    public int defensePower;          // Optional, if you have defensive actions

    [Header("Intent System")]
    public EnemyIntent currentIntent; // What the enemy plans to do this turn
    public Sprite intentIcon;         // Icon shown above the enemy (attack, block, buff)
    public string intentText;         // Text like “Attack” or “Buff Self”
    public int intentValue;           // How much damage or block that intent will do

    [Header("Behavior")]
    public List<EnemyAction> actionPattern; // Optional list of possible actions
    private int actionIndex;                 // Current action in the pattern

    [Header("Metadata")]
    public int enemyID;
    public string enemyName;
    public Sprite artwork;                  // Optional portrait or sprite
    public AudioClip attackSFX;             // Optional sound for attack

    [Header("Animator (Optional)")]
    public RuntimeAnimatorController animatorController; // If set, animator-driven states are used

    [Header("Animation Clips (Optional, fallback when no Animator Controller)")]
    public AnimationClip idleClip;          // Optional idle animation
    public AnimationClip attackClip;        // Optional attack animation
    public AnimationClip hurtClip;          // Optional hurt animation
    public AnimationClip deathClip;         // Optional death animation

    // -------------------------------------------------------
    // Initialization
    // -------------------------------------------------------
    public void Initialize(string name, int maxHealth, int atk, int def)
    {
        enemyName = name;
        attackPower = atk;
        defensePower = def;
        entity = new EntityData();
        entity.Initialize(name, maxHealth);
        actionPattern = new List<EnemyAction>();
        actionIndex = 0;
    }

    // -------------------------------------------------------
    // Enemy chooses what to do next
    // -------------------------------------------------------
    public void DecideNextIntent()
    {
        if (actionPattern == null || actionPattern.Count == 0)
        {
            // Default: simple attack
            currentIntent = EnemyIntent.Attack;
            intentValue = attackPower;
            intentText = "Attack";
            return;
        }

        // Randomly select an action from the pattern (Slay the Spire-like)
        int idx = UnityEngine.Random.Range(0, actionPattern.Count);
        var nextAction = actionPattern[idx];
        currentIntent = nextAction.intent;
        intentValue = nextAction.value;
        intentText = nextAction.intent.ToString();
    }

    // -------------------------------------------------------
    // Execute current intent
    // -------------------------------------------------------
    public void ExecuteIntent(ref EntityData player)
    {
        switch (currentIntent)
        {
            case EnemyIntent.Attack:
                player.TakeDamage(intentValue);
                break;

            case EnemyIntent.Block:
                entity.GainBlock(intentValue);
                break;

            case EnemyIntent.Heal:
                entity.Heal(intentValue);
                break;

            case EnemyIntent.Buff:
                entity.ApplyStatus("Strength", intentValue);
                break;
        }
    }

    public bool IsAlive() => entity.isAlive;
}
