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

    [Header("End Screen UI")]
    public ScreenFadeUI endScreenUI;

    [Header("Timer Settings")]
    [Tooltip("Time in seconds for each player turn")]
    public float turnTimeLimit = 15f;
    
    [Tooltip("Current remaining time in the turn")]
    public float currentTurnTime;
    
    private bool timerActive = false;

    [Header("UI")]
    [Tooltip("Optional: Deck viewer that shows the current hand.")]
    public GameItems.DeckViewer handViewer;
    [Tooltip("Optional: Deck viewer that shows the draw pile.")]
    public GameItems.DeckViewer drawPileViewer;
    [Tooltip("Optional: Deck viewer that shows the discard pile.")]
    public GameItems.DeckViewer discardPileViewer;

    // -------------------------------------------------------
    // Initialization
    // -------------------------------------------------------
    public void Initialize(PlayerManager playerManager, EnemyManager enemyMgr)
    {
        player = playerManager;
        enemyManager = enemyMgr;
    }

    // -------------------------------------------------------
    // Update - Handle turn timer
    // -------------------------------------------------------
    private void Update()
    {
        CheckImmediateEndConditions();
        if (!battleActive || !playerTurn || !timerActive) return;

        // Countdown timer
        currentTurnTime -= Time.deltaTime;

        // Check if timer expired
        if (currentTurnTime <= 0f)
        {
            Debug.Log("⏰ Turn timer expired! Ending player turn.");
            currentTurnTime = 0f;
            timerActive = false;
            EndPlayerTurn();
            return;
        }

        // Check if player has run out of energy (optional auto-end)
        if (player != null && player.playerData != null && player.playerData.currentEnergy <= 0)
        {
            Debug.Log("⚡ Player energy depleted! Ending turn early.");
            timerActive = false;
            EndPlayerTurn();
            return;
        }
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

        RefreshDeckViewers();
        
        // Start turn timer
        StartTurnTimer();
    }

    // -------------------------------------------------------
    // Called when player ends their turn
    // -------------------------------------------------------
    public void EndPlayerTurn()
    {
        if (!battleActive || !playerTurn) return;
        
        // Stop timer
        timerActive = false;
        
        Debug.Log("Player turn ended.");

        player.EndTurn();
        RefreshDeckViewers();
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

        // 
        enemyManager.ResetAllEnemyBlock();

        // Enemies execute their actions
        enemyManager.ExecuteEnemyTurn(ref player.playerData);

        yield return new WaitForSeconds(0.5f);

        if (player.playerData.isAlive && !enemyManager.AllEnemiesDefeated())
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

        RefreshDeckViewers();
        
        // Restart turn timer
        StartTurnTimer();
    }

    // -------------------------------------------------------
    // Start/Restart the turn timer
    // -------------------------------------------------------
    private void StartTurnTimer()
    {
        currentTurnTime = turnTimeLimit;
        timerActive = true;
        Debug.Log($"⏱️ Turn timer started: {turnTimeLimit} seconds");
    }

    // -------------------------------------------------------
    // End battle (victory or defeat)
    // -------------------------------------------------------
    private void EndBattle()
    {
        // Prevent double calls
        if (!battleActive) return;  
        battleActive = false;

        // Redirect to immediate end logic
        CheckImmediateEndConditions();
    }

    private void RefreshDeckViewers()
    {
        if (handViewer != null)
        {
            handViewer.SetPlayer(player);
            handViewer.SetSource(GameItems.DeckViewer.Source.Hand, rebuild: true);
        }

        if (drawPileViewer != null)
        {
            drawPileViewer.SetPlayer(player);
            drawPileViewer.SetSource(GameItems.DeckViewer.Source.DrawPile, rebuild: true);
        }

        if (discardPileViewer != null)
        {
            discardPileViewer.SetPlayer(player);
            discardPileViewer.SetSource(GameItems.DeckViewer.Source.DiscardPile, rebuild: true);
        }
    }

    // -------------------------------------------------------
    // Checks if battle should end RIGHT NOW (not end of round)
    // Call this whenever player/enemy health changes
    // -------------------------------------------------------
    public void CheckImmediateEndConditions()
    {
        if (!battleActive) return;

        // Player dead
        if (player.playerData.currentHealth <= 0)
        {
            Debug.Log("💀 Player died — immediate game over.");
            battleActive = false;
            HandlePlayerLose();
            return;
        }

        // All enemies dead
        if (enemyManager.AllEnemiesDefeated())
        {
            Debug.Log("🎉 All enemies defeated — immediate victory!");
            battleActive = false;
            HandlePlayerWin();
            return;
        }
    }

    // -------------------------------------------------------
    // PLAYER WINS
    // -------------------------------------------------------
    private void HandlePlayerWin()
    {
        Debug.Log("🏆 Player Victory!");

        // Fade screen + show text
        if (endScreenUI != null)
            endScreenUI.ShowMessage("YOU WIN", new Color(1f, 0.84f, 0.0f)); // gold

        // TODO: update timer shit
        var clock = FindObjectOfType<ClockTimer>();
        if (clock != null)
            clock.AddTime(10f);   // TODO: adjust reward amount
            StartCoroutine(ReturnToOverworldDelayed());
        }

    // -------------------------------------------------------
    // PLAYER LOSSES
    // -------------------------------------------------------
    private void HandlePlayerLose()
    {
        Debug.Log("❌ Player Defeat!");

        if (endScreenUI != null)
            endScreenUI.ShowMessage("YOU LOSE", Color.red);

        // TODO: wrong todo just ummm timer change flag shit
        var clock = FindObjectOfType<ClockTimer>();
        if (clock != null)
            clock.RemoveTime(100f);   // TODO: adjust penalty amount
            StartCoroutine(ReturnToOverworldDelayed());
        }

    private IEnumerator ReturnToOverworldDelayed()
    {
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Overworld");
    }
}
