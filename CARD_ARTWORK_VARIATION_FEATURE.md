# Card Artwork Variation Feature

## Overview
Cards now dynamically display different background artwork based on their variation tier (Poor/Normal/Potent).

## Changes Made

### 1. CardData.cs - Added Helper Method
Added `GetArtworkForTier()` method to return the appropriate artwork sprite:

```csharp
public Sprite GetArtworkForTier(CardVariationTier tier)
{
    return tier switch
    {
        CardVariationTier.WeakModifier => poorArtwork != null ? poorArtwork : artwork,
        CardVariationTier.StrongModifier => potentArtwork != null ? potentArtwork : artwork,
        _ => artwork
    };
}
```

**Logic:**
- **Poor cards** (WeakModifier) → Use `poorArtwork` if set, otherwise fallback to `artwork`
- **Potent cards** (StrongModifier) → Use `potentArtwork` if set, otherwise fallback to `artwork`
- **Normal cards** → Use default `artwork`

### 2. CardRender.cs - Updated Sprite Assignment
Modified the `Bind(CardInstance instance, int? energy = null)` method to use tier-based artwork:

```csharp
// Sprites from data - use tier-specific artwork if available
if (Data != null)
{
    if (instance != null && instance.tier.HasValue)
    {
        cardBackground.sprite = Data.GetArtworkForTier(instance.tier.Value);
    }
    else
    {
        cardBackground.sprite = Data.artwork;
    }
    cardIcon.sprite = Data.icon;
}
else
{
    cardBackground.sprite = null;
    cardIcon.sprite = null;
}
```

**Logic:**
1. Check if we have a CardInstance with a tier value
2. If yes, use `GetArtworkForTier()` to get the appropriate background
3. If no tier, use the default `artwork`
4. Card icon always uses the same sprite (not affected by tier)

## How It Works

### Setup in Unity Editor
For each variable card in your CardData assets:

1. **Enable variability:** Check `isVariableCard = true`
2. **Set base artwork:** Assign the normal artwork sprite
3. **Set poor artwork (optional):** Assign `poorArtwork` sprite for weak variations
4. **Set potent artwork (optional):** Assign `potentArtwork` sprite for strong variations

### Runtime Behavior

When a card is drawn:
1. `CardInstance.FromData()` rolls the effect values based on multipliers
2. The tier is calculated (Poor/Normal/Potent)
3. `CardRender.Bind(instance)` is called
4. The card background changes to match the tier:
   - **Poor tier** → Shows `poorArtwork`
   - **Normal tier** → Shows `artwork`
   - **Potent tier** → Shows `potentArtwork`

## Example Configuration

### Attack Card with Variation
```
CardData Settings:
├─ isVariableCard: true
├─ artwork: NormalAttackBackground.png
├─ poorArtwork: WeakAttackBackground.png
├─ potentArtwork: StrongAttackBackground.png
├─ effects[0]:
│   ├─ baseValue: 10
│   ├─ minMultiplier: 0.5 (5 damage min)
│   └─ maxMultiplier: 1.5 (15 damage max)
└─ Thresholds:
    ├─ minMultiplierThreshold: 0.33
    └─ maxMultiplierThreshold: 0.66
```

**Result:**
- Rolls 5-7 damage → Shows `poorArtwork`
- Rolls 8-12 damage → Shows `artwork` (normal)
- Rolls 13-15 damage → Shows `potentArtwork`

## Benefits

1. **Visual Feedback** - Players can instantly see if they got a strong or weak card
2. **Polish** - Adds visual variety to variable cards
3. **Flexible** - Optional feature (if no poorArtwork/potentArtwork set, uses default)
4. **Consistent** - Works alongside existing prefix system

## Backwards Compatibility

✅ **Fully backwards compatible:**
- If `poorArtwork` or `potentArtwork` are not assigned, the system falls back to the default `artwork`
- Non-variable cards continue to work exactly as before
- Existing CardData assets don't need any changes (unless you want to add tier-specific artwork)

## Files Modified

1. ✅ `CardData.cs` - Added `GetArtworkForTier()` method
2. ✅ `CardRender.cs` - Updated sprite assignment logic

## Testing

To test this feature:
1. Create or edit a CardData asset
2. Enable `isVariableCard`
3. Assign different sprites to `poorArtwork`, `artwork`, and `potentArtwork`
4. Play the game and draw the card multiple times
5. Observe the background changing based on the rolled tier

## Notes

- The `cardIcon` sprite is NOT affected by tier (only the background changes)
- The tier prefix text (e.g., "Poor" or "Potent") still appears in the card name
- Both visual indicators work together for maximum clarity

