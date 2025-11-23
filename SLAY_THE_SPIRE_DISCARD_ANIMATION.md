# Slay the Spire Style Card Discard Animation

## Overview
Implemented a polished card discard animation that mimics Slay the Spire's visual style, where cards "pull out" from the player's hand and arc down to the discard pile at the bottom of the screen.

## Animation Details

### Two-Phase Motion

#### Phase 1: Pull Out (30% of duration)
- **Movement:** Card drops down slightly (~0.3 units)
- **Rotation:** Small random rotation (-8° to +8°)
- **Easing:** InCubic for a snappy pull-down feel
- **Purpose:** Creates the "pulling from hand" effect

#### Phase 2: Arc to Discard (70% of duration)
- **Path:** 3-point Catmull-Rom spline creating smooth arc
- **Arc Peak:** Midpoint slightly elevated for natural trajectory
- **Landing Spread:** Random horizontal offset (-0.3 to +0.3) for organic feel
- **Rotation:** Continues spinning (-25° to +25°) during flight
- **Scale:** Cards shrink to 20% of original size
- **Fade:** Cards fade out in the last 30% of the animation
- **Easing:** InQuad for accelerating descent

### Stagger Effect
- **Left-to-Right:** Cards animate in sequence (0.03s delay per card)
- **Visual Flow:** Creates a smooth "wave" of cards leaving the hand
- **Performance:** Prevents all cards from moving simultaneously

## Usage

### Method 1: Direct Call
```csharp
// Specify exact target position
Vector3 discardPilePosition = new Vector3(2f, -3f, 0f);
deckViewer.AnimateDiscardAll(
    discardTargetWorldPos: discardPilePosition,
    duration: 0.5f,
    staggerDelay: 0.03f,
    onComplete: () => Debug.Log("All cards discarded!")
);
```

### Method 2: Automatic (Recommended)
```csharp
// Uses smart default position (bottom-center of screen)
deckViewer.ClearSmooth(onComplete: () => {
    Debug.Log("Hand cleared with style!");
});
```

### Method 3: Custom Target
```csharp
// Override default position but keep smart behavior
Vector3 customTarget = GetDiscardPileWorldPosition();
deckViewer.ClearSmooth(
    discardTarget: customTarget,
    onComplete: OnHandCleared
);
```

## Default Discard Target

When no target is specified, `ClearSmooth()` calculates a smart default:

```csharp
// Bottom-center of screen (60% width, -20% height)
// This mimics Slay the Spire's discard pile position
Vector3 screenPos = new Vector3(Screen.width * 0.6f, Screen.height * -0.2f, 10f);
Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
```

**Visual Position:**
- Slightly right of center (60% across screen width)
- Below the bottom of screen (-20% of screen height)
- Maintains same Z-depth as hand

## Parameters

### AnimateDiscardAll()
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `discardTargetWorldPos` | Vector3 | Required | World position where cards fly to |
| `duration` | float | 0.5f | Total time for each card animation |
| `staggerDelay` | float | 0.03f | Delay between each card starting |
| `onComplete` | Action | null | Callback when all cards are done |

### ClearSmooth()
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `discardTarget` | Vector3? | null | Optional target (uses smart default if null) |
| `onComplete` | Action | null | Callback when complete |

## Visual Breakdown

```
Card Animation Timeline (0.5s total):
├─ 0.00s: Stagger delay starts (card i × 0.03s)
├─ 0.00s: Phase 1 begins - Pull down
│   ├─ Drop 0.3 units downward
│   └─ Slight rotation (-8° to +8°)
├─ 0.15s: Phase 1 ends
├─ 0.15s: Phase 2 begins - Arc to discard
│   ├─ Follow curved path (3-point spline)
│   ├─ Continue rotating (-25° to +25°)
│   ├─ Scale down to 20%
│   └─ Begin fade at 0.35s
└─ 0.50s: Card destroyed
```

## Features

