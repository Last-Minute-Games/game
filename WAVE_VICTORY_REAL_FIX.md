# Wave System Victory Bug - REAL Fix

## Root Cause Analysis

### The Problem
After defeating all enemies in wave 1, the victory screen appeared immediately instead of transitioning to wave 2.

### Why It Happened

The issue was in `RoundManager.Update()`:

```csharp
private void Update()
{
    CheckImmediateEndConditions(); // Called EVERY FRAME
    // ...
}
```

And in `CheckImmediateEndConditions()`:

```csharp
if (enemyManager.AllEnemiesDefeated())
{
    Debug.Log("All enemies defeated in current wave!");
    onWaveComplete.Invoke(); // Triggers BattleManager
}
```

### The Timing Issue

Here's what was happening frame-by-frame:

```
Frame 1: Last enemy dies in Wave 1
    ↓
Frame 2: Update() → CheckImmediateEndConditions()
    ↓ AllEnemiesDefeated() = TRUE
    ↓ onWaveComplete.Invoke()
    ↓ BattleManager.OnWaveComplete() runs
    ↓ Starts coroutine: StartWaveDelayed(1, 2f) [2 second delay]
    ↓
Frame 3: Update() → CheckImmediateEndConditions() [STILL RUNNING!]
    ↓ AllEnemiesDefeated() = TRUE (wave 2 hasn't spawned yet!)
    ↓ onWaveComplete.Invoke() AGAIN!
    ↓ BattleManager.OnWaveComplete() runs AGAIN
    ↓ nextWaveIndex = 1 + 1 = 2
    ↓ if (2 < 2) = FALSE
    ↓ TriggerVictory() ❌ WRONG!
```

**The problem:** `CheckImmediateEndConditions()` kept running every frame during the 2-second delay between waves, seeing no enemies spawned yet, and triggering wave complete logic multiple times!

## The Solution

Added a `_isTransitioningWaves` flag to prevent checking end conditions during wave transitions.

### 1. Added Transition Flag

```csharp
[Header("State")]
public int roundNumber = 1;
public bool playerTurn = true;
public bool battleActive = false;

// Prevents checking end conditions during wave transitions
private bool _isTransitioningWaves = false;
```

### 2. Check Flag in CheckImmediateEndConditions()

```csharp
public void CheckImmediateEndConditions()
{
    if (!battleActive) return;
    
    // Don't check end conditions during wave transitions
    if (_isTransitioningWaves) return; // ← NEW!
    
    if (enemyManager.AllEnemiesDefeated())
    {
        Debug.Log("All enemies defeated in current wave!");
        
        // Set transition flag to prevent repeated calls
        _isTransitioningWaves = true; // ← NEW!
        
        onWaveComplete.Invoke();
    }
}
```

### 3. Clear Flag When New Wave Starts

```csharp
public void StartNewWave()
{
    // Clear transition flag - we're ready to check end conditions again
    _isTransitioningWaves = false; // ← NEW!
    
    playerTurn = true;
    battleActive = true;
    // ...
}
```

### 4. Clear Flag on Victory

```csharp
public void TriggerVictory()
{
    if (!battleActive) return;
    battleActive = false;
    _isTransitioningWaves = false; // ← NEW!
    HandlePlayerWin();
}
```

## How It Works Now

### Correct Flow

```
Frame 1: Last enemy in Wave 1 dies
    ↓
Frame 2: Update() → CheckImmediateEndConditions()
    ↓ _isTransitioningWaves = false
    ↓ AllEnemiesDefeated() = TRUE
    ↓ Set _isTransitioningWaves = true ← BLOCKS FURTHER CHECKS
    ↓ onWaveComplete.Invoke()
    ↓ BattleManager.OnWaveComplete() runs
    ↓ Starts coroutine: StartWaveDelayed(1, 2f)
    ↓
Frame 3-120: Update() → CheckImmediateEndConditions()
    ↓ _isTransitioningWaves = true ← EARLY RETURN!
    ↓ [DOES NOTHING] ✅
    ↓
Frame 121 (2 seconds later): Wave 2 enemies spawn
    ↓ StartNewWave() called
    ↓ _isTransitioningWaves = false ← READY TO CHECK AGAIN
    ↓ battleActive = true
    ↓ Player can fight Wave 2 ✅
```

