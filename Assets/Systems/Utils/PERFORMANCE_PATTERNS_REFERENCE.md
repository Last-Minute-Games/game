# Performance Optimization Quick Reference
## Critical Performance Patterns - DO's and DON'Ts

---

## ? **AVOID - Performance Killers**

### 1. Camera.main in Update/Loops
```csharp
// ? BAD - Calls FindObjectOfType every time
void Update() {
    Camera cam = Camera.main;
    Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
}
```

### 2. FindFirstObjectByType Repeatedly
```csharp
// ? BAD - Very expensive, called every drag
public void OnEndDrag(PointerEventData eventData) {
    var manager = FindFirstObjectByType<PlayerManager>();
    var viewer = FindFirstObjectByType<DeckViewer>();
}
```

### 3. GetComponentsInChildren Multiple Times
```csharp
// ? BAD - Queries all children 5 times
void Awake() {
    cardBackground = FindChildByName<SpriteRenderer>("CardBackground");
    cardIcon = FindChildByName<SpriteRenderer>("CardIcon");
    energyCost = FindChildByName<TMP_Text>("EnergyCost");
    cardName = FindChildByName<TMP_Text>("CardName");
    descriptionText = FindChildByName<TMP_Text>("DescriptionText");
}
```

### 4. Physics Array Allocations
```csharp
// ? BAD - Allocates new array every call
Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
```

### 5. Unnecessary Update Checks
```csharp
// ? BAD - Runs every frame even when mouse doesn't move
void Update() {
    UpdateMouseHover();  // Expensive raycast/overlap checks
}
```

---

## ? **USE - Optimized Patterns**

### 1. Cache Camera.main Once
```csharp
// ? GOOD - Cache in Awake, use throughout with fallback
private Camera _mainCamera;

void Awake() {
    _mainCamera = Camera.main;  // Try to get it early
}

// Helper method with fallback for scenes where camera initializes late
private Camera GetCamera() {
    if (_mainCamera == null)
        _mainCamera = Camera.main;
    return _mainCamera;
}

void Update() {
    Camera cam = GetCamera();
    if (cam != null) {
        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
    }
}
```

**?? IMPORTANT:** Some scenes (like battle scenes) may initialize the camera after your script's `Awake()`. Always include a fallback check!

### 2. Cache Manager References
```csharp
// ? GOOD - Cache once in Awake
private PlayerManager _playerManager;
private DeckViewer _handViewer;
private RoundManager _roundManager;

void Awake() {
    _playerManager = FindFirstObjectByType<PlayerManager>();
    _roundManager = FindFirstObjectByType<RoundManager>();
    _handViewer = FindFirstObjectByType<DeckViewer>();
}

public void OnEndDrag(PointerEventData eventData) {
    if (_playerManager != null) {
        _playerManager.PlayCard(...);
    }
}
```

### 3. Batch GetComponentsInChildren Calls
```csharp
// ? GOOD - Get all components once, search cached array
void Awake() {
    var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    var tmpTexts = GetComponentsInChildren<TMP_Text>(true);
    
    cardBackground = FindInArray(spriteRenderers, "CardBackground");
    cardIcon = FindInArray(spriteRenderers, "CardIcon");
    energyCost = FindInArray(tmpTexts, "EnergyCost");
    cardName = FindInArray(tmpTexts, "CardName");
    descriptionText = FindInArray(tmpTexts, "DescriptionText");
}

private T FindInArray<T>(T[] array, string contains) where T : Component {
    string search = contains.ToLowerInvariant();
    foreach (var item in array) {
        if (item.name.ToLowerInvariant().Contains(search))
            return item;
    }
    return null;
}
```

### 4. Use NonAlloc Physics Methods
```csharp
// ? GOOD - Reusable buffer, zero allocations
private Collider2D[] _colliderBuffer = new Collider2D[10];

private EnemyRender GetEnemyOnMouse(Vector2 screenPosition) {
    int count = Physics2D.OverlapPointNonAlloc(worldPos, _colliderBuffer);
    
    for (int i = 0; i < count; i++) {
        var collider = _colliderBuffer[i];
        // ... process collider
    }
}
```

### 5. Only Update When Needed
```csharp
// ? GOOD - Only check when mouse moves
private Vector3 _lastMousePosition;

void Update() {
    Vector3 currentMousePos = Input.mousePosition;
    if ((currentMousePos - _lastMousePosition).sqrMagnitude > 0.01f) {
        UpdateMouseHover();
        _lastMousePosition = currentMousePos;
    }
}
```

---

## ?? **PERFORMANCE CHECKLIST**

When writing new code, check:

- [ ] Is `Camera.main` cached (not called in Update/loops)?
- [ ] Are manager references cached (not using FindObjectOfType repeatedly)?
- [ ] Are component searches batched (not calling GetComponentsInChildren multiple times)?
- [ ] Are physics checks using NonAlloc versions?
- [ ] Is Update loop only running when needed (not every frame unnecessarily)?
- [ ] Are Debug.Log calls wrapped in conditional compilation for builds?

---

## ?? **HOW TO FIND PERFORMANCE ISSUES**

### 1. Search for these patterns in your code:

```
Camera.main          ? Should only appear in Awake/Start
FindObjectOfType     ? Should only appear in Awake/Start
FindFirstObjectByType ? Should only appear in Awake/Start
GetComponentsInChildren ? Should be batched when possible
Physics2D.OverlapPointAll ? Should use NonAlloc version
```

### 2. Check Update() methods:

- Are they doing expensive operations every frame?
- Can they be optimized to only run when needed?
- Can they use coroutines with delays instead?

### 3. Use Unity Profiler:

- Window > Analysis > Profiler
- Look for CPU spikes in Update loops
- Check for GC allocations (should be minimal during gameplay)

---

## ?? **REFERENCE EXAMPLES**

See these files for properly optimized patterns:

- ? `CardRender.cs` - Caching Camera.main, managers, using NonAlloc
- ? `InteractionDetector.cs` - Optimized Update loop with mouse tracking
- ? `BezierCardArrowHelper.cs` - Proper Camera.main caching

---

**Last Updated:** Today  
**Framework:** .NET Framework 4.7.1  
**Unity:** 2019+
