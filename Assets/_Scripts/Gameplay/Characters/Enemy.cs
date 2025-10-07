using UnityEngine;

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

    private HealthBar healthBarInstance;

    protected override void Awake()
    {
        base.Awake();
        RandomizeStats();

        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<HealthBar>();
            healthBarInstance.Initialize(this);
        }
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
}
