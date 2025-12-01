# Save System - Quick Reference

## Summary

The save system now works with **named saves**. Each player creates a save with a unique name (e.g., "Nikolaus"), and all progress for that playthrough is saved to one file that gets overwritten as they progress through the game.

## Key Concepts

- **One save per name**: "Nikolaus" has ONE save file that updates as you play
- **No day-specific saves**: No "NikolausDay1" or "NikolausDay2" - just "Nikolaus"
- **Auto-save on day progression**: When day flags are set, the game auto-saves
- **Central save management**: `GameFlagsManager` handles all save operations

## For Players

1. **Start New Game**: Click "Play" ? Enter save name ? Game starts
2. **Load Game**: Click "Load Game" ? Select save ? Game continues
3. **Progress Saves Automatically**: When you complete a day, progress auto-saves

## For Developers

### Quick Setup Checklist

- [x] ? Scripts created and compiled
- [ ] ? Create SaveNamePrompt UI in MainMenu scene
- [ ] ? Create LoadGameUI in MainMenu scene  
- [ ] ? Create SaveSlot prefab for LoadGameUI
- [ ] ? Add "Load Game" button to main menu
- [ ] ? Assign references in Startscreen inspector
- [ ] ? Test new game flow
- [ ] ? Test load game flow

### Essential Code Snippets

**Save current game:**
```csharp
GameFlagsManager.SaveCurrentGame();
```

**Get current save name:**
```csharp
string saveName = GameFlagsManager.GetCurrentSaveName();
```

**Check if player has a save:**
```csharp
if (GameFlagsManager.HasCurrentSave())
{
    // Player has saved progress
}
```

### Auto-Save Behavior

These flags trigger automatic saving:
- `day.one`
- `day.two`
- `day.three`
- `day.four`
- `day.five`

When you call `GameFlags.SetFlag("day.two")`, it automatically calls `GameFlagsManager.SaveCurrentGame()`.

### Save File Location

```
Application.persistentDataPath/Saves/GameFlags_[SaveName].json
```

Example:
- Windows: `C:/Users/[User]/AppData/LocalLow/[Company]/[Game]/Saves/GameFlags_Nikolaus.json`

### Button Setup Options

#### Option 1: Use SaveSystemButton Component
1. Add `SaveSystemButton` to your button
2. Select action from dropdown (NewGame, LoadGame, SaveGame)
3. Done!

#### Option 2: Direct Integration
Wire buttons to these methods in `Startscreen`:
- Play Button ? `Startscreen.StartGame()`
- Load Game Button ? `Startscreen.ShowLoadGame()`

## Save File Structure Example

```json
{
  "flags": [
    "character.marco",
    "character.allistair",
    "day.two",
    "card.slash",
    "story.tutorial_complete",
    "npc.met_sebastian"
  ]
}
```

## Common Scenarios

### Scenario 1: Player starts new game with name "Alice"
1. Enter "Alice" as save name
2. `GameFlags_Alice.json` is created with default flags
3. Play through day one
4. When day.two flag is set ? Auto-saves to `GameFlags_Alice.json`
5. Continue playing day two
6. When day.three flag is set ? Auto-saves to `GameFlags_Alice.json` (overwrites previous)

Result: One file (`GameFlags_Alice.json`) with latest progress.

### Scenario 2: Player wants to start a second playthrough
1. Return to main menu
2. Click "Play"
3. Enter "Bob" as save name
4. New file `GameFlags_Bob.json` is created
5. Now you have two saves: Alice and Bob

### Scenario 3: Player wants to continue previous game
1. Click "Load Game"
2. See list of saves: "Alice", "Bob"
3. Click "Load" on "Alice"
4. Game continues from Alice's last save point

## Migration Notes

If you had old PlayerPrefs saves, they're still there but won't appear in the Load Game UI. You can:
1. Keep them as-is (legacy support)
2. Delete them with `GameFlags.DeleteSavedFlags()`

New saves use the file-based system and don't interfere with old PlayerPrefs saves.

## Debugging

### Print current save name:
```csharp
Debug.Log($"Current save: {GameFlagsManager.GetCurrentSaveName()}");
```

### List all save files:
```csharp
string saveDir = Path.Combine(Application.persistentDataPath, "Saves");
if (Directory.Exists(saveDir))
{
    string[] files = Directory.GetFiles(saveDir, "GameFlags_*.json");
    foreach (string file in files)
    {
        Debug.Log($"Save file: {file}");
    }
}
```

### Print all active flags:
```csharp
GameFlags.PrintAllFlags();
```

## Events You Can Subscribe To

```csharp
// Called when a save is created
SaveGameEvents.OnSaveCreated += (saveName) => 
{
    Debug.Log($"Save created: {saveName}");
};

// Called when a save is loaded
SaveGameEvents.OnSaveLoaded += (saveName) => 
{
    Debug.Log($"Save loaded: {saveName}");
};

// Called when a save is deleted
SaveGameEvents.OnSaveDeleted += (saveName) => 
{
    Debug.Log($"Save deleted: {saveName}");
};
```

## Next Steps

1. Follow the detailed setup guide in `SaveSystemSetup.md`
2. Create the UI elements in your MainMenu scene
3. Test the flow: New Game ? Play ? Load Game
4. Customize the UI appearance to match your game's style

## Questions?

- ? "Does each day create a new save?" ? **No**, same save file is overwritten
- ? "Can I have multiple characters?" ? **Yes**, each name is a separate save
- ? "Will old saves break?" ? **No**, old PlayerPrefs saves still work
- ? "Where are save files stored?" ? **Application.persistentDataPath/Saves/**
- ? "When does auto-save happen?" ? **When day progression flags are set**
