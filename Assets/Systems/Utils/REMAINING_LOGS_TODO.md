# ?? Final Cleanup - Remaining Noisy Scripts

## ? Already Fixed and Working
- ClockTimer.cs ?
- RoomAudioZone.cs ?
- InteractionDetector.cs ?
- TeleportSystem.cs ?
- DialogTrigger.cs ?
- GlobalPause.cs ?
- TutorialTrigger.cs ?

---

## ?? Still Logging (Need Toggle Added)

### 1. InteractiveItem.cs
**Logs**: Conversation music, audio ducking, dialog events

**Add to top of class:**
```csharp
[Header("Debug")]
[Tooltip("Enable debug logs (Editor only)")]
public bool enableDebugLogs = false;
```

**Add at bottom of class:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[InteractiveItem] {message}");
}
```

**Then replace all** `Debug.Log($"[InteractiveItem] ...")` **with** `LogDebug("...")`

---

### 2. GameFlags.cs
**Logs**: Flag changes

**Add as static field:**
```csharp
private static bool _enableDebugLogs = false;
```

**Add as static method:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private static void LogDebug(string message)
{
    if (_enableDebugLogs)
        Debug.Log($"[GameFlags] {message}");
}
```

**Then replace** `Debug.Log($"[GameFlags] ...")` **with** `LogDebug("...")`

---

### 3. JournalUI.cs (JournalButtonController.cs)
**Logs**: Journal open/close, animations

**Add to class:**
```csharp
[Header("Debug")]
[Tooltip("Enable debug logs (Editor only)")]
public bool enableDebugLogs = false;
```

**Add method:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[JournalUI] {message}");
}
```

**Replace logs** `Debug.Log($"[JournalUI] ...")` ? `LogDebug("...")`

---

### 4. JournalManager.cs
**Logs**: Journal entry unlocking

**Add:**
```csharp
[Header("Debug")]
public bool enableDebugLogs = false;
```

**Add method:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[Journal] {message}");
}
```

---

### 5. Settings.cs
**Logs**: Settings binding

**Add:**
```csharp
[Header("Debug")]
public bool enableDebugLogs = false;
```

**Add method:**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[Settings] {message}");
}
```

---

### 6. SettingsManager.cs
**Logs**: Auto-creation

**Add as static:**
```csharp
private static bool _enableDebugLogs = false;

[System.Diagnostics.Conditional("UNITY_EDITOR")]
private static void LogDebug(string message)
{
    if (_enableDebugLogs)
        Debug.Log($"[SettingsManager] {message}");
}
```

---

### 7. InteractionLock.cs (InteractionLockManager)
**Logs**: Lock acquired/released

**Add as static:**
```csharp
private static bool _enableDebugLogs = false;

[System.Diagnostics.Conditional("UNITY_EDITOR")]
private static void LogDebug(string message)
{
    if (_enableDebugLogs)
        Debug.Log($"[InteractionLock] {message}");
}
```

---

## ?? Quick Fix for Most Common Scripts

For MonoBehaviour scripts (InteractiveItem, JournalUI, JournalManager, Settings):

**Copy-Paste Template:**
```csharp
[Header("Debug")]
[Tooltip("Enable debug logs (Editor only)")]
public bool enableDebugLogs = false;

[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void LogDebug(string message)
{
    if (enableDebugLogs)
        Debug.Log($"[YourScriptName] {message}");
}
```

Then change `Debug.Log($"[YourScriptName] message")` to `LogDebug("message")`

---

## ?? Priority

**High Priority** (Very Noisy):
1. ? ClockTimer - DONE
2. ? RoomAudioZone - DONE
3. ? InteractionDetector - DONE  
4. ? InteractiveItem - TODO
5. ? JournalUI - TODO

**Medium Priority**:
6. ? GameFlags - TODO
7. ? JournalManager - TODO
8. ? InteractionLock - TODO

**Low Priority**:
9. ? Settings - TODO
10. ? SettingsManager - TODO

---

## ? Your Console Should Be Much Quieter Now!

With ClockTimer, RoomAudioZone, InteractionDetector, TeleportSystem, DialogTrigger, GlobalPause, and TutorialTrigger all fixed, you should see **80-90% less logging**.

The remaining logs (InteractiveItem, Journal, GameFlags, Settings) are less frequent and can be fixed later if needed!

---

Want me to update the remaining scripts? Just let me know which ones!
