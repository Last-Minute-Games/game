# ScreenFader Eyes Opening Fix

## Problem
Eyes were not opening when transitioning from Overworld to Catacombs (or any other scene). The panels would close but stay closed in the destination scene.

## Root Cause
The `OnSceneLoaded` callback in ScreenFader had a logic issue:

1. It was checking `if (!isTransitioning)` first to disable the fadePanel
2. Then checking `shouldOpenEyesOnSceneLoad` 
3. But since `TransitionToSceneKeepPanelsClosed` sets `isTransitioning = true` and the scene loads WHILE transitioning is still true, the fadePanel was being disabled before the eyes could open

## The Fix

### Changed in ScreenFader.cs - OnSceneLoaded()

**Before:**
```csharp
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // If not transitioning, ensure the fade overlay isn't accidentally left active
    if (fadePanel != null && !isTransitioning)
    {
        fadePanel.gameObject.SetActive(false);
    }

    // Check if we should open eyes on this scene load
    if (shouldOpenEyesOnSceneLoad)
    {
        Debug.Log("[ScreenFader] Scene loaded - opening eyes!");
        shouldOpenEyesOnSceneLoad = false; // Reset flag
        StartCoroutine(EyesOpeningEffect());
    }
    else
    {
        // Only fade in if we just came from a transition
        if (isTransitioning)
        {
            StartCoroutine(FadeIn());
        }
    }
}
```

**After:**
```csharp
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Check if we should open eyes on this scene load (must check BEFORE clearing transition flag)
    if (shouldOpenEyesOnSceneLoad)
    {
        Debug.Log("[ScreenFader] Scene loaded - opening eyes!");
        shouldOpenEyesOnSceneLoad = false; // Reset flag
        isTransitioning = false; // Clear transition flag before starting eyes opening
        StartCoroutine(EyesOpeningEffect());
    }
    else
    {
        // Only fade in if we just came from a transition
        if (isTransitioning)
        {
            StartCoroutine(FadeIn());
        }
        else if (fadePanel != null)
        {
            // If not transitioning, ensure the fade overlay isn't accidentally left active
            fadePanel.gameObject.SetActive(false);
        }
    }
}
```

## Key Changes

1. **Priority Check First**: Now checks `shouldOpenEyesOnSceneLoad` BEFORE doing anything with `isTransitioning`
2. **Clear Flag Early**: Sets `isTransitioning = false` before starting `EyesOpeningEffect()` so the effect runs cleanly
3. **Conditional Cleanup**: Only disables fadePanel if NOT opening eyes and NOT transitioning

## Expected Behavior Now

### Overworld ? Catacombs (Timer)
```
1. Timer expires
2. Eyes close (EyesClosingEffect)
3. Set shouldOpenEyesOnSceneLoad = true
4. TransitionToSceneKeepPanelsClosed("Catacombs")
5. Catacombs scene loads
6. OnSceneLoaded fires
7. Checks shouldOpenEyesOnSceneLoad ? TRUE
8. Clears flag, sets isTransitioning = false
9. Eyes open (EyesOpeningEffect) ?
```

### Catacombs Door ? Battle
```
1. Press E at door
2. Eyes close (EyesClosingEffect)
3. Set shouldOpenEyesOnSceneLoad = true
4. TransitionToSceneKeepPanelsClosed("BattleScene")
5. Battle scene loads
6. OnSceneLoaded fires
7. Checks shouldOpenEyesOnSceneLoad ? TRUE
8. Clears flag, sets isTransitioning = false
9. Eyes open (EyesOpeningEffect) ?
```

### Battle ? Overworld
```
1. Battle ends
2. Eyes close (EyesClosingEffect or already closed)
3. Set shouldOpenEyesOnSceneLoad = true
4. TransitionToSceneKeepPanelsClosed("Overworld")
5. Overworld scene loads
6. OnSceneLoaded fires
7. Checks shouldOpenEyesOnSceneLoad ? TRUE
8. Clears flag, sets isTransitioning = false
9. Eyes open (EyesOpeningEffect) ?
```

## Testing

### You should now see:
- ? Eyes close when timer expires in Overworld
- ? Eyes OPEN when Catacombs loads
- ? Eyes close when using door in Catacombs
- ? Eyes OPEN when Battle scene loads
- ? Eyes close when battle ends
- ? Eyes OPEN when Overworld loads

### Debug Logs to Look For
```
[ScreenFader] Split panels created
[ScreenFader] Starting transition to Catacombs - keeping panels closed
[ScreenFader] Scene loaded - opening eyes!
[ScreenFader] Eyes opened!
```

## Build Status
? **Build Successful** - Ready to test!
