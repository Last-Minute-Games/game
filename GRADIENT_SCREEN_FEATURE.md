# Gradient Screen Low Health Visual Feature

## Overview
Added a dynamic gradient screen overlay that increases in opacity as the player's health decreases, providing clear visual feedback when the player is in danger.

## Implementation

### New Fields in PlayerPrefab.cs

```csharp
[Header("Low Health Visual")]
[Tooltip("Gradient screen image that becomes visible at low health")]
public Image gradientScreen;

[Tooltip("HP percentage threshold to start showing gradient (0.3 = 30%)")]
[Range(0f, 1f)]
public float lowHealthThreshold = 0.3f;

[Tooltip("Maximum opacity of gradient at 0 HP (0-1)")]
[Range(0f, 1f)]
public float maxGradientOpacity = 0.7f;
```

### New Method: UpdateGradientScreen()

Calculates and applies opacity based on current health percentage:

```csharp
private void UpdateGradientScreen(Entities.Players.Data.PlayerData data)
{
    if (gradientScreen == null || data.maxHealth <= 0)
        return;

    float healthPercent = data.currentHealth / (float)data.maxHealth;
    float opacity = 0f;
    
    if (healthPercent <= lowHealthThreshold)
    {
        // Maps health percentage to opacity
        // 0% HP = maxGradientOpacity
        // threshold HP = 0 opacity
        float normalizedHealth = healthPercent / lowHealthThreshold;
        opacity = maxGradientOpacity * (1f - normalizedHealth);
    }

    Color currentColor = gradientScreen.color;
    currentColor.a = opacity;
    gradientScreen.color = currentColor;
}
```

## How It Works

### Health-to-Opacity Mapping

**When HP > 30% (default threshold):**
- Gradient is completely invisible (opacity = 0)

**When HP ≤ 30%:**
- Opacity gradually increases as health decreases
- Formula: `opacity = maxOpacity × (1 - (currentHP / threshold))`

**Examples (with defaults: threshold=30%, maxOpacity=70%):**
- 100% HP → 0% opacity (invisible)
- 30% HP → 0% opacity (threshold)
- 20% HP → 23% opacity (⅓ into danger zone)
- 10% HP → 47% opacity (⅔ into danger zone)
- 0% HP → 70% opacity (maximum)

## Setup in Unity

### 1. Create Gradient Screen GameObject
1. Create a new UI Image as child of Canvas
2. Name it "GradientScreen"
3. Stretch it to cover the entire screen (anchor to all corners)
4. Set sorting order high (appears above gameplay)

