# Save System Setup Guide

This guide explains how to set up and use the new save name prompt and load game system in your main menu.

## Overview

The save system now includes:
1. **SaveNamePrompt** - Prompts players to enter a save name when starting a new game
2. **LoadGameUI** - Displays all saved games and allows loading or deleting them
3. **GameFlagsManager** - Manages the current active save slot
4. **GameFlags** - Auto-saves to the current save slot (for day progression)

## Files Created

- `Assets/Systems/UIs/Menu/SaveNamePrompt.cs` - Save name prompt UI component
- `Assets/Systems/UIs/Menu/LoadGameUI.cs` - Load game UI component (includes SaveSlotUI)
- `Assets/Systems/GameFlagsManager.cs` - Centralized save management

## Files Modified

- `Assets/Systems/GameFlags.cs` - Now auto-saves to current save slot
- `Assets/Systems/UIs/Menu/MainMenu.cs` - Integrated with new save system

## Unity Setup Instructions

### 1. Create Save Name Prompt UI

1. In your MainMenu scene, create a new UI GameObject structure:
   ```
   Canvas
   ??? SaveNamePrompt (with CanvasGroup)
       ??? Panel (background)
       ??? Title (TextMeshProUGUI: "Enter Save Name")
       ??? InputField (TMP_InputField)
       ??? ErrorText (TextMeshProUGUI: initially hidden)
       ??? ConfirmButton (Button with TextMeshProUGUI: "Confirm")
       ??? CancelButton (Button with TextMeshProUGUI: "Cancel")
   ```

2. Add the `SaveNamePrompt` component to the SaveNamePrompt GameObject
3. Assign all UI references in the inspector:
   - Prompt Canvas Group
   - Save Name Input (TMP_InputField)
   - Confirm Button
   - Cancel Button
   - Error Text (optional)

### 2. Create Load Game UI

1. Create the main load game UI structure:
   ```
   Canvas
   ??? LoadGameUI (with CanvasGroup)
       ??? Panel (background)
       ??? Title (TextMeshProUGUI: "Load Game")
       ??? ScrollView
       ?   ??? Content (Vertical Layout Group)
       ?       ??? [Save slots will be spawned here]
       ??? NoSavesText (TextMeshProUGUI: "No saved games found")
       ??? BackButton (Button)
       ??? DeleteConfirmPanel
           ??? Panel (background)
           ??? Text (TextMeshProUGUI: "Delete save '[name]'?")
           ??? YesButton (Button)
           ??? NoButton (Button)
   ```

2. Create a Save Slot Prefab:
   ```
   SaveSlotPrefab
   ??? Panel (background with Image)
   ??? SaveNameText (TextMeshProUGUI)
   ??? DateText (TextMeshProUGUI)
   ??? LoadButton (Button with TextMeshProUGUI: "Load")
   ??? DeleteButton (Button with TextMeshProUGUI: "Delete")
   ```

3. Add the `LoadGameUI` component to the LoadGameUI GameObject
4. Assign all UI references in the inspector:
   - Load Game Canvas Group
   - Save Slot Container (the Content GameObject in ScrollView)
   - Save Slot Prefab (the prefab you created)
   - Back Button
   - No Saves Text
   - Delete Confirm Panel
   - Delete Confirm Text
   - Delete Yes Button
   - Delete No Button

### 3. Update Main Menu

1. Find your MainMenu GameObject (the one with the `Startscreen` component)
2. Add a new "Load Game" button to your main menu button layout
3. In the `Startscreen` component inspector, assign:
   - **Save Name Prompt**: The SaveNamePrompt GameObject
   - **Load Game UI**: The LoadGameUI GameObject
4. Wire up the button click events:
   - PlayButton ? `Startscreen.StartGame()` (should already be set up)
   - LoadGameButton ? `Startscreen.ShowLoadGame()` (NEW)

### 4. Initial Setup State

Make sure these UI elements start **inactive** in the scene:
- SaveNamePrompt GameObject
- LoadGameUI GameObject

They will be shown/hidden dynamically by the scripts.

## How It Works

### New Game Flow

1. Player clicks "Play" button in main menu
2. `SaveNamePrompt` appears, asking for a save name
3. Player enters a name (validated for length and invalid characters)
4. System creates a new save file: `Saves/GameFlags_[name].json`
5. GameFlagsManager sets this as the active save
6. Game scene loads

### Load Game Flow

