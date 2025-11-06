using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyData> enemies = new();

    public void InitializeEnemies(List<EnemyData> enemyList)
    {
        enemies = enemyList;
    }

    // Roll/decide next intents for all alive enemies (called at round start)
    public void RollNextIntents()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.entity.isAlive) continue;
            enemy.DecideNextIntent();
        }
    }

    // Enemies execute their previously decided intents
    public void ExecuteEnemyTurn(ref EntityData player)
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.entity.isAlive) continue;
            enemy.ExecuteIntent(ref player);
        }
    }

    public bool AllEnemiesDefeated()
    {
        foreach (var e in enemies)
            if (e.entity.isAlive) return false;
        return true;
    }
}