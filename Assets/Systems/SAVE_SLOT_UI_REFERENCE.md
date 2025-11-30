# Save Slot UI Layout Reference

## Visual Structure

```
???????????????????????????????????????????????????????????????
?  Save Slot Container                                        ?
?                                                             ?
?  ???????????????????????????????????????????????????????  ?
?  ?  1. Jess                                    [Load]  ?  ?
?  ?     Day 2 of Spring, Year 1                         ?  ?
?  ?     ?50:00                              [Delete]    ?  ?
?  ???????????????????????????????????????????????????????  ?
?                                                             ?
?  ???????????????????????????????????????????????????????  ?
?  ?  2. Melvin                                  [Load]  ?  ?
?  ?     Day 1 of Spring, Year 1                         ?  ?
?  ?     ?60:00                              [Delete]    ?  ?
?  ???????????????????????????????????????????????????????  ?
?                                                             ?
?  ???????????????????????????????????????????????????????  ?
?  ?  3. SaveName                                [Load]  ?  ?
?  ?     Day 3 of Spring, Year 1                         ?  ?
?  ?     ?45:30                              [Delete]    ?  ?
?  ???????????????????????????????????????????????????????  ?
?                                                             ?
?                                   [Back to Main Menu]      ?
???????????????????????????????????????????????????????????????
```

## Component Breakdown

### Save Slot Prefab Hierarchy
```
SaveSlotPrefab
??? Background (Image)
?   ??? Border/Frame (Image) - Optional
?
??? NumberText (TextMeshProUGUI)
?   • Text: "1.", "2.", "3.", etc.
?   • Position: Top-left
?   • Font: Bold
?
??? SaveNameText (TextMeshProUGUI)
?   • Text: Character name (e.g., "Jess")
?   • Position: Top-left (after number)
?   • Font: Bold, Large (24-32pt)
?   • Color: White or theme color
?
??? DayInfoText (TextMeshProUGUI)
?   • Text: "Day X of Spring, Year 1"
?   • Position: Below name
?   • Font: Regular (14-18pt)
?   • Color: Light gray
?
??? ClockTimeText (TextMeshProUGUI)
?   • Text: "?MM:SS" (e.g., "?50:00")
?   • Position: Below day info
?   • Font: Monospace recommended (14-18pt)
?   • Color: Yellow/Gold to match clock theme
?
??? LoadButton (Button)
?   • Text: "Load" or "Continue"
?   • Position: Right side, vertically centered
?   • Size: Medium (100x40)
?   • Style: Primary action button
?
??? DeleteButton (Button)
    • Text: "Delete" or "X"
    • Position: Right side, below load button
    • Size: Small (80x30)
    • Style: Danger/warning button
```

## Text Formatting

### Save Name
```
Font: Bold, Sans-serif
Size: 24-32pt
Color: #FFFFFF (White) or theme primary
Alignment: Left
Example: "Jess"
```

### Day Info
```
Font: Regular, Sans-serif
Size: 14-18pt
Color: #CCCCCC (Light gray)
Alignment: Left
Example: "Day 2 of Spring, Year 1"
```

### Clock Time
```
Font: Monospace (Consolas, Courier New, etc.)
Size: 14-18pt
Color: #FFD700 (Gold) or clock theme color
Alignment: Left
Example: "?50:00"
```

## Color Scheme Recommendations

### Default Theme
```
Background: #2C3E50 (Dark blue-gray)
Border: #34495E (Lighter blue-gray)
Text Primary: #FFFFFF (White)
Text Secondary: #BDC3C7 (Light gray)
Clock Icon/Time: #FFD700 (Gold)
Load Button: #27AE60 (Green)
Delete Button: #E74C3C (Red)
```

### Alternative Theme (Warm)
```
Background: #3E2723 (Dark brown)
Border: #5D4037 (Brown)
Text Primary: #FFF8E1 (Cream)
Text Secondary: #D7CCC8 (Light brown)
Clock Icon/Time: #FFA726 (Orange)
Load Button: #66BB6A (Green)
Delete Button: #EF5350 (Red)
```

## Sizing Guidelines

### Save Slot Dimensions
```
Width: 600-800px (depends on screen size)
Height: 80-120px
Spacing between slots: 10-20px
Padding: 15-20px
```

### Button Sizes
```
Load Button:
  Width: 100-120px
  Height: 35-45px

Delete Button:
  Width: 80-100px
  Height: 30-40px
```

