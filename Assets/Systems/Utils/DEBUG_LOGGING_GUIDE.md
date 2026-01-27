# Debug Logging System - Implementation Guide

## ? What's Been Done

### Created Centralized Logging System
1. **DebugLogger.cs** (`Assets/Systems/Utils/DebugLogger.cs`)
   - Centralized logging with categories
   - All logs stripped from builds automatically (uses `[Conditional("UNITY_EDITOR")]`)
   - Toggle individual categories on/off

2. **DebugLoggerSettings.cs** (`Assets/Systems/Utils/DebugLoggerSettings.cs`)
   - ScriptableObject to configure logging in Inspector
   - Easy toggle interface

### Updated ClockTimer.cs
- Added `enableDebugLogs` boolean toggle (default: **false**)
- Added `LogDebug()` wrapper method
- All logs automatically disabled in builds

---

## ?? How to Use

### Quick Fix for Any Script

Add this at the top of your class:
```csharp
[Header("Debug Logging")]
[Tooltip("Enable debug logs (Editor only)")]
public bool enableDebugLogs = false;
```

Add this method at the bottom:
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[YourScriptName] {message}");
}
```

Then replace all `Debug.Log()` with `LogDebug()`:
```csharp
// OLD:
Debug.Log("[ClockTimer] Time left: " + timeLeft);

// NEW:
LogDebug($"Time left: {timeLeft}");
```

---

## ?? Scripts That Need Updating

### High Priority (Very Noisy):
1. ? **ClockTimer.cs** - DONE (has toggle, default OFF)
2. ? **RoomAudioZone.cs** - Needs toggle
3. ? **InteractionDetector.cs** - Needs toggle  
4. ? **TeleportSystem.cs** - Needs toggle

### Medium Priority (Somewhat Noisy):
5. ? **DialogTrigger.cs** (`DialogHandler.cs`)
6. ? **GlobalPause.cs**
7. ? **JournalUI.cs** (`JournalButtonController.cs`)
8. ? **GameFlags.cs**

### Low Priority (Occasional):
9. ? **TutorialTrigger.cs**
10. ? **Settings.cs**
11. ? **JournalManager.cs**

---

## ?? Recommended Approach

### Option 1: Manual (Safest)
For each noisy script:
1. Add `enableDebugLogs` boolean
2. Add `LogDebug()` method  
3. Replace `Debug.Log()` with `LogDebug()`
4. Leave `Debug.LogError()` and `Debug.LogWarning()` unchanged

### Option 2: Search & Replace (Faster)
Use Visual Studio "Find and Replace in Files":
- Find: `Debug.Log(`
- Replace with: `LogDebug(`
- Files: Select specific script
- Then add the toggle and method manually

---

## ?? Quick Start

### 1. Update RoomAudioZone.cs (Highest Priority)

Add at top of class:
```csharp
[Header("Debug")]
public bool enableDebugLogs = false;
```

Add at bottom:
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log(message);
}
```

Replace all `Debug.Log` with `LogDebug` in that file.

### 2. Update InteractionDetector.cs

Same process - add toggle, add method, replace calls.

### 3. Update TeleportSystem.cs

Same process.

---

## ? Benefits

? **No performance cost in builds** - Logs are completely stripped by compiler
? **Easy to toggle** - Just check/uncheck in Inspector
? **Organized** - Each script controls its own logging
? **Safe** - Errors and warnings still always show
? **Default OFF** - Quiet console by default

---

## ?? Testing

After updating a script:
1. Check Inspector - toggle should be **unchecked** by default
2. Press Play - console should be quiet
3. Check the toggle - logs should appear
4. Uncheck toggle - logs should stop
5. Build the game - no logs in builds (stripped by compiler)

---

## ?? Checklist

- [ ] Run game - verify console is quieter
- [ ] Check ClockTimer toggle is OFF
- [ ] Update RoomAudioZone.cs
- [ ] Update InteractionDetector.cs
- [ ] Update TeleportSystem.cs
- [ ] Test each toggle works
- [ ] Errors still show (good!)

---

Want me to update the remaining scripts automatically? Just say which ones!
