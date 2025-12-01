# ?? How to Set Up Save Name Prompt and Load Game UI

Based on your screenshot, here's exactly what you need to do in Unity to set up the save/load system.

---

## ?? What You Currently Have

Looking at your hierarchy, I can see you have:
- ? Main Camera
- ? StartScreen
- ? Buttons (Play, Settings, Credits, Quit)

---

## ?? What You Need to Create

You need to create **TWO UI panels**:

1. **SaveNamePromptPanel** - For entering save names
2. **LoadGameUIPanel** - For displaying and loading saves

---

## ??? Step 1: Create SaveNamePromptPanel

### A. Create the Panel

1. **In Hierarchy:** Right-click on `StartScreen`
2. **Select:** `UI ? Panel`
3. **Rename to:** `SaveNamePromptPanel`

### B. Add CanvasGroup Component

1. **Select** `SaveNamePromptPanel`
2. Click **Add Component**
3. Search for and add: **Canvas Group**

### C. Configure the Panel

**In Inspector for SaveNamePromptPanel:**
```
? GameObject: ACTIVE (checked for now, we'll set it later)
? Canvas Group:
  - Alpha: 0
  - Interactable: OFF (unchecked)
  - Blocks Raycasts: OFF (unchecked)
  
? Image Component:
  - Color: Black (0, 0, 0, 200) - 78% alpha for dark overlay
```

### D. Create UI Elements Inside SaveNamePromptPanel

#### 1. Background/Container Panel
```
Right-click SaveNamePromptPanel ? UI ? Panel
Rename: "Container"
Size: 400 x 250 (in Rect Transform)
Position: Center of parent (Pos X: 0, Pos Y: 0)
Color: Dark brown or medieval parchment color to match your game
```

#### 2. Title Text
```
Right-click Container ? UI ? Text - TextMeshPro
Rename: "TitleText"
Text: "Enter Save Name"
Font Size: 32-36
Color: White or cream
Alignment: Center, Middle
Position: Top of container (Pos Y: 80)
Width: 350
```

#### 3. Input Field
```
Right-click Container ? UI ? InputField - TextMeshPro
Rename: "SaveNameInput"
Placeholder Text: "Enter your name..."
Character Limit: 20 (set in TMP_InputField component)
Font Size: 24
Width: 350
Height: 50
Position: Center (Pos Y: 10)
```

#### 4. Error Text (initially hidden)
```
Right-click Container ? UI ? Text - TextMeshPro
Rename: "ErrorText"
Text: "Error message will appear here"
Font Size: 18
Color: Red (#FF4444)
Alignment: Center
Position: Below input (Pos Y: -40)
Width: 350
Active: OFF (uncheck GameObject in Inspector)
```

#### 5. Confirm Button
```
Right-click Container ? UI ? Button - TextMeshPro
Rename: "ConfirmButton"
Text: "Confirm"
Font Size: 24
Width: 150
Height: 50
Position: Bottom left of container (Pos X: -80, Pos Y: -85)
Match your existing button style (same colors as Play/Settings buttons)
```

#### 6. Cancel Button
```
Right-click Container ? UI ? Button - TextMeshPro
Rename: "CancelButton"
Text: "Cancel"
Font Size: 24
Width: 150
Height: 50
Position: Bottom right of container (Pos X: 80, Pos Y: -85)
Match your existing button style
```

### E. Add SaveNamePrompt Script

1. **Select** `SaveNamePromptPanel`
2. Click **Add Component**
3. Search for: `SaveNamePrompt` (the script already exists in your project)
4. **Assign the fields in Inspector:**

| Field | Drag This |
|-------|-----------|
| `Prompt Canvas Group` | SaveNamePromptPanel (itself) |
| `Save Name Input` | SaveNameInput (the InputField) |
| `Confirm Button` | ConfirmButton |
| `Cancel Button` | CancelButton |
| `Error Text` | ErrorText |
| `Fade Duration` | 0.5 (default) |
| `Min Name Length` | 3 |
| `Max Name Length` | 20 |

