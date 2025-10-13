using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private BattlefieldLayout battlefieldLayout;
    [SerializeField] private HandView handView;
    [SerializeField] private GameObject[] cardPrefabs;
    [SerializeField] private Transform handSpawnPoint;
    [SerializeField] private EndTurnButton endTurnButton;

    [Header("Battle Settings")]
    [SerializeField] private int startingHandSize = 5;
    [SerializeField] private float turnResetDelay = 1.5f;

    [Header("Card Draw Chances (must total 1.0)")]
    [Range(0f, 1f)] public float attackChance = 0.6f;
    [Range(0f, 1f)] public float defenseChance = 0.25f;
    [Range(0f, 1f)] public float healthChance = 0.15f;

    private Player player;
    private readonly List<Enemy> enemies = new();

    private bool playerTurn = true;
    private bool isProcessingTurn = false;

    private void Start()
    {
        StartCoroutine(InitializeBattle());
    }

    private IEnumerator InitializeBattle()
    {
        yield return null;

        player = FindObjectOfType<Player>();
        enemies.AddRange(FindObjectsOfType<Enemy>());

        if (player == null)
        {
            Debug.LogError("❌ BattleSystem: No Player found in scene!");
            yield break;
        }

        if (enemies.Count == 0)
        {
            Debug.LogError("❌ BattleSystem: No Enemies found in scene!");
            yield break;
        }

        Debug.Log($"✅ BattleSystem initialized with {enemies.Count} enemies ({player.characterName})");

        yield return SpawnStartingHand();
        StartPlayerTurn();
    }

    // ==============================
    //  HAND MANAGEMENT
    // ==============================

    private IEnumerator SpawnStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            GameObject cardToSpawn = GetWeightedRandomCard();
            Vector3 spawnPos = handSpawnPoint != null ? handSpawnPoint.position : transform.position;

            GameObject cardObj = Instantiate(cardToSpawn, spawnPos, Quaternion.identity);
            cardObj.transform.position += Vector3.down * 1f;
            cardObj.transform.DOMove(handSpawnPoint.position, 0.25f).SetEase(Ease.OutBack);

            CardView cardView = cardObj.GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.player = player;
                if (enemies.Count > 0)
                    cardView.targetEnemy = enemies[Random.Range(0, enemies.Count)];

                yield return handView.AddCard(cardView);
            }
        }
    }

    private GameObject GetWeightedRandomCard()
    {
        // Normalize if the user didn’t exactly make it total 1
        float total = attackChance + defenseChance + healthChance;
        if (Mathf.Abs(total - 1f) > 0.001f)
        {
            Debug.LogWarning($"⚠️ Card chances don’t sum to 1 (currently {total:F2}). Normalizing automatically.");
            attackChance /= total;
            defenseChance /= total;
            healthChance /= total;
        }

        float roll = Random.value;

        CardType targetType;
        if (roll < attackChance)
            targetType = CardType.Attack;
        else if (roll < attackChance + defenseChance)
            targetType = CardType.Defense;
        else
            targetType = CardType.Healing;

        // Select random prefab of that type
        List<GameObject> matchingCards = new();
        foreach (var prefab in cardPrefabs)
        {
            var cb = prefab.GetComponent<CardBase>();
            if (cb != null && cb.cardType == targetType)
                matchingCards.Add(prefab);
        }

        if (matchingCards.Count == 0)
        {
            Debug.LogWarning($"⚠️ No prefabs found for {targetType}! Defaulting to random card.");
            return cardPrefabs[Random.Range(0, cardPrefabs.Length)];
        }

        return matchingCards[Random.Range(0, matchingCards.Count)];
    }

    private IEnumerator ClearHand()
    {
        if (handView != null)
            yield return handView.ClearAllCards();
    }

    private IEnumerator RefreshPlayerHand()
    {
        yield return ClearHand();
        yield return SpawnStartingHand();
    }

    // ==============================
    //  TURN SYSTEM
    // ==============================

    private void StartPlayerTurn()
    {
        playerTurn = true;
        player.RefillEnergy();
        Debug.Log("🔹 Player’s turn started!");

        foreach (Enemy enemy in enemies)
            enemy?.DecideNextIntention();
    }

    public void EndPlayerTurn()
    {
        if (!playerTurn || isProcessingTurn) return;

        Debug.Log("🔸 Player turn ended → Enemy turn begins...");
        playerTurn = false;

        StartCoroutine(HandleTurnFlow());
    }

    private IEnumerator HandleTurnFlow()
    {
        isProcessingTurn = true;

        yield return ClearHand();

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || player == null) continue;
            yield return enemy.ExecuteIntention(player);
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("🕒 Resetting for next round...");
        yield return new WaitForSeconds(turnResetDelay);

        yield return RefreshPlayerHand();
        StartPlayerTurn();

        isProcessingTurn = false;
    }
}