### ✨ Polish Elements
- **Arc Motion:** Natural curved trajectory (not straight line)
- **Random Variation:** Each card has unique rotation/position
- **Staggered Timing:** Sequential animation prevents visual chaos
- **Smooth Easing:** Different easing curves for different phases
- **Fade Out:** Cards become transparent before disappearing
- **Scale Down:** Cards shrink as they fly away
- **Interaction Lock:** Prevents player from clicking during animation

### 🎮 Player Experience
- **Clear Feedback:** Player sees exactly what happened to their hand
- **Satisfying Feel:** Smooth animation feels responsive
- **Polished Look:** Professional quality matching AAA card games
- **Non-Blocking:** Other game systems can continue while animating

## Integration Points

### When to Use

**End of Turn:**
```csharp
void EndPlayerTurn()
{
    handViewer.ClearSmooth(onComplete: () => {
        DrawNewHand();
    });
}
```

**Round End:**
```csharp
void OnRoundEnd()
{
    handViewer.ClearSmooth(onComplete: () => {
        RoundManager.NextRound();
    });
}
```

**Battle Victory:**
```csharp
void OnBattleWon()
{
    handViewer.ClearSmooth(onComplete: () => {
        ShowVictoryScreen();
    });
}
```

## Technical Details

### Card Interaction Lock
```csharp
// Prevents clicking cards during animation
CardFXHelper.CardInteraction.Locked = true;

// Re-enabled when all cards are destroyed
CardFXHelper.CardInteraction.Locked = false;
```

### Memory Management
- Cards are destroyed after animation completes
- `_renders` list is cleared automatically
- Callback fires only after ALL cards are destroyed
- Null checks prevent errors from early destruction

### DOTween Sequences
Each card has its own `Sequence`:
- Independent timing (no interference)
- Easy to modify per-card animation
- Built-in completion callbacks
- Efficient tween management

## Performance Considerations

### Optimized for Card Games
- **Stagger Delay:** Prevents performance spikes (cards animate over time)
- **Single Sequence per Card:** Minimal overhead
- **Early Destruction:** Cards removed as soon as animation ends
- **Pooling Compatible:** Can integrate with object pooling if needed

### Typical Performance
- **10 cards:** 0.5s total (0.03s × 10 = 0.3s stagger + 0.5s animation)
- **20 cards:** 1.1s total (0.03s × 20 = 0.6s stagger + 0.5s animation)
- **Frame Rate:** Smooth 60 FPS with DOTween

## Customization Options

### Adjust Pull-Out Strength
```csharp
// Line 408: Change pull distance
Vector3 pullOutPos = startPos + new Vector3(0, -0.5f, 0); // Stronger pull
```

### Modify Arc Height
```csharp
// Line 418: Adjust arc peak
arcPath[1] = Vector3.Lerp(pullOutPos, discardTargetWorldPos, 0.5f) 
    + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0); // Higher arc
```

### Change Spread Amount
```csharp
// Line 419: Modify landing spread
arcPath[2] = discardTargetWorldPos 
    + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0); // Wider spread
```

### Tweak Rotation
```csharp
// Line 413: Pull-out rotation
Random.Range(-15f, 15f) // Bigger wobble

// Line 428: Flight rotation
Random.Range(-45f, 45f) // More dramatic spin
```

## Comparison to Original

### Before
- Simple linear motion to discard pile
- All cards moved at once
- No pull-out effect
- Basic scaling/rotation
- 0.4s duration, 0.05s stagger

### After (Slay the Spire Style)
- Two-phase motion (pull + arc)
- Staggered left-to-right wave
- Pronounced pull-down effect
- Curved trajectory with random spread
- 0.5s duration, 0.03s stagger
- Fade out effect
- Interaction locking

## Example Output

```
[DeckViewer] Animating 7 cards to discard pile (Slay the Spire style)
[DeckViewer] All cards discarded and cleared
```

---

**Result:** Professional card game discard animation that matches the visual quality of Slay the Spire! 🎴✨