### F. Final Step for SaveNamePromptPanel

**Set the panel to INACTIVE:**
1. Select `SaveNamePromptPanel`
2. **Uncheck the checkbox** next to the name in Inspector (deactivate it)

---

## ??? Step 2: Create LoadGameUIPanel

### A. Create the Panel

1. **In Hierarchy:** Right-click on `StartScreen`
2. **Select:** `UI ? Panel`
3. **Rename to:** `LoadGameUIPanel`

### B. Add CanvasGroup Component

1. **Select** `LoadGameUIPanel`
2. Click **Add Component**
3. Search for and add: **Canvas Group**

### C. Configure the Panel

**In Inspector for LoadGameUIPanel:**
```
? GameObject: ACTIVE (checked for now)
? Canvas Group:
  - Alpha: 0
  - Interactable: OFF (unchecked)
  - Blocks Raycasts: OFF (unchecked)
  
? Image Component:
  - Color: Black (0, 0, 0, 200) - 78% alpha for dark overlay
```

### D. Create UI Elements Inside LoadGameUIPanel

#### 1. Background/Container Panel
```
Right-click LoadGameUIPanel ? UI ? Panel
Rename: "Container"
Size: 600 x 500 (wider to fit save slots)
Position: Center (Pos X: 0, Pos Y: 0)
Color: Dark brown or medieval parchment color
```

#### 2. Title Text
```
Right-click Container ? UI ? Text - TextMeshPro
Rename: "TitleText"
Text: "Load Game"
Font Size: 36-40
Color: White or cream
Alignment: Center
Position: Top of container (Pos Y: 220)
```

#### 3. Scroll View for Save Slots
```
Right-click Container ? UI ? Scroll View
Rename: "SaveSlotsScrollView"
Size: 550 x 350
Position: Center (Pos Y: -10)

Configure Scroll View:
  - Vertical: ? Checked
  - Horizontal: ? Unchecked
  - Movement Type: Elastic
  - Scrollbar Visibility: Auto Hide
```

**Find and configure these children:**
- **Viewport:** (already created by Scroll View)
  - Add **Mask** component if not present
- **Content:** (inside Viewport)
  - **This is the `Save Slot Container`** - you'll assign this field later
  - Add component: **Vertical Layout Group**
    - Spacing: 10
    - Child Force Expand: Width ?, Height ?
    - Child Controls Size: Width ?, Height ?
  - Add component: **Content Size Fitter**
    - Vertical Fit: Preferred Size

#### 4. No Saves Text (initially hidden)
```
Right-click Container ? UI ? Text - TextMeshPro
Rename: "NoSavesText"
Text: "No saved games found"
Font Size: 24
Color: Gray (#888888)
Alignment: Center, Middle
Position: Center (Pos Y: 0)
Active: OFF (uncheck GameObject)
```

#### 5. Back Button
```
Right-click Container ? UI ? Button - TextMeshPro
Rename: "BackButton"
Text: "Back"
Font Size: 24
Width: 150
Height: 50
Position: Bottom center (Pos Y: -220)
Match your existing button style
```

#### 6. Delete Confirmation Panel (hidden by default)
```
Right-click LoadGameUIPanel ? UI ? Panel
Rename: "DeleteConfirmPanel"
Size: 350 x 200
Position: Center (Pos X: 0, Pos Y: 0)
Color: Very dark (#222222, Alpha: 230)
Active: OFF (uncheck GameObject)

Children:
  - ConfirmText (TextMeshPro)
    - Text: "Delete this save?"
    - Font Size: 24
    - Position: Top (Pos Y: 40)
    
  - YesButton (Button)
    - Text: "Yes, Delete"
    - Size: 150 x 40
    - Position: Bottom left (Pos X: -80, Pos Y: -50)
    - Color: Red-ish for danger
    
  - NoButton (Button)
    - Text: "Cancel"
    - Size: 150 x 40
    - Position: Bottom right (Pos X: 80, Pos Y: -50)
```

### E. Create SaveSlot Prefab

