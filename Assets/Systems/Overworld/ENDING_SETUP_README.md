# Ending Cutscene System - Setup Guide

## Overview
The ending system uses dialog node graphs to display text letter-by-letter on a full-width screen, then shows scrolling credits before returning to the main menu. There are three endings based on game flags.

## Components

### 1. EndingCutsceneManager.cs
Main component that:
- Determines which ending to play based on flags
- Renders text directly to screen with letter-by-letter typing
- Manages background images and music
- Triggers scrolling credits after dialog
- Handles fade transitions

### 2. CreditsScroller.cs
Scrolls credits from bottom to top after the ending dialog completes.

## Ending Definitions

### Bad Ending (Default)
- **Priority**: 0 (lowest)
- **Flags Required**: None
- **Flags Forbidden**: None
- **Plays when**: No special flags are set

### Neutral Ending
- **Priority**: 50 (medium)
- **Flags Required**: `ending.killer.found`
- **Flags Forbidden**: `character.avant.heir`
- **Plays when**: Player found the killer but didn't become the heir

### Good Ending
- **Priority**: 100 (highest)
- **Flags Required**: `ending.killer.found` AND `character.avant.heir`
- **Flags Forbidden**: None
- **Plays when**: Player found the killer AND became the heir

## Unity Setup

### Scene Hierarchy Structure
```
EndingCutscene
??? EndingCutsceneManager (with script)
??? EndingCanvas
?   ??? CanvasGroup (for fade in/out)
?   ??? BackgroundImage (Image component)
?   ??? TextCanvas (CanvasGroup)
?   ?   ??? EndingText (TextMeshProUGUI - full width)
?   ??? CreditsCanvas (CanvasGroup)
?       ??? CreditsScroller (with script)
?           ??? CreditsContainer (RectTransform)
?               ??? CreditsText (TextMeshProUGUI)
??? DialogBehaviour (for node graph processing)
```

### EndingCutsceneManager Configuration

#### References to Assign:
1. **Dialog System**
   - `DialogBehaviour`: Reference to DialogBehaviour component

2. **UI References**
   - `Ending Text`: TextMeshProUGUI for displaying ending dialog
   - `Background Image`: Image component for background

3. **Credits**
   - `Credits Scroller`: CreditsScroller component

4. **Canvas Groups**
   - `Ending Canvas Group`: Main canvas group for scene fade
   - `Text Canvas Group`: Canvas group for dialog text
   - `Credits Canvas Group`: Canvas group for credits

5. **Audio** (optional)
   - `Audio Source`: Will be auto-created if not assigned

6. **Screen Fader** (optional)
   - `Screen Fader`: For eye-opening effect

#### Settings:
- **Char Delay**: 0.05 (typing speed)
- **Advance Keys**: Space, Return, Mouse0
- **Fade In Duration**: 2 seconds
- **Fade Out Duration**: 2 seconds
- **Linger Duration**: 3 seconds (pause after dialog before credits)
- **Main Menu Scene Name**: "MainMenu"

### EndingText (TextMeshProUGUI) Setup

#### RectTransform Settings:
- **Anchor Preset**: Stretch (full width and height)
- **Left**: 100 (margin)
- **Right**: 100 (margin)
- **Top**: 100 (margin)
- **Bottom**: 100 (margin)

#### Text Settings:
- **Font Size**: 24-32 (adjust to preference)
- **Alignment**: Center (both horizontal and vertical)
- **Wrapping**: Enabled
- **Color**: White (or your preference)
- **Max Visible Characters**: Will be controlled by script

### CreditsScroller Setup

#### CreditsContainer RectTransform:
- **Anchor**: Middle Center
- **Pivot**: 0.5, 0 (bottom center)
- **Width**: 600-800 (adjust for your design)
- **Height**: Auto (based on text content)

#### CreditsText (TextMeshProUGUI):
- **Alignment**: Center
- **Font Size**: 18-24
- **Line Spacing**: 1.2
- **Wrapping**: Enabled
- **Best Fit**: Disabled

#### CreditsScroller Settings:
- **Scroll Speed**: 50 (adjust to taste)
- **Start Y Offset**: -500 (below screen)
- **End Y Offset**: 1500 (above screen)
- **Allow Skip**: True
- **Skip Key**: Space

#### Edit Credits Text:
Modify the credits text in the Inspector or via script. Default format:
```
CASTLE OF TIME

PRODUCED BY
Name

ART AND ANIMATION
Names...

ENGINEER
Names...

AUDIO
Names...
```

