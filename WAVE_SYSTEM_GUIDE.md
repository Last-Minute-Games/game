# Wave System Guide

## Overview
The wave system allows you to create dynamic multi-round battles with different enemy configurations. Enemies appear in waves, and the player must defeat all enemies in a wave before the next wave spawns.

## Components

### 1. WaveConfig (ScriptableObject)
A configuration asset that defines all waves for a battle encounter.

**Location:** `Assets/Scripts/GameItems/WaveConfig.cs`

### 2. WaveData (Serializable Class)
Defines a single wave/round of enemies in a battle.

## How to Use

### Method 1: Predefined Waves (Manual Configuration)

1. **Create a WaveConfig Asset:**
   - Right-click in your Project window
   - Select `Create > Battle > Wave Configuration`
   - Name it (e.g., "Tutorial_Battle_Waves")

2. **Configure Waves:**
   - Set `Use Random Waves` to `false`
   - Set the size of the `Waves` list (e.g., 3 for 3 waves)
   - For each wave:
     - **Wave Name:** Give it a descriptive name (e.g., "Wave 1 - Easy Start")
     - **Enemies:** Add EnemyConfig assets to the list
     - **Delay Before Wave:** Optional delay in seconds before wave starts
     - **Wave Message:** Optional message to display (e.g., "Reinforcements incoming!")

3. **Assign to BattleManager:**
   - In your battle scene, select the BattleManager GameObject
   - Drag your WaveConfig asset into the `Wave Config` field

### Method 2: Random Waves (Procedural Generation)

1. **Create a WaveConfig Asset** (same as above)

2. **Configure Random Generation:**
   - Set `Use Random Waves` to `true`
   - Set `Number Of Random Waves` (e.g., 5)
   - Populate `Enemy Pool` with all possible EnemyConfig assets
   - Set `Min Enemies Per Wave` (e.g., 1)
   - Set `Max Enemies Per Wave` (e.g., 3)
   - Set `Difficulty Scaling` (e.g., 0.2 for +20% stats per wave)

3. **Assign to BattleManager** (same as above)

### Method 3: Legacy Single Battle (No Waves)

If you don't assign a WaveConfig:
- BattleManager will use the `Enemy Database` field
- All enemies from the database will spawn in a single wave
- Battle ends when all enemies are defeated

## Features

### Difficulty Scaling
When using random waves, each successive wave can have stronger enemies:
- Wave 1: 1.0x stats (base)
- Wave 2: 1.2x stats (+20%)
- Wave 3: 1.4x stats (+40%)
- etc.

### Wave Completion Flow
1. Player defeats all enemies in current wave
2. `RoundManager.CheckImmediateEndConditions()` detects no living enemies
3. Calls `BattleManager.OnWaveComplete()`
4. BattleManager checks if more waves exist:
   - **More waves:** Spawns next wave after delay, battle continues
   - **No more waves:** Battle ends, player wins

### Wave Transitions
- 2-second delay between waves (configurable in `BattleManager.StartWaveDelayed`)
- Player's health/energy persists between waves
- Player's deck/hand persists between waves
- Round number continues incrementing

## Example Wave Configurations

### Easy to Hard Progression
```
Wave 1: [Goblin]
Wave 2: [Goblin, Goblin]
Wave 3: [Goblin, Orc]
Wave 4: [Orc, Orc]
Wave 5: [Orc, Dragon]
```

### Boss Battle
```
Wave 1: [Minion, Minion]
Wave 2: [Elite Minion]
Wave 3: [Boss]
```

### Survival Mode (Random)
```
Use Random Waves: true
Number Of Random Waves: 10
Enemy Pool: [All enemies]
Min Per Wave: 1
Max Per Wave: 4
Difficulty Scaling: 0.15
```

## Code Architecture

### BattleManager
- Loads WaveConfig
- Manages wave transitions
- Spawns enemies for each wave
- Applies difficulty multipliers

### RoundManager
- Handles turn-based combat
- Detects wave completion
- Triggers `onWaveComplete` callback
- Continues rounds across waves

### EnemyManager
- Spawns/despawns enemy visuals
- Manages enemy actions
- Uses `ExecuteEnemyTurnSequence` for animated turns

## Tips

1. **Test Individual Waves:** Create separate WaveConfigs for testing
2. **Balance Difficulty:** Start with lower scaling values (0.1-0.2)
3. **Wave Messages:** Use them for storytelling or warnings
4. **Mixed Enemies:** Combine different enemy types for variety
5. **Dynamic Battles:** Use random waves for replayability

## Future Enhancements

Possible additions:
- Wave rewards (cards, health, energy)
- Boss wave indicators
- Wave skip/fast-forward for testing
- Wave save/load system
- Conditional waves (based on player performance)

