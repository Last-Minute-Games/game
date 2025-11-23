# Enemy Move Name Popup Feature

## Overview
Added a visual popup system that displays the enemy's move name before they execute their action, providing clear feedback to the player about what's coming.

## What Was Added

### 1. EnemyRender.cs - Visual Components

**New Header Fields:**
```csharp
[Header("Move Name Popup")]
public Vector3 moveNameOffset = new Vector3(0f, -0.4f, 0f);
public float moveNameFontSize = 8f;
public Color moveNameColor = Color.yellow;
public float moveNameDuration = 1.5f;
public float moveNameFloatDistance = 0.3f;
```

**Private Fields:**
```csharp
private TMP_Text _moveNameText;
private GameObject _moveNameObject;
```

**Created in Awake():**
- GameObject for move name text positioned below enemy
- TextMeshPro component with yellow color and bold font
- Proper sorting layer (above everything else)
- Hidden by default

### 2. ShowMoveNamePopup() Method

**Purpose:** Displays the move name with a floating animation

**Animation:**
- Text appears below enemy sprite
- Floats upward over 1.5 seconds
- Fades out in the last half of the animation
- Auto-hides when complete

**DOTween Sequence:**
```csharp
// Float up
transform.DOLocalMoveY(startY + 0.3f, 1.5s) with OutCubic easing

// Fade out (second half)
text.DOFade(0f, 0.75s) with 0.75s delay
```

### 3. GetMoveNameForIntent() Method

**Purpose:** Converts intent enum to display name

**Mappings:**
- `EnemyIntent.Attack` → "Attack"
- `EnemyIntent.Block` → "Defend"
- `EnemyIntent.Heal` → "Heal"
- `EnemyIntent.Buff` → "Buff"
- Unknown → "???"

### 4. EnemyManager Integration

**Updated ExecuteEnemyTurnSequence():**

Before executing each enemy's action:
1. Get the move name for the current intent
2. Show the popup with `ShowMoveNamePopup()`
3. Wait 0.3 seconds for player to see it
4. Then proceed with animation and execution

## Visual Flow

```
Enemy Turn Begins
    ↓
"ATTACK" popup appears below enemy (yellow text)
    ↓
Wait 0.3s (player sees move name)
    ↓
Text starts floating upward
    ↓
Enemy plays attack animation
    ↓
Damage is applied
    ↓
Text fades out while still floating
    ↓
Next enemy's turn
```

## Customization Options

### In Unity Inspector (per EnemyRender):

**Position Offset:**
```
X: 0    (centered)
Y: -0.4 (below enemy)
Z: 0    (same depth)
```

**Font Size:**
```
Default: 8
Larger: 10-12
Smaller: 6-7
```

**Color:**
```
Default: Yellow (#FFFF00)
Aggressive: Red (#FF0000)
Defensive: Blue (#0080FF)
```

**Animation Duration:**
```
Default: 1.5s
Quick: 1.0s
Slow: 2.0s
```

**Float Distance:**
```
Default: 0.3 units upward
More dramatic: 0.5
Subtle: 0.2
```

## Usage Examples

### In EnemyManager (Already Integrated):
```csharp
// Before enemy executes action
if (r != null)
{
    string moveName = r.GetMoveNameForIntent(enemy.currentIntent);
    r.ShowMoveNamePopup(moveName);
    yield return new WaitForSeconds(0.3f);
}
```

### Custom Move Names:
```csharp
// Override in specific enemy scripts
public override string GetMoveNameForIntent(EnemyIntent intent)
{
    if (intent == EnemyIntent.Attack)
        return "Crushing Blow";
    else if (intent == EnemyIntent.Block)
        return "Turtle Up";
    
    return base.GetMoveNameForIntent(intent);
}
```

### Manual Trigger:
```csharp
// Show custom move name
enemyRender.ShowMoveNamePopup("SPECIAL ATTACK", onComplete: () => {
    Debug.Log("Popup finished!");
});
```

## Technical Details

### Animation Breakdown:
```
Time    Event
----    -----
0.00s   Text appears at moveNameOffset position
0.00s   Alpha = 1.0 (fully visible)
0.00s   Float up animation begins
0.75s   Fade out begins (50% through animation)
1.50s   Text fully faded and hidden
1.50s   onComplete callback fires
```

### DOTween Tweens Used:
- `DOLocalMoveY` - Smooth upward float (OutCubic easing)
- `DOFade` - Gradual transparency (Linear easing with delay)

### Memory Management:
- Kills existing tweens before starting new ones
- Prevents animation stacking
- Auto-cleanup on completion

### Performance:
- Lightweight TextMeshPro text
- Simple 2-tween animation
- No allocations during animation
- Efficient sorting layer usage

## Visual Indicators

### Text Style:
- **All caps:** Move names displayed in UPPERCASE
- **Bold font:** Makes text more visible
- **Yellow color:** Stands out against most backgrounds
- **Center aligned:** Balanced below enemy

### Positioning:
- Below enemy sprite (Y: -0.4)
- Above all other UI elements (sorting order +20)
- Floats upward to avoid cluttering enemy area
- Fades out before reaching enemy position

## Integration Points

### When Popup Shows:
- ✅ Before enemy attack animation
- ✅ Before enemy defend/block
- ✅ Before enemy heal
- ✅ Before enemy buff

### When Popup DOESN'T Show:
- Dead enemies (no turn)
- Empty move names
- Null render references

## Benefits

### Player Experience:
- **Clear Communication:** Know exactly what enemy will do
- **Time to React:** 0.3s to see move name before action
- **Visual Polish:** Professional game feel
- **Accessibility:** Text-based indicator alongside icon

### Developer Benefits:
- **Easy Customization:** Inspector-based settings
- **Extensible:** Override `GetMoveNameForIntent()` for custom names
- **No Performance Impact:** Lightweight animation
- **Reusable:** Can show any text, not just move names

## Example Scenarios

### Boss Enemy Special Move:
```csharp
// In BossEnemyBehavior
public override string GetMoveNameForIntent(EnemyIntent intent)
{
    if (isEnraged && intent == EnemyIntent.Attack)
        return "RAGE STRIKE"; // Special name when enraged
    
    return base.GetMoveNameForIntent(intent);
}
```

### Multi-Intent Display:
```csharp
// Show combo move
enemyRender.ShowMoveNamePopup("DOUBLE ATTACK", onComplete: () => {
    // Execute first attack
    ExecuteAttack();
    // Then execute second
    ExecuteAttack();
});
```

## Files Modified

1. ✅ **EnemyRender.cs**
   - Added move name popup fields
   - Created TextMeshPro object in Awake
   - Implemented `ShowMoveNamePopup()` method
   - Added `GetMoveNameForIntent()` helper

2. ✅ **EnemyManager.cs**
   - Updated `ExecuteEnemyTurnSequence()` 
   - Shows popup before each enemy action
   - 0.3s delay for visibility

## Future Enhancements

### Possible Additions:
- **Color Coding:** Different colors per intent type
- **Sound Effects:** Play SFX with popup
- **Particle Effects:** Sparkles or glow on appear
- **Scaling:** Popup in animation
- **Bounce Effect:** Spring easing for more impact
- **Custom Fonts:** Per enemy type or boss

### Advanced Features:
- **Combo Indicators:** "HIT 1 OF 3"
- **Critical Indicators:** "CRITICAL!"
- **Conditional Text:** Based on player state
- **Localization Support:** Multiple languages

---

**Status:** ✅ Complete and Working
**Animation:** Smooth float-up with fade
**Integration:** Automatic in enemy turns
**Customization:** Full Inspector control

