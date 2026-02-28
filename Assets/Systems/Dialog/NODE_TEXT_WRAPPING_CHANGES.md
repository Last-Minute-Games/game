# Node Text Wrapping Changes

## Summary
Updated all dialog node types to support text wrapping with vertical layouts and full-width text areas for optimal readability. Implemented dynamic height calculation similar to AnswerNode pattern.

## Changes Made

### 1. SentenceNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Sentence/SentenceNode.cs`)
- **Text wrapping**: Changed sentence text field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: All fields use **full vertical layout** with labels on top
- **Text area**: Fixed height of **80px** with full width (147px)
- **Sprite field**: Now uses vertical layout with **60px height** and full width (147px)
- **Dynamic height calculation**: Added `CalculateSentenceNodeHeight()` method similar to AnswerNode
- **Node height**: Base height **260px** (280px with external function)
- **Width**: All fields use `TextFieldWidth + LabelFieldSpace` = **147px**

### 2. AnswerNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Answer/AnswerNode.cs`)
- **Text wrapping**: Changed answer text fields from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Vertical layout with label/icon on top, text below
- **Fixed height**: Each answer text area has a fixed height of **50px**
- **Dynamic height calculation**: Uses `CalculateAnswerNodeHeight()` method
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

### Dynamic Height Calculation (SentenceNode)
Similar to AnswerNode, SentenceNode now calculates its height dynamically:
```csharp
public void CalculateSentenceNodeHeight()
{
    if (_isExternalFunc)
        Rect.height = ExternalNodeHeight;  // 280px
    else
        Rect.height = MinNodeHeight;        // 260px
}
```

This method is called:
- In the `Draw()` method at the start
- In the `CheckNodeSize()` method

### Text Wrapping Implementation
All text input fields now use **vertical layout with full width**:
```csharp
EditorGUILayout.BeginVertical();
EditorGUILayout.LabelField("Label");
GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
EditorGUILayout.TextArea(textValue, textAreaStyle, 
    GUILayout.Width(width), GUILayout.Height(height));
EditorGUILayout.EndVertical();
```

### Sprite Field Implementation (SentenceNode)
Sprite selection now uses vertical layout for better usability:
```csharp
EditorGUILayout.BeginVertical();
EditorGUILayout.LabelField("Sprite");
_sentence.CharacterSprite = (Sprite)EditorGUILayout.ObjectField(
    _sentence.CharacterSprite, typeof(Sprite), false, 
    GUILayout.Width(147), GUILayout.Height(60));
EditorGUILayout.EndVertical();
```

### Layout Approach
- **Full-width fields**: All input fields use maximum available width
- **Vertical layouts**: All fields use vertical layout (label on top, field below)
- **Dynamic height calculation**: Nodes calculate their height based on content/state
- **Consistent pattern**: SentenceNode and AnswerNode both use similar height calculation methods
- **Predictable sizing**: Nodes maintain consistent, larger sizes for better graph organization

## Benefits
1. **Text wrapping**: Long text wraps properly within nodes
2. **Fully visible**: All text content is visible without being cut off
3. **Full width usage**: Text areas and sprite fields use the full available width for maximum usability
4. **Vertical layout**: Labels on top, fields below - consistent pattern
5. **Sprite selection works**: Sprite ObjectField now has proper dimensions and is fully functional
6. **Dynamic height**: Nodes adjust height based on content (external function on/off)
7. **Stable layout**: Height calculation prevents visual glitches and node jumping
8. **Consistent**: All nodes use the same text wrapping and layout approach
9. **Similar to AnswerNode**: SentenceNode now uses the same height calculation pattern

## Node Sizes

| Node Type | Node Width | Node Height | Text Area Width | Text Area Height | Additional Fields | Height Calculation |
|-----------|-----------|-------------|-----------------|------------------|-------------------|-------------------|
| SentenceNode | 160px | 260px (280px with ext func) | 147px | 80px | Sprite: 147px × 60px | `CalculateSentenceNodeHeight()` |
| AnswerNode | 190px | 180px + (60px per answer) | 140px | 50px each | N/A | `CalculateAnswerNodeHeight()` |
| GameFlagConditionNode | 200px | 150px | 180px | 45px | N/A | Fixed |
| SetGameFlagNode | 200px | 170px | 180px | 45px | N/A | Fixed |

## Testing
- ? Build successful with no compilation errors
- ? All node types support text wrapping
- ? All text areas use proper width for readability
- ? **Sprite selection field is fully functional** with vertical layout
- ? **Dynamic height calculation works** like AnswerNode
- ? Vertical layouts match AnswerNode pattern
- ? Generous fixed heights ensure all content is visible
- ? Nodes maintain visual consistency with larger sizes
- ? External function toggle properly adjusts node height

## Key Improvements in This Version
- ? **Dynamic height calculation**: Added `CalculateSentenceNodeHeight()` method matching AnswerNode pattern
- ? **Full-width text areas**: Changed from narrow horizontal layout to full-width vertical layout
- ? **Sprite field fixed**: Changed sprite ObjectField to vertical layout with proper dimensions (147px × 60px)
- ? **Consistent pattern**: All fields now follow the same vertical layout approach
- ? **Better readability**: Text areas use maximum available width for text display
- ? **Increased node height**: SentenceNode height increased to 260px (from 180px) to accommodate sprite field
- ? **Height management**: Removed manual height setting from DrawExternalFunctionTextField, now uses centralized calculation
- ? **Label placement**: All labels are now on top of their fields for clarity

## SentenceNode Specific Changes

### Before
- Text area: Horizontal layout, narrow width
- Sprite field: Horizontal layout with label, narrow (100px)
- Node height: 180px (content was cramped/cut off)
- Height management: Manual height setting in DrawExternalFunctionTextField
- Total usable space: Limited

### After
- Text area: Vertical layout, full width (147px), 80px height
- Sprite field: **Vertical layout, full width (147px), 60px height** ? FIXED!
- Node height: 260px base, 280px with external function
- Height management: **Centralized with `CalculateSentenceNodeHeight()`** ? NEW!
- Called in `Draw()` method at start (like AnswerNode)
- Called in `CheckNodeSize()` method
- Total usable space: Maximized
- **Sprite selection now fully functional and visible**
- **Height calculation matches AnswerNode pattern**

## Height Calculation Pattern Comparison

### AnswerNode
```csharp
public void CalculateAnswerNodeHeight()
{
    _currentAnswerNodeHeight = AnswerNodeHeight;
    for (int i = 0; i < _amountOfAnswers - 1; i++)
        _currentAnswerNodeHeight += AdditionalAnswerNodeHeight;
}
```

### SentenceNode (NEW - Similar Pattern)
```csharp
public void CalculateSentenceNodeHeight()
{
    if (_isExternalFunc)
        Rect.height = ExternalNodeHeight;
    else
        Rect.height = MinNodeHeight;
}
```

Both nodes now follow the same pattern:
1. Calculate height based on content/state
2. Call calculation method in `Draw()` at the start
3. Call calculation method in sizing-related methods
4. Centralized height management