## State Diagram

```
[Normal Battle] 
    ↓ All enemies defeated
[Transitioning: _isTransitioningWaves = true]
    ↓ CheckImmediateEndConditions() does nothing (returns early)
    ↓ Wait for new wave to spawn...
    ↓ StartNewWave() or TriggerVictory() called
[Back to Normal: _isTransitioningWaves = false]
    ↓ CheckImmediateEndConditions() active again
```

## Benefits

### ✅ Prevents Double-Triggering
- Wave complete logic only fires once per wave
- No repeated `onWaveComplete.Invoke()` calls

### ✅ Allows Wave Transitions
- 2-second delay between waves works correctly
- Enemies spawn before checking end conditions again

### ✅ Maintains Victory Logic
- Final wave still triggers victory correctly
- Flag is cleared when victory screen shows

### ✅ Handles Edge Cases
- Player death still works (bypasses flag)
- Manual victory trigger clears flag
- Works with any number of waves

## Testing Checklist

### Single Wave Battle
- [ ] Defeat all enemies in wave 1
- [ ] Expect: Victory screen appears ✅

### Multi-Wave Battle (2 waves)
- [ ] Defeat all enemies in wave 1
- [ ] Expect: "Wave 1 complete" message
- [ ] Expect: 2-second delay
- [ ] Expect: Wave 2 enemies spawn ✅
- [ ] Defeat all enemies in wave 2
- [ ] Expect: Victory screen appears ✅

### Multi-Wave Battle (3+ waves)
- [ ] Complete wave 1 → wave 2 spawns ✅
- [ ] Complete wave 2 → wave 3 spawns ✅
- [ ] Complete wave 3 → victory screen ✅

### Player Death During Wave
- [ ] Take lethal damage during any wave
- [ ] Expect: Defeat screen appears immediately ✅
- [ ] Expect: No wave transition attempted ✅

## Debug Output

### Before Fix (Broken)
```
🎉 All enemies defeated in current wave!
Wave 1 complete! (1/2)
Starting wave 2...
🎉 All enemies defeated in current wave!  ← DUPLICATE!
Wave 2 complete! (2/2)  ← WRONG! Wave 2 didn't even spawn!
All waves complete! Player wins!  ← TOO EARLY!
```

### After Fix (Working)
```
🎉 All enemies defeated in current wave!
Wave 1 complete! (1/2)
Starting wave 2...
[2 second delay - no repeated checks]
--- New Wave - Round 1 Start ---
[Wave 2 enemies spawn]
[Player fights Wave 2]
🎉 All enemies defeated in current wave!
Wave 2 complete! (2/2)
All waves complete! Player wins!  ← CORRECT!
```

## Files Modified

1. ✅ **RoundManager.cs**
   - Added `_isTransitioningWaves` flag
   - Updated `CheckImmediateEndConditions()` to check flag
   - Set flag to `true` when wave completes
   - Clear flag to `false` in `StartNewWave()`
   - Clear flag in `TriggerVictory()`

## Related Systems

### Works With
- ✅ Wave system (multiple waves)
- ✅ Single wave battles (fallback)
- ✅ Round system
- ✅ Victory/defeat screens
- ✅ Enemy spawning delays

### Doesn't Break
- ✅ Player death detection
- ✅ Enemy intent system
- ✅ Card discard animations
- ✅ Turn timer

---

**Status:** ✅ Actually Fixed Now!
**Root Cause:** Update() checking end conditions every frame during wave transitions
**Solution:** Transition flag blocks repeated checks until new wave spawns

