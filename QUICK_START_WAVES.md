# Quick Start: Multi-Wave Battle System

## 🎯 What You Can Do Now

Your battle system now supports:
- ✅ Multiple waves of enemies in a single battle
- ✅ Manual wave configuration (design specific encounters)
- ✅ Random wave generation (procedural/replayable battles)
- ✅ Difficulty scaling (enemies get stronger each wave)
- ✅ Wave messages and delays
- ✅ Player persistence across waves (health, deck, etc.)

## 🚀 Quick Setup (5 Minutes)

### Option A: Use the Editor Tool (Recommended)
1. In Unity, go to **Tools → Battle → Wave Config Creator**
2. Click **"Create New WaveConfig"**
3. Save it in your project (e.g., `Assets/_Data/Battle_Tutorial.asset`)
4. Add waves:
   - Enter wave name
   - Drag EnemyConfig assets into slots
   - Click **"Add Wave to Config"**
   - Repeat for each wave
5. Assign the WaveConfig to your BattleManager in the scene

### Option B: Manual Setup
1. Right-click in Project → **Create → Battle → Wave Configuration**
2. Name it (e.g., "Battle_Tutorial")
3. In Inspector:
   - Set wave count (e.g., 3)
   - For each wave, add EnemyConfig assets
4. Assign to BattleManager

## 📋 Example Configurations

### Easy Progression (3 Waves)
```
Wave 1: [Goblin]               // Solo enemy
Wave 2: [Goblin, Goblin]       // Duo
Wave 3: [Goblin, Orc]          // Mixed difficulty
```

### Boss Fight (2 Waves)
```
Wave 1: [Minion, Minion]       // Warm-up
Wave 2: [Boss]                 // Main event
```

### Survival Mode (Random)
```
Use Random Waves: ✓
Number of Waves: 5
Enemy Pool: [All your enemies]
Min Per Wave: 1
Max Per Wave: 3
Difficulty Scaling: 0.2 (20% per wave)
```

## 🎮 How Players Experience It

1. Battle starts → Wave 1 spawns
2. Player fights and defeats all enemies
3. Screen shows "Wave complete!" (2 sec delay)
4. Wave 2 spawns with new enemies
5. Repeat until all waves defeated
6. Victory screen appears

**Player State Persists:**
- Current HP/Energy
- Cards in hand/deck
- Round counter continues

## 🔧 Files Created/Modified

### New Files
- `WaveConfig.cs` - Main wave configuration system
- `WaveConfigCreator.cs` - Unity Editor tool (Tools menu)
- `WAVE_SYSTEM_GUIDE.md` - Full documentation

### Modified Files
- `BattleManager.cs` - Wave loading and transitions
- `RoundManager.cs` - Wave completion detection

### Backward Compatible
- Old battles still work without WaveConfig
- Enemy Database field still functional

## 🎨 Customization Options

### In WaveConfig Asset:
- `waveName` - Display name for wave
- `enemies` - List of EnemyConfig assets
- `delayBeforeWave` - Seconds before wave starts
- `waveMessage` - Text to show (e.g., "Boss incoming!")

### In BattleManager:
- Delay between waves (default: 2 seconds)
- Victory/defeat handling
- Scene transitions

### Difficulty Scaling (Random Waves):
- Wave 1: 100% stats
- Wave 2: 120% stats
- Wave 3: 140% stats
- etc.

## 🐛 Testing Tips

1. **Test Single Wave First:**
   - Create WaveConfig with 1 wave
   - Verify enemies spawn correctly

2. **Test Wave Transitions:**
   - Add 2 waves
   - Defeat Wave 1, watch Wave 2 spawn

3. **Test Random Generation:**
   - Enable "Use Random Waves"
   - Play multiple times to see variety

4. **Debug Logs:**
   - Check Console for wave messages
   - Look for "Wave X complete!" logs

## 🔍 Troubleshooting

**Waves not spawning?**
- Check WaveConfig is assigned to BattleManager
- Verify each wave has at least one enemy

**Battle ends after first wave?**
- Check `RoundManager.onWaveComplete` is set
- Verify BattleManager.OnWaveComplete() is called

**Enemies too easy/hard?**
- Adjust `difficultyScaling` value
- Modify enemy stats in EnemyConfig

## 📚 Next Steps

1. Create your first WaveConfig
2. Test with 2-3 simple waves
3. Experiment with random waves
4. Add wave messages for story/drama
5. Balance difficulty scaling

## 💡 Advanced Ideas

- **Reward System:** Give cards/health between waves
- **Wave Skip:** Add debug button to skip waves
- **Boss Indicators:** Special UI for boss waves
- **Wave Modifiers:** Buffs/debuffs per wave
- **Branching Waves:** Different paths based on performance

---

**Need Help?** Check `WAVE_SYSTEM_GUIDE.md` for detailed documentation.

