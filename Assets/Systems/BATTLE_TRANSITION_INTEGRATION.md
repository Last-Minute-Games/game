# ScreenFader Scene Transition Integration

## Overview
Integrated ScreenFader eye-closing/opening animation into ALL scene transitions with a consistent "departure ? arrival" pattern. Every scene transition now features:
1. Eyes close when leaving a scene
2. Scene loads
3. Eyes open when arriving in the new scene

This creates a cinematic, consistent visual language across the entire game.

## The Universal Pattern

**All Transitions Follow This Flow:**
```
Current Scene
    ?
??? Eyes Close (Departure)
    ?
?? Scene Transition
    ?
? New Scene Loads
    ?
??? Eyes Open (Arrival)
    ?
New Scene (Playing)
```

## Transition Scenarios

### 1. Overworld ? Catacombs (Timer Expiration)
**Trigger:** Clock timer runs out in Overworld

**Flow:**
1. ? Timer reaches zero
2. ?? "YOU DIED!" message displays
3. ??? Eyes close (departure from Overworld)
4. ?? Load Catacombs scene
5. ??? Eyes open (arrival in Catacombs)
6. ??? Player can explore Catacombs

**Why:** Creates a sense of being transported to the underworld/liminal space when time runs out.

### 2. Catacombs ? Battle (Door Interaction)
**Trigger:** Player interacts with door at end of Catacombs

**Flow:**
1. ?? Player presses E at door
2. ?? Door sound plays
3. ??? Eyes close (departure from Catacombs)
4. ?? Load Battle scene
5. ??? Eyes open (arrival in Nether/Battle dimension)
6. ?? Battle begins

**Why:** Voluntary choice to enter battle; eyes open to show conscious arrival in new dimension.

### 3. Battle ? Overworld (Battle Completion)
**Trigger:** Player wins or loses battle

**Flow:**
1. ?? Battle ends (win/lose)
2. ?? Victory/defeat message shows
3. ? 3 second delay
4. ??? Eyes close (departure from Battle)
5. ?? Load Overworld scene
6. ??? Eyes open (waking up back home)
7. ?? Player returns to Overworld

**Why:** Always return to "reality" (Overworld) with eyes opening, like waking from a dream.

## Changes Made

### 1. ClockTimer.cs - Timer-Based Transitions
**File:** `Assets\UIs\UIs\Clock\ClockTimer.cs`

**Changes:**
- Simplified transition logic
- Removed battle-specific detection
- Always sets `shouldOpenEyesOnSceneLoad = true`
- Uses consistent `TransitionToSceneKeepPanelsClosed()`

**Code:**
```csharp
// Eyes should always open in the destination scene
screenFader.shouldOpenEyesOnSceneLoad = true;
yield return StartCoroutine(screenFader.TransitionToSceneKeepPanelsClosed(sceneToLoad));
```

**Why:** Simple, consistent pattern for all timer-based transitions.

### 2. SceneTransitionDoor.cs - Door-Based Transitions
**File:** `Assets\Systems\Teleport\SceneTransitionDoor.cs`

**Changes:**
- Removed battle detection and flag system
- Simplified to always use same pattern
- Always sets `shouldOpenEyesOnSceneLoad = true`
- Closes eyes, transitions, opens eyes

**Code:**
```csharp
// Close eyes
yield return fader.EyesClosingEffect();

// Eyes should always open in the destination scene
fader.shouldOpenEyesOnSceneLoad = true;

// Use the keep-panels-closed transition to maintain state
yield return fader.TransitionToSceneKeepPanelsClosed(sceneName);
```

**Why:** Universal door behavior - consistent across all door types and destinations.

### 3. BattleManager.cs - Battle Scene Entry
**File:** `Assets\Scripts\GameItems\BattleManager.cs`

**Changes:**
- Removed voluntary/involuntary entry detection
- Removed flag checking system
- Simplified to always allow eyes to open
- Consistent with all other scenes

**Code:**
```csharp
// Eyes should open when entering battle scene (consistent arrival animation)
var fader = ScreenFader.Instance;
if (fader != null)
{
    Debug.Log("[BattleManager] Battle scene loaded - eyes will open");
    fader.shouldOpenEyesOnSceneLoad = true;
}
```

**Why:** Battle scene is no different from any other scene - eyes always open on arrival.

### 4. RoundManager.cs - Return from Battle
**File:** `Assets\Scripts\GameItems\RoundManager.cs`

**Changes:**
- Already had correct implementation
- Ensures eyes close before leaving battle
- Sets flag to open eyes in Overworld
- Uses `TransitionToSceneKeepPanelsClosed()`

**Code:**
```csharp
// Close eyes if they aren't already closed
if (!fader.ArePanelsClosed())
{
    yield return fader.EyesClosingEffect();
}

// Set flag to open eyes when Overworld loads
fader.shouldOpenEyesOnSceneLoad = true;

yield return fader.TransitionToSceneKeepPanelsClosed("Overworld");
```

**Why:** Consistent return pattern - always wake up in Overworld.

## Complete Game Flow Example

