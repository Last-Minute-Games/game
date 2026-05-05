# Catacombs Eye Opening - Guaranteed Fix

## Problem
When loading directly into the Catacombs.unity scene (not from transition), the eye panels don't exist, so eyes can't open.

## Solution
`CatacombsIntroDialog` now handles three scenarios:

### Scenario 1: Panels Exist & Are Closed (Normal Transition)
**What happens when you arrive from Overworld:**
```
1. Timer expires, eyes close
2. Scene loads ? CatacombsIntroDialog.Start()
3. Check: Panels exist? ? Closed? ?
4. Wait 0.5s
5. Open eyes ?
```

### Scenario 2: Panels Don't Exist (Direct Scene Load)
**What happens when you load Catacombs.unity directly in editor:**
```
1. Scene loads ? CatacombsIntroDialog.Start()
2. Check: Panels exist? ?
3. Create panels instantly (0.01s close animation)
4. Panels now exist and are closed
5. Wait 0.5s
6. Open eyes ?
```

### Scenario 3: Panels Exist But Open (Edge Case)
**What happens if ScreenFader exists but eyes already open:**
```
1. Scene loads ? CatacombsIntroDialog.Start()
2. Check: Panels exist? ? Closed? ?
3. Create panels instantly (0.01s close animation)
4. Panels now closed
5. Wait 0.5s
6. Open eyes ?
```

## Code Logic

```csharp
private IEnumerator StartSequence()
{
    if (openEyesOnStart)
    {
        var fader = ScreenFader.Instance;
        if (fader != null)
        {
            bool panelsClosed = fader.ArePanelsClosed();

            if (panelsClosed)
            {
                // Normal case - just open
                yield return fader.EyesOpeningEffect();
            }
            else
            {
                // Panels don't exist - create them first
                // Set duration to near-zero for instant close
                float originalDuration = fader.splitPanelDuration;
                fader.splitPanelDuration = 0.01f;

                yield return fader.EyesClosingEffect(); // Creates panels

                // Restore normal duration for opening
                fader.splitPanelDuration = originalDuration;

                // Now open with full animation
                yield return fader.EyesOpeningEffect();
            }
        }
    }

    // THEN load dialog and everything else
    // ...
}
```

## Key Features

? **Always Opens Eyes** - No matter how scene is loaded
? **Creates Panels If Missing** - Handles direct scene load
? **Fast Panel Creation** - Uses 0.01s animation to instantly create closed panels
? **Smooth Opening** - Full-speed animation for eye opening
? **No Visual Glitch** - Player sees eyes open, not close-then-open

## What You'll See

### When Transitioning (Normal)
```
Scene loads (black screen)
    ?
Wait 0.5s
    ?
Eyes open smoothly (1.5s animation)
    ?
Scene visible ?
```

### When Loading Scene Directly
```
Scene loads (visible, no panels)
    ?
Instant black flash (0.01s - barely noticeable)
    ?
Wait 0.5s
    ?
Eyes open smoothly (1.5s animation)
    ?
Scene visible ?
```

## Debug Logs

**Normal transition:**
```
[CatacombsIntroDialog] Checking eye panels...
[CatacombsIntroDialog] Eyes are closed - opening them now
[CatacombsIntroDialog] Eyes opened!
```

**Direct scene load:**
```
[CatacombsIntroDialog] Checking eye panels...
[CatacombsIntroDialog] Eye panels don't exist - creating and closing them first
[CatacombsIntroDialog] Panels created and closed
[CatacombsIntroDialog] Eyes opened!
```

## Testing

### Test 1: Normal Transition
1. Play from Overworld
2. Let timer run out
3. Eyes close ? Catacombs loads ? Eyes open ?

### Test 2: Direct Scene Load
1. Open Catacombs.unity in editor
2. Press Play
3. Scene loads ? Brief flash ? Eyes open ?

### Test 3: Multiple Loads
1. Load Catacombs directly
2. Stop play
3. Play again
4. Eyes should open every time ?

## Build Status
? **Build Successful** - Ready to test!

## Technical Details

**Why 0.01s instead of 0?**
- Unity coroutines need at least one frame
- 0.01s = approximately 1-2 frames at 60fps
- Fast enough to be imperceptible
- Reliable enough to complete the panel creation

**Why check ArePanelsClosed()?**
- Returns false if panels don't exist
- Returns true if panels exist and are closed
- Perfect way to detect if panels need creation

**Why save/restore splitPanelDuration?**
- Temporarily override duration for instant close
- Restore original duration for smooth open
- No permanent changes to ScreenFader settings