You need a prefab for individual save slot entries.

1. **In Hierarchy:** Right-click in empty space (NOT inside panels)
2. **Create:** `UI ? Panel`
3. **Rename to:** `SaveSlotPrefab`

#### Configure SaveSlotPrefab:

**Main Panel:**
```
Size: 520 x 80
Layout Element component:
  - Min Height: 80
  - Preferred Height: 80
```

**Children (create these inside SaveSlotPrefab):**

1. **SaveNameText** (TextMeshPro)
   - Text: "Player Name"
   - Font Size: 24
   - Bold: Yes
   - Position: Left top (Pos X: -200, Pos Y: 20)
   - Alignment: Left, Top

2. **DayInfoText** (TextMeshPro)
   - Text: "Day 1"
   - Font Size: 20
   - Position: Left middle (Pos X: -200, Pos Y: -10)
   - Alignment: Left, Middle

3. **ClockTimeText** (TextMeshPro)
   - Text: "? 10:00"
   - Font Size: 20
   - Position: Left bottom (Pos X: -200, Pos Y: -30)
   - Alignment: Left, Bottom

4. **DateText** (TextMeshPro) - OPTIONAL
   - Text: "Jan 15, 2024"
   - Font Size: 16
   - Position: Right top (Pos X: 150, Pos Y: 20)
   - Color: Gray
   - Can set INACTIVE if you don't want to show dates

5. **LoadButton** (Button)
   - Text: "Load"
   - Size: 100 x 35
   - Position: Right middle (Pos X: 200, Pos Y: 10)
   - Match your button style

6. **DeleteButton** (Button)
   - Text: "Delete"
   - Size: 100 x 35
   - Position: Right middle (Pos X: 200, Pos Y: -30)
   - Color: Red-ish for delete action

#### Turn SaveSlotPrefab into a Prefab:

1. **In Project window:** Navigate to `Assets/Prefabs` (or create this folder)
2. **Drag** `SaveSlotPrefab` from Hierarchy into the Prefabs folder
3. **In Hierarchy:** Delete the SaveSlotPrefab instance (we only need the prefab)

### F. Add LoadGameUI Script

1. **Select** `LoadGameUIPanel`
2. Click **Add Component**
3. Search for: `LoadGameUI` (the script exists)
4. **Assign the fields in Inspector:**

| Field | Drag This |
|-------|-----------|
| `Load Game Canvas Group` | LoadGameUIPanel (itself) |
| `Save Slot Container` | Content (inside Viewport in ScrollView) |
| `Save Slot Prefab` | SaveSlotPrefab (from Prefabs folder) |
| `Back Button` | BackButton |
| `No Saves Text` | NoSavesText |
| `Fade Duration` | 0.5 (default) |
| `Delete Confirm Panel` | DeleteConfirmPanel |
| `Delete Confirm Text` | ConfirmText (inside DeleteConfirmPanel) |
| `Delete Yes Button` | YesButton (inside DeleteConfirmPanel) |
| `Delete No Button` | NoButton (inside DeleteConfirmPanel) |

### G. Final Step for LoadGameUIPanel

**Set the panel to INACTIVE:**
1. Select `LoadGameUIPanel`
2. **Uncheck the checkbox** next to the name in Inspector

---

## ?? Step 3: Assign to MainMenu

Now connect these panels to your MainMenu system:

1. **Select** the `MainMenu` GameObject (or `StartScreen` - the one with `Startscreen` script)
2. **In Inspector ? Startscreen component:**

### Find "Save System References" section:

| Field | Drag This |
|-------|-----------|
| `Save Name Prompt` | SaveNamePromptPanel |
| `Load Game UI` | LoadGameUIPanel |

---

## ? Verification Checklist

Before testing, verify these settings:

### SaveNamePromptPanel:
- [ ] Has CanvasGroup component
- [ ] Canvas Group ? Alpha = 0
- [ ] Canvas Group ? Interactable = OFF
- [ ] Canvas Group ? Blocks Raycasts = OFF
- [ ] GameObject is INACTIVE (unchecked)
- [ ] Has SaveNamePrompt script
- [ ] All fields in SaveNamePrompt script are assigned
- [ ] Input field character limit is set to 20

