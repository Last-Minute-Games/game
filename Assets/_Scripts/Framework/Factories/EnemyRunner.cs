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
            spawnedEnemy.data = data;

            // Appearance
            if (data.animationSet != null && data.animationSet.idle != null && data.animationSet.idle.Length > 0)
            {
                SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = data.animationSet.idle[0];
            }

            // Core stats
            spawnedEnemy.characterName = data.enemyName;
            spawnedEnemy.maxHealth = data.maxHealth;
            spawnedEnemy.currentHealth = data.maxHealth;
            spawnedEnemy.strength = data.baseDamage;
        }
    }
}
