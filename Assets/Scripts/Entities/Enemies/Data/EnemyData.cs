using System;
using System.Collections.Generic;
using Entities.Players.Data;
using UnityEngine;

namespace Entities.Enemies.Data
{
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
        [Header("Core Stats")] public int attackPower;
        public int defensePower;

        [Header("Intent System")] public EnemyIntent currentIntent;
        public Sprite intentIcon;
        public string intentText;
        public int intentValue;

        [Header("Behavior")] public List<EnemyAction> actionPattern;
        private int actionIndex;

        [Header("Metadata")] public int enemyID;
        public string enemyName;
        public Sprite artwork;
        public AudioClip attackSFX;

        [Header("Animator (Optional)")] public RuntimeAnimatorController animatorController;

        [Header("Animation Clips (Optional, fallback when no Animator Controller)")]
        public AnimationClip idleClip;

        public AnimationClip attackClip;
        public AnimationClip hurtClip;
        public AnimationClip deathClip;

        public void Initialize(string name, int maxHealth, int atk, int def)
        {
            enemyName = name;
            attackPower = atk;
            defensePower = def;

            // Call base entity initialization
            base.Initialize(name, maxHealth);

            actionPattern = new List<EnemyAction>();
            actionIndex = 0;
        }

        public void DecideNextIntent()
        {
            if (actionPattern == null || actionPattern.Count == 0)
            {
                currentIntent = EnemyIntent.Attack;
                intentValue = attackPower;
                intentText = "Attack";
                return;
            }

            int idx = UnityEngine.Random.Range(0, actionPattern.Count);
            var nextAction = actionPattern[idx];
            currentIntent = nextAction.intent;
            intentValue = nextAction.value;
            intentText = nextAction.intent.ToString();
        }

        public void ExecuteIntent(PlayerData player)
        {
            switch (currentIntent)
            {
                case EnemyIntent.Attack:
                    player.TakeDamage(intentValue);
                    break;

                case EnemyIntent.Block:
                    GainBlock(intentValue); // now from base class
                    break;

                case EnemyIntent.Heal:
                    Heal(intentValue); // now from base class
                    break;

                case EnemyIntent.Buff:
                    ApplyStatus("Strength", intentValue);
                    break;
            }
        }

        public bool IsAlive() => isAlive; // from base class
    }
}