## Creating Ending Dialog Graphs

### 1. Create Dialog Node Graphs
Create three separate DialogNodeGraph assets:
- `Assets/Dialogues/Endings/GoodEnding.asset`
- `Assets/Dialogues/Endings/NeutralEnding.asset`
- `Assets/Dialogues/Endings/BadEnding.asset`

### 2. Design Your Endings
For each ending, create a sequence of **Sentence Nodes** in the node graph editor:

1. Right-click in Node Editor ? Create Sentence Node
2. In the Sentence Node:
   - Leave **Name** field empty (not used for endings)
   - Fill in **Text** field with your ending text
   - Leave **Sprite** empty (not used)
3. Connect nodes in sequence with right-click drag
4. Last node should have no children (dialog ends)

### Example Good Ending Flow:
```
[Sentence 1: "The mystery is solved..."]
    ?
[Sentence 2: "You have proven yourself worthy..."]
    ?
[Sentence 3: "Welcome, Heir of Castle Time."]
    (no child node - ending finishes)
```

### 3. Assign Dialog Graphs to Endings
In EndingCutsceneManager, expand the `Endings` array and assign:
- **Good Ending ? Ending Dialog Graph**: GoodEnding asset
- **Neutral Ending ? Ending Dialog Graph**: NeutralEnding asset
- **Bad Ending ? Ending Dialog Graph**: BadEnding asset

### 4. Add Background Images
For each ending in the array:
- **Background Image**: Assign a Sprite for the ending background
- **Background Color**: White (or tint color)

### 5. Add Music (Optional)
For each ending:
- **Ending Music**: Assign an AudioClip

## Flow Diagram

```
Start
  ?
Check Flags ? Determine Ending (Good/Neutral/Bad)
  ?
Fade In
  ?
Load Dialog Graph
  ?
[For each Sentence Node]
  ?
Display Text Letter-by-Letter
  ?
Wait for Player Click (Space/Enter/Mouse)
  ?
[Next Sentence or End]
  ?
Dialog Complete
  ?
Linger (3 seconds)
  ?
Fade Out Text
  ?
Fade In Credits
  ?
Scroll Credits
  ?
Credits Complete
  ?
Fade Out
  ?
Return to Main Menu
```

## Testing

### Test Each Ending:
1. **Bad Ending**: Clear all flags before loading ending scene
2. **Neutral Ending**: Set only `ending.killer.found` flag
3. **Good Ending**: Set both `ending.killer.found` and `character.avant.heir` flags

### Console Debug Commands (if available):
```csharp
// Clear all flags
GameFlags.ClearAll();

// Set neutral ending flags
GameFlags.SetFlag("ending.killer.found");

// Set good ending flags
GameFlags.SetFlag("ending.killer.found");
GameFlags.SetFlag("character.avant.heir");
```

## Customization Tips

### Adjust Typing Speed
Change `charDelay` in EndingCutsceneManager (lower = faster)

### Adjust Linger Time
Change `lingerDuration` (time between dialog end and credits)

### Change Credits Speed
Modify `scrollSpeed` in CreditsScroller

### Skip Functionality
- During typing: Press Space/Enter/Click to show full text
- When text complete: Press Space/Enter/Click to advance
- During credits: Press Space to skip to end

### Custom Advance Keys
Modify the `advanceKeys` array in EndingCutsceneManager

## Troubleshooting

### Text Not Appearing
- Check that `endingText` reference is assigned
- Verify TextMeshProUGUI is active and visible
- Check text color isn't transparent

### Dialog Not Advancing
- Ensure Sentence Nodes have `ChildNode` connections
- Check that node graph is assigned to ending definition
- Verify DialogBehaviour is present and configured

### Credits Not Showing
- Check `creditsScroller` reference is assigned
- Verify CreditsCanvas is setup with correct CanvasGroup
- Check RectTransform anchors are correct

### Wrong Ending Playing
- Review flag requirements and priorities
- Check console logs for flag detection
- Verify flag names match exactly (case-sensitive)

### Background Not Showing
- Ensure `backgroundImage` reference is assigned
- Check that Sprite is assigned in ending definition
- Verify Image component is setup correctly (Fill type)

## Performance Notes

- System uses coroutines for smooth transitions
- Text typing is frame-independent
- Credits scrolling can be skipped by player
- Music fades out naturally during transitions
