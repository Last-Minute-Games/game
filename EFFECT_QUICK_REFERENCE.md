# Effect Struct Quick Reference Card

## Quick Start

### Creating an Effect in Inspector
1. Select CardData asset
2. Expand "Effects" list
3. Click "+" to add
4. Configure properties

### Creating an Effect in Code
```csharp
Effect damageEffect = new Effect
{
    targetRule = TargetRule.Enemy,
    operationType = OperationType.Damage,
    baseValue = 10,
    minMultiplier = 1f,
    maxMultiplier = 1f,
    variableColor = Color.red
};
```

## Effect Properties Reference

| Property | Type | Description | Common Values |
|----------|------|-------------|---------------|
| `targetRule` | TargetRule | Who gets the effect | Self, Enemy, AllEnemies, None |
| `operationType` | OperationType | What it does | Damage, AddShield, Heal, AddEnergy |
| `baseValue` | int | Base amount | 1-50 (typical) |
| `minMultiplier` | float | Min random mult | 1.0 (fixed) or 0.5 (variable) |
| `maxMultiplier` | float | Max random mult | 1.0 (fixed) or 1.5 (variable) |
| `duration` | int | How long (0=instant) | 0-20 |
| `durationUnit` | TimeUnit | Turns or Rounds | Turns, Rounds |
| `delay` | int | Delay before active | 0-20 |
| `delayUnit` | TimeUnit | Turns or Rounds | Turns, Rounds |
| `variableColor` | Color | UI highlight color | Red, Blue, Green, etc |

## Common Effect Recipes

### Attack Card (Fixed Damage)
```csharp
targetRule: Enemy
operationType: Damage
baseValue: 10
minMultiplier: 1
maxMultiplier: 1
```

### Block Card (Fixed Defense)
```csharp
targetRule: Self
operationType: AddShield
baseValue: 5
minMultiplier: 1
maxMultiplier: 1
```

### Heal Card (Fixed Healing)
```csharp
targetRule: Self
operationType: Heal
baseValue: 8
minMultiplier: 1
maxMultiplier: 1
```

### Variable Attack (5-15 Damage)
```csharp
targetRule: Enemy
operationType: Damage
baseValue: 10
minMultiplier: 0.5
maxMultiplier: 1.5
```

### AoE Attack (Hit All Enemies)
```csharp
targetRule: AllEnemies
operationType: Damage
baseValue: 7
minMultiplier: 1
maxMultiplier: 1
```

### Energy Card (Gain 2 Energy)
```csharp
targetRule: Self
operationType: AddEnergy
baseValue: 2
minMultiplier: 1
maxMultiplier: 1
```

## Working with Effects in Code

### Access Card Effects
```csharp
CardData card = // ... get card
foreach (Effect effect in card.effects)
{
    Debug.Log($"{effect.operationType}: {effect.baseValue}");
}
```

### Clone with Variability
```csharp
Effect original = card.effects[0];
Effect rolled = original.Clone(applyMultiplier: true);
Debug.Log($"Rolled: {rolled.postCopyValue}");
```

### Get Total by Type
```csharp
CardInstance instance = CardInstance.FromData(cardData, true);
int totalDamage = instance.GetTotal(OperationType.Damage);
int totalBlock = instance.GetTotal(OperationType.AddShield);
```

### Check Effect Type
```csharp
Effect effect = card.effects[0];

if (effect.operationType == OperationType.Damage)
{
    // Handle damage
}
else if (effect.operationType == OperationType.Heal)
{
    // Handle healing
}
```

### Get Color Tag for UI
```csharp
Effect effect = card.effects[0];
string colorHex = effect.GetColorTag();
string richText = $"<color=#{colorHex}>{effect.baseValue}</color>";
```

## Operation Types

| Type | Effect | Target |
|------|--------|--------|
| `Damage` | Deal damage | Enemy/AllEnemies |
| `AddShield` | Gain block | Self |
| `Heal` | Restore HP | Self |
| `AddEnergy` | Gain energy | Self |
| `EndTurn` | End turn | Self |
| `ShuffleDeck` | Shuffle deck | Self |
| `MultiplyPowerScale` | Apply power scaling | Varies |
| `None` | No effect | None |

## Target Rules

| Rule | Targets |
|------|---------|
| `None` | No target |
| `Self` | Player only |
| `Enemy` | Single enemy |
| `AllEnemies` | All enemies |

## Best Practices

### ✅ Do
- Set appropriate target rules for operation types
- Use 0 duration for instant effects
- Set min=max for non-variable cards
- Use meaningful variable colors
- Test variable cards with Clone(true)

### ❌ Don't
- Mix incompatible target + operation (e.g., Self + Damage)
- Leave operationType as None for active effects
- Forget to set baseValue
- Use extreme multipliers (>10x) without testing

## Migration from EffectData

### Old Way (ScriptableObject)
```csharp
// 1. Create asset file
// 2. Configure in asset
// 3. Reference in list
public List<EffectData> effectData;
```

### New Way (Struct)
```csharp
// 1. Add to list in Inspector
// 2. Configure directly
public List<Effect> effects;
```

## Testing

### Quick Test
```csharp
Effect test = Effect.CreateDefault();
Debug.Log($"Default: {test.operationType}"); // None
```

### Full Test
```csharp
// Add EffectSystemTest component
// Assign CardData
// Right-click → "Run Effect System Tests"
```

## Common Issues

| Issue | Solution |
|-------|----------|
| Effect doesn't apply | Check operationType and targetRule |
| No variable damage | Ensure minMult ≠ maxMult and isVariableCard=true |
| Null reference error | Effects are structs, can't be null - check if list is empty |
| Color not showing | Set variableColor in Inspector |

## See Also
- `MIGRATION_GUIDE_EFFECTS.md` - Full migration guide
- `REFACTORING_CHECKLIST.md` - Task checklist
- `EffectSystemTest.cs` - Testing utility

---
**Quick Tip:** Effects follow the same pattern as EnemyAction - if you know how to use one, you know how to use the other!

