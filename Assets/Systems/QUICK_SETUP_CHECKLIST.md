# ? Quick Start: Save/Load UI Setup Checklist

Use this as your step-by-step guide. Check off each item as you complete it!

---

## Phase 1: SaveNamePromptPanel (10 minutes)

### Create the Panel
- [ ] Right-click `StartScreen` ? UI ? Panel
- [ ] Rename to `SaveNamePromptPanel`
- [ ] Add component: **Canvas Group**
- [ ] Set Canvas Group: Alpha = 0, Interactable = OFF, Blocks Raycasts = OFF
- [ ] Set panel color: Black (Alpha: 200)

### Create Container
- [ ] Right-click SaveNamePromptPanel ? UI ? Panel
- [ ] Rename to `Container`
- [ ] Set size: Width = 400, Height = 250
- [ ] Set position: X = 0, Y = 0 (centered)
- [ ] Set color: Match your game (dark brown or parchment)

### Create Children
- [ ] **TitleText** (UI ? Text - TextMeshPro)
  - Text: "Enter Save Name"
  - Font Size: 32
  - Position: Pos Y = 80
  
- [ ] **SaveNameInput** (UI ? InputField - TextMeshPro)
  - Placeholder: "Enter your name..."
  - Character Limit: 20
  - Width: 350, Height: 50
  - Position: Pos Y = 10
  
- [ ] **ErrorText** (UI ? Text - TextMeshPro)
  - Text: "Error message"
  - Font Size: 18
  - Color: Red
  - Position: Pos Y = -40
  - Set INACTIVE (uncheck GameObject)
  
- [ ] **ConfirmButton** (UI ? Button - TextMeshPro)
  - Text: "Confirm"
  - Width: 150, Height: 50
  - Position: X = -80, Y = -85
  
- [ ] **CancelButton** (UI ? Button - TextMeshPro)
  - Text: "Cancel"
  - Width: 150, Height: 50
  - Position: X = 80, Y = -85

### Add Script
- [ ] Select SaveNamePromptPanel
- [ ] Add Component ? **SaveNamePrompt** script
- [ ] Assign all fields:
  - [ ] Prompt Canvas Group ? SaveNamePromptPanel
  - [ ] Save Name Input ? SaveNameInput
  - [ ] Confirm Button ? ConfirmButton
  - [ ] Cancel Button ? CancelButton
  - [ ] Error Text ? ErrorText
  - [ ] Fade Duration ? 0.5
  - [ ] Min Name Length ? 3
  - [ ] Max Name Length ? 20

### Finalize
- [ ] Select SaveNamePromptPanel
- [ ] **Uncheck the GameObject** (set to INACTIVE)

---

## Phase 2: SaveSlotPrefab (10 minutes)

### Create Prefab
- [ ] Right-click in Hierarchy (NOT inside panels) ? UI ? Panel
- [ ] Rename to `SaveSlotPrefab`
- [ ] Set size: Width = 520, Height = 80
- [ ] Add Component ? **Layout Element**
  - Min Height: 80
  - Preferred Height: 80

### Create Children
- [ ] **SaveNameText** (UI ? Text - TextMeshPro)
  - Text: "Player Name"
  - Font Size: 24
  - Bold: Yes
  - Position: X = -200, Y = 20
  - Alignment: Left, Top
  
- [ ] **DayInfoText** (UI ? Text - TextMeshPro)
  - Text: "Day 1"
  - Font Size: 20
  - Position: X = -200, Y = -10
  
- [ ] **ClockTimeText** (UI ? Text - TextMeshPro)
  - Text: "? 10:00"
  - Font Size: 20
  - Position: X = -200, Y = -30
  
- [ ] **DateText** (UI ? Text - TextMeshPro) - OPTIONAL
  - Text: "Date"
  - Font Size: 16
  - Position: X = 150, Y = 20
  - Can set INACTIVE if not needed
  
- [ ] **LoadButton** (UI ? Button - TextMeshPro)
  - Text: "Load"
  - Width: 100, Height: 35
  - Position: X = 200, Y = 10
  
- [ ] **DeleteButton** (UI ? Button - TextMeshPro)
  - Text: "Delete"
  - Width: 100, Height: 35
  - Position: X = 200, Y = -30
  - Consider red color for delete

