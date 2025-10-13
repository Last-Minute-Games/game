using UnityEngine;

public class EnemyRunner : MonoBehaviour
{
    [Header("Reference")]
    public EnemyData data;
    public GameObject enemyPrefab;

    private Enemy spawnedEnemy;

    private void Start()
    {
        if (enemyPrefab == null || data == null)
        {
            Debug.LogError("❌ EnemyRunner missing prefab or data!");
            return;
        }

        // Instantiate enemy
        GameObject enemyObj = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        spawnedEnemy = enemyObj.GetComponent<Enemy>();

        if (spawnedEnemy != null)
        {
            // ✅ Give it its data reference
            spawnedEnemy.data = data;

            // Apply appearance
            if (data.animationSet != null && data.animationSet.idleSprite != null)
            {
                SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = data.animationSet.idleSprite;
            }

            // Basic stats from data
            spawnedEnemy.characterName = data.enemyName;
            spawnedEnemy.maxHealth = data.maxHealth;
            spawnedEnemy.currentHealth = data.maxHealth;
            spawnedEnemy.strength = data.baseDamage;
            spawnedEnemy.defense = data.defense;
        }
    }
}
