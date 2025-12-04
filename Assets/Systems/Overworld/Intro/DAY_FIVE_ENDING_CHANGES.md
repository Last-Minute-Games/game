# Day.Five Ending Trigger - Final Implementation Summary

## Overview

The day.five ending system has been fully implemented. When the timer runs out on day.five, the screen fades to black (eyes close) and stays black, then transitions directly to the ending scene without showing the "YOU DIED!" message.

## Changes Made

### 1. **OverworldWakeUpCutscene.cs**
- **Removed day.five check from `Start()` method** - day.five no longer triggers ending at wake-up
- **Added comment** explaining day.five triggers at END of day via ClockTimer
- **Updated `GetNextDayFlag()`** - day.five ? day.six to allow day.five wake-up dialogue to play

### 2. **ClockTimer.cs**
- **Added "Day Five Ending" header section** with EndTransition component references
- **Added `EndTransition endTransition` field** - component to trigger when day.five ends
- **Added `autoFindEndTransition` bool** - automatically finds EndTransition if not assigned
- **Modified `FadeMessageThenTransition()`**:
  - Eyes close effect plays first (as normal)
  - **Day.five check moved BEFORE "YOU DIED!" message**
  - When day.five detected:
    - Skips "YOU DIED!" message entirely
    - Stops warning audio
    - Sets `start.ending` flag
    - Waits 1 second (dramatic pause with black screen)
    - Triggers `EndTransition.TriggerEndTransition()`
    - Exits coroutine (EndTransition handles scene load)
  - Non-day.five days continue with normal "YOU DIED!" message and scene transition
- **Removed obsolete code**:
  - Removed `PlayDayFiveCutscene()` method
  - Removed `dayfive.cutscene.played` flag check
  - Removed DayFiveCutscene scene loading logic

### 3. **EndTransition.cs**
- **Added eyes-already-closed detection** in `TransitionToEnding()`
- **Checks `screenFader.ArePanelsClosed()`** before calling eyes closing effect
- **Skips eyes closing** if panels are already closed (prevents double closing)
- **Logs appropriate messages** for debugging

### 4. **ScreenFader.cs**
- **Added `ArePanelsClosed()` method** - checks if split panels are in closed position
- Returns `true` if both topPanel and bottomPanel are at anchoredPosition.y ? 0

## Game Flow

### Day Five Complete Flow

**Morning (Wake Up):**
1. Player finishes day.four (battle/timer)
2. `day.five` flag is set
3. Scene loads to Overworld
4. `OverworldWakeUpCutscene` detects day.five
5. **Plays day-specific wake-up dialogue** (just like days 2-4)
6. Clock reconstruction plays
7. Timer starts normally

**Throughout Day:**
- Player explores, collects evidence, talks to NPCs
- Timer counts down normally

**End of Day (Timer Runs Out):**
1. Timer reaches 0
2. Clock breaking animation plays
3. **Eyes closing effect** (screen fades to black)
4. **Screen stays black** (no "YOU DIED!" message)
5. 1 second dramatic pause
6. `EndTransition.TriggerEndTransition()` is called:
   - Checks if eyes are already closed ?
   - **Skips second eyes closing** (prevents double effect)
   - Pauses environment (player input, clock timer, journal)
   - Transitions to ending scene with panels closed
7. Ending scene loads with eyes closed
8. `EndingCutsceneManager` opens eyes
9. Ending dialogue plays based on flags

## Sequence Diagram

```
ClockTimer.FadeMessageThenTransition()
?
??> Eyes Closing Effect (ScreenFader)
?   ??> Panels slide in, screen black
?
??> Check day.five flag
?   ?
?   ??> If day.five:
?   ?   ??> Stop audio
?   ?   ??> Set start.ending flag
?   ?   ??> Wait 1 second (black screen)
?   ?   ??> EndTransition.TriggerEndTransition()
?   ?       ??> Pause environment
?   ?       ??> Check if eyes closed (YES - skip)
?   ?       ??> Load ending scene (eyes stay closed)
?   ?           ??> EndingCutsceneManager opens eyes
?   ?
?   ??> If NOT day.five:
?       ??> Show "YOU DIED!" message
?       ??> Fade message out
?       ??> Load next scene normally
```

