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
    
    // Prevents EndPlayerTurn from being called while a card is being played
    // This avoids conflicts between RebuildSmart() and ClearSmooth()
    private bool _isPlayingCard = false;
    
    // Prevents EndPlayerTurn from being called multiple times
    private bool _isEndingTurn = false;

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

    public void ShowWaveStartUI(int waveNumber)
    {
        if (roundTransitionUI != null)
        {
            StartCoroutine(roundTransitionUI.ShowRoundTransition(waveNumber));
        }
    }

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
            
            // Don't end turn immediately if a card is still being played
            // The card play will trigger EndPlayerTurn when complete
            if (_isPlayingCard)
            {
                Debug.Log("⏰ Timer expired but a card is being played - deferring turn end");
                currentTurnTime = 0f;
                timerActive = false;
                return;
            }
            
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
        // Safety: Unlock card interactions in case they got stuck
        CardFXHelper.CardInteraction.Locked = false;
        
        // Reset draw sound flag so it plays once when cards are drawn this wave
        CardFXHelper.ResetDrawSoundFlagStatic();
        
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
        
        // Start the wave setup (without showing transition UI - already shown)
        StartCoroutine(StartWaveSetup());
    }
    
    private IEnumerator StartWaveSetup()
    {
        // Safety: Unlock card interactions in case they got stuck
        CardFXHelper.CardInteraction.Locked = false;
        
        // Reset draw sound flag so it plays once when cards are drawn this wave
        CardFXHelper.ResetDrawSoundFlagStatic();
        
        // Clear player's hand for fresh start in new wave
        if (player != null && player.cardManager != null)
        {
            Debug.Log("[RoundManager] Clearing hand for new wave");
            player.cardManager.DiscardCardPile(); // Move current hand to discard pile
        }
        
        // Enemies roll their next intents so the player can see them before acting
        enemyManager.RollNextIntents();
        
        // Draw new hand
        player.StartTurn();

        RefreshDeckViewers();

        // Start turn timer
        StartTurnTimer();
        
        // Small delay to ensure everything is visible before proceeding
        yield return new WaitForSeconds(0.1f);
    }

    // -------------------------------------------------------
    // Card playing state management
    // -------------------------------------------------------
    public void SetCardPlayingState(bool isPlaying)
    {
        _isPlayingCard = isPlaying;
        if (isPlaying)
        {
            Debug.Log("[RoundManager] Card play started - timer will defer");
        }
        else
        {
            Debug.Log("[RoundManager] Card play finished");
            // Don't call EndPlayerTurn here - let the timer check on next frame
            // This prevents double execution
        }
    }

    // -------------------------------------------------------
    // Called when player ends their turn
    // -------------------------------------------------------
    public void EndPlayerTurn()
    {
        if (!battleActive || !playerTurn) return;
        
        // Prevent double execution of EndPlayerTurn
        if (_isEndingTurn)
        {
            Debug.LogWarning("[RoundManager] EndPlayerTurn already in progress - skipping duplicate call");
            return;
        }
        
        _isEndingTurn = true;

        // Stop timer
        timerActive = false;

        Debug.Log("Player turn ended.");

        // Hide all arrows from any cards that might be mid-drag
        HideAllCardArrows();
        
        // Stop any ongoing hand layout animations before discarding
        // This prevents cards from being mid-rebuild when we try to clear them
        if (handViewer != null)
        {
            Debug.Log("[RoundManager] Stopping any ongoing hand layout animations");
            handViewer.StopLayoutAnimation();
        }

        // Animate cards discarding BEFORE clearing data
        if (handViewer != null && handViewer.GetRenders().Count > 0)
        {
            handViewer.ClearSmooth(onComplete: () =>
            {
                // After animation completes, clear the data and continue
                _isEndingTurn = false;
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
            _isEndingTurn = false;
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
        
        // Safety: Unlock card interactions in case they got stuck from previous turn
        CardFXHelper.CardInteraction.Locked = false;

        // Reset draw sound flag so it plays once when cards are drawn this round
        CardFXHelper.ResetDrawSoundFlagStatic();

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
        Debug.Log("[RoundManager] ============================================");
        Debug.Log("[RoundManager] 🏆 PLAYER VICTORY!");
        Debug.Log("[RoundManager] ============================================");

        // Advance day flag (this will trigger auto-save)
        Debug.Log("[RoundManager] Calling AdvanceDayFlag() after victory...");
        AdvanceDayFlag();

        // Fade screen + show text
        if (endScreenUI != null)
            endScreenUI.ShowMessage("YOU WIN", new Color(1f, 0.84f, 0.0f)); // gold

        // TODO: update timer shit
        var clock = FindObjectOfType<ClockTimer>();
        if (clock != null)
        {
            clock.AddTime(10f); // TODO: adjust reward amount
            Debug.Log("[RoundManager] Added 10 seconds to clock timer as reward");
        }
        
        Debug.Log("[RoundManager] Starting return to overworld sequence...");
        StartCoroutine(ReturnToOverworldDelayed());
    }

    // -------------------------------------------------------
    // PLAYER LOSSES
    // -------------------------------------------------------
    private void HandlePlayerLose()
    {
        Debug.Log("[RoundManager] ============================================");
        Debug.Log("[RoundManager] ❌ PLAYER DEFEAT!");
        Debug.Log("[RoundManager] ============================================");

        // Advance day flag (this will trigger auto-save)
        Debug.Log("[RoundManager] Calling AdvanceDayFlag() after defeat...");
        AdvanceDayFlag();

        if (endScreenUI != null)
            endScreenUI.ShowMessage("YOU LOSE", Color.red);

        // TODO: wrong todo just ummm timer change flag shit
        var clock = FindObjectOfType<ClockTimer>();
        if (clock != null)
        {
            clock.RemoveTime(100f); // TODO: adjust penalty amount
            Debug.Log("[RoundManager] Removed 100 seconds from clock timer as penalty");
        }
        
        Debug.Log("[RoundManager] Starting return to overworld sequence...");
        StartCoroutine(ReturnToOverworldDelayed());
    }

    private IEnumerator ReturnToOverworldDelayed()
    {
        yield return new WaitForSeconds(3f);

        // Normal return to overworld
        // ClockTimer will handle day five cutscene check when transitioning from overworld
        Debug.Log("[RoundManager] Returning to Overworld");

        // Using ScreenFader if available
        var fader = ScreenFader.Instance;
        if (fader != null)
        {
            Debug.Log("[RoundManager] Using ScreenFader for transition");
            yield return fader.TransitionToSceneWithEyesClosing("Overworld");
        }
        else
        {
            Debug.LogWarning("[RoundManager] ScreenFader missing! Falling back to direct load.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Overworld");
        }
    }

    // handle advancing day flags
    // Day flags now auto-save when set (implemented in GameFlags.SetFlag)
    private void AdvanceDayFlag()
    {
        Debug.Log("[RoundManager] ============================================");
        Debug.Log("[RoundManager] AdvanceDayFlag() called - Checking day progression");
        
        // Log current day state
        Debug.Log($"[RoundManager] Current day flags: " +
                  $"day.one={GameFlags.HasFlag("day.one")}, " +
                  $"day.two={GameFlags.HasFlag("day.two")}, " +
                  $"day.three={GameFlags.HasFlag("day.three")}, " +
                  $"day.four={GameFlags.HasFlag("day.four")}, " +
                  $"day.five={GameFlags.HasFlag("day.five")}");
        
        if (GameFlags.HasFlag("day.one") && !GameFlags.HasFlag("day.two"))
        {
            Debug.Log("[RoundManager] 📅 DAY PROGRESSION: day.one → day.two");
            GameFlags.SetFlag("day.two");
            Debug.Log("[RoundManager] ✅ Successfully advanced to day.two (auto-saved)");
            Debug.Log("[RoundManager] ============================================");
            return;
        }
        if (GameFlags.HasFlag("day.two") && !GameFlags.HasFlag("day.three"))
        {
            Debug.Log("[RoundManager] 📅 DAY PROGRESSION: day.two → day.three");
            GameFlags.SetFlag("day.three");
            Debug.Log("[RoundManager] ✅ Successfully advanced to day.three (auto-saved)");
            Debug.Log("[RoundManager] ============================================");
            return;
        }
        if (GameFlags.HasFlag("day.three") && !GameFlags.HasFlag("day.four"))
        {
            Debug.Log("[RoundManager] 📅 DAY PROGRESSION: day.three → day.four");
            GameFlags.SetFlag("day.four");
            Debug.Log("[RoundManager] ✅ Successfully advanced to day.four (auto-saved)");
            Debug.Log("[RoundManager] ============================================");
            return;
        }
        if (GameFlags.HasFlag("day.four") && !GameFlags.HasFlag("day.five"))
        {
            Debug.Log("[RoundManager] 📅 DAY PROGRESSION: day.four → day.five ⭐ FINAL DAY!");
            GameFlags.SetFlag("day.five");
            Debug.Log("[RoundManager] ✅ Successfully advanced to day.five (auto-saved)");
            Debug.Log("[RoundManager] 🎉 FINAL DAY REACHED - Special cutscene will trigger!");
            Debug.Log("[RoundManager] ============================================");
            return;
        }

        // Already at day five - no more progression
        Debug.Log("[RoundManager] ⚠️ Already at maximum day (day.five) - no progression");
        Debug.Log("[RoundManager] ============================================");
    }
}