### LoadGameUIPanel:
- [ ] Has CanvasGroup component
- [ ] Canvas Group ? Alpha = 0
- [ ] Canvas Group ? Interactable = OFF
- [ ] Canvas Group ? Blocks Raycasts = OFF
- [ ] GameObject is INACTIVE (unchecked)
- [ ] Has LoadGameUI script
- [ ] All fields in LoadGameUI script are assigned
- [ ] SaveSlotPrefab is created and assigned
- [ ] Content has Vertical Layout Group
- [ ] Content has Content Size Fitter

### MainMenu:
- [ ] Save Name Prompt field points to SaveNamePromptPanel
- [ ] Load Game UI field points to LoadGameUIPanel

---

## ?? Testing

### Test 1: First Play (No Saves)
1. Run the game
2. Click "Play" button
3. ? SaveNamePromptPanel should appear
4. Enter a name (e.g., "TestPlayer")
5. Click Confirm
6. ? Game should start and create save

### Test 2: With Existing Save
1. Play game once to create a save
2. Return to main menu or restart game
3. Click "Play"
4. ? PlayChoicePanel should appear with "New Game" and "Continue"
5. Click "Continue"
6. ? LoadGameUIPanel should appear with your saves
7. Click "Load" on a save
8. ? Game should load with that save

---

## ?? Styling Tips

To match your game's medieval aesthetic:

### Colors:
- **Panels:** Dark brown (#3E2723) or parchment (#F5E6D3)
- **Text:** Cream (#FFF8E1) or dark brown (#3E2723)
- **Buttons:** Match your existing button style (I can see cream/tan in your screenshot)

### Fonts:
- Use the same decorative font you have for "Castle of Time"
- Ensure all TextMeshPro components use your game's font

### Background:
- Add a semi-transparent dark overlay (black, 80% opacity)
- Consider adding medieval borders/frames

---

## ?? Common Issues

### "SaveNamePrompt not assigned!"
**Fix:** Drag SaveNamePromptPanel to the field in MainMenu Inspector

### Input field not clickable
**Fix:** 
1. Check EventSystem exists in scene
2. Verify CanvasGroup.blocksRaycasts = true AFTER fade in
3. Check CanvasGroup.interactable = true

### Load UI shows no saves but they exist
**Fix:**
1. Check saves are in correct folder: `%AppData%\..\LocalLow\[Company]\[Game]\Saves\`
2. Verify files are named `GameFlags_[NAME].json`

### Prefab doesn't show up
**Fix:**
1. Verify SaveSlotPrefab is assigned in LoadGameUI Inspector
2. Check Content has Vertical Layout Group
3. Ensure prefab has SaveSlotUI script (will be added automatically)

---

## ?? Your Final Hierarchy Should Look Like:

```
StartScreen
??? Main Camera
??? Buttons
?   ??? PlayButton
?   ??? SettingsButton
?   ??? CreditsButton
?   ??? QuitButton
??? SaveNamePromptPanel (INACTIVE)
?   ??? Container
?   ?   ??? TitleText
?   ?   ??? SaveNameInput
?   ?   ??? ErrorText (inactive)
?   ?   ??? ConfirmButton
?   ?   ??? CancelButton
??? LoadGameUIPanel (INACTIVE)
?   ??? Container
?   ?   ??? TitleText
?   ?   ??? SaveSlotsScrollView
?   ?   ?   ??? Viewport
?   ?   ?       ??? Content ? This is Save Slot Container
?   ?   ??? NoSavesText (inactive)
?   ?   ??? BackButton
?   ??? DeleteConfirmPanel (inactive)
?       ??? ConfirmText
?       ??? YesButton
?       ??? NoButton
??? ...other menu stuff...
```

---

## ? You're Done!

Once you've created both panels and assigned them to MainMenu, your save/load system will be fully functional!

Need help with any specific step? Let me know! ???
