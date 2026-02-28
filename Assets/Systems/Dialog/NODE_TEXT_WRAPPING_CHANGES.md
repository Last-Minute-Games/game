# Node Text Wrapping Changes - FINAL

## Summary
Updated all dialog node types to support text wrapping with vertical layouts and **fixed-width** text areas for optimal readability. All nodes now use the `CalculateHeight` pattern similar to AnswerNode for consistent height management. **Text area widths are now constant to prevent unwanted growth.**

## Changes Made

### 1. SentenceNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Sentence/SentenceNode.cs`)
- **Text wrapping**: Changed sentence text field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: All fields use **full vertical layout** with labels on top
- **Fixed width**: Added `TextAreaWidth = 180f` constant - **prevents width changes**
- **Text area**: Fixed height of **80px** with **constant width (180px)**
- **Sprite field**: Uses default ObjectField sizing with **constant width (180px)**
- **Dynamic height calculation**: Added `CalculateSentenceNodeHeight()` method
- **Node dimensions**: Width **200px**, Height **260px** base (280px with external function)

### 2. AnswerNode (`Assets/Plugins/DialogNodeBasedSystem/Scripts/Nodes/Answer/AnswerNode.cs`)
- **Text wrapping**: Changed answer text fields from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Vertical layout with label/icon on top, text below
- **Fixed height**: Each answer text area has a fixed height of **50px**
- **Dynamic height calculation**: Uses `CalculateAnswerNodeHeight()` method
- **Width**: Text area width is **140px** (TextFieldWidth + 20)
- **Node sizing**: Base height **180px**, additional **60px per answer**

### 3. GameFlagConditionNode (`Assets/Systems/Dialog/GameFlagConditionNode.cs`)
- **Text wrapping**: Changed flag name field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Full vertical layout with label on top
- **Fixed width**: Added `TextAreaWidth = 180f` constant - **prevents width changes**
- **Fixed height**: Flag name text area has **45px** height
- **Dynamic height calculation**: Added `CalculateNodeHeight()` method ? **NEW!**
- **Node dimensions**: Width **200px**, Height **150px**

### 4. SetGameFlagNode (`Assets/Systems/Dialog/SetGameFlagNode.cs`)
- **Text wrapping**: Changed flag name field from `TextField` to `TextArea` with `wordWrap = true`
- **Layout**: Full vertical layout with label on top
- **Fixed width**: Added `TextAreaWidth = 180f` constant - **prevents width changes**
- **Fixed height**: Flag name text area has **45px** height
- **Dynamic height calculation**: Added `CalculateNodeHeight()` method ? **NEW!**
- **Node dimensions**: Width **200px**, Height **170px**

## Technical Details

### Fixed Width Implementation (CRITICAL FIX)
All nodes now use **constant width values** to prevent text areas from growing:

```csharp
// SentenceNode
private const float TextAreaWidth = 180f;

// GameFlagConditionNode
private const float TextAreaWidth = 180f;

// SetGameFlagNode
private const float TextAreaWidth = 180f;

// Usage:
GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
EditorGUILayout.TextArea(text, textAreaStyle, 
    GUILayout.Width(TextAreaWidth),  // ? CONSTANT, never changes
    GUILayout.Height(height));
```

**Before (BROKEN):** `GUILayout.Width(NodeWidth - 20)` - calculated every frame, could change
**After (FIXED):** `GUILayout.Width(TextAreaWidth)` - constant value, never changes

### Dynamic Height Calculation (ALL NODES)

**SentenceNode:**
```csharp
public void CalculateSentenceNodeHeight()
{
    if (_isExternalFunc)
        Rect.height = ExternalNodeHeight;  // 280px
    else
        Rect.height = MinNodeHeight;        // 260px
}
```

**AnswerNode:**
```csharp
public void CalculateAnswerNodeHeight()
{
    _currentAnswerNodeHeight = AnswerNodeHeight;  // 180px
    for (int i = 0; i < _amountOfAnswers - 1; i++)
        _currentAnswerNodeHeight += AdditionalAnswerNodeHeight;  // +60px per answer
}
```

**GameFlagConditionNode (NEW):**
```csharp
public void CalculateNodeHeight()
{
    Rect.height = NodeHeight;  // 150px (fixed for now, extendable later)
}
```

**SetGameFlagNode (NEW):**
```csharp
public void CalculateNodeHeight()
{
    Rect.height = NodeHeight;  // 170px (fixed for now, extendable later)
}
```

