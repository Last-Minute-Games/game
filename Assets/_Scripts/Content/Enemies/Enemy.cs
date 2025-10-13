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
        if (data == null || data.availableCards.Count == 0)
        {
            Debug.LogWarning($"{name}: No cards available to choose!");
            return;
        }

        // 🎯 Weighted random draw
        currentCard = GetWeightedRandomCard();

        // 🪧 Update intention UI
        if (intentionText != null)
        {
            string text = !string.IsNullOrEmpty(currentCard.intentionText)
                ? currentCard.intentionText
                : currentCard.cardName;
            intentionText.text = text;
        }

        // 🎞️ Animate idle while preparing
        animator?.PlayIdle();
    }

    // ===========================
    //   EXECUTE MOVE
    // ===========================
    public IEnumerator ExecuteIntention(Player player)
    {
        if (currentCard == null)
        {
            FloatingTextManager.Instance?.SpawnText(
                transform.position + Vector3.up * 2f,
                "Idle",
                Color.gray
            );
            yield break;
        }

        animator?.PlayAttack();
        yield return new WaitForSeconds(0.4f);

        // 💥 Execute card logic
        var runnerObj = new GameObject("TempCardRunner");
        var runner = runnerObj.AddComponent<CardRunner>();
        runner.data = currentCard;
        runner.transform.SetParent(transform);

        runner.Execute(this, player);
        Destroy(runnerObj);

        // 🧾 Reset UI & return to float animation
        if (intentionText != null)
            intentionText.text = "Waiting...";

        animator?.PlayFloat();

        // Behavior evolution for next turn
        data.ModifyBehavior(currentCard);
    }
}
