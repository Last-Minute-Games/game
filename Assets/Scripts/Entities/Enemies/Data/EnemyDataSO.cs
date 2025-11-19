using System.Collections.Generic;
using GameItems;
using UnityEngine;

namespace Entities.Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "NewEnemy")]
    public class EnemyDataSO : ScriptableObject
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

    [Header("Sprite Animations (used if no Animator Controller)")]
    [Tooltip("Looping idle animation for this enemy.")]
    public SpriteAnimation idleAnim;
    [Tooltip("Attack animation for this enemy.")]
    public SpriteAnimation attackAnim;
    [Tooltip("Hurt animation for this enemy.")]
    public SpriteAnimation hurtAnim;
    [Tooltip("Death animation for this enemy.")]
    public SpriteAnimation deathAnim;

    public EnemyData CreateRuntimeInstance()
    {
        var data = new EnemyData();
        data.Initialize(enemyName, maxHealth, attackPower, defensePower);
        data.actionPattern = new List<EnemyAction>(actionPattern);
        data.artwork = artwork;
        // Propagate animator controller and animation clips
        data.animatorController = animatorController;
        data.idleAnim = idleAnim;
        data.attackAnim = attackAnim;
        data.hurtAnim = hurtAnim;
        data.deathAnim = deathAnim;
        return data;
    }
}
}