```
?? OVERWORLD
   Player explores, timer ticks
       ?
   ? Timer expires
       ?
   ?? "YOU DIED!" message
       ?
   ?????? Eyes close
       ?

??? CATACOMBS
       ?
   ?????? Eyes open
       ?
   ?? Player walks through catacombs
       ?
   ?? Reaches door, presses E
       ?
   ?????? Eyes close
       ?

?? BATTLE (NETHER)
       ?
   ?????? Eyes open
       ?
   ?? Battle gameplay
       ?
   ?? Win or ?? Lose
       ?
   ?? "YOU WIN" or "YOU LOSE"
       ?
   ?????? Eyes close
       ?

?? OVERWORLD
       ?
   ?????? Eyes open
       ?
   ?? Cycle repeats
```

## Technical Implementation

### ScreenFader Methods

**EyesClosingEffect():**
- Creates two black panels (top and bottom)
- Animates panels sliding toward center
- Duration: `splitPanelDuration` (default 1.5s)
- Result: Screen covered by black panels (eyes closed)

**EyesOpeningEffect():**
- Animates panels sliding away from center
- Duration: `splitPanelDuration` (default 1.5s)
- Destroys panels after animation
- Result: Screen clear (eyes open)

**TransitionToSceneKeepPanelsClosed():**
- Loads new scene asynchronously
- Maintains current panel state
- Doesn't auto-open or auto-close eyes
- Waits for scene load to complete

**shouldOpenEyesOnSceneLoad:**
- Boolean flag checked in `OnSceneLoaded()` callback
- If `true`: Plays `EyesOpeningEffect()` after scene loads
- If `false`: Leaves panels in current state
- Reset to `false` after use

### Scene Load Flow

1. **Current Scene:**
   - Trigger transition (timer/door/battle end)
   - Call `EyesClosingEffect()`
   - Wait for eyes to close
   - Set `shouldOpenEyesOnSceneLoad = true`
   - Call `TransitionToSceneKeepPanelsClosed(sceneName)`

2. **Scene Transition:**
   - Unity loads new scene asynchronously
   - ScreenFader persists (DontDestroyOnLoad)
   - Panels remain closed during load

3. **New Scene:**
   - Scene finishes loading
   - `OnSceneLoaded()` callback fires
   - Check `shouldOpenEyesOnSceneLoad`
   - If true, play `EyesOpeningEffect()`
   - Reset flag to `false`

## Design Philosophy

### Why Universal Eye Opening?

**Narrative Consistency:**
- Every scene is a "place" you arrive at
- Opening eyes = arriving/waking up/becoming aware
- Closing eyes = leaving/departing/transitioning
- Creates consistent spatial awareness

**Player Orientation:**
- Eyes opening helps players realize they've arrived
- Provides a moment to take in new environment
- Natural "reveal" of each scene
- Reduces disorientation from scene changes

**Cinematic Quality:**
- Professional transition effect
- Feels intentional and polished
- Adds drama to every transition
- Better than instant cuts

**Technical Simplicity:**
- One rule: eyes always open on arrival
- No special cases or conditions
- Easy to debug and maintain
- Predictable behavior

### No Special Cases

**Removed Complexity:**
- ? No voluntary/involuntary detection
- ? No battle-specific logic
- ? No flag management system
- ? No scene type checking

**Added Simplicity:**
- ? Same pattern everywhere
- ? Predictable behavior
- ? Easy to understand
- ? Maintainable code

## Benefits

? **Consistency** - Same behavior across all transitions
? **Simplicity** - One pattern, no special cases
? **Cinematic** - Professional, polished feel
? **Reliable** - Works the same every time
? **Maintainable** - Easy to understand and debug
? **Scalable** - New scenes automatically work
? **Player-Friendly** - Clear visual feedback for transitions

## Testing Checklist

### Overworld ? Catacombs
- [ ] Timer expires in Overworld
- [ ] "YOU DIED!" message appears
- [ ] Eyes close smoothly
- [ ] Catacombs scene loads
- [ ] Eyes open smoothly
- [ ] Player can move in Catacombs

### Catacombs ? Battle
- [ ] Walk to door in Catacombs
- [ ] Press E to interact
- [ ] Door sound plays
- [ ] Eyes close smoothly
- [ ] Battle scene loads
- [ ] Eyes open smoothly
- [ ] Battle starts normally

### Battle ? Overworld
- [ ] Complete battle (win or lose)
- [ ] Victory/defeat message shows
- [ ] 3 second delay
- [ ] Eyes close smoothly
- [ ] Overworld scene loads
- [ ] Eyes open smoothly
- [ ] Player is back in Overworld

### Edge Cases
- [ ] Multiple transitions in succession work
- [ ] No visual glitches or flickering
- [ ] Smooth animation timing
- [ ] No black screen stuck issues
- [ ] ScreenFader persists across scenes
- [ ] Works after multiple game sessions

## Future Enhancements

Possible improvements:
- Variable eye opening/closing speeds based on context
- Different transition styles (fade, wipe, etc.)
- Sound effects for eyes closing/opening
- Particle effects during transitions
- Customizable per-door transition effects
- Tutorial messages during first transitions
- Achievement triggers for specific transitions
