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
            _battleWaves = waveConfig.GetWavesForCurrentDay();
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
                    waveMessage = "All enemies at once!",
                    guaranteedEnemies = enemyDatabase != null 
                        ? new List<EnemyConfig>(enemyDatabase)
                        : new List<EnemyConfig>()
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
        Debug.Log($"Wave {_currentWaveIndex + 1} complete! ({_currentWaveIndex + 1}/{_battleWaves.Count})");

        int nextWaveIndex = _currentWaveIndex + 1;

        if (nextWaveIndex < _battleWaves.Count)
        {
            _currentWaveIndex = nextWaveIndex;
            StartCoroutine(WaveTransitionSequence(_currentWaveIndex));
        }
        else
        {
            Debug.Log("All waves complete! Player wins!");
            if (roundManager != null)
            {
                roundManager.TriggerVictory();
            }
        }
    }

    private System.Collections.IEnumerator WaveTransitionSequence(int waveIndex)
    {
        // Wait 1.5 seconds after enemies are defeated
        yield return new WaitForSeconds(1.5f);

        // Show the wave transition UI (which also handles fading)
        roundManager.ShowWaveStartUI(waveIndex + 1);

        // Wait a moment while the UI is visible before spawning enemies
        yield return new WaitForSeconds(1f);

        // Now, spawn the enemies for the new wave
        StartWave(waveIndex);
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

        // Optional message
        if (!string.IsNullOrEmpty(wave.waveMessage))
            Debug.Log($">>> {wave.waveMessage} <<<");

        // 1️⃣ Build final enemy list from this wave's rules
        List<EnemyConfig> waveEnemyConfigs = wave.GenerateEnemiesForWave();

        if (waveEnemyConfigs == null || waveEnemyConfigs.Count == 0)
        {
            Debug.LogWarning($"BattleManager: Wave {waveIndex} produced NO enemies!");
            return;
        }

        // 2️⃣ Apply this wave's scaling increment once
        if (wave.statMultiplierIncrease != 0f)
        {
            _waveMultiplier += wave.statMultiplierIncrease;
        }

        List<EnemyData> enemies = new();
        foreach (var config in waveEnemyConfigs)
        {
            if (config == null) continue;

            EnemyData enemy = config.CreateRuntimeInstance();

            // Always scaled by the current _waveMultiplier
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * _waveMultiplier);
            enemy.currentHealth = enemy.maxHealth;
            enemy.attackPower = Mathf.RoundToInt(enemy.attackPower * _waveMultiplier);
            enemy.defensePower = Mathf.RoundToInt(enemy.defensePower * _waveMultiplier);

            enemies.Add(enemy);
        }

        if (enemies.Count == 0)
        {
            Debug.LogWarning($"BattleManager: Wave {waveIndex} produced NO valid enemies after generation!");
            return;
        }

        // 3️⃣ Spawn enemies
        enemyManager.InitializeEnemies(enemies);

        // 4️⃣ Start Round System
        if (waveIndex == 0)
            roundManager.StartRound();
        else
            roundManager.StartNewWave();
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
