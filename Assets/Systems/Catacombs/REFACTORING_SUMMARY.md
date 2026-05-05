# Catacombs Trigger Refactoring Summary

## Overview
Refactored the Catacombs dialogue trigger system from auto-initialized static/singleton classes to scene-based MonoBehaviour GameObjects, similar to `OverworldWakeUpCutscene.cs`.

## Changes Made

### 1. CatacombsIntroDialog.cs
**Before:** Static class with `RuntimeInitializeOnLoadMethod` that auto-subscribed to scene load events
**After:** Regular MonoBehaviour that can be placed in the Catacombs scene

**Key Changes:**
- Removed `RuntimeInitializeOnLoadMethod` attribute
- Removed scene name checking and SceneManager events
- Converted from static class to instance-based MonoBehaviour
- Added serialized fields for inspector configuration:
  - `dialogBehaviour` - Reference to DialogBehaviour component
  - `introDialogGraph` - Reference to the intro dialogue graph
  - `autoFindDialogBehaviour` - Toggle to auto-find DialogBehaviour
  - `autoFindPlayer` - Toggle to auto-find player
- Plays dialogue on `Start()` instead of on scene load event

**Usage:**
1. Add `CatacombsIntroDialog` component to a GameObject in the Catacombs scene
2. Optionally assign references in the inspector (or leave auto-find enabled)
3. The dialogue will play automatically when the scene starts (if the flag hasn't been set)

### 2. CatacombsDevilDialogueTrigger.cs
**Before:** Singleton MonoBehaviour with `RuntimeInitializeOnLoadMethod` that persisted across scenes
**After:** Regular scene-based MonoBehaviour

**Key Changes:**
- Removed `RuntimeInitializeOnLoadMethod` attribute
- Removed `DontDestroyOnLoad` singleton pattern
- Removed scene name checking and SceneManager events
- Converted `DialogueStep` from readonly struct to serializable class
- Changed `Steps` from static readonly array to serialized `dialogueSteps` array
- Added serialized fields for inspector configuration:
  - `dialogueSteps` - Array of dialogue steps (configurable in inspector)
  - `dialogBehaviour` - Reference to DialogBehaviour component
  - `autoFindDialogBehaviour` - Toggle to auto-find DialogBehaviour
  - `autoFindPlayer` - Toggle to auto-find player
- Removed `_isWatching` flag (no longer needed since component only exists in Catacombs scene)
- Each `DialogueStep` now supports both assigned DialogNodeGraph and resource path fallback

**Usage:**
1. Add `CatacombsDevilDialogueTrigger` component to a GameObject in the Catacombs scene
2. Configure dialogue steps in the inspector:
   - Adjust trigger Y positions
   - Assign dialogue graphs directly or use resource paths
   - Set appropriate flag names
3. Optionally assign references in the inspector (or leave auto-find enabled)
4. The component will automatically trigger dialogues as the player progresses through the level

## Benefits

### Improved Editor Workflow
- Components are now visible in the scene hierarchy
- All configuration is accessible through the Unity Inspector
- No hidden auto-initialized code running in the background
- Easier to debug and test

### Better Scene Management
- No cross-scene persistence required
- Components only exist when the scene is loaded
- Cleaner scene setup and teardown
- No singleton management needed

### More Flexible Configuration
- Dialogue graphs can be assigned directly in the inspector
- Trigger positions and flags are easily adjustable
- Auto-find toggles provide flexibility for different scene setups
- Follows Unity best practices for scene-based components

### Consistency
- Matches the pattern used in `OverworldWakeUpCutscene.cs`
- Consistent approach across the codebase
- Easier for other developers to understand and maintain

## Migration Notes

### For the Catacombs Scene:
1. Open the Catacombs scene
2. Create a new empty GameObject named "CatacombsIntroDialog"
3. Add the `CatacombsIntroDialog` component to it
4. Create another GameObject named "CatacombsDevilDialogueTrigger"
5. Add the `CatacombsDevilDialogueTrigger` component to it
6. (Optional) Assign references in the inspectors or leave auto-find enabled
7. Save the scene

The auto-find features will locate the necessary components automatically if you prefer not to manually assign references.
