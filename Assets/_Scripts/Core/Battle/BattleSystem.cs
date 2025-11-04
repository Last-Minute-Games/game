using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BattleSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private BattlefieldLayout battlefieldLayout;
    [SerializeField] private HandView handView;
    [SerializeField] private CardFactory cardFactory;
    [SerializeField] private Transform handSpawnPoint;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private ScreenFader screenFader; // assign FadeCanvas ScreenFader

    [Header("Battle Settings")]
    [SerializeField] private int startingHandSize = 5;
    [SerializeField] private float turnResetDelay = 1.5f;

    private Player player;
    private readonly List<Enemy> enemies = new();
    private bool playerTurn = true;
    private bool isProcessingTurn = false;

    public List<Enemy> GetEnemies()
    {
        return enemies;
    }
    private TurnTimer turnTimer; // timer


    private bool queuedTurnEndRequest = false;

    private void Start()
    {
        turnTimer = FindFirstObjectByType<TurnTimer>();
        StartCoroutine(InitializeBattle());
    }

    public void RegisterPlayer(Player p)
    {
        player = p;
    }

    // -------------------------- INITIALIZATION --------------------------
    public void UpdateEnemies()
    {
        enemies.AddRange(FindObjectsOfType<Enemy>());
        enemies.RemoveAll(e => e == null || e.IsDead);
    }
    
    private IEnumerator InitializeBattle()
    {
        yield return null;

        UpdateEnemies();

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

    // -------------------------- HAND MANAGEMENT --------------------------
    private IEnumerator SpawnStartingHand()
    {
        int guaranteedAttackId = 0;
        int guaranteedDefenseId = 1;

        // Guaranteed Attack card
        GameObject attackCard = cardFactory.PullCardById(guaranteedAttackId, handSpawnPoint.position);
        if (attackCard != null)
        {
            CardView cv = attackCard.GetComponent<CardView>();
            if (cv != null)
            {
                cv.player = player;
                if (enemies.Count > 0)
                    cv.targetEnemy = enemies[Random.Range(0, enemies.Count)];
                yield return handView.AddCard(cv);
            }
        }

        // Guaranteed Defense card
        GameObject defenseCard = cardFactory.PullCardById(guaranteedDefenseId, handSpawnPoint.position);
        if (defenseCard != null)
        {
            CardView cv = defenseCard.GetComponent<CardView>();
            if (cv != null)
            {
                cv.player = player;
                if (enemies.Count > 0)
                    cv.targetEnemy = enemies[Random.Range(0, enemies.Count)];
                yield return handView.AddCard(cv);
            }
        }

        // Fill remaining hand randomly
        for (int i = 0; i < startingHandSize - 2; i++)
        {
            GameObject cardObj = cardFactory.CreateRandomCard(
                handSpawnPoint.position,
                0.6f, 0.25f, 0.15f,
                forPlayer: true
            );

            if (cardObj == null)
            {
                Debug.LogError("❌ BattleSystem: Failed to create random card!");
                continue;
            }

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
        if (handView != null)
            yield return handView.ClearAllCards();
    }

    public IEnumerator RefreshPlayerHand()
    {
        yield return ClearHand();
        yield return SpawnStartingHand();
    }

    // -------------------------- TURN MANAGEMENT --------------------------
    public void StartPlayerTurn()
    {

        enemies.Clear();
        enemies.AddRange(FindObjectsOfType<Enemy>());
        enemies.RemoveAll(e => e == null || e.IsDead);

        playerTurn = true;
        player.RefillEnergy();
        EnergySystem.Instance?.OnNewTurn();

        turnTimer?.StartTimer();   // start countdown

        Debug.Log("🔹 Player’s turn started!");

        if (queuedTurnEndRequest)
        {
            Debug.Log("🔁 Processing queued turn-end request from previous busy state.");
            queuedTurnEndRequest = false;
            EndPlayerTurn();
            return;
        }

        enemies.RemoveAll(e => e == null || e.IsDead);
        foreach (Enemy enemy in enemies)
            enemy?.PrepareNextCard();
    }

    public void EndPlayerTurn()
    {
        if (!playerTurn || isProcessingTurn) return;

        turnTimer?.StopTimer();
        Debug.Log("🔸 Player turn ended → Enemy turn begins...");
        playerTurn = false;

        StartCoroutine(HandleTurnFlow());
    }

    private IEnumerator HandleTurnFlow()
    {
        isProcessingTurn = true;

        yield return ClearHand();

        foreach (Enemy enemy in enemies)
            enemy?.EndTurn();

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead || player == null) continue;
            yield return enemy.ExecuteIntention(player);
            yield return new WaitForSeconds(0.5f);
        }

        enemies.RemoveAll(e => e == null || e.IsDead);

        // ---------------- WIN/LOSE CONDITIONS ----------------
        BattlefieldLayout layout = FindFirstObjectByType<BattlefieldLayout>();
        bool allEnemiesDead = enemies.Count == 0;
        bool playerDead = (player == null || player.currentHealth <= 0);
        bool allRoundsComplete = (layout != null && layout.IsFinalRoundComplete());

        // Player death → immediate loss
        if (playerDead)
        {
            Debug.Log("💀 Player defeated!");
            if (layout != null) layout.StopAllCoroutines();
            yield return StartCoroutine(HandleBattleFinished(false));
            yield break;
        }

        // True win only when all rounds complete AND no enemies remain
        if (allEnemiesDead && allRoundsComplete)
        {
            Debug.Log("🏁 All rounds cleared! Player wins!");
            yield return StartCoroutine(HandleBattleFinished(true));
            yield break;
        }

        // If enemies are dead but rounds remain, BattlefieldLayout will transition/spawn next round.
        if (allEnemiesDead)
        {
            Debug.Log("🌀 Round cleared — waiting for next round transition...");
            yield break;
        }

        // Normal loop
        player?.EndTurn();
        yield return new WaitForSeconds(turnResetDelay);
        yield return RefreshPlayerHand();
        StartPlayerTurn();

        isProcessingTurn = false;
    }

    // Add inside BattleSystem class, near bottom:
    public IEnumerator HandleBattleFinished(bool playerWon)
    {
        Debug.Log(playerWon ? "🏆 Player Wins!" : "💀 Player Loses!");
        yield return new WaitForSeconds(1f);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.TransitionToScene("Overworld"));
        else
            SceneManager.LoadScene("Overworld");
    }


    public void RefreshEnemyList()
    {
        enemies.Clear();
        enemies.AddRange(FindObjectsOfType<Enemy>());
        enemies.RemoveAll(e => e == null || e.IsDead);

        Debug.Log($"🔄 Enemy list refreshed: {enemies.Count} enemies registered.");
    }

    public void RefillPlayerEnergy()
    {
        if (player != null)
        {
            player.RefillEnergy();
            Debug.Log("⚡ Player energy reset for new round.");
        }
    }


    public void RequestTurnEnd(string reason = "Unknown")
    {
        // if not ready, queue the request
        if (!playerTurn || isProcessingTurn)
        {
            Debug.Log($"⚠ Turn end requested ({reason}) but system busy — queued for next opportunity.");
            queuedTurnEndRequest = true;
            return;
        }

        Debug.Log($"🔸 Turn end requested safely by {reason}.");
        EndPlayerTurn();
    }
}
