# Save/Load Menu System Guide

## Overview
The game now features a comprehensive save/load system with automatic saving on day progression. Manual saving has been removed to streamline the player experience.

## Key Features

### 1. **Auto-Save System**
- Game automatically saves when progressing to a new day
- Auto-save triggers when day flags are set (`day.one`, `day.two`, etc.)
- Clock time is saved along with game flags
- No manual save button needed

### 2. **Save Data Includes**
Each save file stores:
- **All active game flags** (character unlocks, cards, story progress, etc.)
- **Clock time remaining** (the time you have left in the current day)
- **Current day** (day.one through day.five)
- **Character name** (the save file name)

### 3. **Load Game UI**
The load game menu displays:
- **Character/Save Name** (e.g., "Jess", "Melvin")
- **Day Information** (e.g., "Day 2 of Spring, Year 1")
- **Clock Time** (e.g., "?50:00" - the time you get for that day)
- **Delete Button** (with confirmation dialog)

## File Structure

### Save Files Location
```
Application.persistentDataPath/Saves/GameFlags_[SaveName].json
```

For example:
- Windows: `C:/Users/[User]/AppData/LocalLow/[Company]/[Game]/Saves/GameFlags_Jess.json`
- Mac: `~/Library/Application Support/[Company]/[Game]/Saves/GameFlags_Jess.json`

### Save Data Format (JSON)
```json
{
  "flags": [
    "day.two",
    "character.marco",
    "card.slash",
    ...
  ],
  "clockTimeLeft": 50.5,
  "currentDay": "day.two"
}
```

## Implementation Details

### Modified Files

#### 1. **GameFlags.cs**
Added support for saving/loading clock time:
- `GameFlagsSaveData` now includes `clockTimeLeft` and `currentDay`
- `GetSaveMetadata()` - Get save info without loading the entire save
- `GetCurrentDay()` - Helper to determine which day the player is on

#### 2. **ClockTimer.cs**
Added methods to save/restore timer state:
- `GetTimeLeft()` - Returns current time remaining
- `RestoreTimeLeft(float time)` - Restores time when loading a save

#### 3. **LoadGameUI.cs**
Updated to display comprehensive save information:
- Shows character name
- Shows current day (formatted as "Day X of Spring, Year 1")
- Shows clock time remaining (formatted as "?MM:SS")
- Hides date/time created to match design

#### 4. **SaveSlotUI** (in LoadGameUI.cs)
Individual save slot component:
- `saveNameText` - Character/save name
- `dayInfoText` - Day information
- `clockTimeText` - Clock time remaining
- `loadButton` - Load this save
- `deleteButton` - Delete this save

#### 5. **GameFlagsSaveManager.cs**
Updated to clarify auto-save behavior:
- Manual `SaveFlags()` method is now private
- Added comments explaining auto-save system
- All saves go through `GameFlagsManager.SaveCurrentGame()`

## Usage

### For Designers

#### Setting Up Save Slot Prefab
Your save slot prefab should have these TextMeshProUGUI components:
- `SaveNameText` - Character name (e.g., "Jess")
- `DayInfoText` - Day progress (e.g., "Day 2 of Spring, Year 1")
- `ClockTimeText` - Clock time (e.g., "?50:00")
- `DateText` - (optional, hidden by default)

And these buttons:
- `LoadButton` - Loads the save
- `DeleteButton` - Shows delete confirmation

#### Recommended Layout
```
SaveSlot (GameObject)
??? Background (Image)
??? SaveNameText (TextMeshProUGUI) - Bold, large font
??? DayInfoText (TextMeshProUGUI) - Regular font
??? ClockTimeText (TextMeshProUGUI) - Monospace font recommended
??? LoadButton (Button)
??? DeleteButton (Button)
```

### For Programmers

#### Creating a New Save
```csharp
// Player enters name "Jess"
string saveName = "Jess";
GameFlagsManager.CreateNewSave(saveName);
```

#### Loading a Save
```csharp
// Set the active save
GameFlagsManager.SetCurrentSaveName("Jess");

// Load the save (restores flags and clock time)
bool success = GameFlagsManager.LoadCurrentGame();
```

#### Auto-Save on Day Progression
```csharp
// When advancing to a new day, this automatically saves:
GameFlags.SetFlag("day.two"); // Triggers auto-save
```

