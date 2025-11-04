# 🛏️ Overworld Wake-Up Cutscene - Setup Guide

## What This Does

After the king gets murdered in the tutorial scene, the character wakes up in their bed in the Overworld scene, realizing it was just a dream. The cutscene includes:

1. Fade in from black
2. Character blinking eyes (waking up)
3. Character automatically getting out of bed
4. Internal monologue: "Oh, it was just a dream..."

## Quick Setup (5 Minutes)

### Step 1: In Overworld Scene

1. **Create the Cutscene Manager**:
   - Create Empty GameObject → Name it `OverworldWakeUpManager`
   - Add Component → `OverworldWakeUpCutscene`

2. **Create Position Markers**:
   - Create Empty GameObject → Name it `BedPosition`
     - Move it to where Nikolaus should be lying in bed
   - Create Empty GameObject → Name it `StandingPosition`
     - Move it beside/below the bed where he should stand

### Step 2: Assign References in Inspector

Select `OverworldWakeUpManager` and fill in:

#### Character Sprites
- **Nikolaus Sleep Sprite**: Your `nikolausSleeping` sprite
- **Nikolaus Awake Sprite**: Your `nikolausidlefinal` sprite (or create an "eyes open" variant)

#### Bed Setup
- **Bed Position**: Drag the `BedPosition` GameObject
- **Standing Position**: Drag the `StandingPosition` GameObject

#### Audio Clips (Optional)
- **Breathing Sound**: Optional gasp/breathing sound when waking
- **Rustling Sound**: Optional bed sheets sound

#### Dialogue
- **Dialog Behaviour**: Drag your scene's `DialogBehaviour` component
- **Wake Up Dialog Graph**: Create dialogue (see next step)

### Step 3: Create the "It Was Just a Dream" Dialogue

1. **Create DialogNodeGraph**:
   ```
   Right-click in Project → Create → Scriptable Objects → Node Graph → Dialog Node Graph
   Name: "WakeUpFromDream"
   ```

2. **Open the graph and create sentence nodes**:

   **Example dialogue**:
   ```
   Node 1: "Ugh... my head..."
   Node 2: "That dream... the king..."
   Node 3: "It felt so real..."
   Node 4: "But it was just a dream. Right?"
   ```

   **Or simpler**:
   ```
   Node 1: "What a nightmare..."
   Node 2: "It was just a dream. Just a dream."
   ```

3. **Style for internal monologue**:
   - Character Name: Leave empty or use "Nikolaus"
   - Character Portrait: Leave empty (indicates internal thoughts)
   - Connect nodes in sequence

4. **Assign to cutscene**:
   - Drag `WakeUpFromDream` to the `Wake Up Dialog Graph` field

### Step 4: Position Nikolaus in Bed

In your Overworld scene, ensure:
- There's a bed sprite/object where you want Nikolaus to wake up
- The `BedPosition` marker is on the bed (where he lies)
- The `StandingPosition` marker is next to the bed (where he walks to)

## How It Works

### Trigger Flow
```
King Murder Scene → King dies → Screen corrupts → 
Load Overworld → Wake-up cutscene plays automatically → Dialogue → Gameplay
```

### What Happens
1. TutorialScene calls `OverworldWakeUpCutscene.TriggerWakeUpCutscene()` before loading Overworld
2. Overworld scene loads
3. `OverworldWakeUpCutscene` detects the trigger flag
4. Plays the wake-up sequence
5. Clears the flag so it only plays once

## Sprite Details

You have:
- ✅ `nikolausSleeping.aseprite` - Perfect for sleeping state
- ✅ `nikolausidlefinal.aseprite` - Perfect for awake/standing state

The script will:
1. Start with sleeping sprite
2. Blink eyes (alternating between sleeping and awake sprites)
3. End with awake sprite
4. Move character while showing awake sprite

**Optional Enhancement**: Create an intermediate sprite with eyes half-open for more realistic blinking, but not required!

## Testing

### Method 1: Play Through
1. Start from NewTutorial scene
2. Watch king death sequence
3. Should automatically transition to Overworld with wake-up

### Method 2: Force Trigger (For Testing)
Add this to any script and call it:
```csharp
OverworldWakeUpCutscene.TriggerWakeUpCutscene();
UnityEngine.SceneManagement.SceneManager.LoadScene("Overworld");
```

## Customization

In the Inspector, you can adjust:
- **Fade In Duration**: How long it takes to fade from black (default: 2s)
- **Blink Duration**: How long eyes stay open during blinks (default: 0.3s)
- **Get Up Speed**: How fast character moves from bed to standing (default: 2)

## Example Dialogue Ideas

### Short & Simple
```
"What a nightmare..."
"It was just a dream."
```

### Medium
```
"That dream again..."
"The king... his death felt so real."
"But it couldn't be real. Could it?"
```

### Detailed (Recommended)
```
"Ugh... my head..."
"That dream... the king was murdered..."
"It felt so vivid. The blood, the screams..."
"But it was just a dream. Just a dream."
"I need to get up and clear my head."
```

### Mysterious
```
"The king... dead..."
"Why do I keep seeing this?"
"Is it a warning? Or just my mind playing tricks?"
"I need answers."
```

## Troubleshooting

**Cutscene doesn't play:**
- Make sure you play from NewTutorial scene, not Overworld
- Check that PlayerPrefs is working (may not work in some Editor modes)
- Verify scene name is exactly "Overworld"

**Character doesn't appear in bed:**
- Check `BedPosition` is assigned and positioned correctly
- Verify Nikolaus GameObject exists in Overworld scene and is active
- Make sure SpriteRenderer, CharacterMotor2D, and PlayerInput2D components are on Nikolaus

**Dialogue doesn't show:**
- Verify DialogBehaviour component exists in Overworld scene
- Check that WakeUpDialogGraph is created and assigned
- Make sure dialogue nodes are connected in the graph

**Character doesn't move:**
- Verify both `BedPosition` and `StandingPosition` are assigned
- Check they're in different positions
- Ensure CharacterMotor2D component is assigned

## Files Modified

✅ **Created**:
- `OverworldWakeUpCutscene.cs` - The main cutscene script

✅ **Modified**:
- `TutorialScene.cs` - Added trigger call before scene transition

## Next Steps

1. ✅ Set up the GameObject and positions in Overworld scene
2. ✅ Assign your sprites
3. ✅ Create the dialogue graph
4. ✅ Test by playing from NewTutorial scene
5. ✅ Adjust timings and dialogue to your liking

The cutscene will feel like the BedIntroCutscene but works perfectly for the dream-to-reality transition! 🎮✨
