# Effect System Migration Guide

## What Changed?

The effect system has been refactored from using **EffectData ScriptableObjects** to using **Effect structs**, following the same pattern as the EnemyAction system.

### Before (Old System)
```csharp
// Had to create separate ScriptableObject assets
public List<EffectData> effectData = new List<EffectData>();
```

### After (New System)
```csharp
// Directly editable in Inspector
public List<Effect> effects = new List<Effect>();
```

## Benefits

1. **No asset clutter** - No need to create separate .asset files
2. **Easier to edit** - Configure effects directly in Inspector lists
3. **Consistent pattern** - Matches how EnemyAction works
4. **Better performance** - Structs are value types (no heap allocation)
5. **Type safety** - No null reference exceptions

## Migration Steps

### Step 1: Update Your CardData Assets

1. **Open Unity Editor**
2. **Navigate to your CardData assets** (usually in `Assets/_Data/Cards/` or similar)
3. **For each CardData asset:**
   - You'll see "Missing" references where EffectData used to be
   - In the Inspector, expand the **"Effects"** list
   - Click the **+** button to add new effects
   - Configure each effect with these fields:

#### Effect Configuration Fields

**Effect Rules:**
- `Target Rule` - Who receives this effect (Self, Enemy, AllEnemies, None)
- `Min Multiplier` - Minimum random multiplier (for variable cards)
- `Max Multiplier` - Maximum random multiplier (for variable cards)

**Effect Data:**
- `Operation Type` - What the effect does:
  - `Damage` - Deal damage to target
  - `AddShield` - Add block/defense
  - `Heal` - Restore health
  - `AddEnergy` - Add energy to player
  - `EndTurn` - End the current turn
  - `ShuffleDeck` - Shuffle the deck
  - `MultiplyPowerScale` - Apply power scaling
  - `None` - No operation
- `Base Value` - The base amount before multipliers

**Timing:**
- `Duration` - How long effect lasts (0 = instant)
- `Duration Unit` - Turns or Rounds
- `Delay` - Delay before effect activates (0 = immediate)
- `Delay Unit` - Turns or Rounds

**UI & Display:**
- `Variable Color` - Color for highlighting values in card text

### Step 2: Example Effect Configurations

#### Attack Card (Deal 10 Damage)
```
Target Rule: Enemy
Operation Type: Damage
Base Value: 10
Min Multiplier: 1
Max Multiplier: 1
Duration: 0
Delay: 0
Variable Color: Red (#FF0000)
```

#### Defense Card (Gain 5 Block)
```
Target Rule: Self
Operation Type: AddShield
Base Value: 5
Min Multiplier: 1
Max Multiplier: 1
Duration: 0
Delay: 0
Variable Color: Blue (#0080FF)
```

#### Healing Card (Restore 8 HP)
```
Target Rule: Self
Operation Type: Heal
Base Value: 8
Min Multiplier: 1
Max Multiplier: 1
Duration: 0
Delay: 0
Variable Color: Green (#00FF00)
```

#### Variable Attack Card (5-15 Damage)
```
Target Rule: Enemy
Operation Type: Damage
Base Value: 10
Min Multiplier: 0.5
Max Multiplier: 1.5
Duration: 0
Delay: 0
Variable Color: Red (#FF0000)
```

### Step 3: Testing

1. **Add the EffectSystemTest component** to any GameObject
2. **Assign a CardData** to the `testCard` field
3. **Right-click the component** and select **"Run Effect System Tests"**
4. **Check the Console** for test results

### Step 4: Clean Up (Optional)

After migration is complete, you can:
1. Delete old EffectData ScriptableObject assets (they're no longer used)
2. Delete `EffectData.cs` file (it's obsolete)
3. Keep them as backup until you're sure everything works

## Code Examples

### Creating Effects in Code

```csharp
// Create a damage effect
Effect damageEffect = new Effect
{
    targetRule = TargetRule.Enemy,
    operationType = OperationType.Damage,
    baseValue = 10,
    minMultiplier = 1f,
    maxMultiplier = 1f,
    variableColor = Color.red
};

// Add to a card
CardData card = ScriptableObject.CreateInstance<CardData>();
card.effects.Add(damageEffect);

// Clone with multiplier
Effect rolled = damageEffect.Clone(true);
Debug.Log($"Rolled damage: {rolled.postCopyValue}");
```

### Working with CardInstance

```csharp
// Create a card instance from data
CardInstance instance = CardInstance.FromData(cardData, applyVariability: true);

// Get total damage
int totalDamage = instance.GetTotal(OperationType.Damage);

// Access rolled effects
foreach (Effect effect in instance.rolledEffects)
{
    Debug.Log($"{effect.operationType}: {effect.postCopyValue}");
}
```

## Troubleshooting

### Issue: "Cannot resolve symbol 'effects'"
**Solution:** This is a caching issue. Restart your IDE or rebuild the solution.

### Issue: Missing references in Inspector
**Solution:** This is expected. The old EffectData references are gone. Re-add effects manually using the new system.

### Issue: Card effects not working
**Solution:** Make sure you've added at least one effect to the `effects` list. Check that `operationType` and `targetRule` are set correctly.

### Issue: Variable cards not rolling values
**Solution:** Ensure `isVariableCard` is checked and `minMultiplier`/`maxMultiplier` are different values.

## File Changes Reference

### Modified Files
- `GameItemData.cs` - Base class for all items
- `CardData.cs` - Card-specific data
- `CardInstance.cs` - Runtime card instances
- `PlayerManager.cs` - Card effect application
- `CardManager.cs` - Card management
- `EffectEnums.cs` - Added Effect struct

### New Files
- `EffectSystemTest.cs` - Testing utility

### Obsolete Files
- `EffectData.cs` - Old ScriptableObject (can be deleted after migration)

## Support

If you encounter issues:
1. Check the test script results
2. Verify effect configurations in Inspector
3. Check Console for error messages
4. Ensure all card assets have been updated

## Comparison with EnemyAction

This refactoring follows the same pattern as `EnemyAction`:

```csharp
// EnemyAction (existing pattern)
[Serializable]
public struct EnemyAction
{
    public EnemyIntent intent;
    public int value;
}

// Effect (new pattern - similar structure)
[Serializable]
public struct Effect
{
    public OperationType operationType;
    public int baseValue;
    // ... additional fields
}
```

Both are structs that can be directly edited in Unity Inspector lists!

