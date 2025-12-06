# Dialog Pause System - Debugging Guide

## Why Some NPCs Don't Trigger GlobalPause

### Root Cause
All NPCs in the scene share the **same `DialogBehaviour` instance**. When any dialog starts, the `DialogBehaviour` broadcasts `OnDialogStarted` to **all** subscribed `DialogTrigger` components. Each `DialogTrigger` uses the `_isMyConversation` flag to determine if it should respond to the event.

### Common Issues

#### 1. **Null DialogBehaviour Reference**
**Symptom**: NPC dialog starts but GlobalPause is not triggered.

**Cause**: The `DialogTrigger` component on the NPC doesn't have a `dialogBehaviour` reference assigned in the Inspector.

**Solution**: 
- Check each NPC GameObject in the Inspector
- Ensure the `DialogTrigger` component has the `Dialog Behaviour` field assigned
- Usually points to a shared DialogBehaviour in the scene (often on a UI canvas)

#### 2. **Runtime Creation Issues**
**Symptom**: NPCs created dynamically (via `AddComponent<DialogTrigger>()`) don't pause properly.

**Cause**: If `dialogBehaviour` is assigned after `Start()` runs, the event listeners are never registered.

**Solution**: Assign `dialogBehaviour` **before** the component's `Start()` method runs, or manually call the subscription:
```csharp
var dialogTrigger = npcObject.AddComponent<DialogTrigger>();
dialogTrigger.dialogBehaviour = myDialogBehaviour; // Set BEFORE Start runs
dialogTrigger.dialogGraph = myGraph;
```

#### 3. **Missing Event Subscriptions**
**Symptom**: Dialog starts but the NPC doesn't face the player or freeze properly.

**Cause**: The `DialogTrigger` failed to subscribe to `OnDialogStarted` and `OnDialogFinished` events during `Start()`.

**Solution**: Check the Console for error messages:
- `"[DialogTrigger] '{name}' has no DialogBehaviour assigned!"`
- If you see this, assign the DialogBehaviour in the Inspector

## How the System Works

### Shared DialogBehaviour Architecture
```
Scene Hierarchy:
??? Canvas (UI)
?   ??? DialogBehaviour (Component) ? Shared instance
??? NPC_Guard
?   ??? DialogTrigger (dialogBehaviour ? points to shared)
??? NPC_Maid  
?   ??? DialogTrigger (dialogBehaviour ? points to shared)
??? NPC_Butler
    ??? DialogTrigger (dialogBehaviour ? points to shared)
```

### Dialog Flow
1. Player presses `E` near NPC_Guard
2. `NPC_Guard.DialogTrigger.StartDialogue()` is called
3. Sets `_isMyConversation = true` (only for NPC_Guard)
4. Calls `dialogBehaviour.StartDialog(dialogGraph)`
5. DialogBehaviour fires `OnDialogStarted` event
6. **ALL** DialogTriggers receive the event (Guard, Maid, Butler)
7. Only NPC_Guard responds because only it has `_isMyConversation = true`
8. NPC_Guard calls `GlobalPause.SetMinigamePaused(true)`

### The _isMyConversation Flag
This flag is the key to the system:
- Set to `true` only when **this** NPC initiates the dialog
- Checked in `OnDialogStart()` and `OnDialogFinished()` to filter events
- Reset to `false` when the dialog ends

## Debugging Checklist

When an NPC dialog doesn't trigger GlobalPause:

1. **Check Console Logs**
   - Look for: `"[DialogTrigger] '{name}' successfully subscribed to DialogBehaviour events"`
   - Missing? The NPC didn't subscribe properly

2. **Verify DialogBehaviour Assignment**
   - Select the NPC in the Hierarchy
   - Look at the DialogTrigger component in the Inspector
   - Is the `Dialog Behaviour` field assigned?

3. **Check Dialog Graph Assignment**
   - Same Inspector view
   - Is the `Dialog Graph` field assigned?

4. **Test Dialog Initiation**
   - Stand near the NPC
   - Press E
   - Look for: `"[DialogTrigger] '{name}' starting dialog (will trigger GlobalPause)"`
   - Missing? The dialog isn't starting at all

5. **Verify GlobalPause Call**
   - Look for: `"[DialogTrigger] GlobalPause minigame pause enabled (NPCs and timer paused)"`
   - Missing? The `OnDialogStart()` method isn't being called

## Example: TutorialScene Dynamic NPC Creation

The `TutorialScene.cs` creates "scary" NPCs dynamically:

```csharp
private void CreateScaryDialogue(Transform npcTransform)
{
    var dialogTrigger = npcTransform.gameObject.AddComponent<DialogTrigger>();
    dialogTrigger.dialogBehaviour = dialogBehaviour; // ? Assigned immediately
    
    var newGraph = ScriptableObject.CreateInstance<DialogNodeGraph>();
    dialogTrigger.dialogGraph = newGraph; // ? Assigned before Start()
    
    dialogTrigger.OnDialogCompleted = new UnityEvent();
    dialogTrigger.OnDialogCompleted.AddListener(ActivateMeltingSequence);
    
    // ... rest of setup
}
```

This works correctly because:
1. `dialogBehaviour` is assigned immediately after `AddComponent`
2. The assignment happens before Unity calls `Start()` on the component
3. When `Start()` runs, it successfully subscribes to the events

## Recent Improvements

### Added Null Checks (2024)
The `DialogTrigger.Start()` method now includes safety checks:
```csharp
if (dialogBehaviour == null)
{
    Debug.LogError($"[DialogTrigger] '{gameObject.name}' has no DialogBehaviour assigned!");
    return; // Don't subscribe if null
}
```

### Added Debug Logging
Both `Start()` and `StartDialogue()` now log their actions:
- Successful subscription: `"successfully subscribed to DialogBehaviour events"`
- Starting dialog: `"starting dialog (will trigger GlobalPause)"`
- Errors: Clear messages with object name and context

## Testing NPCs

To verify an NPC is set up correctly:

1. **Play the game**
2. **Open the Console** (Window ? General ? Console)
3. **Interact with the NPC**
4. **Look for this sequence**:
   ```
   [DialogTrigger] 'NPC_Name' successfully subscribed to DialogBehaviour events
   [DialogTrigger] 'NPC_Name' starting dialog (will trigger GlobalPause)
   [DialogTrigger] GlobalPause minigame pause enabled (NPCs and timer paused)
   ```

If you see errors or missing logs, follow the debugging checklist above.
