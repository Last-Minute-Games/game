# QUICK START: Setting Up Save/Load Menu (5 Minutes)

## What You Need to Do

The code is done ? - you just need to set up the UI in Unity!

---

## Step 1: Choose Your Main Menu Style (30 seconds)

Pick ONE:

### Option A: Two Separate Buttons (Recommended ?)
```
Main Menu
??? [New Game]    ? Creates new save
??? [Continue]    ? Loads existing save
??? [Settings]
??? [Credits]
??? [Quit]
```

### Option B: Single Play Button with Submenu
```
Main Menu
??? [Play] ? Opens submenu:
?            ??? [New Game]
?            ??? [Load Game]
??? [Settings]
??? [Credits]
??? [Quit]
```

**?? Use Option A - it's simpler and cleaner**

---

## Step 2: Update MainMenu Scene UI (2 minutes)

### If Using Option A (Recommended):

1. **Rename existing button** "PlayButton" ? "NewGameButton"
2. **Duplicate it** to create "ContinueButton"
3. **Position buttons** vertically in menu

### If Using Option B:

1. Keep "PlayButton" as is
2. Create a new panel "PlayChoicePanel" (hidden by default)
3. Add two buttons inside: "NewGameButton" and "LoadGameButton"

---

## Step 3: Create SaveNamePrompt Panel (1 minute)

Right-click in Hierarchy ? UI ? Panel ? Name it "SaveNamePromptPanel"

Add these inside:
- **TMP_InputField** (name it "NameInputField")
- **Button** "ConfirmButton" with text "Start Game"
- **Button** "CancelButton" with text "Cancel"
- **TextMeshProUGUI** "ErrorText" (red color, initially hidden)

Add component: **CanvasGroup** to the panel

---

## Step 4: Create LoadGameUI Panel (1 minute)

Right-click in Hierarchy ? UI ? Panel ? Name it "LoadGameUIPanel"

Add these inside:
- **ScrollView** with Vertical Layout Group
- **TextMeshProUGUI** "NoSavesText" (shows when no saves)
- **Button** "BackButton"
- **Panel** "DeleteConfirmPanel" (hidden by default)
  - Add inside: "Yes" and "No" buttons

Add component: **CanvasGroup** to the panel

---

## Step 5: Create SaveSlot Prefab (30 seconds)

Right-click in Hierarchy ? UI ? Panel ? Name it "SaveSlotPrefab"

Add these TextMeshProUGUI inside (EXACT NAMES):
- **SaveNameText** (bold, large, white)
- **DayInfoText** (regular, smaller, gray)
- **ClockTimeText** (monospace, gold color)

Add buttons:
- **LoadButton**
- **DeleteButton**

Drag to Project folder to make it a prefab, then delete from scene.

---

## Step 6: Wire Everything Up in Inspector (30 seconds)

### Select MainMenu GameObject:

In **Startscreen** component:
- Drag **SaveNamePromptPanel** to `saveNamePrompt` field
- Drag **LoadGameUIPanel** to `loadGameUI` field

### Select LoadGameUIPanel:

In **LoadGameUI** component:
- Drag **SaveSlotPrefab** to `saveSlotPrefab` field
- Drag **ScrollView/Content** to `saveSlotContainer` field
- Drag **BackButton** to `backButton` field
- Drag **NoSavesText** to `noSavesText` field
- Drag **DeleteConfirmPanel** to `deleteConfirmPanel` field

### Select SaveNamePromptPanel:

In **SaveNamePrompt** component (add if missing):
- Drag all the child components to their fields

---

## Step 7: Connect Button Events (30 seconds)

### NewGameButton:
- Inspector ? Button ? OnClick()
- Drag **MainMenu** GameObject
- Select `Startscreen.OnNewGameClicked`

### ContinueButton:
- Inspector ? Button ? OnClick()
- Drag **MainMenu** GameObject
- Select `Startscreen.OnContinueClicked`

**That's it!** The rest auto-wires internally.

---

## Step 8: Update MainMenu.cs Code ? COMPLETE

**This step has been completed automatically!** The MainMenu.cs file has been updated with:

- ? Renamed `playButton` ? `newGameButton`
- ? Renamed `loadGameButton` ? `continueButton`
- ? Updated `Start()` method to find new button names
- ? Added `UpdateContinueButton()` method
- ? Added `CheckIfAnySavesExist()` method
- ? Added `OnNewGameClicked()` method
- ? Added `OnContinueClicked()` method
- ? Updated all references to use new button names

**What you still need to do:**
- Wire up the button OnClick() events in Unity Inspector (see Step 7)
- Update your UI scene with the new button names (see Steps 1-6)

---

## Test It! (1 minute)

1. **Press Play** in Unity
2. **Click New Game**
3. **Enter a name** "Test"
4. **Click Start Game**
5. **Save created!** ?

6. **Return to menu**
7. **Continue button enabled!** ?
8. **Click Continue**
9. **Your save appears!** ?

---

## What Happens Automatically

? Game auto-saves when day progresses  
? Clock time is saved  
? Load menu shows: name, day, clock time  
? Continue button disabled when no saves  
? Delete button has confirmation dialog  

---

## Troubleshooting

**Continue button not working?**
- Make sure you renamed the variable from `loadGameButton` to `continueButton`

**Save name prompt doesn't appear?**
- Check if SaveNamePrompt component exists on the panel
- Verify `saveNamePrompt` field is assigned in Inspector

**Load game UI empty?**
- Check if `saveSlotPrefab` is assigned
- Verify save folder exists (create a save first!)

**Buttons don't respond?**
- Check EventSystem exists in scene
- Verify OnClick() events are wired correctly

---

## That's It!

Your save/load system is now complete! The game will:
- ? Auto-save when days progress
- ? Show beautiful save slots with day and time
- ? Handle multiple saves per player
- ? Restore clock time when loading

**Want more details?** Check:
- `MAIN_MENU_SAVE_FLOW_SETUP.md` - Full setup guide
- `SAVE_FLOW_DIAGRAMS.md` - Visual flowcharts
- `SAVE_SLOT_UI_REFERENCE.md` - UI styling guide
