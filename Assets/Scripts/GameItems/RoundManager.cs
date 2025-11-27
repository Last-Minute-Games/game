using UnityEngine;
using System.Collections;
using Entities.Enemies.Manager;
using GameItems.Cards.Helpers;

public class RoundManager : MonoBehaviour
{
    [Header("Managers")] public PlayerManager player;
    public EnemyManager enemyManager;

    [Header("State")] 
    public int roundNumber = 1;
    public int waveNumber = 1;
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

        // Check if player has no cards left
        if (player != null && player.cardManager != null && player.cardManager.hand.Count == 0)
        {
            Debug.Log("🃏 Player has no cards left! Ending turn early.");
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
        waveNumber = 1;
        playerTurn = true;
        battleActive = true;

        Debug.Log($"--- Wave {waveNumber} - Round {roundNumber} Start ---");
        
        // Show wave 1 transition
        StartCoroutine(StartFirstWaveWithTransition());
    }
    
    private IEnumerator StartFirstWaveWithTransition()
    {
        // Show "WAVE 1" transition immediately
        if (roundTransitionUI != null)
        {
            // FIRST WAVE → starts fully dark, only fades out
            StartCoroutine(roundTransitionUI.ShowRoundTransition(waveNumber, isFirstWave: true));
        }
        
        // While the transition is showing, prepare the wave
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        
        // Draw starting hand - this happens while transition is still showing/fading out
        player.StartTurn();

        RefreshDeckViewers();

        // Start turn timer
        StartTurnTimer();
        
        // Small delay to ensure everything is visible before proceeding
        yield return new WaitForSeconds(0.1f);
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

        // Reduce strength at the start of a new wave
        if (player != null && player.playerData != null)
        {
            player.playerData.LoseStrength(1);
        }

        // Clear transition flag - we're ready to check end conditions again
        _isTransitioningWaves = false;

        // Increment wave number and reset round counter for the new wave
        waveNumber++;
        roundNumber = 1;
        playerTurn = true;
        battleActive = true;

        Debug.Log($"--- Wave {waveNumber} - Round {roundNumber} Start ---");
        
        // Show wave transition UI
        StartCoroutine(StartWaveWithTransition());
    }
    
    private IEnumerator StartWaveWithTransition()
    {
        // Show "WAVE X" transition immediately
        if (roundTransitionUI != null)
        {
            // LATER WAVES → fade in & out
            StartCoroutine(roundTransitionUI.ShowRoundTransition(waveNumber, isFirstWave: false));
        }
        
        // While the transition is fading in/holding, prepare the next wave
        // Clear player's hand for fresh start in new wave
        if (player != null && player.cardManager != null)
        {
            Debug.Log("[RoundManager] Clearing hand for new wave");
            player.cardManager.DiscardCardPile(); // Move current hand to discard pile
        }
        
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        
        // Draw new hand - this happens while transition is still showing/fading out
        player.StartTurn(); // This already draws cards!

        RefreshDeckViewers();

        // Start turn timer
        StartTurnTimer();
        
        // Small delay to ensure everything is visible before proceeding
        yield return new WaitForSeconds(0.1f);
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

        // Hide all arrows from any cards that might be mid-drag
        HideAllCardArrows();

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
            handViewer.SetSource(GameItems.DeckViewer.Source.Hand, rebuild: false);
            handViewer.RebuildSmart(); // Use smart rebuild to smoothly add new cards
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

    /// <summary>
    /// Hides all bezier arrows from all cards in hand.
    /// Called when turn ends or cards are being cleared.
    /// </summary>
    private void HideAllCardArrows()
    {
        if (handViewer == null) return;

        var cardRenders = handViewer.GetRenders();
        
        foreach (var cardRender in cardRenders)
        {
            if (cardRender == null) continue;

            // Try to get the BezierCardArrowHelper and hide it
            var arrowHelper = cardRender.GetComponent<GameItems.Cards.Helpers.BezierCardArrowHelper>();
            if (arrowHelper != null)
            {
                arrowHelper.StopDrawing();
            }
            
            // Clear enemy hover sprites from each card's animation helper
            var animHelper = cardRender.GetComponent<GameItems.Cards.Helpers.CardAnimationHelper>();
            if (animHelper != null)
            {
                animHelper.ClearEnemyHoverSprites();
            }
        }

        Debug.Log($"[RoundManager] Hid arrows for {cardRenders.Count} cards");
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

        AdvanceDayFlag();

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

        AdvanceDayFlag();

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

    // handle advancing day flags
    private void AdvanceDayFlag()
    {
        if (GameFlags.HasFlag("day.one"))
        {
            GameFlags.SetFlag("day.two");
            return;
        }
        if (GameFlags.HasFlag("day.two"))
        {
            GameFlags.SetFlag("day.three");
            return;
        }
        if (GameFlags.HasFlag("day.three"))
        {
            GameFlags.SetFlag("day.four");
            return;
        }
        if (GameFlags.HasFlag("day.four"))
        {
            GameFlags.SetFlag("day.five");
            return;
        }

        // Already maxed → do nothing
    }
}
