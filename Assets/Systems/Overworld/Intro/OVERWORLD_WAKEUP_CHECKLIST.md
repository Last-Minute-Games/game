# ✅ Overworld Wake-Up Checklist

## In Unity Editor - Overworld Scene

### 1. Create Manager GameObject
- [ ] Create Empty GameObject
- [ ] Name it `OverworldWakeUpManager`
- [ ] Add Component: `OverworldWakeUpCutscene`

### 2. Create Position Markers
- [ ] Create Empty GameObject named `BedPosition`
- [ ] Move `BedPosition` to the bed location (center of bed)
- [ ] Create Empty GameObject named `StandingPosition`
- [ ] Move `StandingPosition` beside/below bed

### 3. Create Wake-Up Dialogue
- [ ] Right-click in Project
- [ ] Create → Scriptable Objects → Node Graph → Dialog Node Graph
- [ ] Name it `WakeUpFromDream`
- [ ] Double-click to open graph editor
- [ ] Create 2-5 sentence nodes with dialogue:
  - Example: "What a nightmare..."
  - Example: "It was just a dream..."
- [ ] Connect nodes in sequence
- [ ] Save graph

### 4. Configure OverworldWakeUpCutscene Component

Select `OverworldWakeUpManager`, then in Inspector:

#### Character Sprites
- [ ] Assign `nikolausSleeping` to **Nikolaus Sleep Sprite**
- [ ] Assign `nikolausidlefinal` to **Nikolaus Awake Sprite**

#### Bed Setup
- [ ] Drag `BedPosition` GameObject to **Bed Position**
- [ ] Drag `StandingPosition` GameObject to **Standing Position**

#### Audio Clips (Optional - can skip)
- [ ] Assign breathing sound to **Breathing Sound** (if you have one)
- [ ] Assign rustling sound to **Rustling Sound** (if you have one)

#### Dialogue
- [ ] Drag DialogBehaviour from scene to **Dialog Behaviour**
- [ ] Drag `WakeUpFromDream` asset to **Wake Up Dialog Graph**

#### Settings (Optional - use defaults)
- [ ] Fade In Duration: 2 (default is fine)
- [ ] Blink Duration: 0.3 (default is fine)
- [ ] Get Up Speed: 2 (default is fine)

### 5. Verify Scene Setup
- [ ] Nikolaus GameObject exists in Overworld scene
- [ ] Nikolaus has SpriteRenderer component
- [ ] Nikolaus has CharacterMotor2D component
- [ ] Nikolaus has PlayerInput2D component
- [ ] FadeCanvasGroup exists in scene
- [ ] DialogBehaviour exists in scene

### 6. Test
- [ ] Save Overworld scene
- [ ] Open NewTutorial scene
- [ ] Press Play
- [ ] Watch king death sequence
- [ ] Verify scene transitions to Overworld
- [ ] Verify wake-up cutscene plays:
  - [ ] Fades in from black
  - [ ] Nikolaus in bed with sleeping sprite
  - [ ] Eyes blink (sprite changes)
  - [ ] Nikolaus gets out of bed automatically
  - [ ] Dialogue appears
  - [ ] Player control enabled after dialogue

## Quick Dialogue Examples

Choose one or create your own:

**Option 1 - Simple:**
```
1. "What a nightmare..."
2. "It was just a dream."
```

**Option 2 - Confused:**
```
1. "Ugh... my head..."
2. "That dream... the king..."
3. "It felt so real..."
4. "But it was just a dream. Right?"
```

**Option 3 - Mysterious:**
```
1. "The king... murdered..."
2. "Why do I keep seeing this?"
3. "Is it real? Or just a nightmare?"
```

## Common Issues

❌ **Cutscene doesn't play**
- Make sure you start from NewTutorial, not Overworld
- Check scene name is exactly "Overworld" (case-sensitive)

❌ **Nikolaus doesn't appear in bed**
- Verify BedPosition is assigned and at correct location
- Check Nikolaus exists in Overworld scene

❌ **Dialogue doesn't show**
- Verify DialogBehaviour is assigned
- Check dialogue graph is created and connected
- Make sure dialogue nodes are linked

❌ **Can't move after cutscene**
- Check that PlayerInput2D is assigned
- Verify cutscene completes (check Console for completion message)

## That's It! 🎉

Once completed, you'll have a smooth transition from the king murder nightmare to Nikolaus waking up in his bed, confused about the dream!
