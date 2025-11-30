# Unity Inspector Setup Guide for Smart Play Button Flow

## Overview
This guide shows you **exactly where to drag and drop everything** in the Unity Inspector to make the smart play button flow work.

---

## Step 1: Find the MainMenu GameObject

In your Unity scene hierarchy, locate the GameObject that has the `Startscreen` component attached. This is typically named:
- `MainMenu` or
- `StartScreen` or
- Similar name

**Select this GameObject** - all the following assignments happen in **THIS** GameObject's Inspector.

---

## Step 2: Inspector Assignments

Once you've selected the MainMenu GameObject, scroll to the `Startscreen` component in the Inspector.

### ?? Main Menu Elements Section

| Field Name | What to Drag | Where to Find It |
|------------|--------------|------------------|
| `mainMenuRaycaster` | The Canvas component that contains your menu | Usually on a GameObject named "Canvas" or "MainMenuCanvas" |
| `mainMenuLogo` | The GameObject with your "Castle of Time" logo | Usually named "Logo" or "Title" under MainMenu |
| `buttonsParent` | Parent GameObject containing Play/Settings/Credits/Quit | Usually named "Buttons" or "MenuButtons" |
| `mainMenuLogoCanvasGroup` | Same as mainMenuLogo (will auto-detect component) | Drag the same logo GameObject |
| `buttonsCanvasGroup` | Same as buttonsParent (will auto-detect component) | Drag the same buttons parent GameObject |
| `playButton` | The single Play button GameObject | Should be named "PlayButton" in hierarchy |
| `settingsButton` | The Settings button GameObject | Should be named "SettingsButton" |
| `creditsButton` | The Credits button GameObject | Should be named "CreditsButton" |
| `quitButton` | The Quit button GameObject | Should be named "QuitButton" |

---

### ?? Play Choice Menu Section (NEW)

**These are the fields you need to fill for the smart flow to work:**

| Field Name | What to Drag | Where to Find It | Notes |
|------------|--------------|------------------|-------|
| `playChoicePanel` | The PlayChoicePanel GameObject | You need to create this! See Step 3 below | This is the menu that appears when saves exist |
| `playChoiceCanvasGroup` | Same as playChoicePanel | Drag the same PlayChoicePanel GameObject | Unity will automatically get the CanvasGroup component |
| `newGameChoiceButton` | The "Start New Game" button | Inside PlayChoicePanel ? NewGameButton | |
| `continueChoiceButton` | The "Continue" button | Inside PlayChoicePanel ? ContinueButton | |
| `backChoiceButton` | The "Back" button | Inside PlayChoicePanel ? BackButton | |

---

### ?? Save System References Section

| Field Name | What to Drag | Where to Find It | Notes |
|------------|--------------|------------------|-------|
| `saveNamePrompt` | The SaveNamePrompt component | Find GameObject with SaveNamePrompt script | Usually named "SaveNamePromptPanel" or similar |
| `loadGameUI` | The LoadGameUI component | Find GameObject with LoadGameUI script | Usually named "LoadGameUIPanel" or similar |

---

### ?? Other Sections (Already Set Up)

You should already have these configured:
- **Credits Section** - creditLogo, creditText, etc.
- **Settings Section** - settingsComponent
- **Music Section** - musicManager, menuLoop, etc.

---

## Step 3: Create the PlayChoicePanel UI

If you haven't created the PlayChoicePanel yet, follow these steps:

### A. Create the Panel

1. **In Unity Hierarchy:**
   - Right-click on `MainMenu` (or wherever your menu lives)
   - Select: `UI ? Panel`
   - Rename it to: `PlayChoicePanel`

2. **Add CanvasGroup Component:**
   - With PlayChoicePanel selected
   - Click `Add Component`
   - Search for and add: `Canvas Group`

3. **Configure Initial State:**
   - ? Set GameObject **Active** to FALSE (uncheck in Inspector)
   - Set CanvasGroup ? Alpha to `0`
   - Set CanvasGroup ? Interactable to FALSE (unchecked)
   - Set CanvasGroup ? Blocks Raycasts to FALSE (unchecked)

### B. Create the Background