## Debug Logs Sequence (Day.Five)

When day.five timer runs out, you should see:
```
[ClockTimer] Timer reached 0! Starting death sequence...
[ScreenFader] Eyes closing effect starting...
[ScreenFader] Eyes closed!
[ClockTimer] ?? Day five timer ended - triggering ending sequence (skipping death message)
[ClockTimer] Setting start.ending flag for day.five
[ClockTimer] Triggering EndTransition for day.five
[EndTransition] Flag 'start.ending' exists - starting transition
[EndTransition] Starting transition to ending scene
[EndTransition] Pausing environment and player
[EndTransition] Eyes already closed - skipping closing effect  ? KEY: Single eyes closing!
[EndTransition] Preparing transition to scene 'Ending'
[EndTransition] Calling ScreenFader.TransitionToSceneKeepPanelsClosed('Ending')
[ScreenFader] Starting transition to Ending - keeping panels closed
[EndingCutsceneManager] Opening eyes before ending
[ScreenFader] Eyes opened!
```

## Configuration Required

### ClockTimer (Unity Inspector):
- ? **End Transition**: Assign `EndTransition` component (or enable `Auto Find End Transition`)
- ? **Ending Scene Name**: Can be left empty (not used for day.five)
- ? **Next Scene Name**: Set to "Overworld" (used for days 1-4)

### OverworldWakeUpCutscene (Unity Inspector):
- ? **End Transition**: Assign `EndTransition` component (for day.six fallback)
- ? **Day Wake Up Dialogues**: Add entry for day.five with wake-up dialogue graph

### EndTransition (Unity Inspector):
- ? **Screen Fader**: Assign ScreenFader component
- ? **Ending Scene Name**: Set to "Ending"
- ? **Required Flag Name**: Set to "start.ending"
- ? **Delay Before Transition**: 1f (already includes pause from ClockTimer)

## Testing

### Manual Test:
1. Set flag: `GameFlags.SetFlag("day.five")`
2. Load Overworld: `SceneManager.LoadScene("Overworld")`
3. Day.five wake-up dialogue should play
4. Use debug key `K` to reduce timer to 0
5. Verify:
   - ? Eyes close once (not twice)
   - ? Screen stays black
   - ? No "YOU DIED!" message appears
   - ? Ending scene loads after ~1 second
   - ? Eyes open in ending scene
   - ? Ending dialogue plays

### Natural Test:
1. Play through days 1-4 normally
2. On day four, let clock run out
3. Day.five flag is set
4. Wake up on day.five (dialogue plays)
5. Play through day.five
6. Let timer run out
7. Verify ending sequence

## Differences from Days 1-4

| Aspect | Days 1-4 | Day.Five |
|--------|----------|----------|
| **Wake Up** | Normal dialogue/cutscene | Normal dialogue (like days 2-4) |
| **Gameplay** | Timer runs, player explores | Timer runs, player explores |
| **Timer Runs Out** | Eyes close ? "YOU DIED!" ? Overworld | Eyes close ? Black screen ? Ending |
| **Death Message** | ? Shows "YOU DIED!" | ? Skipped |
| **Eyes Closing** | Once | **Once** (fixed!) |
| **Scene Transition** | Returns to Overworld | Goes to Ending scene |
| **Post-Transition** | Clock reconstruction | Ending cutscene plays |

## Related Files Modified
- `Assets/Systems/Overworld/Intro/OverworldWakeUpCutscene.cs`
- `Assets/Systems/UIs/Clock/ClockTimer.cs`
- `Assets/Systems/Overworld/EndTransition.cs`
- `Assets/Systems/ScreenFader.cs` (ArePanelsClosed method confirmed present)

## Notes

- **No "YOU DIED!" message on day.five** - This is intentional since the player is progressing to the ending, not dying
- **Screen stays black** between eyes closing and ending scene load - This creates a dramatic pause
- **Single eyes closing effect** - EndTransition detects that eyes are already closed and skips redundant closing
- **Day.six still supported** - If day.six flag is set at wake-up, ending triggers immediately (alternative path)
- **Flag progression**: `day.one` ? `day.two` ? `day.three` ? `day.four` ? `day.five` ? (timer ends) ? ending
