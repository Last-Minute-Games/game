using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private BattlefieldLayout battlefieldLayout;
    [SerializeField] private HandView handView;
    [SerializeField] private CardFactory cardFactory;
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

    public void RegisterPlayer(Player p)
    {
        player = p;
    }

    private IEnumerator InitializeBattle()
    {
        yield return null;

        // player = FindObjectOfType<Player>(); ❌ remove this
        enemies.AddRange(FindObjectsOfType<Enemy>());

        if (player == null)
        {
            Debug.LogError("❌ BattleSystem: Player not registered!");
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
            // Ask the factory to handle random card creation (weighted logic handled inside)
            GameObject cardObj = cardFactory.CreateRandomCard(handSpawnPoint.position, 0.6f, 0.25f, 0.15f);

            if (cardObj == null)
            {
                Debug.LogError("❌ BattleSystem: Failed to create card via factory!");
                continue;
            }

            // Animate into place
            cardObj.transform.position += Vector3.down * 1f;
            cardObj.transform.DOMove(handSpawnPoint.position, 0.25f).SetEase(Ease.OutBack);

            // Set up CardView
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
            enemy?.PrepareNextCard();
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

        // 🧱 Step 1: (DON’T clear player block here)
        // The player’s block should persist while enemies attack.

        // 🧱 Step 2: Start enemy turn → clear THEIR previous block
        foreach (Enemy enemy in enemies)
            enemy?.EndTurn();

        // 🧨 Step 3: Enemies act
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || player == null) continue;
            yield return enemy.ExecuteIntention(player);
            yield return new WaitForSeconds(0.5f);
        }

        // 🕒 Step 4: After enemies act, player’s block now expires
        player?.EndTurn();

        Debug.Log("🕒 Resetting for next round...");
        yield return new WaitForSeconds(turnResetDelay);

        yield return RefreshPlayerHand();
        StartPlayerTurn();

        isProcessingTurn = false;
    }

}
