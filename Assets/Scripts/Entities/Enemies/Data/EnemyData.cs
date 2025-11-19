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
}

public enum EnemyIntent
{
    Attack,
    Block,
    Heal,
    Buff
}

[Serializable]
public class EnemyData : EntityData
{
    [Header("Core Stats")]
    public int attackPower;           // Base attack power for intents
    public int defensePower;          // Optional, if you have defensive actions

    [Header("Intent System")]
    public EnemyIntent currentIntent; // What the enemy plans to do this turn
    public Sprite intentIcon;         // Icon shown above the enemy (attack, block, buff)
    public string intentText;         // Text like "Attack" or "Buff Self"
    public int intentValue;           // How much damage or block that intent will do

    [Header("Behavior")]
    public List<EnemyAction> actionPattern; // Optional list of possible actions

    [Header("Metadata")]
    public int enemyID;
    public string enemyName;
    public Sprite artwork;                  // Optional portrait or sprite
    public AudioClip attackSFX;             // Optional sound for attack

    [Header("Animator (Optional)")]
    public RuntimeAnimatorController animatorController; // If set, animator-driven states are used

    [Header("Sprite Animations (Lightweight alternative to AnimationClips)")]
    public SpriteAnimation idleAnim;
    public SpriteAnimation attackAnim;
    public SpriteAnimation hurtAnim;
    public SpriteAnimation deathAnim;

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


