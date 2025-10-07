using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : CharacterBase
{
    [Header("Enemy Parameters")]
    public int minHealth = 50;
    public int maxHealthRange = 85;
    public int minDamage = 8;
    public int maxDamage = 30;
    public Sprite[] possibleSprites;

    [Header("UI")]
    public GameObject healthBarPrefab;
    public TMP_Text intentionText;

    private HealthBar healthBarInstance;

    private enum EnemyIntention { Idle, Attack, Defend }
    private EnemyIntention currentIntention;

    protected override void Awake()
    {
        base.Awake();
        RandomizeStats();

        // Spawn health bar
        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<HealthBar>();
            healthBarInstance.Initialize(this);
        }

        if (intentionText != null)
            intentionText.text = "💤 Waiting...";
    }

    private void RandomizeStats()
    {
        maxHealth = Random.Range(minHealth, maxHealthRange);
        currentHealth = maxHealth;
        strength = Random.Range(minDamage, maxDamage);
        defense = Random.Range(0, 5);

        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
        }

        Debug.Log($"Spawned enemy '{characterName}' with HP {maxHealth}, STR {strength}, DEF {defense}");
    }

    protected override void Die()
    {
        Debug.Log($"{characterName} defeated!");
        Destroy(gameObject, 0.5f);
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        if (healthBarInstance != null)
            healthBarInstance.UpdateBar();
    }

    // --- INTENTION SYSTEM ---
    public void DecideNextIntention()
    {
        int roll = Random.Range(0, 3);
        switch (roll)
        {
            case 0:
                currentIntention = EnemyIntention.Attack;
                intentionText.text = $"⚔️ Attack ({strength})";
                break;
            case 1:
                currentIntention = EnemyIntention.Defend;
                intentionText.text = "🛡️ Defend";
                break;
            default:
                currentIntention = EnemyIntention.Idle;
                intentionText.text = "💤 Idle";
                break;
        }
    }

    public IEnumerator ExecuteIntention(Player player)
    {
        switch (currentIntention)
        {
            case EnemyIntention.Attack:
                yield return new WaitForSeconds(0.4f);
                PerformAttack(player);
                break;

            case EnemyIntention.Defend:
                AddBlock(5);
                Debug.Log($"{characterName} defends and gains 5 block!");
                ShowBlockFeedback(5); // ✅ FIXED: Correct method name
                break;

            case EnemyIntention.Idle:
                FloatingTextManager.Instance?.SpawnText(
                    transform.position + Vector3.up * 2f,
                    "💤 Idle",
                    Color.gray
                );
                break;
        }

        intentionText.text = "💤 Waiting...";
    }

    private void PerformAttack(CharacterBase target)
    {
        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");

        // ✅ Show floating text above the *player*, not the enemy
        FloatingTextManager.Instance?.SpawnText(
            target.transform.position + Vector3.up * 2f,
            $"-{damage}",
            Color.red
        );

        // ✅ Apply hit effects (shake, flash, etc.)
        target.ShowDamageFeedback(damage);

        // ✅ Apply actual damage
        target.TakeDamage(damage);
    }
}
