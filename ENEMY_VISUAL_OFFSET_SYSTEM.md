# Enemy Visual Offset System

## Overview
Manually adjust enemy sprite position and scale directly in the EnemyConfig ScriptableObject to ensure enemies appear correctly positioned and sized in gameplay.

## Changes Made

### 1. EnemyConfig.cs
Added visual adjustment fields under a new "Visual Adjustments" header:

```csharp
[Header("Visual Adjustments")]
[Tooltip("Position offset to adjust where the enemy sprite appears in the scene.")]
public Vector3 positionOffset = Vector3.zero;

[Tooltip("Scale multiplier to adjust the size of the enemy sprite (1 = normal size).")]
public Vector3 scaleOffset = Vector3.one;
```

Updated `CreateRuntimeInstance()` to propagate these values:
```csharp
// Propagate visual adjustments
data.positionOffset = positionOffset;
data.scaleOffset = scaleOffset;
```

### 2. EnemyData.cs
Added matching fields to store runtime values:

```csharp
[Header("Visual Adjustments")]
public Vector3 positionOffset = Vector3.zero;
public Vector3 scaleOffset = Vector3.one;
```

### 3. EnemyRender.cs
Updated `Bind()` method to apply offsets when spawning enemies:

```csharp
// Apply visual adjustments from EnemyData
if (data != null)
{
    // Apply position offset
    transform.localPosition += data.positionOffset;
    
    // Apply scale offset
    transform.localScale = Vector3.Scale(transform.localScale, data.scaleOffset);
}
```

## How to Use

### In Unity Inspector (EnemyConfig asset):

1. **Select your EnemyConfig asset** in the Project window

2. **Find "Visual Adjustments" section** in the Inspector

3. **Adjust Position Offset:**
   ```
   Position Offset:
   ├─ X: Horizontal position (-left, +right)
   ├─ Y: Vertical position (-down, +up)
   └─ Z: Depth position (usually 0)
   ```

4. **Adjust Scale Offset:**
   ```
   Scale Offset:
   ├─ X: Horizontal scale (1 = normal, 2 = double width, 0.5 = half width)
   ├─ Y: Vertical scale (1 = normal, 2 = double height, 0.5 = half height)
   └─ Z: Depth scale (usually 1)
   ```

## Common Use Cases

### Enemy Positioned Too Low
```
Position Offset:
  Y: 0.5  (moves enemy up)
```

### Enemy Positioned Too High
```
Position Offset:
  Y: -0.3  (moves enemy down)
```

### Enemy Positioned Too Far Left
```
Position Offset:
  X: 0.2  (moves enemy right)
```

### Enemy Too Large
```
Scale Offset:
  X: 0.7
  Y: 0.7
  (makes enemy 70% of original size)
```

### Enemy Too Small
```
Scale Offset:
  X: 1.5
  Y: 1.5
  (makes enemy 150% of original size)
```

### Horizontally Stretched Enemy
```
Scale Offset:
  X: 1.2  (wider)
  Y: 1.0  (normal height)
```

### Tall Thin Enemy
```
Scale Offset:
  X: 0.8  (narrower)
  Y: 1.3  (taller)
```

## Examples

### Small Flying Enemy (needs to be higher and smaller)
```
Position Offset: (0, 0.8, 0)
Scale Offset: (0.6, 0.6, 1)
```

### Large Boss Enemy (needs to be bigger and centered)
```
Position Offset: (0, 0.2, 0)
Scale Offset: (2.5, 2.5, 1)
```

### Ground Crawler (needs to be lower)
```
Position Offset: (0, -0.4, 0)
Scale Offset: (1, 0.8, 1)  // slightly flatter
```

## Technical Details

### Application Flow
1. **Design Time:** Set offsets in EnemyConfig ScriptableObject
2. **Spawn Time:** EnemyConfig.CreateRuntimeInstance() copies values to EnemyData
3. **Render Time:** EnemyRender.Bind() applies offsets to transform

### Transform Operations
- **Position:** Uses `transform.localPosition += offset` (additive)
- **Scale:** Uses `Vector3.Scale(transform.localScale, offset)` (multiplicative)

### Default Values
- **positionOffset:** `Vector3.zero` (no offset)
- **scaleOffset:** `Vector3.one` (100% original size)

## Tips

### Finding the Right Values
1. **Start with position:** Get the enemy in the right spot first
2. **Then adjust scale:** Make sure it's the right size
3. **Use small increments:** Try 0.1 or 0.2 at a time
4. **Test in gameplay:** See how it looks with other enemies and UI

### Common Ranges
- **Position X/Y:** Usually between -1.0 and +1.0
- **Scale X/Y:** Usually between 0.5 and 2.0
- **Z values:** Usually leave at 0 (position) or 1 (scale)

### Uniform vs Non-Uniform Scaling
- **Uniform (proportional):** Set X and Y to same value (e.g., 1.5, 1.5)
- **Non-Uniform (stretched):** Set X and Y to different values (e.g., 1.2, 0.8)

## Troubleshooting

### Enemy Still Not Positioned Correctly
- Check if spawn position is also affecting placement
- Verify values are being saved in the EnemyConfig asset
- Make sure you're testing with the updated EnemyConfig

### Enemy Appears Distorted
- Check if scale values are too different (X vs Y)
- Try using uniform scaling first (same X and Y)

### Changes Not Appearing in Game
- Save the EnemyConfig asset after making changes
- Ensure the correct EnemyConfig is being used by enemy spawner
- Check if there are multiple copies of the enemy config

### Intent Icon Position Affected
- Intent icon is a child of the enemy, so it will move with position offset
- Intent icon uses local offset, so scale shouldn't affect it much
- If needed, adjust `intentIconOffset` in EnemyRender component

## Files Modified

1. ✅ `EnemyConfig.cs` - Added positionOffset and scaleOffset fields
2. ✅ `EnemyData.cs` - Added runtime storage for offsets
3. ✅ `EnemyRender.cs` - Applied offsets during Bind()

## Backwards Compatibility

✅ **Fully backwards compatible:**
- Default values (Vector3.zero and Vector3.one) produce no change
- Existing enemies continue to work without modification
- Only adjust values for enemies that need repositioning

---

**Quick Reference:**
- **Position Offset:** Moves the sprite in world space
- **Scale Offset:** Resizes the sprite
- **Defaults:** No change (zero position, one scale)
- **When Applied:** During enemy spawn/bind

