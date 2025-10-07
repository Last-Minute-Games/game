using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private BattlefieldLayout battlefieldLayout;  // Reference to layout spawner
    [SerializeField] private HandView handView;                    // Handles card visuals/positions
    [SerializeField] private GameObject[] cardPrefabs;             // AttackCard, DefenseCard, HealingCard
    [SerializeField] private Transform handSpawnPoint;             // Where to spawn cards (optional)

    [Header("Battle Settings")]
    [SerializeField] private int startingHandSize = 5;

    private Player player;
    private List<Enemy> enemies = new();

    private void Start()
    {
        StartCoroutine(InitializeBattle());
    }

    private IEnumerator InitializeBattle()
    {
        // 1️⃣ Wait a frame to ensure BattlefieldLayout has spawned everything
        yield return null;

        // 2️⃣ Find player and enemies that BattlefieldLayout created
        player = FindObjectOfType<Player>();
        enemies.AddRange(FindObjectsOfType<Enemy>());

        if (player == null)
        {
            Debug.LogError("BattleSystem: No Player found in scene!");
            yield break;
        }

        if (enemies.Count == 0)
        {
            Debug.LogError("BattleSystem: No Enemies found in scene!");
            yield break;
        }

        Debug.Log($"BattleSystem initialized with {enemies.Count} enemies and player {player.characterName}.");

        // 3️⃣ Spawn the player’s initial hand
        yield return SpawnStartingHand();

        // 4️⃣ Begin first turn
        StartPlayerTurn();
    }

    private IEnumerator SpawnStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            GameObject randomCard = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
            Vector3 spawnPos = handSpawnPoint != null ? handSpawnPoint.position : transform.position;

            GameObject cardObj = Instantiate(randomCard, spawnPos, Quaternion.identity);
            cardObj.transform.position += Vector3.down * 1f; // spawn lower
            cardObj.transform.DOMove(handSpawnPoint.position, 0.25f).SetEase(Ease.OutBack);
            CardView cardView = cardObj.GetComponent<CardView>();

            if (cardView != null)
            {
                cardView.player = player;
                cardView.targetEnemy = enemies[Random.Range(0, enemies.Count)];
                yield return handView.AddCard(cardView);
            }
        }
    }

    private void StartPlayerTurn()
    {
        player.RefillEnergy();
        Debug.Log("🔹 Player's turn started!");

        // Enemies decide what to do next turn
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                enemy.DecideNextIntention();
        }
    }

    public void EndPlayerTurn()
    {
        Debug.Log("🔸 Player turn ended. Enemy turn begins...");
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || player == null) continue;
            yield return enemy.ExecuteIntention(player);
            yield return new WaitForSeconds(0.5f);
        }

        StartPlayerTurn();
    }
}
