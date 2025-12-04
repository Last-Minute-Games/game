# Ending Cutscene System - Setup Guide

## Overview
The ending system uses dialog node graphs to display text letter-by-letter on a full-width screen. Each sentence node in the graph is displayed one at a time, advancing when the player clicks. After all sentences are shown, scrolling credits appear before returning to the main menu. There are three endings based on game flags.

## Components

### 1. EndingCutsceneManager.cs
Main component that:
- Determines which ending to play based on flags
- Reads through DialogNodeGraph sentence by sentence
- Renders text directly to screen with letter-by-letter typing
- Advances to next sentence node on player click
- Manages background images and music
- Triggers scrolling credits after all dialog nodes
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
?   ??? BackgroundImage (Image component - full screen)
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
- **Char Delay**: 0.05 (typing speed per character)
- **Advance Keys**: Space, Return, Mouse0 (keys to skip typing or advance)
- **Fade In Duration**: 2 seconds
- **Fade Out Duration**: 2 seconds
- **Linger Duration**: 3 seconds (pause after all dialog before credits)
- **Main Menu Scene Name**: "MainMenu"

### EndingText (TextMeshProUGUI) Setup

#### RectTransform Settings:
- **Anchor Preset**: Stretch (full width and height)
- **Left**: 100 (margin from left)
- **Right**: 100 (margin from right)
- **Top**: 100 (margin from top)
- **Bottom**: 100 (margin from bottom)

#### Text Settings:
- **Font Size**: 24-32 (adjust to preference)
- **Alignment**: Center (both horizontal and vertical)
- **Wrapping**: Enabled
- **Color**: White (or your preference)
- **Max Visible Characters**: Controlled by script (leave at default)

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

### 2. Design Your Endings with Sentence Nodes
For each ending, create a **chain of Sentence Nodes** in the node graph editor:

#### Creating the Chain:
1. Open the node graph (double-click the asset)
2. Right-click ? Create Sentence Node
3. Fill in the **Text** field with your ending text (one sentence/paragraph per node)
4. Leave **Name** and **Sprite** fields empty (not used for endings)
5. Create another Sentence Node for the next text
6. **Connect them**: Right-click on first node ? drag to second node
7. Repeat for as many sentences as you want
8. The last node should have **no child** (ends the dialog)

#### Important Notes:
- ? **Only use Sentence Nodes** in ending graphs
- ? Connect nodes in a **linear chain** (no branching)
- ? First node should have **no parent nodes**
- ? Last node should have **no child nodes**
- ? Don't use Answer Nodes (not supported for endings)
- ? Don't leave Name or Sprite fields filled (they're ignored)

### Example Good Ending Flow:
```
[Start] ? [Sentence 1: "The castle's secrets are revealed..."]
              ?
          [Sentence 2: "You have proven yourself worthy..."]
              ?
          [Sentence 3: "Welcome, Heir of Castle Time."]
              ?
          [End] (no child node)
```

Each sentence will:
1. Type out letter-by-letter on screen
2. Wait for player to click (Space/Enter/Mouse)
3. Advance to next sentence node
4. Repeat until no more nodes

### 3. Assign Dialog Graphs to Endings
In EndingCutsceneManager Inspector:
1. Expand the `Endings` array (should show 3 elements)
2. For each ending:
   - **Good Ending (Element 0)**:
     - Ending Dialog Graph: Assign `GoodEnding.asset`
     - Background Image: Assign good ending sprite
     - Background Color: White (or tint)
     - Ending Music: (optional) Assign AudioClip
   
   - **Neutral Ending (Element 1)**:
     - Ending Dialog Graph: Assign `NeutralEnding.asset`
     - Background Image: Assign neutral ending sprite
     - Background Color: White (or tint)
     - Ending Music: (optional) Assign AudioClip
   
   - **Bad Ending (Element 2)**:
     - Ending Dialog Graph: Assign `BadEnding.asset`
     - Background Image: Assign bad ending sprite
     - Background Color: White (or tint)
     - Ending Music: (optional) Assign AudioClip

## Flow Diagram

```
Start
  ?
Check Flags ? Determine Ending (Good/Neutral/Bad)
  ?
Load Appropriate Dialog Graph
  ?
Fade In (with background image)
  ?
?????????????????????????????????
For Each Sentence Node in Graph:
  ?
  Display Text Letter-by-Letter
  ?
  [Player clicks]
    ? If typing: Skip to full text
    ? If text complete: Advance to next node
  ?
  Next Sentence Node
?????????????????????????????????
  ?
All Nodes Complete
  ?
Linger (3 seconds)
  ?
Fade Out Text
  ?
Fade In Credits
  ?
Scroll Credits (can skip with Space)
  ?
Credits Complete
  ?
Fade Out
  ?
Return to Main Menu
```

