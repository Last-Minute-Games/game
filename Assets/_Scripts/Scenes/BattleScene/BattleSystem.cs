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
        yield return null; // Wait for BattlefieldLayout to finish setting up

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
    //  SPAWN & REFILL HAND
    // ==============================

    private IEnumerator SpawnStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            GameObject randomCard = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
            Vector3 spawnPos = handSpawnPoint != null ? handSpawnPoint.position : transform.position;

            GameObject cardObj = Instantiate(randomCard, spawnPos, Quaternion.identity);
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

    private IEnumerator ClearHand()
    {
        List<GameObject> toDestroy = new();
        foreach (Transform child in handView.transform)
            toDestroy.Add(child.gameObject);

        foreach (GameObject card in toDestroy)
        {
            card.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(card));
        }

        yield return new WaitForSeconds(0.25f);
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

        if (endTurnButton != null)
        {
            endTurnButton.gameObject.SetActive(true);  // make sure it’s active
            endTurnButton.EnableButton();              // re-enable interactivity
        }

        // Enemies plan their next moves
        foreach (Enemy enemy in enemies)
            if (enemy != null)
                enemy.DecideNextIntention();
    }

    public void EndPlayerTurn()
    {
        if (!playerTurn || isProcessingTurn) return;

        Debug.Log("🔸 Player turn ended → Enemy turn begins...");
        playerTurn = false;

        endTurnButton?.DisableButton();

        // Discard player’s hand right away (for visual feedback)
        StartCoroutine(ClearHand());

        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        isProcessingTurn = true;

        // 1️⃣ Enemies act
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || player == null) continue;
            yield return enemy.ExecuteIntention(player);
            yield return new WaitForSeconds(0.5f);
        }

        // 2️⃣ Short pause for pacing
        Debug.Log("🕒 Resetting for next round...");
        yield return new WaitForSeconds(turnResetDelay);

        // 3️⃣ Discard old hand and draw new 5
        yield return RefreshPlayerHand();

        // 4️⃣ Begin player’s next turn
        StartPlayerTurn();

        isProcessingTurn = false;
    }
}
