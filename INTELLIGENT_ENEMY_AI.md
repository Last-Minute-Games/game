# Intelligent Enemy AI System

## Overview
Replaced the simple random move selection with a strategic AI system that makes intelligent decisions based on the enemy's current health, block status, and available actions.

## AI Decision Tree

### Priority Levels (from highest to lowest)

#### 1. **CRITICAL HEALTH (< 20% HP)**
**Goal:** Survive at all costs

**Priority 1:** HEAL
- If heal actions available → Use heal (100% chance)
- Rationale: Emergency healing to avoid death

**Priority 2:** BLOCK
- If block actions available AND no block up → Use block (100% chance)
- Rationale: Survive the next attack

#### 2. **LOW HEALTH (20% - 50% HP)**
**Goal:** Play defensively, recover

**60% chance:** HEAL (if available)
- Prioritize recovery over offense
- Get back to fighting strength

**40% chance:** BLOCK (if available and no block up)
- Protect against incoming damage
- Buy time to recover

#### 3. **NO BLOCK UP**
**Goal:** Avoid taking full damage

**30% chance:** BLOCK (if available)
- Even at full health, consider defense
- Balanced risk management

#### 4. **FULL HEALTH (> 80% HP)**
**Goal:** Setup for big plays

**25% chance:** BUFF (if available)
- Safe to invest in future damage
- Maximize long-term value

#### 5. **DEFAULT - ATTACK**
**Goal:** Deal damage aggressively

**100% chance:** ATTACK (if available)
- Pressure the player
- Default aggressive behavior
- Always attack if no other condition met

## Code Structure

### Main Method: `DecideNextIntent()`
```csharp
public void DecideNextIntent()
{
    // Validate action pattern exists
    // Call ChooseStrategicAction()
    // Set currentIntent, intentValue, intentText
}
```

### Strategic Decision: `ChooseStrategicAction()`
```csharp
private EnemyAction ChooseStrategicAction()
{
    // 1. Calculate health percentage
    // 2. Categorize available actions by type
    // 3. Run through decision tree
    // 4. Return chosen action
}
```

### Helper: `GetRandomAction()`
```csharp
private EnemyAction GetRandomAction(List<EnemyAction> actions)
{
    // Picks a random action from the filtered list
    // Adds variety within the same action type
}
```

## Example Scenarios

### Scenario 1: Critical Health
```
Enemy: Goblin
Health: 15/100 (15%)
Block: 0
Available Actions: Attack (8), Block (5), Heal (10)

AI Decision:
✓ Health < 20% → CRITICAL
✓ Heal available → Choose HEAL (10 HP)

Result: "Goblin is critical (15%) - choosing HEAL"
```

### Scenario 2: Low Health, No Heal
```
Enemy: Orc
Health: 35/100 (35%)
Block: 0
Available Actions: Attack (12), Block (8)

AI Decision:
✓ Health < 50% → LOW HEALTH
✗ No heal available
✓ 60% roll succeeds → Choose BLOCK (8)

Result: "Orc is low health (35%) - choosing BLOCK"
```

### Scenario 3: Healthy and Buffing
```
Enemy: Wizard
Health: 85/100 (85%)
Block: 5
Available Actions: Attack (10), Buff (3)

AI Decision:
✓ Health > 80% → FULL HEALTH
✓ Buff available
✓ 25% roll succeeds → Choose BUFF (3)

Result: "Wizard is healthy (85%) - choosing BUFF"
```

### Scenario 4: Aggressive Default
```
Enemy: Warrior
Health: 65/100 (65%)
Block: 6
Available Actions: Attack (15), Attack (20), Block (10)

AI Decision:
✗ Not critical
✗ Not low health
✓ Has block up
✗ Not full health enough for buff
✓ Default → Choose ATTACK (random 15 or 20)

Result: "Warrior choosing ATTACK (default aggressive behavior)"
```

## Statistical Breakdown

### Health-Based Behavior

| Health Range | Primary Behavior | Secondary Behavior |
|--------------|------------------|-------------------|
| 0% - 20%     | Heal (100%) or Block (100%) | Survival mode |
| 20% - 50%    | Heal (60%) or Block (40%) | Defensive play |
| 50% - 80%    | Attack (default) | Block (30% if no block) |
| 80% - 100%   | Attack (default) | Buff (25%) |

### Action Type Priority

