using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : CharacterBase
{
    [Header("UI References")]
    public GameObject healthBarPrefab;
    public TMP_Text intentionText;

    [Header("Runtime")]
    public EnemyData data; // assigned by EnemyRunner
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

    public void PrepareNextCard()
    {
        if (data == null || data.availableCards.Count == 0)
        {
            Debug.LogWarning($"{name}: No cards available to choose!");
            return;
        }

        // Weighted or random choice — for now uniform
        currentCard = data.availableCards[Random.Range(0, data.availableCards.Count)];

        // update UI intention
        if (intentionText != null)
        {
            string text = !string.IsNullOrEmpty(currentCard.intentionText)
                ? currentCard.intentionText
                : currentCard.cardName;
            intentionText.text = text;
        }

        // optionally animate float/idle while waiting
        animator?.PlayIdle();
    }

    public IEnumerator ExecuteIntention(Player player)
    {
        if (currentCard == null)
        {
            FloatingTextManager.Instance?.SpawnText(transform.position + Vector3.up * 2f, "Idle", Color.gray);
            yield break;
        }

        animator?.PlayAttack();
        yield return new WaitForSeconds(0.4f);

        // actually use the card logic
        var runner = new GameObject("TempCardRunner").AddComponent<CardRunner>();
        runner.data = currentCard;

        runner.transform.SetParent(this.transform);  // allows CardEffect to find this enemy’s runner

        runner.Execute(this, player);
        Destroy(runner.gameObject);

        // reset UI
        if (intentionText != null)
            intentionText.text = "Waiting...";

        animator?.PlayFloat();
    }
}