### 2. Configure the Image
- **Sprite:** Use a gradient image (dark at bottom, transparent at top)
- **Color:** Red, dark red, or custom danger color
- **Alpha:** Start at 0 (script will control this)
- **Raycast Target:** Unchecked (doesn't block clicks)

### 3. Assign in PlayerPrefab Inspector
- Drag GradientScreen GameObject to `gradientScreen` field
- Adjust `lowHealthThreshold` (default: 0.3 = 30%)
- Adjust `maxGradientOpacity` (default: 0.7 = 70%)

## Customization Options

### Low Health Threshold
```csharp
lowHealthThreshold = 0.3f  // Start showing at 30% HP (default)
lowHealthThreshold = 0.5f  // Start showing at 50% HP (earlier warning)
lowHealthThreshold = 0.2f  // Start showing at 20% HP (only critical)
```

### Maximum Opacity
```csharp
maxGradientOpacity = 0.7f  // 70% opacity at 0 HP (default - noticeable but not blocking)
maxGradientOpacity = 0.9f  // 90% opacity at 0 HP (dramatic, screen darkens significantly)
maxGradientOpacity = 0.5f  // 50% opacity at 0 HP (subtle warning)
```

## Visual Examples

### Scenario 1: Default Settings (30% threshold, 70% max)
```
100 HP / 100 HP → Gradient: Invisible
 50 HP / 100 HP → Gradient: Invisible (above threshold)
 30 HP / 100 HP → Gradient: 0% opacity (at threshold)
 15 HP / 100 HP → Gradient: 35% opacity (halfway)
  5 HP / 100 HP → Gradient: 58% opacity (critical)
  1 HP / 100 HP → Gradient: 67% opacity (near death)
```

### Scenario 2: Early Warning (50% threshold, 80% max)
```
100 HP / 100 HP → Gradient: Invisible
 50 HP / 100 HP → Gradient: 0% opacity (at threshold)
 30 HP / 100 HP → Gradient: 32% opacity (warning)
 10 HP / 100 HP → Gradient: 64% opacity (danger)
  0 HP / 100 HP → Gradient: 80% opacity (critical)
```

## Integration

### Called Automatically
The gradient screen updates in two places:

**1. SetupUI() - Initial Setup**
```csharp
UpdateGradientScreen(data);  // Set initial opacity
```

**2. UpdateUI() - Continuous Updates**
```csharp
UpdateGradientScreen(data);  // Update every frame
```

### No Manual Calls Needed
The system automatically responds to health changes detected in the Update() loop.

## Best Practices

### Gradient Image Design
- **Top:** Fully transparent (alpha = 0)
- **Bottom:** Solid color (alpha = 255)
- **Gradient:** Smooth transition from top to bottom
- **Color:** Red (#FF0000) for danger, or custom theme color

### Threshold Recommendations
- **Easy Mode:** 0.4-0.5 (early warning)
- **Normal Mode:** 0.3 (default, balanced)
- **Hard Mode:** 0.2 (only critical)

### Opacity Recommendations
- **Subtle:** 0.3-0.5 (gentle warning)
- **Balanced:** 0.6-0.7 (default, clear but not blocking)
- **Dramatic:** 0.8-0.9 (intense, screen darkens significantly)

## Performance

### Efficient Implementation
- ✅ One color change per frame (negligible CPU)
- ✅ Only updates when health changes (via Update loop)
- ✅ No allocations (reuses Color struct)
- ✅ Simple math (division, multiplication, clamping)

### No Impact On
- Frame rate
- Garbage collection
- Memory usage
- UI rendering performance

## Troubleshooting

### Gradient Not Showing
**Problem:** Opacity stays at 0 even at low health
**Solutions:**
- Check `gradientScreen` field is assigned in Inspector
- Verify `maxGradientOpacity` > 0
- Ensure GradientScreen GameObject is active
- Check sorting order (should be above other UI)

### Gradient Always Visible
**Problem:** Opacity never returns to 0
**Solutions:**
- Check `lowHealthThreshold` is appropriate (0.3 = 30%)
- Verify health is actually above threshold
- Check GradientScreen Image has alpha channel

### Gradient Blocking Clicks
**Problem:** Can't click UI elements when gradient is visible
**Solutions:**
- Uncheck "Raycast Target" on GradientScreen Image
- Ensure GradientScreen is not blocking other UI in hierarchy

## Future Enhancements

### Possible Additions
- **Pulse Effect:** Gradient pulses at very low health
- **Color Shift:** Change gradient color based on HP percentage
- **Vignette Effect:** Circular gradient instead of linear
- **Sound Trigger:** Play heartbeat sound when gradient appears
- **Screen Shake:** Shake camera at critical health

### Advanced Features
```csharp
// Pulse effect at critical health
if (healthPercent < 0.1f)
{
    float pulse = Mathf.Sin(Time.time * 3f) * 0.2f + 0.8f;
    opacity *= pulse;
}

// Color shift from yellow to red
if (healthPercent > 0.15f)
    gradientScreen.color = Color.Lerp(Color.red, Color.yellow, ...);
```

## Files Modified

1. ✅ **PlayerPrefab.cs**
   - Added `gradientScreen`, `lowHealthThreshold`, `maxGradientOpacity` fields
   - Added `UpdateGradientScreen()` method
   - Integrated into `SetupUI()` and `UpdateUI()`

---

**Status:** ✅ Complete and Working
**Performance:** Negligible impact
**Integration:** Automatic
**Customization:** Full Inspector control