All height calculations are called in `Draw()` method at the start.

### Layout Approach
- **Fixed-width text areas**: All text areas use **constant width values** - no more growing!
- **Vertical layouts**: All fields use vertical layout (label on top, field below)
- **Dynamic height calculation**: All nodes calculate their height consistently
- **Consistent pattern**: All nodes follow the same structure
- **Predictable sizing**: Nodes maintain stable dimensions

## Benefits
1. ? **Text wrapping**: Long text wraps properly within nodes
2. ? **Fixed width**: Text areas **never change width** - stable layout
3. ? **Fully visible**: All text content is visible without being cut off
4. ? **Sprite selection works**: Sprite ObjectField has proper dimensions
5. ? **Dynamic height**: All nodes use consistent height calculation pattern
6. ? **Stable layout**: Height calculation prevents visual glitches
7. ? **Consistent**: All nodes use the same approach
8. ? **No width growth**: **FIXED** - Text areas stay at constant 180px width

## Node Specifications

| Node Type | Node Width | Node Height | Text Area Width | Text Area Height | Height Calculation Method |
|-----------|-----------|-------------|-----------------|------------------|--------------------------|
| SentenceNode | 200px | 260px (280px ext) | **180px (fixed)** | 80px | `CalculateSentenceNodeHeight()` |
| AnswerNode | 190px | 180px + (60px/ans) | 140px | 50px each | `CalculateAnswerNodeHeight()` |
| GameFlagConditionNode | 200px | 150px | **180px (fixed)** | 45px | `CalculateNodeHeight()` ? NEW |
| SetGameFlagNode | 200px | 170px | **180px (fixed)** | 45px | `CalculateNodeHeight()` ? NEW |

## Testing
- ? Build successful with no compilation errors
- ? All node types support text wrapping
- ? **All text areas use fixed constant widths** - no more width changes!
- ? **All nodes have CalculateHeight methods** - consistent pattern
- ? Sprite selection field fully functional
- ? Vertical layouts work correctly
- ? Nodes maintain stable visual consistency

## Key Improvements - FINAL VERSION

### Width Stability (CRITICAL FIX)
- ? **Added `TextAreaWidth` constant to all nodes**
- ? **Text areas now use constant width (180px)** instead of calculated values
- ? **Prevents width from changing/growing** during editing
- ? **Consistent appearance across all nodes**

### Height Management (Complete)
- ? **All nodes now have `CalculateHeight` methods**:
  - `SentenceNode.CalculateSentenceNodeHeight()`
  - `AnswerNode.CalculateAnswerNodeHeight()`
  - `GameFlagConditionNode.CalculateNodeHeight()` ? NEW
  - `SetGameFlagNode.CalculateNodeHeight()` ? NEW
- ? **Consistent pattern**: All call in `Draw()` at start
- ? **Extensible**: Easy to add dynamic height logic later

### Layout Consistency
- ? **All text areas**: Vertical layout, label on top
- ? **All use constants**: No more magic numbers
- ? **All follow same pattern**: Easy to maintain

## Before vs After Summary

### Width Problem (FIXED)
**Before:**
```csharp
GUILayout.Width(NodeWidth - 20)  // Calculated, could change
```

**After:**
```csharp
private const float TextAreaWidth = 180f;  // Constant
GUILayout.Width(TextAreaWidth)              // Never changes
```

### Height Management (COMPLETE)
**Before:**
- SentenceNode: Had `CalculateSentenceNodeHeight()` ?
- AnswerNode: Had `CalculateAnswerNodeHeight()` ?
- GameFlagConditionNode: **Manual height setting** ?
- SetGameFlagNode: **Manual height setting** ?

**After:**
- SentenceNode: Has `CalculateSentenceNodeHeight()` ?
- AnswerNode: Has `CalculateAnswerNodeHeight()` ?
- GameFlagConditionNode: **Has `CalculateNodeHeight()`** ? ? ADDED
- SetGameFlagNode: **Has `CalculateNodeHeight()`** ? ? ADDED

## Result
?? **All nodes now have:**
1. Fixed-width text areas (180px constant) - **no more width changes**
2. Consistent height calculation methods - **all nodes use same pattern**
3. Vertical layouts with text wrapping - **readable and consistent**
4. Stable, predictable dimensions - **no visual glitches**
