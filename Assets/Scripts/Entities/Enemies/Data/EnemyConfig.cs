using System.Collections.Generic;
using UnityEngine;
using Entities.Enemies.Helpers;

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

        [Header("Sprite Animations (Lightweight, used if no Animator Controller)")]
        [Tooltip("Looping idle sprite animation for this enemy (optional).")]
        public SpriteAnimation idleAnim;
        [Tooltip("Attack sprite animation for this enemy (optional).")]
        public SpriteAnimation attackAnim;
        [Tooltip("Hurt sprite animation for this enemy (optional).")]
        public SpriteAnimation hurtAnim;
        [Tooltip("Death sprite animation for this enemy (optional).")]
        public SpriteAnimation deathAnim;

        public EnemyData CreateRuntimeInstance()
        {
            var data = new EnemyData();
            data.Initialize(enemyName, maxHealth, attackPower, defensePower);
            data.actionPattern = new List<EnemyAction>(actionPattern);
            data.artwork = artwork;
            // Propagate animator controller and sprite animations
            data.animatorController = animatorController;
            data.idleAnim = idleAnim;
            data.attackAnim = attackAnim;
            data.hurtAnim = hurtAnim;
            data.deathAnim = deathAnim;
            return data;
        }
    }
}

