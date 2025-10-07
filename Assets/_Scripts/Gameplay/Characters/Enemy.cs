using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : CharacterBase
{
    [Header("Enemy Parameters")]
    public int minHealth = 50;
    public int maxHealthRange = 150;
    public int minDamage = 5;
    public int maxDamage = 20;
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

        // Start with no intention
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

    // --- NEW INTENTION SYSTEM ---
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
        if (currentIntention == EnemyIntention.Attack)
        {
            yield return new WaitForSeconds(0.5f);
            Attack(player);
        }
        else if (currentIntention == EnemyIntention.Defend)
        {
            AddBlock(5);
            Debug.Log($"{characterName} defends and gains 5 block!");
        }

        // Reset intention text after acting
        intentionText.text = "💤 Waiting...";
    }

    public void Attack(CharacterBase target)
    {
        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
        target.TakeDamage(damage);
    }
}
