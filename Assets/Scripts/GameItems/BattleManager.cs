using UnityEngine;
using System.Collections.Generic;
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
    
    [Header("Wave System")]
    [Tooltip("Wave configuration for this battle. If set, uses waves instead of enemyDatabase.")]
    [SerializeField] private WaveConfig waveConfig;
    
    private List<WaveData> _battleWaves;
    private int _currentWaveIndex = 0;
    private float _waveMultiplier = 1f;
    
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

        // 2️⃣ Initialize Wave System
        if (waveConfig != null)
        {
            _battleWaves = waveConfig.GetWaves();
            Debug.Log($"BattleManager: Initialized with {_battleWaves.Count} waves");
        }
        else
        {
            Debug.LogWarning("BattleManager: No WaveConfig assigned; generating single wave from enemy database.");
            _battleWaves = new List<WaveData>
            {
                new WaveData
                {
                    waveName = "Single Wave",
                    enemies = new List<EnemyConfig>(enemyDatabase)
                }
            };
        }

        // 3️⃣ Link Managers and set wave callback
        roundManager.Initialize(playerManager, enemyManager);
        roundManager.onWaveComplete = OnWaveComplete;

        // 4️⃣ Start the first wave
        StartWave(0);
        
        // 5️⃣ Build the deck visualization
        if (deckViewer != null)
        {
            deckViewer.SetSource(DeckViewer.Source.Hand);
            deckViewer.Rebuild();
        }
        
        // 5️⃣ Start camera bobbing for combat feel
        CameraShake.StartBobbing(bobSpeed: 0.5f, bobAmount: 0.05f);
    }
    
    [Header("Legacy - Enemy Database (Fallback)")]
    [Tooltip("Fallback enemy database if no WaveConfig is assigned")]
    [SerializeField] private List<EnemyConfig> enemyDatabase;

    /// <summary>
    /// Called when a wave is completed (all enemies defeated)
    /// </summary>
    private void OnWaveComplete()
    {
        _currentWaveIndex++;
        
        // Check if there are more waves
        if (_currentWaveIndex < _battleWaves.Count)
        {
            Debug.Log($"Wave {_currentWaveIndex} complete! Starting next wave...");
            
            // Apply difficulty scaling
            if (waveConfig != null && waveConfig.useRandomWaves)
            {
                _waveMultiplier += waveConfig.difficultyScaling;
            }
            
            // Start next wave after a delay
            StartCoroutine(StartWaveDelayed(_currentWaveIndex, 2f));
        }
        else
        {
            Debug.Log("All waves complete! Player wins!");
            // Trigger victory through RoundManager
            if (roundManager != null)
            {
                roundManager.TriggerVictory();
            }
        }
    }

    /// <summary>
    /// Start a wave with optional delay
    /// </summary>
    private System.Collections.IEnumerator StartWaveDelayed(int waveIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartWave(waveIndex);
    }

    /// <summary>
    /// Starts a specific wave
    /// </summary>
    private void StartWave(int waveIndex)
    {
        if (waveIndex >= _battleWaves.Count)
        {
            Debug.LogError($"BattleManager: Wave index {waveIndex} out of range!");
            return;
        }

        WaveData wave = _battleWaves[waveIndex];
        
        // Display wave message if available
        if (!string.IsNullOrEmpty(wave.waveMessage))
        {
            Debug.Log($">>> {wave.waveMessage} <<<");
            // TODO: Show wave message in UI
        }

        // Generate enemies for this wave
        List<EnemyData> enemies = new();
        foreach (var enemyConfig in wave.enemies)
        {
            if (enemyConfig != null)
            {
                EnemyData enemy = enemyConfig.CreateRuntimeInstance();
                
                // Apply wave difficulty multiplier
                if (_waveMultiplier != 1f)
                {
                    enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * _waveMultiplier);
                    enemy.currentHealth = enemy.maxHealth;
                    enemy.attackPower = Mathf.RoundToInt(enemy.attackPower * _waveMultiplier);
                    enemy.defensePower = Mathf.RoundToInt(enemy.defensePower * _waveMultiplier);
                }
                
                enemies.Add(enemy);
            }
        }

        if (enemies.Count == 0)
        {
            Debug.LogWarning($"BattleManager: Wave {waveIndex} has no enemies!");
            return;
        }

        // Initialize enemies
        enemyManager.InitializeEnemies(enemies);
        
        // Start/resume the round system
        if (waveIndex == 0)
        {
            roundManager.StartRound();
        }
        else
        {
            roundManager.StartNewWave();
        }
    }

    private List<EnemyData> GenerateEnemyWave()
    {
        List<EnemyData> wave = new();

        // Fallback: load 2 random enemies from your database
        if (enemyDatabase == null || enemyDatabase.Count == 0)
        {
            Debug.LogWarning("BattleManager: Enemy database is empty!");
            return wave;
        }

        for (int i = 0; i < 2; i++)
        {
            var randomEnemy = enemyDatabase[Random.Range(0, enemyDatabase.Count)];
            wave.Add(randomEnemy.CreateRuntimeInstance());
        }

        return wave;
    }
}