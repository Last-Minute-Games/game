using System.Collections.Generic;
using UnityEngine;

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

    [Header("Animation Clips")]
    [Tooltip("Looping idle animation clip for this enemy (optional).")]
    public AnimationClip idleClip;
    [Tooltip("Attack animation clip for this enemy (optional).")]
    public AnimationClip attackClip;
    [Tooltip("Death animation clip for this enemy (optional).")]
    public AnimationClip deathClip;

    public EnemyData CreateRuntimeInstance()
    {
        var data = new EnemyData();
        data.Initialize(enemyName, maxHealth, attackPower, defensePower);
        data.actionPattern = new List<EnemyAction>(actionPattern);
        data.artwork = artwork;
        // Propagate animation clips
        data.idleClip = idleClip;
        data.attackClip = attackClip;
        data.deathClip = deathClip;
        return data;
    }
}