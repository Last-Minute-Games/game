# CRITICAL FIX: Catacombs Eyes Not Opening

## The Problem

The eyes aren't opening in Catacombs because **the `CatacombsIntroDialog` component doesn't exist in the Catacombs scene!**

Looking at your logs:
```
[CatacombsIntroDialog] ===== START SEQUENCE BEGIN =====
[CatacombsIntroDialog] Eye opening complete, proceeding to dialog check...
```

It jumps immediately from "BEGIN" to "complete" without running any of the eye opening code. This means `openEyesOnStart` is FALSE or not set.

## The Fix

### Step 1: Open Catacombs Scene
1. In Unity, double-click `Catacombs.unity` to open it

### Step 2: Create the GameObject
1. In the Hierarchy, right-click ? Create Empty
2. Name it `CatacombsIntroDialog`

### Step 3: Add the Component
1. Select the `CatacombsIntroDialog` GameObject
2. In Inspector, click "Add Component"
3. Search for "CatacombsIntroDialog"
4. Add it

### Step 4: Configure the Component
In the Inspector, you should see:

**Dialogue:**
- Dialog Behaviour: (leave empty, auto-find enabled)
- Intro Dialog Graph: (leave empty, will load from Resources)

**Auto-Find Components:**
- ?? Auto Find Dialog Behaviour (checked)
- ?? Auto Find Player (checked)

**Screen Transition:**
- ?? **Open Eyes On Start** (MUST BE CHECKED!) ? THIS IS CRITICAL
- Eye Open Delay: 0.5

### Step 5: Save the Scene
1. File ? Save (or Ctrl+S)

### Step 6: Test
1. Play from Overworld
2. Let timer run out
3. Eyes should close ? Catacombs loads ? Eyes should OPEN! ?

## Why This Fixes It

**Before:**
- CatacombsIntroDialog component doesn't exist in scene
- Nothing calls eye opening code
- Eyes stay closed forever

**After:**
- CatacombsIntroDialog exists in scene
- Start() runs automatically when scene loads
- Eyes open properly

## Alternative: Quick Test

If you just want to test without setting up the GameObject:

### Temporary Fix in ScreenFader
Change `OnSceneLoaded` to always open eyes:

```csharp
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // TEMP FIX: Always open eyes
    if (shouldOpenEyesOnSceneLoad)
    {
        Debug.Log("[ScreenFader] Scene loaded - opening eyes!");
        shouldOpenEyesOnSceneLoad = false;
        isTransitioning = false;
        StartCoroutine(EyesOpeningEffect());
    }
    // ... rest
}
```

But the **proper fix** is to add the CatacombsIntroDialog GameObject to the scene!

## Verification

After adding the GameObject, the logs should show:

```
[CatacombsIntroDialog] ===== START SEQUENCE BEGIN =====
[CatacombsIntroDialog] ScreenFader found, checking eye panels...
[ScreenFader] ArePanelsClosed check: topPanel=exists, bottomPanel=exists
[ScreenFader] Panel positions: top.y=0, bottom.y=0
[ScreenFader] ArePanelsClosed returning TRUE
[CatacombsIntroDialog] ArePanelsClosed returned: true
[CatacombsIntroDialog] Eyes are closed (panels exist) - will open them
[CatacombsIntroDialog] Waiting 0.5s before opening eyes...
[CatacombsIntroDialog] Starting EyesOpeningEffect...
[ScreenFader] Eyes opened!
[CatacombsIntroDialog] Eyes opened!
[CatacombsIntroDialog] Eye opening complete, proceeding to dialog check...
```

## Same Fix Needed for BattleScene

You'll also need to ensure the BattleManager component exists in your BattleScene and has `openEyesOnStart = true`.

## Summary

? Add `CatacombsIntroDialog` GameObject to Catacombs scene
? Ensure `Open Eyes On Start` is CHECKED
? Save scene
? Test

That's it! The code is fine - you just need the GameObject in the scene!
