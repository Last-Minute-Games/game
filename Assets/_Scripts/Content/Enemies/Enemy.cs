using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : CharacterBase
{
    [Header("UI References")]
    public GameObject healthBarPrefab;
    public TMP_Text intentionText;

    [Header("Runtime")]
    public EnemyData data; // assigned by BattlefieldLayout / EnemyRunner
    private EnemyAnimator2D animator;
    private CardLibrary library;
    private CardData currentCard;

    [Header("Scaling")]
    [Tooltip("Affects how strong this character’s card effects are.")]
    public float globalPowerScale = 1.0f;

    private CardRunner pendingRunner; // keep between prepare & execute

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<EnemyAnimator2D>();
        library = FindFirstObjectByType<CardLibrary>();

        SetupUI();
    }

    // ===========================
    //   UI SETUP
    // ===========================
    private void SetupUI()
    {
        if (healthBarPrefab != null)
        {
            var barObj = Instantiate(healthBarPrefab);
            healthBarInstance = barObj.GetComponent<EnemyHealthBar>();
            healthBarInstance.Initialize(this);

            // Hide defense panel for enemies
            Transform defensePanel = barObj.transform.Find("DefensePanel");
            if (defensePanel != null)
                defensePanel.gameObject.SetActive(false);
        }

        if (intentionText != null)
            intentionText.text = "Waiting...";
    }

    // ===========================
    //   CARD SELECTION (Weighted)
    // ===========================
    private CardData GetWeightedRandomCard()
    {
        if (data == null || data.availableCards.Count == 0)
            return null;

        float totalWeight = 0f;

        // Calculate total weight (with low HP scaling)
        foreach (var entry in data.availableCards)
        {
            if (entry.card == null) continue;

            float weight = Mathf.Max(0f, entry.baseWeight);

            // 🧠 Behavior evolution — boost when HP ≤ ⅓ max
            if (entry.prioritizeWhenLowHP && currentHealth <= maxHealth / 3f)
                weight *= data.lowHPWeightMultiplier;

            totalWeight += weight;
        }

        if (totalWeight <= 0f)
            return data.availableCards[0].card; // fallback

        // Roll between 0 and totalWeight
        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        // Select the card
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

        return data.availableCards[0].card; // safety fallback
    }

    // ===========================
    //   PREPARE NEXT MOVE
    // ===========================
    public void PrepareNextCard()
    {
        if (data == null || data.availableCards.Count == 0) { Debug.LogWarning($"{name}: No cards available!"); return; }

        currentCard = GetWeightedRandomCard();
        if (currentCard == null) return;

        // Create/keep a runner that locks the roll now
        if (pendingRunner == null)
        {
            var go = new GameObject("EnemyPendingRunner");
            go.transform.SetParent(transform);
            pendingRunner = go.AddComponent<CardRunner>();
        }
        pendingRunner.data = currentCard;
        pendingRunner.RollIfNeeded(currentCard); // lock the randomScale now

        // Compute preview X with SAME roll + this enemy’s scale
        int x = pendingRunner.GetPreviewX(this);

        // Build colored <X> & prefix (purely display; no asset mutation)
        string colorHex = "#FFCF40";
        foreach (var eff in currentCard.effects)
        {
            if (eff is DamageEffect) colorHex = "#FF4040";
            else if (eff is HealEffect) colorHex = "#40FF70";
            else if (eff is BlockEffect) colorHex = "#40BFFF";
        }
        string coloredX = $"<color={colorHex}>{x}</color>";

        string intent = string.IsNullOrEmpty(currentCard.intentionText) ? currentCard.cardName : currentCard.intentionText;
        if (!string.IsNullOrEmpty(intent) && intent.Contains("<X>"))
            intent = intent.Replace("<X>", coloredX);

        if (intentionText != null) intentionText.text = intent;

        animator?.PlayIdle();
    }

    // ===========================
    //   EXECUTE MOVE
    // ===========================
    public IEnumerator ExecuteIntention(Player player)
    {
        if (currentCard == null)
        {
            FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up * 2f, "Idle", Color.gray);
            yield break;
        }

        animator?.PlayAttack();
        yield return new WaitForSeconds(0.4f);

        // Use the SAME runner so the roll matches the intention
        if (pendingRunner == null)
        {
            var go = new GameObject("EnemyPendingRunner_Fallback");
            go.transform.SetParent(transform);
            pendingRunner = go.AddComponent<CardRunner>();
            pendingRunner.data = currentCard;
            pendingRunner.RollIfNeeded(currentCard); // fallback lock
        }

        pendingRunner.Execute(this, player);

        // cleanup
        if (intentionText != null) intentionText.text = "Waiting...";
        animator?.PlayFloat();

        Destroy(pendingRunner.gameObject);
        pendingRunner = null;

        data.ModifyBehavior(currentCard);
    }
}
