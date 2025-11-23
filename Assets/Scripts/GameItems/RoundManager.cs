using UnityEngine;
using System.Collections;
using Entities.Enemies.Manager;

public class RoundManager : MonoBehaviour
{
    [Header("Managers")] public PlayerManager player;
    public EnemyManager enemyManager;

    [Header("State")] public int roundNumber = 1;
    public bool playerTurn = true;
    public bool battleActive = false;
    
    // Prevents checking end conditions during wave transitions
    private bool _isTransitioningWaves = false;

    [Header("End Screen UI")] 
    public ScreenFadeUI endScreenUI;
    
    [Header("Round Transition UI")]
    [Tooltip("Optional: UI for showing round transitions (e.g., 'ROUND 1'). Can be the same as endScreenUI.")]
    public ScreenFadeUI roundTransitionUI;

    [Header("Timer Settings")] [Tooltip("Time in seconds for each player turn")]
    public float turnTimeLimit = 15f;

    [Tooltip("Current remaining time in the turn")]
    public float currentTurnTime;

    private bool timerActive = false;

    [Header("UI")] [Tooltip("Optional: Deck viewer that shows the current hand.")]
    public GameItems.DeckViewer handViewer;

    [Tooltip("Optional: Deck viewer that shows the draw pile.")]
    public GameItems.DeckViewer drawPileViewer;

    [Tooltip("Optional: Deck viewer that shows the discard pile.")]
    public GameItems.DeckViewer discardPileViewer;

    // -------------------------------------------------------
    // Wave System
    // -------------------------------------------------------
    [System.NonSerialized]
    public System.Action onWaveComplete; // Callback when all enemies in current wave are defeated

    // -------------------------------------------------------
    // Public method to trigger victory (called by BattleManager)
    // -------------------------------------------------------
    public void TriggerVictory()
    {
        if (!battleActive) return;
        battleActive = false;
        _isTransitioningWaves = false; // Clear flag
        HandlePlayerWin();
    }

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
        
        // Show round transition UI
        StartCoroutine(StartRoundWithTransition());
    }
    
    private IEnumerator StartRoundWithTransition()
    {
        // Show "ROUND 1" transition
        if (roundTransitionUI != null)
        {
            yield return StartCoroutine(roundTransitionUI.ShowRoundTransition(roundNumber, isNewWave: false));
        }
        
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        player.StartTurn();

        RefreshDeckViewers();

        // Start turn timer
        StartTurnTimer();
    }

    // -------------------------------------------------------
    // Start a new wave (called by BattleManager)
    // -------------------------------------------------------
    public void StartNewWave()
    {
        if (player == null || enemyManager == null)
        {
            Debug.LogError("RoundManager: Missing managers!");
            return;
        }

        // Clear transition flag - we're ready to check end conditions again
        _isTransitioningWaves = false;

        // Don't reset round number - continue incrementing through waves
        playerTurn = true;
        battleActive = true;

        Debug.Log($"--- New Wave - Round {roundNumber} Start ---");
        
        // Show wave transition UI
        StartCoroutine(StartWaveWithTransition());
    }
    
    private IEnumerator StartWaveWithTransition()
    {
        // Show "WAVE X" transition
        if (roundTransitionUI != null)
        {
            yield return StartCoroutine(roundTransitionUI.ShowRoundTransition(roundNumber, isNewWave: true));
        }
        
        // Clear player's hand for fresh start in new wave
        if (player != null && player.cardManager != null)
        {
            Debug.Log("[RoundManager] Clearing hand for new wave");
            player.cardManager.DiscardCardPile(); // Move current hand to discard pile
            // Don't call DrawStartingHand() here - player.StartTurn() will draw cards!
        }
        
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        player.StartTurn(); // This already draws cards!

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

        // Animate cards discarding BEFORE clearing data
        if (handViewer != null && handViewer.GetRenders().Count > 0)
        {
            handViewer.ClearSmooth(onComplete: () =>
            {
                // After animation completes, clear the data and continue
                player.EndTurn();
                // Refresh other viewers but not hand (already cleared with animation)
                RefreshDeckViewers(skipHand: true);
                playerTurn = false;
                StartCoroutine(EnemyPhase());
            });
        }
        else
        {
            // No cards to animate, proceed normally
            player.EndTurn();
            RefreshDeckViewers();
            playerTurn = false;
            StartCoroutine(EnemyPhase());
        }
    }

    // -------------------------------------------------------
    // Enemy turn phase coroutine
    // -------------------------------------------------------
    private IEnumerator EnemyPhase()
    {
        Debug.Log("Enemy turn begins...");

        yield return new WaitForSeconds(.5f);

        // Enemies execute their actions with delayed sequence
        yield return StartCoroutine(enemyManager.ExecuteEnemyTurnSequence(player.playerData));

        yield return new WaitForSeconds(.5f);
        
        // Reset enemy block AFTER their turn completes
        // This means block gained this turn will protect through the NEXT full round
        if (enemyManager != null)
        {
            enemyManager.ResetAllEnemyBlock();
            Debug.Log("Enemy block reset after enemy turn completes");
        }

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
        
        // Show round transition UI before starting the round
        StartCoroutine(NextRoundWithTransition());
    }
    
    private IEnumerator NextRoundWithTransition()
    {
        // Show "ROUND X" transition
        if (roundTransitionUI != null)
        {
            yield return StartCoroutine(roundTransitionUI.ShowRoundTransition(roundNumber, isNewWave: false));
        }
        
        // DON'T reset enemy block here - let it persist through this round
        // Block will be reset AFTER the enemy turn executes
        
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

    private void RefreshDeckViewers(bool skipHand = false)
    {
        if (handViewer != null && !skipHand)
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
        
        // Don't check end conditions during wave transitions
        if (_isTransitioningWaves) return;

        // Player dead
        if (player.playerData.currentHealth <= 0)
        {
            Debug.Log("💀 Player died — immediate game over.");
            battleActive = false;
            HandlePlayerLose();
            return;
        }

        // All enemies dead - could be wave complete or battle complete
        if (enemyManager.AllEnemiesDefeated())
        {
            Debug.Log("🎉 All enemies defeated in current wave!");
            
            // Set transition flag to prevent repeated calls
            _isTransitioningWaves = true;
            
            // Notify BattleManager that wave is complete
            // BattleManager will decide if there are more waves or if battle is won
            if (onWaveComplete != null)
            {
                onWaveComplete.Invoke();
            }
            else
            {
                // Fallback: if no wave system, just win immediately
                battleActive = false;
                HandlePlayerWin();
            }
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
            clock.AddTime(10f); // TODO: adjust reward amount
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
            clock.RemoveTime(100f); // TODO: adjust penalty amount
        StartCoroutine(ReturnToOverworldDelayed());
    }

    private IEnumerator ReturnToOverworldDelayed()
    {
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Overworld");
    }
}