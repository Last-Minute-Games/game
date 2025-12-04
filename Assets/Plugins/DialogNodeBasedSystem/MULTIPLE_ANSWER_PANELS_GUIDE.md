# Multiple Answer Panels Setup Guide

## Overview
This implementation allows you to use different answer panel UIs for different answer nodes in your dialog system. For example, you can have a standard answer panel for regular dialogs and a special character selection panel with custom styling.

## What Was Changed

### 1. **AnswerPanelType Enum** (in `AnswerNode.cs`)
Added an enum to specify which panel type to use:
- `Default` (0) - Uses the standard answer panel
- `CharacterSelection` (1) - Uses the character selection panel

### 2. **AnswerNode.cs**
- Added `PanelType` field to specify which answer panel to use
- Added dropdown in the Unity editor (Node Graph window) to select panel type

### 3. **DialogDisplayer.cs**
- Added `_characterSelectionPanel` serialized field for the second answer panel
- Added `_currentAnswerPanel` to track which panel is currently active
- Modified `EnableDialogAnswerPanel()` to switch panels based on the node's `PanelType`
- Updated all methods to use `_currentAnswerPanel` instead of hardcoded `_dialogAnswerPanel`
- Created `DisableAllAnswerPanels()` method to properly disable all panels
- Created `SetUpAllAnswerPanelButtons()` to initialize buttons for all panels

## Setup Instructions in Unity

### Step 1: Create the Second Answer Panel

1. Open your Dialog Prefab in the Unity hierarchy
2. Navigate to: `Dialog Prefab > Dialog Canvas > Dialog UI`
3. Find your existing `Answer Panel` GameObject
4. **Duplicate it** (Ctrl+D or right-click > Duplicate)
5. Rename the duplicate to `Character Selection Panel`

### Step 2: Customize the New Panel

Style your new `Character Selection Panel` however you want:
- Change the layout (different positioning, spacing, etc.)
- Modify button styles (colors, sizes, fonts)
- Adjust the panel background/appearance
- Change button prefab if needed (assign in AnswerPanel component)

### Step 3: Assign the New Panel to DialogDisplayer

1. In the Unity hierarchy, find your `Dialog Prefab` object
2. Select the object that has the `DialogDisplayer` component (likely under `Dialog Prefab`)
3. In the Inspector, you'll see a new field: **Character Selection Panel**
4. Drag your newly created `Character Selection Panel` GameObject into this field

### Step 4: Use the New Panel in Dialog Graphs

1. Open any Dialog Node Graph (double-click a DialogNodeGraph asset)
2. Select an Answer Node where you want to use the custom panel
3. In the Inspector, you'll now see a **Panel Type** dropdown
4. Select `CharacterSelection` for nodes that should use your custom panel
5. Leave it as `Default` for standard dialog choices

## Example Use Case

For your character selection scene (as shown in your screenshot):
1. Create an Answer Node with 7 answers (the character names)
2. Set its **Panel Type** to `CharacterSelection`
3. Style your `Character Selection Panel` to display the 7 options in a grid or list layout
4. When this node activates, it will automatically use your custom panel instead of the default one

## Testing

1. Make sure both panels are disabled by default in the Unity editor (unchecked in hierarchy)
2. The system will automatically enable/disable the correct panel at runtime
3. Test by running a dialog that uses both panel types

## Extending the System

To add more panel types in the future:

1. Edit `AnswerNode.cs` and add a new value to the `AnswerPanelType` enum:
   ```csharp
   public enum AnswerPanelType
   {
       Default = 0,
       CharacterSelection = 1,
       ShopSelection = 2  // New type
   }
   ```

2. Add a new panel field in `DialogDisplayer.cs`:
   ```csharp
   [SerializeField] private AnswerPanel _shopSelectionPanel;
   ```

3. Update the switch statement in `EnableDialogAnswerPanel()`:
   ```csharp
   _currentAnswerPanel = currentAnswerNode.PanelType switch
   {
       AnswerPanelType.CharacterSelection => _characterSelectionPanel,
       AnswerPanelType.ShopSelection => _shopSelectionPanel,
       _ => _dialogAnswerPanel
   };
   ```

4. Update `DisableAllAnswerPanels()` and `SetUpAllAnswerPanelButtons()` to include the new panel

## Troubleshooting

- **Panel doesn't switch**: Make sure you've assigned the Character Selection Panel in the DialogDisplayer inspector
- **Buttons don't work**: Verify that both panels have the AnswerPanel component and button prefab assigned
- **Panel Type dropdown not showing**: Make sure you saved the Answer Node asset after the update
- **Wrong panel appears**: Check the Panel Type setting on your Answer Node in the dialog graph

## Technical Notes

- Both panels are set up with buttons at dialog start (based on max answers in the graph)
- Only one panel is active at a time
- The system automatically handles button setup, click events, and localization for whichever panel is active
- All existing functionality (localization, variable replacement, etc.) works with both panels
