using DG.Tweening;
using UnityEngine;
using DG.Tweening;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Unnamed";
    public Sprite portrait;

    [Header("Core Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int block;         // temporary damage reduction
    public int strength = 0;  // bonus for attack cards
    public int defense = 0;   // bonus for defense cards

    [Header("Inventory")]
    public Inventory inventory; // optional (hook later)

    [Header("UI")]
    protected HealthBar healthBarInstance;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        int mitigated = Mathf.Max(amount - block - defense, 0);
        block = Mathf.Max(block - amount, 0); // block consumed first
        currentHealth -= mitigated;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{characterName} took {mitigated} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{characterName} healed {amount}! HP: {currentHealth}/{maxHealth}");
    }

    public virtual void AddBlock(int amount)
    {
        block += amount;
        Debug.Log($"{characterName} gains {amount} block! (Total block: {block})");
    }

    protected virtual void Die()
    {
        Debug.Log($"{characterName} has fallen!");

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
            healthBarInstance = null;
        }

        gameObject.SetActive(false);
    }

    public void ShowDamageFeedback(int amount)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color baseColor = sr.color;
        sr.DOColor(Color.red, 0.1f).OnComplete(() => sr.DOColor(baseColor, 0.2f));
        transform.DOShakePosition(0.25f, 0.3f, 15, 90);

        FloatingTextManager.Instance.SpawnText(transform.position + Vector3.up, $"-{amount}", Color.red);
    }

    public void ShowHealFeedback(int amount)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color baseColor = sr.color;
        sr.DOColor(Color.green, 0.1f).OnComplete(() => sr.DOColor(baseColor, 0.2f));
        transform.DOMoveY(transform.position.y + 0.2f, 0.15f).SetLoops(2, LoopType.Yoyo);

        FloatingTextManager.Instance.SpawnText(transform.position + Vector3.up, $"+{amount}", Color.green);
    }

    public void ShowBlockFeedback(int amount)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.DOColor(Color.cyan, 0.1f).OnComplete(() => sr.DOColor(Color.white, 0.2f));
        transform.DOMoveY(transform.position.y + 0.2f, 0.15f).SetLoops(2, LoopType.Yoyo);

        FloatingTextManager.Instance.SpawnText(transform.position + Vector3.up, $"+{amount} Block", Color.cyan);
    }
}
