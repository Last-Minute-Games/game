# ?? Quick Inspector Assignment Checklist

## Use this as a quick reference when setting up the Inspector!

---

## Step 1: Find Your MainMenu GameObject

Location in Hierarchy: `MainMenu` or `StartScreen` (has Startscreen component)

---

## Step 2: Create PlayChoicePanel (IF IT DOESN'T EXIST YET)

```
Right-click MainMenu ? UI ? Panel ? Name: "PlayChoicePanel"
  ?
  ?? Add Component: Canvas Group
  ?  ?? Alpha: 0
  ?  ?? Interactable: OFF
  ?  ?? Blocks Raycasts: OFF
  ?
  ?? Set Active: OFF (uncheck in Inspector)
  ?
  ?? Title (TextMeshProUGUI)
  ?  ?? Text: "Welcome Back!"
  ?
  ?? NewGameButton (Button - TextMeshPro)
  ?  ?? Text: "Start New Game"
  ?  ?? OnClick() ? MainMenu ? Startscreen.OnNewGameFromChoice()
  ?
  ?? ContinueButton (Button - TextMeshPro)
  ?  ?? Text: "Continue"
  ?  ?? OnClick() ? MainMenu ? Startscreen.OnContinueFromChoice()
  ?
  ?? BackButton (Button - TextMeshPro)
     ?? Text: "Back"
     ?? OnClick() ? MainMenu ? Startscreen.OnBackFromChoice()
```

---

## Step 3: Inspector Assignments (MainMenu GameObject)

Select `MainMenu` GameObject ? Inspector ? `Startscreen` component:

### ? Play Choice Menu Section

| Field Name | Drag This | From Where |
|-----------|-----------|------------|
| `Play Choice Panel` | PlayChoicePanel GameObject | Your hierarchy |
| `Play Choice Canvas Group` | PlayChoicePanel GameObject | Same as above |
| `New Game Choice Button` | NewGameButton | Inside PlayChoicePanel |
| `Continue Choice Button` | ContinueButton | Inside PlayChoicePanel |
| `Back Choice Button` | BackButton | Inside PlayChoicePanel |

### ? Save System References Section

| Field Name | Drag This | From Where |
|-----------|-----------|------------|
| `Save Name Prompt` | SaveNamePrompt component | Find GameObject with this script |
| `Load Game UI` | LoadGameUI component | Find GameObject with this script |

### ? Main Menu Elements Section (Verify)

| Field Name | Should Point To |
|-----------|----------------|
| `Play Button` | Your single "Play" button |
| `Settings Button` | Your "Settings" button |
| `Credits Button` | Your "Credits" button |
| `Quit Button` | Your "Quit" button |

---

## Step 4: Button OnClick Events

### Main Play Button (on main menu):
```
PlayButton ? Inspector ? Button component ? OnClick()
  ?? Drag: MainMenu GameObject
  ?? Function: Startscreen.StartGame()
```

### NewGameButton (inside PlayChoicePanel):
```
NewGameButton ? Inspector ? Button component ? OnClick()
  ?? Drag: MainMenu GameObject
  ?? Function: Startscreen.OnNewGameFromChoice()
```

### ContinueButton (inside PlayChoicePanel):
```
ContinueButton ? Inspector ? Button component ? OnClick()
  ?? Drag: MainMenu GameObject
  ?? Function: Startscreen.OnContinueFromChoice()
```

### BackButton (inside PlayChoicePanel):
```
BackButton ? Inspector ? Button component ? OnClick()
  ?? Drag: MainMenu GameObject
  ?? Function: Startscreen.OnBackFromChoice()
```

---

## Step 5: Verify Initial States

### PlayChoicePanel:
- [ ] GameObject is **INACTIVE** (unchecked)
- [ ] CanvasGroup ? Alpha = `0`
- [ ] CanvasGroup ? Interactable = `unchecked`
- [ ] CanvasGroup ? Blocks Raycasts = `unchecked`

### MainMenu Startscreen Component:
- [ ] All "Play Choice Menu" fields assigned (5 fields)
- [ ] All "Save System References" fields assigned (2 fields)
- [ ] All button OnClick events wired up (4 buttons)

---

## ?? Quick Test

### Test 1: No Saves
1. Delete saves folder
2. Run game
3. Click "Play"
4. ? Should skip to Save Name Prompt

### Test 2: With Saves
1. Create a save
2. Return to menu
3. Click "Play"
4. ? Should show PlayChoicePanel

---

## ?? Common Issues

### "PlayChoicePanel not assigned!"
? Drag PlayChoicePanel to the field in Inspector

### Buttons don't work
? Check OnClick events point to MainMenu GameObject

### Choice menu doesn't appear
? Verify save files exist in: `%AppData%\..\LocalLow\[Company]\[Game]\Saves\`

### Can't click buttons in choice menu
? Check EventSystem exists in scene (GameObject ? UI ? Event System)

---

## ? You're Done When:

- [ ] PlayChoicePanel exists and is set up
- [ ] All Inspector fields in MainMenu are green (assigned)
- [ ] All button OnClick events are wired
- [ ] Both test cases pass (with and without saves)

---

**Need more details?** See `INSPECTOR_SETUP_GUIDE.md` for the full guide.