1. Player clicks "Load Game" button in main menu
2. `LoadGameUI` appears, showing all saved games
3. Player clicks "Load" on a save slot
4. GameFlagsManager sets this as the active save
5. Flags are loaded from the save file
6. Game scene loads

### In-Game Saving

- **Manual Save**: Call `GameFlagsManager.SaveCurrentGame()` from anywhere
- **Auto-Save**: When day progression flags are set (day.one, day.two, etc.), the game auto-saves to the current save slot

### Save File Location

Save files are stored in:
- **Windows**: `%USERPROFILE%/AppData/LocalLow/[CompanyName]/[GameName]/Saves/`
- **macOS**: `~/Library/Application Support/[CompanyName]/[GameName]/Saves/`
- **Linux**: `~/.config/unity3d/[CompanyName]/[GameName]/Saves/`

Files are named: `GameFlags_[SaveName].json`

## Code Usage Examples

### From Any Script - Save Current Game
```csharp
GameFlagsManager.SaveCurrentGame();
```

### From Any Script - Get Current Save Name
```csharp
string currentSave = GameFlagsManager.GetCurrentSaveName();
Debug.Log($"Currently playing: {currentSave}");
```

### From Any Script - Check If Current Save Exists
```csharp
if (GameFlagsManager.HasCurrentSave())
{
    Debug.Log("Save file exists for current game");
}
```

### From Any Script - Delete Current Save
```csharp
GameFlagsManager.DeleteCurrentSave();
```

### Subscribe to Save Events
```csharp
void OnEnable()
{
    SaveGameEvents.OnSaveCreated += OnSaveCreated;
    SaveGameEvents.OnSaveLoaded += OnSaveLoaded;
    SaveGameEvents.OnSaveDeleted += OnSaveDeleted;
}

void OnDisable()
{
    SaveGameEvents.OnSaveCreated -= OnSaveCreated;
    SaveGameEvents.OnSaveLoaded -= OnSaveLoaded;
    SaveGameEvents.OnSaveDeleted -= OnSaveDeleted;
}

void OnSaveCreated(string saveName)
{
    Debug.Log($"Save created: {saveName}");
}

void OnSaveLoaded(string saveName)
{
    Debug.Log($"Save loaded: {saveName}");
}

void OnSaveDeleted(string saveName)
{
    Debug.Log($"Save deleted: {saveName}");
}
```

## Validation Rules

Save names must:
- Be at least 3 characters long (configurable in SaveNamePrompt)
- Be at most 20 characters long (configurable in SaveNamePrompt)
- Not contain invalid filename characters (e.g., `/ \ : * ? " < > |`)
- Not already exist (prevents overwriting)

## Customization

### SaveNamePrompt Settings
- **Min Name Length**: Minimum characters required
- **Max Name Length**: Maximum characters allowed
- **Fade Duration**: How fast the prompt fades in/out

### LoadGameUI Settings
- **Fade Duration**: How fast the UI fades in/out

## Troubleshooting

### "SaveNamePrompt not assigned!"
Make sure you've assigned the SaveNamePrompt GameObject to the Startscreen component in the inspector.

### "LoadGameUI not assigned!"
Make sure you've assigned the LoadGameUI GameObject to the Startscreen component in the inspector.

### Save files not appearing in LoadGameUI
- Check that the `Saves` folder exists in the persistent data path
- Verify save files are named correctly: `GameFlags_[name].json`
- Use `Debug.Log(Application.persistentDataPath)` to find the save directory

### Saves getting overwritten
This is intentional! All saves with the same name share one save file. When you set day.two, it overwrites the existing save for that character name.

## Migration from Old System

The old PlayerPrefs-based save system (`GameFlags.SaveFlags()` and `GameFlags.LoadFlags()`) still works for backward compatibility, but it's recommended to use the new file-based system:

**Old way (PlayerPrefs):**
```csharp
GameFlags.SaveFlags();
GameFlags.LoadFlags();
```

**New way (File-based with save names):**
```csharp
GameFlagsManager.SaveCurrentGame();
GameFlagsManager.LoadCurrentGame();
```

## Testing

1. Start the game
2. Click "Play" ? Enter a save name (e.g., "Nikolaus") ? Game starts
3. Play through day one until day.two is set (should auto-save)
4. Return to main menu
5. Click "Load Game" ? You should see "Nikolaus" in the list
6. Click "Load" on the save ? Game loads with your progress
7. The save file is at: `[PersistentDataPath]/Saves/GameFlags_Nikolaus.json`

All day progression (day.one ? day.two ? day.three ? day.four ? day.five) will overwrite the same "Nikolaus" save file.
