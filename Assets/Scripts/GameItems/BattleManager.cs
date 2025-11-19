using UnityEngine;
using System.Collections.Generic;
using Entities.Enemies.Data;
using Entities.Enemies.Manager;
using Entities.Players.Data;
using GameItems;

public class BattleManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public RoundManager roundManager;
    [SerializeField] private DeckViewer deckViewer; // Reference to the DeckViewer

    [Header("Config")]
    [Tooltip("Player configuration asset used to initialize the runtime player.")]
    [SerializeField] private PlayerConfig playerConfig;
    
    private void Start()
    {
        if (playerManager == null)
        {
            Debug.LogError("BattleManager: PlayerManager is not assigned.");
            return;
        }

        // 1️⃣ Initialize Player using config asset
        if (playerConfig != null)
        {
            playerManager.Initialize(playerConfig);
        }
        else
        {
            Debug.LogWarning("BattleManager: No PlayerConfig assigned; using PlayerManager's existing PlayerConfig.");
        }

        // 2️⃣ Initialize Enemies
        List<EnemyData> enemies = GenerateEnemyWave();
        enemyManager.InitializeEnemies(enemies);

        // 3️⃣ Link Managers
        roundManager.Initialize(playerManager, enemyManager);

        // 4️⃣ Start the first round
        roundManager.StartRound();
        
        // Build the deck visualization
        if (deckViewer != null)
        {
            deckViewer.SetSource(DeckViewer.Source.Hand);
            deckViewer.Rebuild();
        }
    }
    
    [SerializeField] private List<EnemyConfig> enemyDatabase;

    private List<EnemyData> GenerateEnemyWave()
    {
        List<EnemyData> wave = new();

        // Example: load 2 random enemies from your database
        for (int i = 0; i < 2; i++)
        {
            var randomEnemy = enemyDatabase[Random.Range(0, enemyDatabase.Count)];
            wave.Add(randomEnemy.CreateRuntimeInstance());
        }

        return wave;
    }
}