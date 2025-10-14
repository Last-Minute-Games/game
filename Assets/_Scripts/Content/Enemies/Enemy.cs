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
        if (data == null || data.availableCards.Count == 0)
        {
            Debug.LogWarning($"{name}: No cards available to choose!");
            return;
        }

        // 🎯 Weighted random draw
        currentCard = GetWeightedRandomCard();
        if (currentCard == null)
        {
            Debug.LogWarning($"{name}: No valid card selected!");
            return;
        }

        // 🎲 Simulate potency preview for intention display
        float randomScale = Random.Range(currentCard.minMultiplier, currentCard.maxMultiplier);
        float totalScale = randomScale * globalPowerScale;

        // 💪 Calculate effective value based on card type
        float effectiveValue = 0f;
        if (currentCard.effects != null)
        {
            foreach (var eff in currentCard.effects)
            {
                if (eff is DamageEffect dmg)
                    effectiveValue = dmg.baseDamage * totalScale;
                else if (eff is HealEffect heal)
                    effectiveValue = heal.baseHeal * totalScale;
                else if (eff is BlockEffect blk)
                    effectiveValue = blk.baseBlock * totalScale;
            }
        }

        // 🎨 Choose color depending on effect type (damage/gain)
        string colorHex = "#FFCF40"; // gold default
        foreach (var eff in currentCard.effects)
        {
            if (eff is DamageEffect) colorHex = "#FF4040"; // red
            if (eff is HealEffect) colorHex = "#40FF70";   // green
            if (eff is BlockEffect) colorHex = "#40BFFF";  // blue
        }

        string coloredValue = $"<color={colorHex}>{effectiveValue:F0}</color>";

        // 🧩 Substitute into intention text
        string intentDisplay = currentCard.intentionText;
        if (!string.IsNullOrEmpty(intentDisplay) && intentDisplay.Contains("<X>"))
            intentDisplay = intentDisplay.Replace("<X>", coloredValue);

        // 🪧 Dynamic prefix logic for naming
        float range = currentCard.maxMultiplier - currentCard.minMultiplier;
        string prefix = "";
        if (range > 0.5f)
        {
            float normalized = (randomScale - currentCard.minMultiplier) / range;
            if (normalized < 0.33f) prefix = "Poor ";
            else if (normalized > 0.66f) prefix = "Potent ";
        }

        string cardDisplayName = prefix + currentCard.cardName.Split(' ')[^1];

        // 🧾 Update UI text
        if (intentionText != null)
        {
            string finalText = !string.IsNullOrEmpty(intentDisplay)
                ? intentDisplay
                : cardDisplayName;
            intentionText.text = finalText;
        }

        // 🎞️ Animate idle while preparing
        animator?.PlayIdle();

        Debug.Log($"🎯 {name} preparing: {cardDisplayName} → {intentionText.text} (scale={totalScale:F2})");
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