#### Getting Save Metadata
```csharp
// Get save info without loading
GameFlagsSaveData saveData = GameFlags.GetSaveMetadata("Jess");
if (saveData != null)
{
    Debug.Log($"Day: {saveData.currentDay}, Time: {saveData.clockTimeLeft}");
}
```

#### Checking Clock Time Before Save
```csharp
ClockTimer clockTimer = FindObjectOfType<ClockTimer>();
float timeLeft = clockTimer.GetTimeLeft();
Debug.Log($"Current time: {timeLeft}s");
```

## UI Flow

### Main Menu ? New Game
1. Player clicks "Play"
2. `SaveNamePrompt` appears
3. Player enters character name
4. `GameFlagsManager.CreateNewSave(name)` is called
5. Game starts with defaults
6. First auto-save happens when `day.one` flag is set

### Main Menu ? Load Game
1. Player clicks "Load Game"
2. `LoadGameUI` appears with all save slots
3. Each slot shows: name, day, clock time
4. Player clicks load button
5. `GameFlagsManager.SetCurrentSaveName(name)` is called
6. `GameFlagsManager.LoadCurrentGame()` restores everything
7. Game scene loads with restored state

### During Gameplay
1. Player completes a day
2. Battle ends, returns to overworld
3. `ClockTimer` triggers day progression
4. `GameFlags.SetFlag("day.two")` is called
5. **Auto-save triggers automatically**
6. Clock time is saved along with all flags

## Testing

### Test Auto-Save
```csharp
// In Unity Editor
1. Start a new game with name "Test"
2. Check Saves folder: GameFlags_Test.json should exist
3. Progress through first day
4. Open GameFlags_Test.json
5. Verify "day.two" is in flags array
6. Verify clockTimeLeft has a value
```

### Test Load Game
```csharp
// In Unity Editor
1. Create multiple saves with different names
2. Open Load Game UI
3. Verify each save shows correct info:
   - Name matches save file
   - Day info displays correctly
   - Clock time shows proper format
4. Load a save
5. Verify clock timer restores correct time
6. Verify all flags are loaded
```

### Test Delete Save
```csharp
// In Unity Editor
1. Create a test save
2. Click delete button
3. Confirm deletion
4. Verify file is removed from Saves folder
5. Verify UI updates and slot disappears
```

## Troubleshooting

### Clock Time Not Saving
**Problem**: Clock time is always default value (60s) when loading
**Solution**: Make sure `ClockTimer` exists in the scene when saving

### Day Info Shows "Unknown"
**Problem**: Save data doesn't have currentDay field
**Solution**: Save was created before this update. Re-save the game.

### Save Slot UI Not Updating
**Problem**: Save slot doesn't show day/time info
**Solution**: Check that SaveSlotUI has all required TextMeshProUGUI references

### Auto-Save Not Triggering
**Problem**: Game doesn't save when day changes
**Solution**: Verify day flags are being set with `GameFlags.SetFlag("day.two")` etc.

## Future Enhancements

Potential additions to consider:
1. **Play time tracking** - Show total hours played
2. **Money display** - Show gold/currency in save slot
3. **Portrait images** - Show character portrait in save slot
4. **Multiple seasons/years** - Extend day system beyond Day 5
5. **Cloud saves** - Sync saves across devices
6. **Save backup** - Automatic backup of save files

## Best Practices

### For Saves
- Always use `GameFlagsManager` for save operations
- Never manually call `GameFlags.SaveToFile()` directly
- Let auto-save handle day progression
- Keep save names unique per player

### For UI
- Show loading indicator when loading saves
- Always confirm before deleting saves
- Provide clear feedback when save succeeds/fails
- Sort saves by most recent first

### For Testing
- Test with multiple save slots
- Test loading very old saves
- Test with corrupted save files
- Test when save folder doesn't exist

## Related Files
- `Assets/Systems/GameFlags.cs` - Core flag system
- `Assets/Systems/GameFlagsManager.cs` - Save management
- `Assets/Systems/GameFlagsSaveManager.cs` - UI helper component
- `Assets/Systems/UIs/Menu/LoadGameUI.cs` - Load menu UI
- `Assets/Systems/UIs/Menu/SaveNamePrompt.cs` - Name input UI
- `Assets/Systems/UIs/Clock/ClockTimer.cs` - Clock timer
- `Assets/Systems/GAMEFLAGS_SAVE_SYSTEM_GUIDE.md` - General save system guide