**Critical Health (<20%):**
1. Heal - 100% (if available)
2. Block - 100% (if available, no heal)
3. Attack - Fallback

**Low Health (20-50%):**
1. Heal - 60% (if available)
2. Block - 40% (if available, no block up)
3. Attack - Fallback

**Normal Health (50-80%):**
1. Attack - Default
2. Block - 30% (if no block up)

**High Health (>80%):**
1. Attack - Default
2. Buff - 25%
3. Block - 30% (if no block up)

## Benefits

### For Players
- **Predictable yet varied:** Enemies behave logically but with randomness
- **Strategic depth:** Low health enemies play defensively (plan accordingly!)
- **Fair gameplay:** Enemies make smart moves but not perfect
- **Telegraphed:** Health-based behavior gives hints about next move

### For Designers
- **Easy to balance:** Adjust percentages for difficulty
- **Flexible:** Works with any action pattern
- **Debuggable:** Clear logs show AI reasoning
- **Extensible:** Easy to add new decision factors

## Debug Logs

The AI logs its reasoning for every decision:

```
[AI] Goblin is critical (15%) - choosing HEAL
[AI] Orc is low health (35%) - choosing BLOCK
[AI] Dragon has no block - choosing BLOCK
[AI] Wizard is healthy (85%) - choosing BUFF
[AI] Warrior choosing ATTACK (default aggressive behavior)
[AI] Slime has no attack actions - picking random
```

## Tuning Parameters

### Health Thresholds
```csharp
// Current values
Critical: < 0.2f  (20%)
Low:      < 0.5f  (50%)
High:     > 0.8f  (80%)

// Easy mode (more defensive)
Critical: < 0.3f
Low:      < 0.6f
High:     > 0.7f

// Hard mode (more aggressive)
Critical: < 0.15f
Low:      < 0.4f
High:     > 0.85f
```

### Probability Weights
```csharp
// Current values
Heal at low health:     0.6f  (60%)
Block at low health:    0.4f  (40%)
Block when undefended:  0.3f  (30%)
Buff at high health:    0.25f (25%)

// More defensive AI
Heal at low health:     0.8f
Block at low health:    0.6f
Block when undefended:  0.5f
Buff at high health:    0.1f

// More aggressive AI
Heal at low health:     0.4f
Block at low health:    0.2f
Block when undefended:  0.15f
Buff at high health:    0.4f
```

## Advanced Features

### Future Enhancements

**1. Player Awareness:**
```csharp
// Consider player's stats
if (player.currentEnergy > 5)
{
    // Player likely to play big cards, prioritize block
    blockChance += 0.2f;
}
```

**2. Turn Counter:**
```csharp
// First turn setup
if (turnCount == 0 && buffActions.Count > 0)
{
    return GetRandomAction(buffActions);
}
```

**3. Team Tactics:**
```csharp
// If ally is low health, play defensive
if (allyHealthPercent < 0.3f && blockActions.Count > 0)
{
    return GetRandomAction(blockActions);
}
```

**4. Pattern Breaking:**
```csharp
// Prevent repetition
if (lastAction.intent == EnemyIntent.Block && consecutiveBlocks > 2)
{
    // Force attack or heal instead
}
```

## Comparison to Old System

### Before (Random)
```csharp
public void DecideNextIntent()
{
    int idx = Random.Range(0, actionPattern.Count);
    var nextAction = actionPattern[idx];
    currentIntent = nextAction.intent;
    intentValue = nextAction.value;
}
```
**Issues:**
- ❌ No strategic thinking
- ❌ May heal at full health
- ❌ May attack when critical
- ❌ Predictable and boring

### After (Intelligent)
```csharp
public void DecideNextIntent()
{
    EnemyAction chosenAction = ChooseStrategicAction();
    currentIntent = chosenAction.intent;
    intentValue = chosenAction.value;
}
```
**Benefits:**
- ✅ Health-aware decisions
- ✅ Survival instincts at low HP
- ✅ Aggressive when safe
- ✅ Unpredictable but logical

## Files Modified

1. ✅ **EnemyData.cs**
   - Replaced `DecideNextIntent()` with intelligent version
   - Added `ChooseStrategicAction()` method
   - Added `GetRandomAction()` helper

---

**Status:** ✅ Complete and Working
**Complexity:** Strategic AI with 5-tier decision tree
**Difficulty Tuning:** Easily adjustable percentages and thresholds
**Debug Support:** Full logging of AI decisions

