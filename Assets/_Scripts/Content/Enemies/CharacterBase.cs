using UnityEngine;
using DG.Tweening;

/// <summary>
/// Base class for all combat entities (Player and Enemy).
/// Handles health, block, strength scaling, visuals, and feedback.
/// </summary>
public abstract class CharacterBase : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Unnamed";
    public Sprite portrait;

    [Header("Core Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int strength = 0;  // offensive scaling
    public int block = 0;     // temporary shield, resets each turn

    [Header("UI")]
    public HealthBarBase healthBarInstance;

    [Header("Visuals")]
    public GameObject shieldPrefab; // optional VFX for player only
    protected GameObject activeShield;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // ==============================
    // DAMAGE / HEAL / BLOCK
    // ==============================

    /// <summary>
    /// Applies incoming damage, consuming block first.
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int totalDamage = amount;

        // 1️⃣ Apply block first
        if (block > 0)
        {
            int absorbed = Mathf.Min(block, totalDamage);
            block -= absorbed;
            totalDamage -= absorbed;
            UpdateBlockUI();
        }

        // 2️⃣ Apply remaining to HP
        if (totalDamage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - totalDamage);
            healthBarInstance?.UpdateHealth(currentHealth, maxHealth);
            ShowDamageFeedback(totalDamage);
        }

        Debug.Log($"{characterName} took {totalDamage} damage (HP {currentHealth}/{maxHealth}, Block {block})");

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Heals this entity, unaffected by block.
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        healthBarInstance?.UpdateHealth(currentHealth, maxHealth);

        ShowHealFeedback(amount);
        Debug.Log($"{characterName} healed {amount} (HP {currentHealth}/{maxHealth})");
    }

    /// <summary>
    /// Adds temporary block (consumed before HP).
    /// </summary>
    public virtual void AddBlock(int amount)
    {
        if (amount <= 0) return;

        block += amount;
        UpdateBlockUI();
        ShowBlockFeedback(amount);

        Debug.Log($"{characterName} gained {amount} Block (Total {block})");
    }

    /// <summary>
    /// Clears all block at end of turn.
    /// </summary>
    public virtual void EndTurn()
    {
        if (block > 0)
        {
            block = 0;
            UpdateBlockUI();
            Debug.Log($"{characterName}'s Block dissipated.");
        }
    }

    // ==============================
    // UI / VISUALS
    // ==============================

    protected virtual void UpdateBlockUI()
    {
        if (healthBarInstance != null)
            healthBarInstance.UpdateBlock(block);

        UpdateShieldVisual();
    }

    protected virtual void UpdateShieldVisual()
    {
        // Player-only shield particle feedback
        if (!(this is Player)) return;

        if (block > 0)
        {
            if (activeShield == null && shieldPrefab != null)
            {
                activeShield = Instantiate(shieldPrefab, transform);
                activeShield.transform.localScale = Vector3.one * 5f;

                if (activeShield.TryGetComponent(out ParticleSystem ps))
                    ps.Play();
            }
        }
        else if (activeShield != null)
        {
            Destroy(activeShield);
            activeShield = null;
        }
    }

    // ==============================
    // FEEDBACK
    // ==============================

    public virtual void ShowDamageFeedback(int amount)
    {
        if (!TryGetComponent(out SpriteRenderer sr)) return;

        Color baseColor = sr.color;
        sr.DOColor(Color.red, 0.1f).OnComplete(() => sr.DOColor(baseColor, 0.2f));
        transform.DOShakePosition(0.25f, 0.25f, 15, 90);

        FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up, $"-{amount}", Color.red);
    }

    public virtual void ShowHealFeedback(int amount)
    {
        if (!TryGetComponent(out SpriteRenderer sr)) return;

        Color baseColor = sr.color;
        sr.DOColor(Color.green, 0.1f).OnComplete(() => sr.DOColor(baseColor, 0.2f));
        transform.DOMoveY(transform.position.y + 0.2f, 0.15f).SetLoops(2, LoopType.Yoyo);

        FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up, $"+{amount}", Color.green);
    }

    public virtual void ShowBlockFeedback(int amount)
    {
        if (!TryGetComponent(out SpriteRenderer sr)) return;

        sr.DOColor(Color.cyan, 0.1f).OnComplete(() => sr.DOColor(Color.white, 0.2f));
        transform.DOMoveY(transform.position.y + 0.15f, 0.1f).SetLoops(2, LoopType.Yoyo);

        FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up, $"+{amount} Block", Color.cyan);
    }

    // ==============================
    // DEATH
    // ==============================

    protected virtual void Die()
    {
        Debug.Log($"{characterName} has fallen.");

        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);

        if (activeShield != null)
            Destroy(activeShield);

        gameObject.SetActive(false);
    }
}
