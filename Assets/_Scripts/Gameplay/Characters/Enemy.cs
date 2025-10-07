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

    [Header("Behavior Chances (must sum to 1.0f or 100%)")]
    [Range(0f, 1f)] public float idleChance = 0.2f;
    [Range(0f, 1f)] public float attackChance = 0.6f;
    [Range(0f, 1f)] public float defendChance = 0.2f;

    [Header("UI")]
    public GameObject healthBarPrefab;
    public TMP_Text intentionText;

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
            intentionText.text = "Waiting...";
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
        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);
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
        // ✅ ensure values sum up properly
        float total = idleChance + attackChance + defendChance;
        if (Mathf.Abs(total - 1f) > 0.001f)
        {
            Debug.LogWarning($"{characterName}: Intention chances do not sum to 1 (total={total:F2}). Normalizing automatically.");
            idleChance /= total;
            attackChance /= total;
            defendChance /= total;
        }

        float roll = Random.value; // 0.0 → 1.0
        if (roll < idleChance)
        {
            currentIntention = EnemyIntention.Idle;
            intentionText.text = "Idle";
        }
        else if (roll < idleChance + attackChance)
        {
            currentIntention = EnemyIntention.Attack;
            intentionText.text = $"Attack ({strength})";
        }
        else
        {
            currentIntention = EnemyIntention.Defend;
            intentionText.text = "Defend";
        }

        Debug.Log($"{characterName} intention decided: {currentIntention}");
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
                ShowBlockFeedback(5);
                break;

            case EnemyIntention.Idle:
                FloatingTextManager.Instance?.SpawnText(
                    transform.position + Vector3.up * 2f,
                    "Idle",
                    Color.gray
                );
                break;
        }

        intentionText.text = "Waiting...";
    }

    private void PerformAttack(CharacterBase target)
    {
        int damage = strength;
        Debug.Log($"{characterName} attacks {target.characterName} for {damage} damage!");
        FloatingTextManager.Instance?.SpawnText(target.transform.position + Vector3.up * 2f, $"-{damage}", Color.red);
        target.ShowDamageFeedback(damage);
        target.TakeDamage(damage);
    }
}
