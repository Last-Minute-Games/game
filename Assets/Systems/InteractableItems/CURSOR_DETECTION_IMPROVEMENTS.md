# Cursor Detection Improvements for NPCs

## Problem
The cursor wasn't reliably changing when hovering over NPCs because:
1. **Multiple colliders**: NPCs often have both trigger colliders (for interaction zones) and solid colliders (for physics), and the old code didn't prioritize correctly
2. **Small hover radius**: Default 0.5f was too small for typical NPC sprites
3. **No raycast detection**: Only used `OverlapPoint` which can miss sprites with complex collider setups

## Solution Implemented

### 1. **Raycast Detection (Primary Method)**
- New setting: `useRaycastDetection` (enabled by default)
- New setting: `raycastLayerMask` to control which layers to check
- Uses `Physics2D.RaycastAll` to detect what's under the mouse cursor
- Checks both the main GameObject and child GameObjects
- **Most reliable method for NPCs with sprite renderers**

### 2. **Improved Collider Detection (Fallback)**
- Prioritizes **trigger colliders** first (these are typically interaction zones)
- Falls back to **solid colliders** if no trigger is hit
- Provides better logging to help debug which collider was detected

### 3. **Increased Hover Radius**
- Changed default `hoverCheckRadius` from **0.5f to 1.0f**
- Makes the fallback radius detection more forgiving for NPCs

## How to Use

### For Most NPCs (Recommended Setup)
1. Ensure your NPC has a **CircleCollider2D** or **BoxCollider2D** set as a trigger
2. Set the collider size to cover the sprite area you want to be clickable
3. Make sure the NPC GameObject is on a layer that's included in the InteractionDetector's `raycastLayerMask`

### Inspector Settings
On the **InteractionDetector** (on the Player):
- ? `Use Raycast Detection` - Keep this **enabled** for best results
- `Raycast Layer Mask` - Set to include layers with NPCs/items (default -1 means all layers)
- `Hover Check Radius` - Increased to **1.0f** (adjust if needed)
- `Enable Directional Hover` - Keep enabled for Stardew Valley-style pointing
- `Directional Hover Max Distance` - How far you can point at NPCs (default 3f)
- `Directional Hover Angle Tolerance` - How precise you need to point (default 60° = forgiving)

### Debugging
Enable `enableDebugLogs` on the InteractionDetector to see detailed console output showing:
- Which detection method succeeded (RAYCAST, TRIGGER_COLLIDER, SOLID_COLLIDER, RADIUS, DIRECTIONAL)
- Distance calculations
- Why certain interactables were chosen over others

## Detection Method Priority

The system tries these methods in order:

1. **RAYCAST** - Raycast at mouse position (most reliable for sprites)
2. **RAYCAST_CHILD** - Raycast hit a child collider
3. **TRIGGER_COLLIDER** - Mouse overlaps a trigger collider
4. **SOLID_COLLIDER** - Mouse overlaps a solid collider
5. **RADIUS** - Mouse within `hoverCheckRadius` of object center
6. **DIRECTIONAL** - Mouse pointing toward object (Stardew Valley style)

## Performance Notes

- Raycast detection is very efficient in Unity 2D
- The system only checks objects already in the player's trigger zone (via `nearbyInteractables`)
- Layer masks help optimize raycast performance by ignoring irrelevant layers

## Troubleshooting

### "Cursor still doesn't change for my NPC"
1. Check the NPC has a collider component
2. Verify the NPC's layer is in the `raycastLayerMask`
3. Enable debug logs and check console to see what detection method is being used
4. Make sure the collider covers the sprite area

### "Cursor changes too early/too far away"
1. Reduce `hoverCheckRadius` (try 0.75f instead of 1.0f)
2. Adjust `directionalHoverMaxDistance` to limit range
3. Reduce `directionalHoverAngleTolerance` for more precision

### "Cursor changes for wrong NPC when multiple are nearby"
This is working as designed - the system picks the closest one that matches. The interaction priority and distance determine which is chosen.