## Interactive States

### Load Button States
```
Normal: Green, solid
Hover: Brighter green, slight scale
Pressed: Darker green, scale down
Disabled: Gray, no interaction
```

### Delete Button States
```
Normal: Red/Orange, outlined or subtle
Hover: Brighter red, shows warning color
Pressed: Darker red
Disabled: Gray, no interaction
```

### Slot Background States
```
Normal: Default background color
Hover: Slightly lighter background
Selected: Border highlight
```

## Animation Recommendations

### On Slot Appear
```
Fade in: 0.2s ease-out
Slide in from left: 0.3s ease-out
Stagger: 0.1s delay between each slot
```

### On Button Click
```
Scale: 0.95x for 0.1s
Flash: Subtle highlight
Sound: Click sound effect
```

### On Delete Confirmation
```
Shake: 2-3 times quickly
Fade out: 0.3s
Slide down: Remove gap smoothly
```

## Accessibility

### Text Contrast
- Ensure all text has minimum 4.5:1 contrast ratio
- Use larger font sizes for primary information
- Avoid color-only indicators

### Button Accessibility
- Minimum touch target: 44x44px
- Clear visual feedback on hover/focus
- Keyboard navigation support
- Screen reader labels

### Error Handling
- Clear error messages if load fails
- Confirmation before delete
- Loading indicators for long operations

## Implementation Checklist

### Unity Setup
- [ ] Create SaveSlotPrefab with all components
- [ ] Assign TextMeshProUGUI references
- [ ] Assign Button references
- [ ] Set up button onClick listeners
- [ ] Configure layout group (Vertical Layout)
- [ ] Set up scroll view if needed
- [ ] Add content size fitter

### Scripting
- [ ] SaveSlotUI.Initialize() called correctly
- [ ] Text formatting implemented
- [ ] Button callbacks connected
- [ ] Delete confirmation working
- [ ] Load feedback implemented
- [ ] Error handling in place

### Visual Polish
- [ ] Background images assigned
- [ ] Fonts imported and assigned
- [ ] Colors match theme
- [ ] Button sprites created
- [ ] Hover/pressed states set up
- [ ] Animations implemented
- [ ] Sound effects added

### Testing
- [ ] Test with 0 saves (empty state)
- [ ] Test with 1 save
- [ ] Test with multiple saves
- [ ] Test with max saves (if limit exists)
- [ ] Test load functionality
- [ ] Test delete functionality
- [ ] Test with corrupted save
- [ ] Test performance with many saves

## Empty State

When no saves exist:
```
???????????????????????????????????????????????????????????????
?                                                             ?
?                                                             ?
?                    No saved games found                     ?
?                                                             ?
?                Start a new game from the                    ?
?                      main menu                              ?
?                                                             ?
?                                                             ?
?                      [Back to Menu]                         ?
???????????????????????????????????????????????????????????????
```

## Delete Confirmation Dialog

```
??????????????????????????????????????
?  Confirm Delete                    ?
?                                    ?
?  Delete save 'Jess'?               ?
?  This action cannot be undone.     ?
?                                    ?
?    [Cancel]        [Delete]        ?
??????????????????????????????????????
```

## Example Code for Styling

### TextMeshProUGUI Settings
```csharp
// Save name
saveNameText.fontSize = 28;
saveNameText.fontStyle = FontStyles.Bold;
saveNameText.color = Color.white;

// Day info
dayInfoText.fontSize = 16;
dayInfoText.color = new Color(0.8f, 0.8f, 0.8f);

// Clock time
clockTimeText.fontSize = 16;
clockTimeText.color = new Color(1f, 0.843f, 0f); // Gold
```

### Button Styling
```csharp
// Load button colors
ColorBlock loadColors = loadButton.colors;
loadColors.normalColor = new Color(0.15f, 0.68f, 0.38f); // Green
loadColors.highlightedColor = new Color(0.2f, 0.78f, 0.48f);
loadColors.pressedColor = new Color(0.1f, 0.58f, 0.28f);
loadButton.colors = loadColors;

// Delete button colors
ColorBlock deleteColors = deleteButton.colors;
deleteColors.normalColor = new Color(0.9f, 0.3f, 0.24f); // Red
deleteColors.highlightedColor = new Color(1f, 0.4f, 0.34f);
deleteColors.pressedColor = new Color(0.8f, 0.2f, 0.14f);
deleteButton.colors = deleteColors;
```
