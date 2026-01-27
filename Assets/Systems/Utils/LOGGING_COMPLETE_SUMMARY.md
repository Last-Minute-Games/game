# ?? Toggleable Debug Logging - COMPLETE!

## ? All Scripts Updated!

### Core Scripts with Toggleable Logging:
1. ? **ClockTimer.cs** - Timer logs (OFF by default)
2. ? **RoomAudioZone.cs** - Room audio zone triggers (OFF by default)
3. ? **InteractionDetector.cs** - Interaction detection & hover (OFF by default)
4. ? **TeleportSystem.cs** - Door/teleport interactions (OFF by default)
5. ? **DialogTrigger.cs** (DialogHandler.cs) - NPC dialogues (OFF by default)
6. ? **GlobalPause.cs** - Pause system (OFF by default)
7. ? **TutorialTrigger.cs** - Tutorial triggers (OFF by default)

---

## ?? How to Use

### Toggle Logs Per-Script in Inspector

**For MonoBehaviour scripts** (most of them):
1. Select the GameObject in scene hierarchy
2. Look for **"Debug"** header in Inspector
3. Check/uncheck **"Enable Debug Logs"**

**Example:**
- Select `ClockTimer` GameObject ? Check "Enable Debug Logs" ? See timer logs
- Uncheck ? Logs stop immediately

**For Static Classes** (GlobalPause):
- Logs are OFF by default
- Currently need code change to enable (can add Inspector toggle if needed)

---

## ?? Each Script's Toggle

| Script | Toggle Location | What It Logs |
|--------|----------------|--------------|
| **ClockTimer** | ClockTimer GameObject ? Enable Debug Logs | Frame changes, time updates, death sequence |
| **RoomAudioZone** | Each RoomAudioZone ? Enable Debug Logs | Player enter/exit, NPC triggers |
| **InteractionDetector** | Player ? InteractionDetector ? Enable Debug Logs | E key, right-click, hover, nearby items |
| **TeleportSystem** | Each Door ? Enable Debug Logs | Player near, teleport triggers, range checks |
| **DialogTrigger** | Each NPC ? Enable Debug Logs | Dialog start/end, GlobalPause calls |
| **GlobalPause** | (Static - code only) | Pause state changes |
| **TutorialTrigger** | Each trigger ? Enable Debug Logs | Tutorial activation, flag setting |

---

## ?? Before & After

### Before (Console Spam):
```
[ClockTimer] Time left: 120.0s
[ClockTimer] Time left: 119.0s  
[ClockTimer] Time left: 118.0s
OnTriggerEnter2D with: Knight (5)
OnTriggerEnter2D with: Maid (3)
OnTriggerEnter2D with: Knight (7)
[ClockTimer] Frame changed: 0/12 | Time left: 119.98s
PLAYER ENTERED zone: Bedroom
OnTriggerEnter2D with: Maid (11)
[InteractionDetector] Added interactable: Spawn
[TeleportSystem] Spawn: Player entered range!
[InteractionDetector] E key pressed! Nearby interactables: 1
[TeleportSystem] Spawn: Interact() called!
[InteractionLock] Lock acquired.
... (hundreds more per second)
```

### After (Clean Console):
```
(empty - only errors/warnings when they occur)
```

### With Selective Logging Enabled:
```
// Only ClockTimer enabled:
[ClockTimer] Time left: 120.0s
[ClockTimer] Frame changed: 0/12 | Time left: 119.98s
[ClockTimer] Time left: 119.0s

// Only InteractionDetector enabled:
[InteractionDetector] Added interactable: Spawn
[InteractionDetector] E key pressed! Nearby interactables: 1
[InteractionDetector] Right-click detected!
```

---

## ?? Common Use Cases

### Debugging Timer Issues:
1. Enable **ClockTimer** logs only
2. See exact time progression
3. Verify frame changes
4. Check pause/resume

### Debugging Interaction Problems:
1. Enable **InteractionDetector** logs
2. Enable **TeleportSystem** or **DialogTrigger** for specific type
3. See what's being detected
4. See priority calculations
5. See interaction flow

### Debugging NPC Dialogues:
1. Enable **DialogTrigger** logs
2. Enable **GlobalPause** if needed
3. See dialog start/end
4. See pause system calls

### Debugging Audio Zones:
1. Enable **RoomAudioZone** logs
2. See exactly when zones trigger
3. See which NPCs are triggering them

---

## ? Technical Benefits

? **Zero Performance Cost** - Logs completely removed from builds via `[Conditional("UNITY_EDITOR")]`
? **Per-Script Control** - Enable only what you're debugging
? **Runtime Toggle** - Change in Inspector during Play mode (takes effect immediately)
? **Safe** - Errors and warnings ALWAYS show (not affected by toggles)
? **Clean Code** - Simple `LogDebug()` calls throughout

---

## ??? How It Works

### Each script has:

**1. Debug Toggle Field:**
```csharp
[Header("Debug")]
[Tooltip("Enable debug logs (Editor only)")]
public bool enableDebugLogs = false;
```

**2. LogDebug Method:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[ScriptName] {message}");
}
```

**3. Replaced Debug.Log calls:**
```csharp
// OLD:
Debug.Log("[ClockTimer] Time left: " + timeLeft);

// NEW:
LogDebug($"Time left: {timeLeft}");
```

**4. Errors always show:**
```csharp
Debug.LogError("This always shows!"); // Not affected by toggle
Debug.LogWarning("This always shows!"); // Not affected by toggle
```

---

## ?? Remaining Scripts (Not Critical)

These scripts have minimal/rare logging - can add toggles if needed:

- GameFlags.cs (only logs flag changes - could add toggle)
- JournalButtonController.cs (JournalUI - verbose but less frequent)
- JournalManager.cs (only logs journal entries)
- Settings.cs (only on settings load)
- InteractiveItem.cs (minimal logging)
- BlackjackEntrance.cs (only on interaction)

**To add logging to these later:**
Just copy-paste the Debug header + LogDebug method from any updated script!

---

## ?? Testing Checklist

- [x] Build compiles successfully
- [x] All toggles default to OFF
- [x] Console is clean by default
- [x] Toggling in Inspector works
- [x] Logs appear when enabled
- [x] Logs stop when disabled
- [x] Errors/warnings still always show
- [x] Works in Play mode
- [x] Logs stripped from builds

---

## ?? Inspector View

When you select a GameObject with logging, you'll see:

```
?? Debug ??????????????????????????
? ? Enable Debug Logs             ?
?   (Tooltip: Enable debug logs   ?
?    Editor only)                  ?
????????????????????????????????????
```

Just check the box ? logs appear!
Uncheck ? logs stop!

---

## ?? Quick Reference

| Want to Debug | Enable Logs On |
|--------------|----------------|
| Timer issues | ClockTimer |
| Door interactions | TeleportSystem |
| NPC dialogues | DialogTrigger |
| Item interactions | InteractionDetector |
| Hover detection | InteractionDetector |
| Audio zones | RoomAudioZone |
| Tutorial triggers | TutorialTrigger |
| Pause system | (GlobalPause - code only) |

---

## ?? Pro Tips

1. **Enable multiple at once** for complex debugging
2. **Leave all OFF normally** for clean console
3. **Errors always show** - you can't miss real problems
4. **Works in Play mode** - toggle during gameplay
5. **No performance cost** - logs disappear in builds

---

## ? Summary

**7 major scripts updated**
**All logging toggleable**
**All OFF by default**
**Console is now clean!**
**Easy to debug when needed!**

Your console will thank you! ??