1. **The panel itself acts as background:**
   - Select PlayChoicePanel
   - In Image component:
     - Color: Black (0, 0, 0)
     - Alpha: 200 (out of 255) or 0.8 (80% opacity)

### C. Create the Title Text

1. **Right-click PlayChoicePanel** ? `UI ? Text - TextMeshPro`
2. **Rename to:** `Title`
3. **Configure:**
   - Text: "Welcome Back!" (or "Choose Your Path")
   - Font Size: 48-60
   - Alignment: Center, Middle
   - Color: White or Cream (#FFF8E1)
   - Position: Top center of panel
   - Add Outline if desired (matches your game style)

### D. Create the Buttons

For each button (NewGameButton, ContinueButton, BackButton):

1. **Right-click PlayChoicePanel** ? `UI ? Button - TextMeshPro`
2. **Rename to:**
   - `NewGameButton`
   - `ContinueButton`
   - `BackButton`

3. **Configure Each Button:**
   - Text content:
     - NewGameButton: "Start New Game"
     - ContinueButton: "Continue"
     - BackButton: "Back"
   - Font Size: 24-32
   - Colors: Match your existing menu buttons
   - Size: 200-300 width, 50-60 height
   - Layout: Stack vertically in center of panel

### E. Position Everything

Your layout should look like this:

```
PlayChoicePanel
??? Title (at top)
??? NewGameButton (center, top of stack)
??? ContinueButton (center, middle of stack)
??? BackButton (center, bottom of stack)
```

**Suggested Y positions** (if using anchored position):
- Title: Y = 150
- NewGameButton: Y = 50
- ContinueButton: Y = -20
- BackButton: Y = -90

---

## Step 4: Wire Up Button Click Events

### A. PlayButton (Main Menu)

1. **Select** `PlayButton` in hierarchy
2. **Inspector ? Button Component ? On Click ()**
3. Click the **+** button
4. Drag **MainMenu GameObject** to the object field
5. Select function: `Startscreen ? StartGame()`

### B. NewGameButton (Choice Menu)

1. **Select** `NewGameButton` in PlayChoicePanel
2. **Inspector ? Button Component ? On Click ()**
3. Click the **+** button
4. Drag **MainMenu GameObject** to the object field
5. Select function: `Startscreen ? OnNewGameFromChoice()`

### C. ContinueButton (Choice Menu)

1. **Select** `ContinueButton` in PlayChoicePanel
2. **Inspector ? Button Component ? On Click ()**
3. Click the **+** button
4. Drag **MainMenu GameObject** to the object field
5. Select function: `Startscreen ? OnContinueFromChoice()`

### D. BackButton (Choice Menu)

1. **Select** `BackButton` in PlayChoicePanel
2. **Inspector ? Button Component ? On Click ()**
3. Click the **+** button
4. Drag **MainMenu GameObject** to the object field
5. Select function: `Startscreen ? OnBackFromChoice()`

---

## Step 5: Verify Your Setup

### Checklist

- [ ] MainMenu GameObject has `Startscreen` component
- [ ] All Main Menu Elements fields are assigned
- [ ] PlayChoicePanel exists and is initially INACTIVE
- [ ] PlayChoicePanel has CanvasGroup component (alpha=0)
- [ ] All 3 buttons exist inside PlayChoicePanel
- [ ] All button OnClick events are wired up
- [ ] `playChoicePanel` field is assigned in Inspector
- [ ] `playChoiceCanvasGroup` field is assigned in Inspector
- [ ] `newGameChoiceButton` field is assigned in Inspector
- [ ] `continueChoiceButton` field is assigned in Inspector
- [ ] `backChoiceButton` field is assigned in Inspector
- [ ] `saveNamePrompt` field is assigned in Inspector
- [ ] `loadGameUI` field is assigned in Inspector

---

## Step 6: Test It

### Test 1: First Time (No Saves)

1. Delete any saves from: `C:/Users/[YourUser]/AppData/LocalLow/[Company]/[Game]/Saves/`
2. Run the game
3. Click "Play" button
4. ? Should skip directly to Save Name Prompt (no choice menu)

### Test 2: With Existing Saves

1. Play the game once to create a save
2. Return to main menu or restart game
3. Click "Play" button
4. ? Should show PlayChoicePanel with 3 buttons
5. Click "New Game" ? Should show Save Name Prompt
6. Click "Continue" ? Should show Load Game Menu
7. Click "Back" ? Should return to Main Menu

---

## Troubleshooting

### "PlayChoicePanel not assigned" Error

**Fix:** Select MainMenu GameObject ? Inspector ? Startscreen component ? Drag PlayChoicePanel to the `playChoicePanel` field

### Choice Menu Doesn't Appear

**Check:**
1. PlayChoicePanel exists in hierarchy
2. PlayChoicePanel has CanvasGroup component
3. All button OnClick events point to MainMenu GameObject
4. Console shows: `[MainMenu] Saves detected - showing choice menu`

### Buttons Don't Work

**Check:**
1. EventSystem exists in scene (Create one: GameObject ? UI ? Event System)
2. Each button's OnClick() event is correctly wired
3. Target GameObject is MainMenu (the one with Startscreen component)
4. Function selected is correct (OnNewGameFromChoice, etc.)

### Name Prompt Doesn't Appear

**Check:**
1. SaveNamePrompt GameObject exists
2. Has SaveNamePrompt script component
3. saveNamePrompt field is assigned in MainMenu Inspector

---

## Visual Reference: Complete Inspector

When you select your MainMenu GameObject, the Startscreen component should look like this:

```
???????????????????????????????????????????
? Startscreen (Script)                    ?
???????????????????????????????????????????
? Fade Duration: 1                        ?
???????????????????????????????????????????
? Main Menu Raycaster: [Canvas]           ?
???????????????????????????????????????????
? ? Main Menu Elements                    ?
?   Main Menu Logo: [Logo]                ?
?   Buttons Parent: [Buttons]             ?
?   Main Menu Logo Canvas Group: [Logo]   ?
?   Buttons Canvas Group: [Buttons]       ?
?   Play Button: [PlayButton]             ?
?   Settings Button: [SettingsButton]     ?
?   Credits Button: [CreditsButton]       ?
?   Quit Button: [QuitButton]             ?
???????????????????????????????????????????
? ? Play Choice Menu                      ?
?   Play Choice Panel: [PlayChoicePanel]  ? ? NEW!
?   Play Choice Canvas Group: [PlayChoice]? ? NEW!
?   New Game Choice Button: [NewGameBtn]  ? ? NEW!
?   Continue Choice Button: [ContinueBtn] ? ? NEW!
?   Back Choice Button: [BackButton]      ? ? NEW!
???????????????????????????????????????????
? ? Save System References                ?
?   Save Name Prompt: [SaveNamePrompt]    ? ? NEW!
?   Load Game UI: [LoadGameUI]            ? ? NEW!
???????????????????????????????????????????
? ? Credits                                ?
?   ... (existing credits fields)          ?
???????????????????????????????????????????
? ? Settings                               ?
?   ... (existing settings fields)         ?
???????????????????????????????????????????
? ? Music                                  ?
?   ... (existing music fields)            ?
???????????????????????????????????????????
```

---

## Summary

### What You Created:
- ? PlayChoicePanel with CanvasGroup
- ? 3 buttons inside (New Game, Continue, Back)
- ? Title text

### What You Assigned in Inspector:
- ? playChoicePanel ? PlayChoicePanel GameObject
- ? playChoiceCanvasGroup ? PlayChoicePanel GameObject (auto-detects component)
- ? newGameChoiceButton ? NewGameButton GameObject
- ? continueChoiceButton ? ContinueButton GameObject
- ? backChoiceButton ? BackButton GameObject
- ? saveNamePrompt ? SaveNamePrompt GameObject
- ? loadGameUI ? LoadGameUI GameObject

### What You Wired Up:
- ? PlayButton ? Startscreen.StartGame()
- ? NewGameButton ? Startscreen.OnNewGameFromChoice()
- ? ContinueButton ? Startscreen.OnContinueFromChoice()
- ? BackButton ? Startscreen.OnBackFromChoice()

Your smart play button flow is now complete! ???
