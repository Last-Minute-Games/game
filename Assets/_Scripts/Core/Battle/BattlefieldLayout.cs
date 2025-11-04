using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattlefieldLayout : MonoBehaviour
{
    [Header("Prefabs & Libraries")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private EnemyLibrary enemyLibrary;
    [SerializeField] private GameObject playerPrefab;

    private Player playerInstance;
    private readonly List<Enemy> activeEnemies = new();

    [Header("Enemy Positioning")]
    [SerializeField] private float horizontalSpacing = 2.5f;
    [SerializeField] private float rearOffsetY = 0.5f;
    [SerializeField] private float rearScale = 0.8f;

    [Header("Round Control")]
    [SerializeField] private bool autoStartNextRound = true;
    private int currentRound = 0;
    private bool isTransitioning = false;

    // Define your rounds here by enemyID
    private readonly List<List<string>> roundEnemyIDs = new()
    {
        new() { "sexy_eyeball" },
        new() { "sexy_eyeball", "sexy_eyeball" },
        new() { "sexy_eyeball", "sexy_eyeball", "sexy_eyeball" },
    };

    private void Start()
    {
        SpawnPlayer();
        StartFirstRound();
    }

    private void Update()
    {
        if (!autoStartNextRound || isTransitioning || activeEnemies.Count == 0)
            return;

        bool allDead = true;
        foreach (Enemy e in activeEnemies)
        {
            if (e != null && !e.IsDead)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            StartCoroutine(HandleNextRoundTransition());
        }
    }

    // --------------------------
    // ROUND MANAGEMENT
    // --------------------------

    private void StartFirstRound()
    {
        ClearEnemies();
        SpawnEnemiesByIDs(roundEnemyIDs[0]);
        currentRound = 1;
        // ❌ Removed redundant hand spawn — BattleSystem.Init handles first hand
    }

    private IEnumerator HandleNextRoundTransition()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        var battleSystem = FindFirstObjectByType<BattleSystem>();
        var ui = FindFirstObjectByType<RoundTransitionUI>();

        // 🔹 Clear hand BEFORE transition so cards vanish
        if (battleSystem != null)
            yield return battleSystem.StartCoroutine("ClearHand");

        // --- WIN CONDITION ---
        if (currentRound >= roundEnemyIDs.Count)
        {
            Debug.Log("✅ All rounds complete! Player wins!");
            if (battleSystem != null)
                battleSystem.StartCoroutine("HandleBattleFinished", true);
            yield break;
        }

        int roundNum = currentRound + 1;

        if (ui != null)
        {
            yield return ui.FadeIn();
            yield return ui.ShowRoundText(roundNum);

            ClearEnemies();
            SpawnEnemiesByIDs(roundEnemyIDs[currentRound]);
            currentRound++;

            // 🟩 Sync with BattleSystem here
            if (battleSystem != null)
            {
                battleSystem.RefreshEnemyList();     // update new enemy references
                battleSystem.RefillPlayerEnergy();   // reset energy to 3
            }

            yield return new WaitForSeconds(0.25f);
            yield return ui.FadeOut();
        }

        // Redraw cards after fade out
        if (battleSystem != null)
        {
            yield return battleSystem.StartCoroutine("RefreshPlayerHand");
            battleSystem.StartPlayerTurn(); // ✅ ensures intentions are shown
        }

        isTransitioning = false;
    }

    // --------------------------
    // SPAWNING LOGIC
    // --------------------------

    private void SpawnEnemiesByIDs(List<string> enemyIDs)
    {
        if (enemyPrefab == null || enemyLibrary == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Enemy Prefab or Library!");
            return;
        }

        activeEnemies.Clear();
        Vector3 center = Vector3.zero;
        int enemyCount = enemyIDs.Count;

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData data = enemyLibrary.GetEnemyByID(enemyIDs[i]);
            if (data == null)
            {
                Debug.LogWarning($"⚠️ Enemy ID '{enemyIDs[i]}' not found in library!");
                continue;
            }

            Vector3 spawnPos = GetSpawnPosition(i, enemyCount, center);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy newEnemy = enemyObj.GetComponent<Enemy>();
            if (newEnemy == null)
            {
                Debug.LogError("❌ Prefab is missing Enemy component!");
                continue;
            }

            newEnemy.InitializeFromData(data);

            if (enemyCount == 3 && i == 2)
            {
                enemyObj.transform.localScale *= rearScale;
                enemyObj.transform.position += Vector3.up * rearOffsetY;
            }

            activeEnemies.Add(newEnemy);
        }

        Debug.Log($"🌀 Spawned Round {currentRound + 1} with {activeEnemies.Count} enemies");
    }

    private void ClearEnemies()
    {
        foreach (Enemy e in activeEnemies)
            if (e != null) Destroy(e.gameObject);
        activeEnemies.Clear();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ BattlefieldLayout: Missing Player Prefab!");
            return;
        }

        GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerObj.name = "Player";

        playerInstance = playerObj.GetComponent<Player>();
        if (playerInstance == null)
        {
            Debug.LogError("❌ Player prefab missing Player component!");
            return;
        }

        playerInstance.characterName = "Player";
        playerInstance.currentHealth = playerInstance.maxHealth;

        var battleSystem = FindFirstObjectByType<BattleSystem>();
        if (battleSystem != null)
            battleSystem.RegisterPlayer(playerInstance);
    }

    private Vector3 GetSpawnPosition(int index, int total, Vector3 center)
    {
        if (total == 1)
            return center;
        if (total == 2)
            return index == 0
                ? center + Vector3.left * horizontalSpacing / 2f
                : center + Vector3.right * horizontalSpacing / 2f;
        if (total == 3)
        {
            if (index == 0) return center + Vector3.left * horizontalSpacing;
            if (index == 1) return center + Vector3.right * horizontalSpacing;
            return center + Vector3.back * 0.1f;
        }
        return center + Vector3.right * (index - total / 2f) * horizontalSpacing;
    }

    public IReadOnlyList<Enemy> GetEnemies() => activeEnemies;

    public bool IsFinalRoundComplete()
    {
        // true if we have completed all rounds
        return currentRound >= roundEnemyIDs.Count && activeEnemies.TrueForAll(e => e == null || e.IsDead);
    }
}
