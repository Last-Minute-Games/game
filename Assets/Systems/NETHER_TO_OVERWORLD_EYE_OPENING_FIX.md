# Nether to Overworld Eye Opening Fix

## Problem
When transitioning from the Nether (battle scene) back to the Overworld after winning or losing a battle, the eye-opening animation (split panels opening) was not playing. The screen would stay black with the panels closed.

## Root Cause
The issue was a timing problem with the `shouldOpenEyesOnSceneLoad` flag:

1. **RoundManager** sets the flag before transitioning:
   ```csharp
   fader.shouldOpenEyesOnSceneLoad = true;
   yield return fader.TransitionToSceneKeepPanelsClosed("Overworld");
   ```

2. **ScreenFader.OnSceneLoaded()** was clearing the flag immediately when the scene loaded:
   ```csharp
   if (shouldOpenEyesOnSceneLoad)
   {
       shouldOpenEyesOnSceneLoad = false; // ? CLEARED TOO EARLY!
       isTransitioning = false;
   }
   ```

3. **ClockTimer.ReconstructClock()** would check the flag later to open eyes, but it was already cleared:
   ```csharp
   if (screenFader != null && screenFader.shouldOpenEyesOnSceneLoad)
   {
       // This never executed because flag was already false!
       yield return StartCoroutine(screenFader.EyesOpeningEffect());
   }
   ```

## Solution

### Fix 1: Don't Clear Flag in OnSceneLoaded
**File:** `Assets/Systems/ScreenFader.cs`

Changed `OnSceneLoaded()` to NOT clear the `shouldOpenEyesOnSceneLoad` flag, allowing scene managers to check it and clear it themselves:

```csharp
if (shouldOpenEyesOnSceneLoad)
{
    Debug.Log("[ScreenFader] Scene loaded with shouldOpenEyesOnSceneLoad=true - scene will handle eye opening");
    // Don't clear the flag - let scene managers handle it
    isTransitioning = false;
}
```

### Fix 2: More Robust Eye Opening Check
**File:** `Assets/UIs/UIs/Clock/ClockTimer.cs`

Enhanced the eye-opening check in `ReconstructClock()` to open eyes if EITHER the flag is set OR the panels are actually closed:

```csharp
if (screenFader != null)
{
    // Check if eyes should open based on flag OR if panels are actually closed
    bool shouldOpenEyes = screenFader.shouldOpenEyesOnSceneLoad || screenFader.ArePanelsClosed();

    if (shouldOpenEyes)
    {
        LogDebug("Playing eyes opening before clock reconstruction");
        screenFader.shouldOpenEyesOnSceneLoad = false; // Clear the flag
        yield return StartCoroutine(screenFader.EyesOpeningEffect());
    }
}
```

This makes the system more robust - even if the flag gets cleared accidentally, the eyes will still open if the panels are detected as closed.

## Flow After Fix

1. **Battle Victory/Defeat** ? RoundManager closes eyes and sets flag
2. **Transition** ? Scene loads with panels closed and flag set
3. **OnSceneLoaded** ? Flag remains set (not cleared)
4. **PlayDaySpecificWakeUpDialogue** ? Calls ClockTimer.ReconstructClock()
5. **ReconstructClock** ? Checks flag OR panel state ? Opens eyes ? Clears flag
6. **Clock Animation** ? Plays reconstruction animation
7. **Player Control Restored** ? Normal gameplay resumes

## Testing
- Win a battle in the Nether ? eyes should open smoothly when returning to Overworld
- Lose a battle in the Nether ? eyes should open smoothly when returning to Overworld
- Works for all days (day.two, day.three, day.four, day.five)

## Related Files
- `Assets/Systems/ScreenFader.cs`
- `Assets/UIs/UIs/Clock/ClockTimer.cs`
- `Assets/Scripts/GameItems/RoundManager.cs`
- `Assets/Systems/Overworld/Intro/OverworldWakeUpCutscene.cs`