### Save as Prefab
- [ ] Create folder: `Assets/Prefabs` (if doesn't exist)
- [ ] Drag SaveSlotPrefab from Hierarchy into Prefabs folder
- [ ] Delete SaveSlotPrefab from Hierarchy (keep only the prefab file)

---

## Phase 3: LoadGameUIPanel (15 minutes)

### Create the Panel
- [ ] Right-click `StartScreen` ? UI ? Panel
- [ ] Rename to `LoadGameUIPanel`
- [ ] Add component: **Canvas Group**
- [ ] Set Canvas Group: Alpha = 0, Interactable = OFF, Blocks Raycasts = OFF
- [ ] Set panel color: Black (Alpha: 200)

### Create Container
- [ ] Right-click LoadGameUIPanel ? UI ? Panel
- [ ] Rename to `Container`
- [ ] Set size: Width = 600, Height = 500
- [ ] Set position: X = 0, Y = 0 (centered)
- [ ] Set color: Match your game

### Create Children
- [ ] **TitleText** (UI ? Text - TextMeshPro)
  - Text: "Load Game"
  - Font Size: 36
  - Position: Pos Y = 220
  
- [ ] **SaveSlotsScrollView** (UI ? Scroll View)
  - Width: 550, Height: 350
  - Position: Pos Y = -10
  - Vertical: ?, Horizontal: ?
  
- [ ] Configure Scroll View **Content** (inside Viewport):
  - [ ] Add component: **Vertical Layout Group**
    - Spacing: 10
    - Child Force Expand: Width ?, Height ?
    - Child Controls Size: Width ?, Height ?
  - [ ] Add component: **Content Size Fitter**
    - Vertical Fit: Preferred Size
  
- [ ] **NoSavesText** (UI ? Text - TextMeshPro)
  - Text: "No saved games found"
  - Font Size: 24
  - Color: Gray
  - Position: Center
  - Set INACTIVE
  
- [ ] **BackButton** (UI ? Button - TextMeshPro)
  - Text: "Back"
  - Width: 150, Height: 50
  - Position: Pos Y = -220

### Create Delete Confirmation
- [ ] Right-click LoadGameUIPanel ? UI ? Panel
- [ ] Rename to `DeleteConfirmPanel`
- [ ] Set size: Width = 350, Height = 200
- [ ] Set color: Very dark (#222222, Alpha: 230)
- [ ] Set INACTIVE

Children of DeleteConfirmPanel:
- [ ] **ConfirmText** (UI ? Text - TextMeshPro)
  - Text: "Delete this save?"
  - Font Size: 24
  - Position: Pos Y = 40
  
- [ ] **YesButton** (UI ? Button - TextMeshPro)
  - Text: "Yes, Delete"
  - Width: 150, Height: 40
  - Position: X = -80, Y = -50
  
- [ ] **NoButton** (UI ? Button - TextMeshPro)
  - Text: "Cancel"
  - Width: 150, Height: 40
  - Position: X = 80, Y = -50

### Add Script
- [ ] Select LoadGameUIPanel
- [ ] Add Component ? **LoadGameUI** script
- [ ] Assign all fields:
  - [ ] Load Game Canvas Group ? LoadGameUIPanel
  - [ ] Save Slot Container ? Content (inside Viewport in ScrollView)
  - [ ] Save Slot Prefab ? SaveSlotPrefab (from Prefabs folder)
  - [ ] Back Button ? BackButton
  - [ ] No Saves Text ? NoSavesText
  - [ ] Fade Duration ? 0.5
  - [ ] Delete Confirm Panel ? DeleteConfirmPanel
  - [ ] Delete Confirm Text ? ConfirmText
  - [ ] Delete Yes Button ? YesButton
  - [ ] Delete No Button ? NoButton

### Finalize
- [ ] Select LoadGameUIPanel
- [ ] **Uncheck the GameObject** (set to INACTIVE)

---

## Phase 4: Connect to MainMenu (5 minutes)

### Assign References
- [ ] Select `MainMenu` GameObject (or `StartScreen` with Startscreen script)
- [ ] In Inspector ? Startscreen component
- [ ] Find **"Save System References"** section
- [ ] Drag SaveNamePromptPanel ? **Save Name Prompt** field
- [ ] Drag LoadGameUIPanel ? **Load Game UI** field

---

## Phase 5: Test Everything (10 minutes)

### Test Save Name Prompt
- [ ] Run game
- [ ] Click "Play"
- [ ] ? SaveNamePromptPanel appears
- [ ] Try typing a name
- [ ] Try name too short (< 3 chars) ? Error should appear
- [ ] Try valid name ? Confirm button enabled
- [ ] Try invalid characters ? Error appears
- [ ] Click Cancel ? Returns to menu
- [ ] Enter valid name ? Click Confirm ? Game starts

### Test Load Game UI
- [ ] Create at least 2 saves
- [ ] Return to main menu
- [ ] Click "Play"
- [ ] Click "Continue"
- [ ] ? LoadGameUIPanel appears
- [ ] ? All saves are listed
- [ ] Each slot shows: Name, Day, Clock time
- [ ] Click Load ? Game loads
- [ ] Click Delete ? Confirmation appears
- [ ] Click Yes ? Save is deleted, list refreshes
- [ ] Click No ? Cancels delete
- [ ] Click Back ? Returns to menu

### Test Flow
- [ ] **No saves:** Play ? Name Prompt (skips choice menu)
- [ ] **With saves:** Play ? Choice Menu ? New Game ? Name Prompt
- [ ] **With saves:** Play ? Choice Menu ? Continue ? Load UI
- [ ] **With saves:** Play ? Choice Menu ? Back ? Main Menu

---

## ?? Troubleshooting

If something doesn't work, check:

### SaveNamePrompt not showing:
- [ ] Is SaveNamePromptPanel assigned in MainMenu Inspector?
- [ ] Is panel INACTIVE before running game?
- [ ] Does panel have CanvasGroup component?
- [ ] Are all fields in SaveNamePrompt script assigned?

### LoadGameUI not showing:
- [ ] Is LoadGameUIPanel assigned in MainMenu Inspector?
- [ ] Is panel INACTIVE before running game?
- [ ] Does panel have CanvasGroup component?
- [ ] Are all fields in LoadGameUI script assigned?
- [ ] Is SaveSlotPrefab assigned?

### No saves showing in list:
- [ ] Do saves exist in: `%AppData%\..\LocalLow\[Company]\[Game]\Saves\`
- [ ] Are files named `GameFlags_[NAME].json`?
- [ ] Is Content assigned as Save Slot Container?
- [ ] Does Content have Vertical Layout Group?

### Can't click buttons:
- [ ] Does EventSystem exist in scene?
- [ ] Is CanvasGroup.interactable = true after fade?
- [ ] Is CanvasGroup.blocksRaycasts = true?

### Save slots not appearing:
- [ ] Is SaveSlotPrefab assigned in Inspector?
- [ ] Does prefab exist in Prefabs folder?
- [ ] Does Content have Vertical Layout Group?
- [ ] Does Content have Content Size Fitter?

---

## ? Success Criteria

You're done when:

- [ ] SaveNamePromptPanel exists and is set up correctly
- [ ] LoadGameUIPanel exists and is set up correctly
- [ ] SaveSlotPrefab exists in Prefabs folder
- [ ] Both panels assigned in MainMenu Inspector
- [ ] All scripts have required fields assigned
- [ ] Both panels start INACTIVE
- [ ] Test flow works: No saves ? Name Prompt
- [ ] Test flow works: With saves ? Choice Menu ? New Game
- [ ] Test flow works: With saves ? Choice Menu ? Continue ? Load UI
- [ ] Saves display correctly in Load UI
- [ ] Load/Delete buttons work
- [ ] All animations work smoothly

---

## ?? Time Estimate

- SaveNamePromptPanel: **10 minutes**
- SaveSlotPrefab: **10 minutes**
- LoadGameUIPanel: **15 minutes**
- Connect to MainMenu: **5 minutes**
- Testing: **10 minutes**

**Total: ~50 minutes**

---

## ?? Bonus: Styling

To match your game's medieval aesthetic:

- [ ] Use the same font as "Castle of Time" title
- [ ] Match button colors (cream/tan from your screenshot)
- [ ] Add subtle texture to panels (parchment, stone)
- [ ] Use cream (#FFF8E1) for text
- [ ] Use dark brown (#3E2723) for panels
- [ ] Consider adding decorative borders

---

You've got this! Follow the checklist step by step and you'll have a fully functional save/load system! ???