## Player Experience

1. **Game ends** and loads ending scene
2. **Screen fades in** with background image
3. **First sentence appears** letter-by-letter
4. **Player clicks**:
   - If text is typing ? Shows full text immediately
   - If text is complete ? Advances to next sentence
5. **Repeat** for each sentence node in the graph
6. **After last sentence**, text lingers for 3 seconds
7. **Text fades out**, credits fade in
8. **Credits scroll** upward (can skip with Space)
9. **Fades to main menu**

## Testing

### Test Each Ending:

#### Bad Ending Test:
```csharp
// Clear all flags
GameFlags.ClearAll();
// Load ending scene
SceneManager.LoadScene("EndingScene");
```

#### Neutral Ending Test:
```csharp
// Set neutral ending flag
GameFlags.SetFlag("ending.killer.found");
// Make sure heir flag is NOT set
GameFlags.RemoveFlag("character.avant.heir");
// Load ending scene
SceneManager.LoadScene("EndingScene");
```

#### Good Ending Test:
```csharp
// Set both flags
GameFlags.SetFlag("ending.killer.found");
GameFlags.SetFlag("character.avant.heir");
// Load ending scene
SceneManager.LoadScene("EndingScene");
```

### Verify Node Graph Flow:
1. Open your ending dialog graph in Node Editor
2. Check that:
   - First node has no parent nodes
   - All sentence nodes are connected in sequence
   - Last node has no child node
   - All text fields are filled in
   - Name and Sprite fields are empty

### Console Debugging:
Watch console logs to see:
- Which ending is selected: `"Playing ending: Good Ending"`
- Each sentence activation: `"New sentence: [text]..."`
- Node advancement: `"Advancing to next node"`
- Dialog completion: `"All dialog nodes finished"`

## Customization Tips

### Adjust Typing Speed
Change `charDelay` in EndingCutsceneManager (lower = faster)
- `0.05` = normal speed
- `0.03` = faster
- `0.08` = slower

### Adjust Linger Time
Change `lingerDuration` (time between last sentence and credits)
- Default: 3 seconds
- Recommended range: 2-5 seconds

### Change Credits Speed
Modify `scrollSpeed` in CreditsScroller
- `50` = normal
- `75` = faster
- `30` = slower

### Multiple Paragraphs Per Screen
You can include multiple paragraphs in a single Sentence Node's Text field:
```
This is paragraph one.

This is paragraph two.

This is paragraph three.
```
Player still needs to click to advance to the next node.

### Change Advance Keys
Modify the `advanceKeys` array in EndingCutsceneManager to add/remove input keys.

## Troubleshooting

### Text Not Appearing
- Check that `endingText` reference is assigned
- Verify TextMeshProUGUI is active and visible
- Check text color isn't transparent
- Verify Canvas is on correct layer and Camera is setup

### Dialog Not Advancing on Click
- Ensure sentence nodes are properly connected (right-click drag)
- Check console for "Advancing to next node" messages
- Verify `advanceKeys` array has at least one key assigned
- Check that DialogBehaviour is assigned and active

### Multiple Sentences Skip Too Fast
- This is expected if nodes aren't connected properly
- Verify each node has a child node (except the last one)
- Check that connections show in Node Editor window

### Credits Not Showing
- Check `creditsScroller` reference is assigned
- Verify CreditsCanvas CanvasGroup is setup correctly
- Check RectTransform pivot and anchors match guide
- Verify all dialog nodes completed before credits trigger

### Wrong Ending Playing
- Review flag requirements in endings array
- Check priority values (higher = checked first)
- Check console logs for flag detection
- Verify flag names match exactly (case-sensitive)

### Background Not Showing
- Ensure `backgroundImage` reference is assigned
- Check that background Sprite is assigned in ending definition
- Verify Image component stretch settings
- Check that image isn't behind other UI elements

### Typing Too Fast/Slow
- Adjust `charDelay` value (per character delay)
- Lower values = faster typing
- Higher values = slower typing

## Advanced: Dynamic Text Length
Each sentence node can contain as much text as you want. The system automatically:
- Types out all text letter-by-letter
- Wraps text based on your TextMeshProUGUI settings
- Waits for player input before advancing
- Handles any length of text without modification

## Performance Notes

- System uses coroutines for smooth transitions
- Text typing is frame-independent
- Node graph traversal is efficient (linear)
- Credits scrolling can be skipped by player
- Music fades out naturally during transitions
- No memory leaks from event subscriptions (properly unsubscribed)
