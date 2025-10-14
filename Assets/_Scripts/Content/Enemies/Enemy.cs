using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : CharacterBase
{
    [Header("UI References")]
    public GameObject healthBarPrefab;
    public TMP_Text intentionText;
    public UnityEngine.UI.Image intentionIcon;

    [Header("Runtime")]
    public EnemyData data; // assigned at runtime via EnemyRunner
    private EnemyAnimator2D animator;
    private CardLibrary library;
    private CardData currentCard;
    private CardRunner pendingRunner;

    [Header("Scaling")]
    [Tooltip("Affects how strong this character’s card effects are.")]
    public float globalPowerScale = 1.0f;

    public bool IsDead { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<EnemyAnimator2D>();
        library = FindFirstObjectByType<CardLibrary>();
        SetupUI();
    }

    private void SetupUI()
    {
        // Health bar
        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<EnemyHealthBar>();
            healthBarInstance.Initialize(this);

            Transform defensePanel = barObj.transform.Find("DefensePanel");
            if (defensePanel != null)
                defensePanel.gameObject.SetActive(false);
        }

        // Initial "waiting" state for intentions
        if (intentionText != null)
        {
            intentionText.text = string.Empty;
            intentionText.alpha = 0f;
        }

        if (intentionIcon != null)
        {
            Color c = intentionIcon.color;
            c.a = 0f;
            intentionIcon.color = c;
        }
    }

    // =====================================================
    //  CARD SELECTION
    // =====================================================

    private CardData GetWeightedRandomCard()
    {
        if (IsDead || data == null || data.availableCards.Count == 0)
            return null;

        float totalWeight = 0f;
        foreach (var entry in data.availableCards)
        {
            if (entry.card == null) continue;
            float weight = Mathf.Max(0f, entry.baseWeight);
            if (entry.prioritizeWhenLowHP && currentHealth <= maxHealth / 3f)
                weight *= data.lowHPWeightMultiplier;
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
            return data.availableCards[0].card;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in data.availableCards)
        {
            if (entry.card == null) continue;
            float weight = Mathf.Max(0f, entry.baseWeight);
            if (entry.prioritizeWhenLowHP && currentHealth <= maxHealth / 3f)
                weight *= data.lowHPWeightMultiplier;
            cumulative += weight;
            if (roll <= cumulative)
                return entry.card;
        }

        return data.availableCards[0].card;
    }

    // =====================================================
    //  INTENTION PREPARATION
    // =====================================================

    public void PrepareNextCard()
    {
        if (IsDead) return;
        if (data == null || data.availableCards.Count == 0)
        {
            Debug.LogWarning($"{name}: No cards available!");
            return;
        }

        currentCard = GetWeightedRandomCard();
        if (currentCard == null) return;

        if (pendingRunner == null)
        {
            var go = new GameObject("EnemyPendingRunner");
            go.transform.SetParent(transform);
            pendingRunner = go.AddComponent<CardRunner>();
        }

        pendingRunner.data = currentCard;
        pendingRunner.RollIfNeeded(currentCard);

        int x = pendingRunner.GetPreviewX(this);

        // Colorize <X>
        string colorHex = "#FFCF40";
        foreach (var eff in currentCard.effects)
        {
            if (eff is DamageEffect) colorHex = "#FF4040";
            else if (eff is HealEffect) colorHex = "#40FF70";
            else if (eff is BlockEffect) colorHex = "#40BFFF";
        }
        string coloredX = $"<color={colorHex}>{x}</color>";

        // Build intention text
        string intent = currentCard.intentionText;
        if (!string.IsNullOrEmpty(intent) && intent.Contains("<X>"))
            intent = intent.Replace("<X>", coloredX);

        bool hasIcon = currentCard.intentionIcon != null;
        bool hasText = !string.IsNullOrEmpty(intent);

        // ----- ICON -----
        if (intentionIcon != null)
        {
            if (hasIcon)
            {
                intentionIcon.sprite = currentCard.intentionIcon;
                Color c = intentionIcon.color;
                c.a = 1f;
                intentionIcon.color = c;
                intentionIcon.enabled = true;
            }
            else
            {
                intentionIcon.enabled = false;
            }
        }

        // ----- TEXT -----
        if (intentionText != null)
        {
            if (hasText)
            {
                intentionText.text = intent;
                intentionText.enabled = true;
                intentionText.alpha = 1f;
            }
            else
            {
                intentionText.text = string.Empty;
                intentionText.enabled = false;
                intentionText.alpha = 0f;
            }
        }

        // ----- CENTERING -----
        if (intentionIcon != null && intentionText != null)
        {
            if (hasIcon && !hasText)
                intentionIcon.rectTransform.anchoredPosition = Vector2.zero;
            else if (!hasIcon && hasText)
                intentionText.rectTransform.anchoredPosition = Vector2.zero;
        }

        animator?.PlayIdle();
    }

    // =====================================================
    //  EXECUTION
    // =====================================================

    public IEnumerator ExecuteIntention(Player player)
    {
        if (IsDead) yield break;

        if (currentCard == null)
        {
            FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up * 2f, "Idle", Color.gray);
            yield break;
        }

        animator?.PlayAttack();
        yield return new WaitForSeconds(0.4f);
        if (IsDead) yield break;

        if (pendingRunner == null)
        {
            var go = new GameObject("EnemyPendingRunner_Fallback");
            go.transform.SetParent(transform);
            pendingRunner = go.AddComponent<CardRunner>();
            pendingRunner.data = currentCard;
            pendingRunner.RollIfNeeded(currentCard);
        }

        pendingRunner.Execute(this, player);

        // Reset to "waiting" state
        if (intentionText != null)
        {
            intentionText.text = string.Empty;
            intentionText.alpha = 0f;
        }

        if (intentionIcon != null)
        {
            Color c = intentionIcon.color;
            c.a = 0f;
            intentionIcon.color = c;
        }

        animator?.PlayIdle();

        if (pendingRunner != null)
        {
            Destroy(pendingRunner.gameObject);
            pendingRunner = null;
        }

        if (!IsDead)
            data.ModifyBehavior(currentCard);
    }

    // =====================================================
    //  DAMAGE & DEATH
    // =====================================================

    public override void TakeDamage(int amount)
    {
        if (IsDead) return;
        base.TakeDamage(amount);

        if (amount > 0 && currentHealth > 0)
            StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        if (IsDead) yield break;
        animator?.PlayHurt();
        yield return new WaitForSeconds(0.5f);
        if (!IsDead) animator?.PlayIdle();
    }

    protected override void Die()
    {
        if (IsDead) return;
        IsDead = true;
        StopAllCoroutines();

        if (intentionText != null) intentionText.text = string.Empty;

        if (pendingRunner != null)
        {
            Destroy(pendingRunner.gameObject);
            pendingRunner = null;
        }

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (animator != null)
            StartCoroutine(DeathRoutine());
        else
            FinalizeDeath();
    }

    private IEnumerator DeathRoutine()
    {
        animator.PlayDeath();
        yield return new WaitForSeconds(0.8f);
        FinalizeDeath();
    }

    private void FinalizeDeath()
    {
        base.Die();
        Destroy(gameObject, 0.1f);
    }

    // =====================================================
    //  INITIALIZATION
    // =====================================================

    public void InitializeFromData(EnemyData baseData)
    {
        // Clone the template so runtime changes don’t modify the asset
        this.data = Instantiate(baseData);

        characterName = data.enemyName;

        // Roll health ONCE
        maxHealth = data.GetRandomizedHealth();
        currentHealth = maxHealth;

        globalPowerScale = data.globalCardMultiplier;

        if (animator == null)
            animator = GetComponent<EnemyAnimator2D>();

        if (animator != null && data.animationSet != null)
            animator.SetAnimSet(data.animationSet);

        if (data.portrait != null && TryGetComponent(out SpriteRenderer sr))
            sr.sprite = data.portrait;

        animator?.PlayIdle();
    }
}
