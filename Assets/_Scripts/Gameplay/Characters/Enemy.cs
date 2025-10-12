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

    [Header("Behavior Chances")]
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

        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<HealthBar>();
            healthBarInstance.Initialize(this);

            // Hide defense panel for enemies
            Transform defensePanel = barObj.transform.Find("DefensePanel");
            if (defensePanel != null)
                defensePanel.gameObject.SetActive(false);
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
    }

    public void DecideNextIntention()
    {
        float total = idleChance + attackChance + defendChance;
        idleChance /= total;
        attackChance /= total;
        defendChance /= total;

        float roll = Random.value;
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
    }

    public IEnumerator ExecuteIntention(Player player)
    {
        switch (currentIntention)
        {
            case EnemyIntention.Attack:
                yield return new WaitForSeconds(0.4f);
                player.TakeDamage(strength);
                break;
            case EnemyIntention.Defend:
                AddBlock(5); // block added internally, no visual
                ShowBlockFeedback(5); // optional feedback
                break;
            case EnemyIntention.Idle:
                FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up * 2f, "Idle", Color.gray);
                break;
        }

        intentionText.text = "Waiting...";
    }
}
