# Node Text Wrapping Changes

## Summary
Updated all dialog node types to support text wrapping with vertical layouts and full-width text areas for optimal readability.

## Changes Made

### 1. SentenceNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Sentence/SentenceNode.cs`)
- **Text wrapping**: Changed sentence text field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Text area uses **full vertical layout** with label on top
- **Fixed height**: Text area has a fixed height of **80px** to accommodate multiple lines
- **Width**: Text area uses **full node width** (NodeWidth - 20 = 140px)
- **Node height**: Base height increased to **180px** (200px with external function)

### 2. AnswerNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Answer/AnswerNode.cs`)
- **Text wrapping**: Changed answer text fields from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Vertical layout with label/icon on top, text below
- **Fixed height**: Each answer text area has a fixed height of **50px**
- **Width**: Text area width is **140px** (TextFieldWidth + 20)
- **Node sizing**: Adjusted base height to **180px** and additional height per answer to **60px**

### 3. GameFlagConditionNode (`Assets/Systems/Dialog/GameFlagConditionNode.cs`)
- **Text wrapping**: Changed flag name field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Full vertical layout with label on top
- **Fixed height**: Flag name text area has a fixed height of **45px**
- **Width**: Text area uses **full node width** (NodeWidth - 20 = 180px)
- **Node height**: Increased to **150px**

### 4. SetGameFlagNode (`Assets/Systems/Dialog/SetGameFlagNode.cs`)
- **Text wrapping**: Changed flag name field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Full vertical layout with label on top
- **Fixed height**: Flag name text area has a fixed height of **45px**
- **Width**: Text area uses **full node width** (NodeWidth - 20 = 180px)
- **Node height**: Increased to **170px**

## Technical Details

### Text Wrapping Implementation
All text input fields now use **vertical layout with full width**:
```csharp
EditorGUILayout.LabelField("Text");
GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
EditorGUILayout.TextArea(textValue, textAreaStyle, 
    GUILayout.Width(NodeWidth - 20), GUILayout.Height(height));
```

### Layout Approach
- **Full-width text areas**: Text areas use almost the full node width (NodeWidth - 20 for padding)
- **Vertical layouts**: All text fields use vertical layout (label on top, text area below)
- **Large fixed heights**: All nodes use generously sized fixed heights to ensure content visibility
- **Predictable sizing**: Nodes maintain consistent, larger sizes for better graph organization

## Benefits
1. **Text wrapping**: Long text wraps properly within nodes
2. **Fully visible**: All text content is visible without being cut off
3. **Full width usage**: Text areas use the full available width for maximum readability
4. **Vertical layout**: Labels on top, text below - consistent with AnswerNode pattern
5. **Stable layout**: Fixed heights prevent visual glitches and node jumping
6. **Consistent**: All nodes use the same text wrapping and layout approach

## Node Sizes

| Node Type | Node Width | Node Height | Text Area Width | Text Area Height | Notes |
|-----------|-----------|-------------|-----------------|------------------|-------|
| SentenceNode | 160px | 180px (200px with ext func) | 140px (full width) | 80px | Vertical layout, label on top |
| AnswerNode | 190px | 180px + (60px per answer) | 140px | 50px each | Vertical layout per answer |
| GameFlagConditionNode | 200px | 150px | 180px (full width) | 45px | Vertical layout, label on top |
| SetGameFlagNode | 200px | 170px | 180px (full width) | 45px | Vertical layout, label on top |

## Testing
- Build successful with no compilation errors
- All node types support text wrapping
- All text areas use full node width
- Vertical layouts match AnswerNode pattern
- Generous fixed heights ensure all content is visible
- Nodes maintain visual consistency with larger sizes

## Key Improvements in This Version
- ? **Full-width text areas**: Changed from narrow horizontal layout to full-width vertical layout
- ? **Consistent pattern**: All nodes now follow the same vertical layout approach as AnswerNode
- ? **Better readability**: Text areas use maximum available width for text display
- ? **Label placement**: Labels are now on top of text areas for clarity
- ? **No width constraints**: Removed narrow width limitations (was 100-120px, now 140-180px)

## Before vs After

### Before
- SentenceNode: Text area was 147px wide (TextFieldWidth + LabelFieldSpace) in horizontal layout
- GameFlagConditionNode: Text area was 180px but label took horizontal space
- Text was cramped with labels side-by-side

### After
- SentenceNode: Text area is 140px wide (NodeWidth - 20) in vertical layout
- GameFlagConditionNode: Text area is 180px wide (NodeWidth - 20) in vertical layout  
- SetGameFlagNode: Text area is 180px wide (NodeWidth - 20) in vertical layout
- All use full width with labels on top for maximum readability
