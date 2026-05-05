# Clock Timer Progression System

## Overview
Modified ClockTimer to have progressive timeout behavior:
- **First timeout**: Go to Catacombs
- **Second timeout onwards**: Skip Catacombs, go straight to Battle

## Changes Made

### New Fields Added

**In Inspector:**
- **Overworld Timeout Scene Name** - "Catacombs" (first time destination)
- **Overworld Timeout Battle Scene Name** - "BattleScene" (second+ time destination)

**Internal:**
- `FIRST_TIMEOUT_FLAG` = "clock.first.timeout.complete" (tracks if player has visited Catacombs)

### How It Works

#### First Time Timer Expires
```
1. Player in Overworld
2. Timer hits 0
3. Check flag "clock.first.timeout.complete" ? NOT SET
4. Load Catacombs scene ?
5. SET flag "clock.first.timeout.complete"
6. Player explores Catacombs
7. Player uses door ? Battle scene
```

#### Second Time Timer Expires
```
1. Player back in Overworld
2. Timer hits 0
3. Check flag "clock.first.timeout.complete" ? IS SET
4. Skip Catacombs
5. Load BattleScene directly ?
6. Player fights battle
7. Return to Overworld
```

#### Third Time and Beyond
```
Same as second time - always goes straight to Battle
```

## Game Flag

**Flag Name:** `clock.first.timeout.complete`

**Set When:** First time timer expires in Overworld  
**Purpose:** Track whether player has already visited Catacombs via timeout  
**Persistent:** Yes (saved with GameFlags)  

**To Reset (for testing):**
```csharp
GameFlags.RemoveFlag("clock.first.timeout.complete");
```

## Flow Diagram

```
???????????????
?  OVERWORLD  ?
?  (playing)  ?
???????????????
       ?
       ?
   ? Timer = 0
       ?
       ?
   Check Flag?
       ?
    ???????
    ?     ?
   NO    YES
    ?     ?
    ?     ?
?????????? ??????????????
?Catacombs? ?BattleScene ?
?(1st)   ? ?(2nd+)      ?
?????????? ??????????????
    ?            ?
    ??? Door     ??? Fight
    ?            ?
    ?            ?
??????????????  ?
?BattleScene ?  ?
??????????????  ?
      ?         ?
      ???????????
           ?
    ??/?? Battle Ends
           ?
           ?
      ???????????
      ?Overworld?
      ?(return) ?
      ???????????
```

## Configuration in Unity

### ClockTimer Component Settings

**Overworld Timeout:**
- Scene Name: `"Overworld"`
- Timeout Scene Name: `"Catacombs"` ? First time destination
- Timeout Battle Scene Name: `"BattleScene"` ? Second+ time destination

**The system automatically:**
- Detects when in Overworld
- Checks if first timeout has occurred
- Routes to appropriate scene
- Sets flag after first timeout

## Example Play Session

### Day 1
```
Overworld ? Timer expires ? Catacombs (first time)
  ? Explore catacombs
  ? Find door
  ? Enter battle
  ? Win/lose
  ? Return to Overworld
```

### Day 2
```
Overworld ? Timer expires ? BattleScene (skip Catacombs!)
  ? Fight battle
  ? Win/lose
  ? Return to Overworld
```

### Day 3+
```
Overworld ? Timer expires ? BattleScene (always skip Catacombs)
  ? Fight battle
  ? Win/lose
  ? Return to Overworld
```

## Benefits

? **First-time Experience** - Players explore Catacombs once  
? **Streamlined Gameplay** - Skip Catacombs on repeat visits  
? **Faster Pacing** - Direct to battle after first time  
? **No Backtracking** - Don't need to walk through empty Catacombs  
? **Persistent** - Flag saves with game state  

## Debug Logs

**First timeout:**
```
[ClockTimer] Overworld timer ended - FIRST TIME - going to Catacombs: 'Catacombs'.
[ClockTimer] Set flag 'clock.first.timeout.complete' - next timeout will go to battle
```

**Second+ timeout:**
```
[ClockTimer] Overworld timer ended - SECOND+ TIME - going straight to Battle: 'BattleScene'.
```

## Testing

### Test First Timeout
1. Start in Overworld
2. Make sure flag is cleared: `GameFlags.RemoveFlag("clock.first.timeout.complete")`
3. Let timer run out
4. Should go to Catacombs ?
5. Flag should be set ?

### Test Second Timeout
1. Return to Overworld (from battle)
2. Flag should still be set
3. Let timer run out
4. Should go directly to BattleScene ?
5. Should skip Catacombs ?

### Reset for Testing
To test the first timeout again:
```csharp
GameFlags.RemoveFlag("clock.first.timeout.complete");
```

## Notes

- Players can still manually visit Catacombs via doors/teleports
- This only affects the **timer timeout** behavior
- The flag persists across game sessions (saved)
- First timeout always shows Catacombs for narrative introduction
- Subsequent timeouts prioritize gameplay speed

## Future Enhancements

Possible improvements:
- Add configuration for how many times to show Catacombs
- Add different Catacombs variations for repeat visits
- Add special events in Catacombs on certain days
- Allow Catacombs to be re-enabled based on story flags
