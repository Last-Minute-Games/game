using System.Collections.Generic;
using UnityEngine;

namespace Entities.Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "NewEnemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Core")]
        public string enemyName;
        public int maxHealth;
        public int attackPower;
        public int defensePower;
        public Sprite artwork;
        public List<EnemyAction> actionPattern;

        [Header("Animator (Optional)")]
        [Tooltip("If assigned, this RuntimeAnimatorController will drive the enemy's animation states (Idle/Attack/Hurt/Death). Drag & drop here.")]
        public RuntimeAnimatorController animatorController;

        [Header("Animation Clips (Optional, used if no Animator Controller)")]
        [Tooltip("Looping idle animation clip for this enemy (optional).")]
        public AnimationClip idleClip;
        [Tooltip("Attack animation clip for this enemy (optional).")]
        public AnimationClip attackClip;
        [Tooltip("Hurt animation clip for this enemy (optional).")]
        public AnimationClip hurtClip;
        [Tooltip("Death animation clip for this enemy (optional).")]
        public AnimationClip deathClip;

        public EnemyData CreateRuntimeInstance()
        {
            var data = new EnemyData();
            data.Initialize(enemyName, maxHealth, attackPower, defensePower);
            data.actionPattern = new List<EnemyAction>(actionPattern);
            data.artwork = artwork;
            // Propagate animator controller and animation clips
            data.animatorController = animatorController;
            data.idleClip = idleClip;
            data.attackClip = attackClip;
            data.hurtClip = hurtClip;
            data.deathClip = deathClip;
            return data;
        }
    }
}