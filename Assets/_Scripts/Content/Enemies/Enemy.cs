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
    public EnemyData data; // assigned by BattlefieldLayout / EnemyRunner
    private EnemyAnimator2D animator;
    private CardLibrary library;
    private CardData currentCard;

    [Header("Scaling")]
    [Tooltip("Affects how strong this character’s card effects are.")]
    public float globalPowerScale = 1.0f;

    private CardRunner pendingRunner; // keep between prepare & execute

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

        // Initialize intention UI in "waiting" state
        if (intentionText != null)
        {
            intentionText.text = string.Empty;
            intentionText.alpha = 0f;
        }

        if (intentionIcon != null)
        {
            Color c = intentionIcon.color;
            c.a = 0f; // make icon invisible
            intentionIcon.color = c;
        }
    }

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

        // Determine color
        string colorHex = "#FFCF40";
        foreach (var eff in currentCard.effects)
        {
            if (eff is DamageEffect) colorHex = "#FF4040";
            else if (eff is HealEffect) colorHex = "#40FF70";
            else if (eff is BlockEffect) colorHex = "#40BFFF";
        }
        string coloredX = $"<color={colorHex}>{x}</color>";

        // Format intention text
        string intent = currentCard.intentionText;
        if (!string.IsNullOrEmpty(intent) && intent.Contains("<X>"))
            intent = intent.Replace("<X>", coloredX);

        // =============================
        // Set Intention Icon + Text
        // =============================
        bool hasIcon = currentCard.intentionIcon != null;
        bool hasText = !string.IsNullOrEmpty(intent);

        // Handle icon
        if (intentionIcon != null)
        {
            if (hasIcon)
            {
                intentionIcon.sprite = currentCard.intentionIcon;
                Color c = intentionIcon.color;
                c.a = 1f; // fully visible
                intentionIcon.color = c;
                intentionIcon.enabled = true;
            }
            else
            {
                intentionIcon.enabled = false;
            }
        }

        // Handle text
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

        // Handle centering
        if (intentionIcon != null && intentionText != null)
        {
            if (hasIcon && !hasText)
            {
                // Center icon alone
                intentionIcon.rectTransform.anchoredPosition = Vector2.zero;
            }
            else if (!hasIcon && hasText)
            {
                // Center text alone
                intentionText.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        animator?.PlayIdle();
    }

    public IEnumerator ExecuteIntention(Player player)
    {
        if (IsDead)
            yield break;

        if (currentCard == null)
        {
            FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up * 2f, "Idle", Color.gray);
            yield break;
        }

        animator?.PlayAttack();
        yield return new WaitForSeconds(0.4f);

        if (IsDead)
            yield break;

        if (pendingRunner == null)
        {
            var go = new GameObject("EnemyPendingRunner_Fallback");
            go.transform.SetParent(transform);
            pendingRunner = go.AddComponent<CardRunner>();
            pendingRunner.data = currentCard;
            pendingRunner.RollIfNeeded(currentCard);
        }

        pendingRunner.Execute(this, player);

        // Reset to waiting state
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

    public void InitializeFromData(EnemyData baseData)
    {
        // Create a runtime clone so we don’t mutate the original asset
        this.data = Instantiate(baseData);

        characterName = data.enemyName;
        maxHealth = data.maxHealth;
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
