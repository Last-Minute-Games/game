using UnityEngine;
using System.Collections.Generic;
using Entities.Enemies.Manager;
using GameItems;

public class BattleManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public RoundManager roundManager;
    [SerializeField] private DeckViewer deckViewer; // Reference to the DeckViewer
    
    private void Start()
    {
        // 1️⃣ Initialize Player (runtime-only example)
        var playerData = ScriptableObject.CreateInstance<PlayerData>();
        playerData.InitializeRuntime();

        // Give the player a starter pool/deck (CardData ScriptableObjects in Resources/Cards)
        playerData.usableCards = new List<CardData>(Resources.LoadAll<CardData>("Cards"));
        playerManager.Initialize(playerData);

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
    
    [SerializeField] private List<EnemyDataSO> enemyDatabase;

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