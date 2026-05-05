# Scene-Managed Eye Opening Fix

## Problem
Eyes weren't opening when transitioning to Catacombs or Battle scenes because the `ScreenFader.OnSceneLoaded` callback had timing issues and wasn't reliably opening eyes.

## Solution
**Move eye opening control to individual scene managers** instead of relying on the centralized `OnSceneLoaded` callback.

## What Changed

### 1. CatacombsIntroDialog.cs
**Now handles eye opening when Catacombs scene loads**

**Added:**
- `openEyesOnStart` toggle (default: true)
- `eyeOpenDelay` setting (default: 0.5s)
- Coroutine-based `StartSequence()` that:
  1. Checks if eyes are closed
  2. Waits for a small delay (scene settling)
  3. Opens eyes with `EyesOpeningEffect()`
  4. Then proceeds with normal intro dialog

**Code:**
```csharp
private IEnumerator StartSequence()
{
    // First, handle eye opening if needed
    if (openEyesOnStart)
    {
        var fader = ScreenFader.Instance;
        if (fader != null && fader.ArePanelsClosed())
        {
            Debug.Log("[CatacombsIntroDialog] Eyes are closed - opening them now");

            if (eyeOpenDelay > 0)
            {
                yield return new WaitForSeconds(eyeOpenDelay);
            }

            yield return fader.EyesOpeningEffect();
            Debug.Log("[CatacombsIntroDialog] Eyes opened!");
        }
    }

    // Then check if we should play the intro dialog
    // ... rest of original Start() logic
}
```

### 2. BattleManager.cs
**Now handles eye opening when Battle scene loads**

**Added:**
- `openEyesOnStart` toggle (default: true)
- `eyeOpenDelay` setting (default: 0.5s)
- Eye opening logic at the start of `Start()` coroutine
- Same pattern as CatacombsIntroDialog

**Code:**
```csharp
private IEnumerator Start()
{
    // First, handle eye opening if needed
    if (openEyesOnStart)
    {
        var fader = ScreenFader.Instance;
        if (fader != null && fader.ArePanelsClosed())
        {
            Debug.Log("[BattleManager] Eyes are closed - opening them now");

            if (eyeOpenDelay > 0)
            {
                yield return new WaitForSeconds(eyeOpenDelay);
            }

            yield return fader.EyesOpeningEffect();
            Debug.Log("[BattleManager] Eyes opened!");
        }
    }

    // ... rest of initialization
}
```

### 3. ScreenFader.cs - OnSceneLoaded()
**Disabled automatic eye opening**

**Changed:**
- No longer calls `EyesOpeningEffect()` in `OnSceneLoaded`
- Just resets the `shouldOpenEyesOnSceneLoad` flag
- Leaves eye opening to scene managers

**Why:** Prevents conflicts and timing issues. Each scene now has full control.

## Benefits

? **Reliable Timing** - Eyes open after scene is fully loaded and settled
? **Scene Control** - Each scene controls its own eye opening
? **Debuggable** - Clear log messages show which scene is opening eyes
? **Configurable** - Can disable/adjust eye opening per scene
? **No Race Conditions** - No dependency on callback timing

## Flow Comparison

### Before (Broken)
```
1. Timer expires ? Eyes close
2. Scene loads
3. OnSceneLoaded callback fires
4. Check shouldOpenEyesOnSceneLoad
5. ? Timing issue - might not work
6. Eyes might not open
```

### After (Working)
```
1. Timer expires ? Eyes close
2. Scene loads
3. Scene's Start() runs
4. Scene checks if eyes are closed
5. ? Scene opens eyes directly
6. Eyes open reliably!
```

## Expected Behavior

### Overworld ? Catacombs (Timer)
```
1. ? Timer expires
2. ??? Eyes close
3. ?? Scene loads
4. ??? CatacombsIntroDialog.Start()
5. ?? Wait 0.5s (scene settling)
6. ??? Eyes open ?
7. ?? Intro dialog plays (if not seen)
```

### Catacombs ? Battle (Door)
```
1. ?? Press E at door
2. ??? Eyes close
3. ?? Scene loads
4. ?? BattleManager.Start()
5. ?? Wait 0.5s (scene settling)
6. ??? Eyes open ?
7. ?? Battle initializes
```

### Battle ? Overworld (Return)
```
1. ?? Battle ends
2. ??? Eyes close (or already closed)
3. ?? Scene loads
4. ?? OverworldWakeUpCutscene.Start()
5. ??? Eyes open ? (already handled)
6. ?? Wake-up sequence plays
```

## Inspector Settings

Both `CatacombsIntroDialog` and `BattleManager` now have:

**Screen Transition:**
- ?? **Open Eyes On Start** - Enable/disable automatic eye opening
- **Eye Open Delay** - Seconds to wait before opening (default: 0.5)

You can:
- Disable eye opening for specific scenes
- Adjust timing if eyes open too early/late
- Debug by watching console logs

## Debug Logs

Look for these in the console:

```
[CatacombsIntroDialog] Eyes are closed - opening them now
[CatacombsIntroDialog] Eyes opened!

[BattleManager] Eyes are closed - opening them now
[BattleManager] Eyes opened!

[ScreenFader] Eyes opened!
```

## Troubleshooting

**Eyes not opening?**
- Check `openEyesOnStart` is enabled in Inspector
- Check ScreenFader is in the scene
- Check console for "[SceneName] Eyes are closed" message

**Eyes open too early?**
- Increase `eyeOpenDelay` in Inspector

**Eyes open too late?**
- Decrease `eyeOpenDelay` in Inspector

**Scene doesn't have eye opening?**
- Add the logic pattern from CatacombsIntroDialog or BattleManager

## Build Status
? **Build Successful** - Ready to test!

## Testing

1. **Overworld to Catacombs:**
   - Let timer run out
   - Watch eyes close
   - Watch Catacombs load
   - **Eyes should open after 0.5s** ?

2. **Catacombs to Battle:**
   - Walk to door, press E
   - Watch eyes close
   - Watch Battle load
   - **Eyes should open after 0.5s** ?

3. **Battle to Overworld:**
   - Win/lose battle
   - Watch eyes close
   - Watch Overworld load
   - **Eyes should open** ? (handled by OverworldWakeUpCutscene)
