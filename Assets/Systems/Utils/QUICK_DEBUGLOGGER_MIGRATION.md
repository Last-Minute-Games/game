# Quick Migration Guide: Replace Debug.Log with DebugLogger

## For ALL remaining files, do this find-and-replace:

### GameFlags.cs
```csharp
// Find all instances of:
Debug.Log($"[GameFlags] {message}")
Debug.Log("[GameFlags] ...")

// Replace with:
DebugLogger.LogGameFlags(message)
DebugLogger.LogGameFlags("...")
```

### JournalManager.cs
```csharp
// Find all instances of:
Debug.Log($"[Journal] {message}")
Debug.Log("[Journal] ...")

// Replace with:
DebugLogger.LogJournal(message)
DebugLogger.LogJournal("...")
```

### JournalButtonController.cs (JournalUI)
```csharp
// Find all instances of:
Debug.Log($"[JournalUI] {message}")
Debug.Log("[JournalUI] ...")

// Replace with:
DebugLogger.LogJournalUI(message)
DebugLogger.LogJournalUI("...")
```

### JournalTabController.cs (JournalUI_Named)
```csharp
// Find all instances of:
Debug.Log("[JournalUI_Named] ...")

// Replace with:
DebugLogger.LogJournalNamed("...")
```

### JournalPaginationController.cs
```csharp
// Find all instances of:
Debug.Log("[Pagination] ...")

// Replace with:
DebugLogger.LogPagination("...")
```

### SettingsManager.cs
```csharp
// Find all instances of:
Debug.Log("[SettingsManager] ...")

// Replace with:
DebugLogger.LogSettingsManager("...")
```

### Settings.cs
```csharp
// Find all instances of:
Debug.Log("[Settings] ...")

// Replace with:
DebugLogger.LogSettings("...")
```

### OverworldWakeUpCutscene.cs
```csharp
// Find all instances of:
Debug.Log("[OverworldWakeUpCutscene] ...")

// Replace with:
// Add new category to DebugLogger first!
DebugLogger.LogCutscene("...") 
// OR just use LogGeneral for now
```

## Pattern for ALL files:
1. Remove the `[Tag]` from the message (DebugLogger adds it automatically)
2. Replace `Debug.Log($"[Tag] {message}")` with `DebugLogger.LogTag(message)`
3. Replace `Debug.Log("[Tag] text")` with `DebugLogger.LogTag("text")`

## Keep Debug.LogWarning and Debug.LogError as-is!
These should ALWAYS show, so use:
- `DebugLogger.LogWarning(...)` 
- `DebugLogger.LogError(...)`

---

**After migration, ALL logs will be:**
? Disabled by default in editor  
? Completely stripped from production builds  
? Toggleable per-category  
? Zero performance overhead
