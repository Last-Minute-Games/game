using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "NewEnemy")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public int attackPower;
    public int defensePower;
    public Sprite artwork;
    public List<EnemyAction> actionPattern;

    public EnemyData CreateRuntimeInstance()
    {
        var data = new EnemyData();
        data.Initialize(enemyName, maxHealth, attackPower, defensePower);
        data.actionPattern = new List<EnemyAction>(actionPattern);
        data.artwork = artwork;
        return data;
    }
}