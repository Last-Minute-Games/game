# Build Interaction Fix

## Problem
Interactions were completely broken in build versions (both keyboard E key and mouse right-click) but working perfectly in the editor.

## Root Causes Identified

### 1. CursorMode.ForceSoftware (Fixed)
The `InteractionDetector.cs` was using `CursorMode.ForceSoftware` for all cursor operations. This cursor mode has known compatibility issues in Unity builds, especially on certain platforms (Windows standalone, WebGL, etc.).

**Why it worked in Editor but not in Build:**
- Editor has more fault-tolerant cursor handling
- `ForceSoftware` mode can silently fail in builds
- When cursor operations fail, the interaction system may not properly detect hover states
- Failed cursor operations can cause the entire Update loop to malfunction

### 2. Physics2D Layer Collision Matrix (Likely Culprit)
If interactions are still broken after the cursor fix, the issue is almost certainly **Physics2D collision detection**. The `OnTriggerEnter2D` and `OnTriggerExit2D` methods rely on Unity's Physics2D system to detect when NPCs/items are near the player.

**Common build issues:**
- Layer collision matrix differs between Editor and Build
- Physics2D settings not properly saved in build
- Trigger colliders not configured correctly on InteractionDetector or interactable objects

## Solutions Applied

### 1. Fixed Cursor Mode
Changed all `Cursor.SetCursor()` calls from `CursorMode.ForceSoftware` to `CursorMode.Auto`.

### 2. Removed Unnecessary Toggle
Removed `enableKeyboardInteraction` toggle - keyboard interaction should always be enabled.

### Files Modified:
- `Assets\Systems\InteractableItems\InteractionDetector.cs`
  - Changed all cursor operations to use `CursorMode.Auto`
  - Removed `enableKeyboardInteraction` field
  - Simplified E key input check

## Testing Recommendations

### Step 1: Build and Test
1. **Build the game** and test all interactions:
   - NPCs (dialog triggers)
   - Interactive items (doors, objects)
   - Minigame launchers (coin game, riddle, maze)

2. **Test both input methods**:
   - E key interaction (keyboard)
   - Right-click interaction (mouse)

3. **Test hover detection**:
   - Verify cursor changes when hovering over NPCs/items
   - Verify interaction prompt appears
   - Verify directional hover works correctly

### Step 2: If Still Broken - Physics2D Diagnostics

#### Check Layer Collision Matrix
1. Open **Edit ? Project Settings ? Physics 2D**
2. Scroll down to **Layer Collision Matrix**
3. Verify that the layer containing your **Player** (with InteractionDetector) can collide with the layer containing **NPCs/Interactive Items**
4. Common setup:
   - Player on "Player" layer (Layer 8)
   - NPCs/Items on "Default" layer (Layer 0) or "Interactable" layer
   - Ensure these layers can collide in the matrix

#### Verify Collider Setup
1. **On Player GameObject** (with InteractionDetector):
   - Must have a 2D Collider (BoxCollider2D or CircleCollider2D)
   - **Must be set as Trigger** (check "Is Trigger")
   - Recommended size: Radius 1.5-2.0 for full body coverage

2. **On NPC/Interactive Item GameObjects**:
   - Must have a 2D Collider
   - Can be trigger or non-trigger
   - Must be on a layer that can collide with Player layer

#### Debug in Build
Since `LogDebug()` is stripped from builds, you need alternative debugging:

1. **Visual debugging**: Temporarily enable the interaction prompt to always show (modify `UpdatePopupVisibility()`)
2. **Add build-safe logging**: Use `Debug.Log()` directly instead of `LogDebug()` for build testing
3. **Check trigger counts**: Add a UI text element showing `nearbyInteractables.Count`

Example temporary debug code:
```csharp
void Update()
{
    // TEMPORARY: Build debug - remove after testing
    Debug.Log($"[BUILD DEBUG] Nearby: {nearbyInteractables.Count}, E pressed: {Input.GetKeyDown(KeyCode.E)}");

    if (enableHoverDetection)
    {
        UpdateMouseHover();
    }
    // ... rest of Update
}
```

### Step 3: Advanced Troubleshooting

#### Force Trigger Detection (Nuclear Option)
If Physics2D triggers still don't work, implement manual distance checking:

```csharp
void Update()
{
    // FALLBACK: Manual detection if Physics2D triggers fail in build
    if (nearbyInteractables.Count == 0)
    {
        ManualInteractableDetection();
    }

    // ... rest of Update
}

private void ManualInteractableDetection()
{
    // Find all interactables in scene (expensive - only use as fallback!)
    var allInteractables = FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>();

    foreach (var interactable in allInteractables)
    {
        MonoBehaviour mb = interactable as MonoBehaviour;
        if (mb == null) continue;

        float distance = Vector2.Distance(transform.position, mb.transform.position);
        if (distance < 2.0f) // Detection radius
        {
            if (!nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
                Debug.Log($"[MANUAL DETECT] Added: {mb.gameObject.name}");
            }
        }
        else
        {
            nearbyInteractables.Remove(interactable);
        }
    }
}
```

## Additional Notes

### Other Potential Build Issues (Not Current Problems):
1. **LayerMask initialization**: The code uses `raycastLayerMask = -1` which represents "Everything". This should work, but if you encounter raycast issues in builds, consider explicitly setting the layer mask in the Inspector.

2. **Debug Logging**: All `LogDebug()` calls are stripped from builds due to `[System.Diagnostics.Conditional("UNITY_EDITOR")]` attribute. This is by design for performance.

3. **Physics2D Settings**: Ensure your Physics2D layer collision matrix is identical between Editor and Build settings.

## Related Files
- `Assets\UIs\UIs\CursorManager.cs` - Uses `CursorMode.Auto` (correct approach)
- `Assets\Systems\InteractableItems\IInteractable.cs` - Interface definition
- `Assets\Systems\InteractableItems\InteractiveItem.cs` - Implementation example
