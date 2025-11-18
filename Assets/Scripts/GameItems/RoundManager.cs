using UnityEngine;
using System.Collections;
using Entities.Enemies.Manager;

public class RoundManager : MonoBehaviour
{
    [Header("Managers")]
    public PlayerManager player;
    public EnemyManager enemyManager;

    [Header("State")]
    public int roundNumber = 1;
    public bool playerTurn = true;
    public bool battleActive = false;

    // -------------------------------------------------------
    // Initialization
    // -------------------------------------------------------
    public void Initialize(PlayerManager playerManager, EnemyManager enemyMgr)
    {
        player = playerManager;
        enemyManager = enemyMgr;
    }

    // -------------------------------------------------------
    // Start first round
    // -------------------------------------------------------
    public void StartRound()
    {
        if (player == null || enemyManager == null)
        {
            Debug.LogError("RoundManager: Missing managers!");
            return;
        }

        roundNumber = 1;
        playerTurn = true;
        battleActive = true;

        Debug.Log($"--- Round {roundNumber} Start ---");
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        player.StartTurn();
    }

    // -------------------------------------------------------
    // Called when player ends their turn
    // -------------------------------------------------------
    public void EndPlayerTurn()
    {
        if (!battleActive) return;
        Debug.Log("Player turn ended.");

        player.EndTurn();
        playerTurn = false;

        StartCoroutine(EnemyPhase());
    }

    // -------------------------------------------------------
    // Enemy turn phase coroutine
    // -------------------------------------------------------
    private IEnumerator EnemyPhase()
    {
        Debug.Log("Enemy turn begins...");

        yield return new WaitForSeconds(0.5f);

        // Enemies execute their actions
        enemyManager.ExecuteEnemyTurn(ref player.playerData.entity);

        yield return new WaitForSeconds(0.5f);

        if (player.playerData.entity.isAlive && !enemyManager.AllEnemiesDefeated())
        {
            NextRound();
        }
        else
        {
            EndBattle();
        }
    }

    // -------------------------------------------------------
    // Begin the next round
    // -------------------------------------------------------
    private void NextRound()
    {
        roundNumber++;
        playerTurn = true;

        Debug.Log($"--- Round {roundNumber} Start ---");
        // Roll intents for the upcoming enemy turn so the player can plan accordingly
        enemyManager.RollNextIntents();
        player.StartTurn();
    }

    // -------------------------------------------------------
    // End battle (victory or defeat)
    // -------------------------------------------------------
    private void EndBattle()
    {
        battleActive = false;

        if (!player.playerData.entity.isAlive)
        {
            Debug.Log("💀 Player defeated!");
        }
        else if (enemyManager.AllEnemiesDefeated())
        {
            Debug.Log("🎉 Victory!");
        }
        else
        {
            Debug.Log("⚠️ Battle ended unexpectedly.");
        }
    }
}
