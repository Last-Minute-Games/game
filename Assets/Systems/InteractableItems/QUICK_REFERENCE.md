# Interaction System - Quick Reference

## ?? For Level Designers / Scene Setup

### Setting Up an NPC
1. Add `InteractiveItem` component to NPC GameObject
2. Add a **Collider2D** (BoxCollider2D or CircleCollider2D)
   - Set as **Trigger** ?
   - Size it to cover the clickable area
3. Assign **DialogBehaviour** and **DialogNodeGraph**
4. Set **Interaction Range** (default 1.0f is good)
5. *(Optional)* Add conversation music

**That's it!** The cursor will automatically change when hovering.

---

## ?? For Programmers

### Creating a Custom Interactable

```csharp
public class MyCustomInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Your interaction logic here
    }
    
    public int GetInteractionPriority()
    {
        return 5; // Lower = higher priority (0-10)
    }
    
    public bool CanInteract()
    {
        return true; // Add your conditions here
    }
    
    public bool ShowInteractionPrompt()
    {
        return true; // Show "E to interact" prompt?
    }
}
```

### Interaction Priorities
```
0 = Critical story/dialog triggers
1 = Teleports/doors
2 = NPCs/interactive items (default)
5 = Minigames
10 = Generic pickups
```

---

## ?? Debugging

### Enable Debug Logs
1. Select **Player** GameObject
2. Find **InteractionDetector** component
3. Check ? **Enable Debug Logs**
4. Watch Console for hover detection messages

### What to Look For
```
[InteractionDetector] Hover: None -> NPC_Alice
```
- Shows when cursor changes
- Shows which interactable is hovered
- Shows detection method used (RAYCAST, TRIGGER, etc.)

### Common Debug Messages
- `Hover: None -> ObjectName` - Started hovering
- `Hover: ObjectA -> ObjectB` - Changed hover target
- `> Hover: ObjectB [RAYCAST] replaces ObjectA` - Why target changed

---

## ?? Inspector Settings Reference

### InteractionDetector (on Player)

| Setting | Default | Description |
|---------|---------|-------------|
| **Use Raycast Detection** | ? | Most reliable (recommended) |
| **Raycast Layer Mask** | -1 (all) | Limit to NPC/item layers for performance |
| **Hover Check Radius** | 1.0f | Fallback detection radius |
| **Enable Directional Hover** | ? | Stardew Valley pointing style |
| **Directional Max Distance** | 3.0f | How far pointing works |
| **Directional Angle Tolerance** | 60° | How precise (higher = easier) |
| **Enable Keyboard Interaction** | ? | Allow E key for interactions |

### InteractiveItem (on NPC)

| Setting | Default | Description |
|---------|---------|-------------|
| **Interaction Range** | 1.0f | Max distance to interact |
| **Dialog Behaviour** | Required | Reference to DialogBehaviour |
| **Dialog Graph** | Required | Conversation to play |
| **Flags To Set** | Optional | Flags set after dialog finishes |
| **Conversation Music** | Optional | Music during conversation |

---

## ?? Tips & Tricks

### For Best Cursor Detection
1. Use **trigger colliders** on NPCs (not solid colliders)
2. Make collider **slightly larger** than sprite for forgiving detection
3. Keep `hoverCheckRadius` at **1.0f** or higher for NPCs
4. Test with debug logs enabled first

### For Better Performance
1. Set `raycastLayerMask` to exclude unnecessary layers
2. Don't put too many interactables in one trigger zone
3. Keep `directionalHoverMaxDistance` reasonable (3-5 units)

### For Stardew Valley-Style Gameplay
1. Keep `enableDirectionalHover` = ?
2. Set `directionalHoverAngleTolerance` = 60° or higher
3. Use `directionalHoverMaxDistance` = 3-4 units
4. This lets players "point" at NPCs without precise aiming

---

## ?? Advanced Usage

### Custom Hover Detection
Override detection methods in a derived class:

```csharp
public class MyInteractionDetector : InteractionDetector
{
    // Override to add custom detection logic
    protected override HoverDetectionResult TryCustomDetection(...)
    {
        // Your custom logic here
        return new HoverDetectionResult(true, distance, "CUSTOM");
    }
}
```

### Priority-Based Interactions
Ensure critical interactions take precedence:

```csharp
public int GetInteractionPriority()
{
    // Override priority based on quest state
    if (GameFlags.HasFlag("urgent_quest"))
        return 0; // Highest priority
    return 2; // Normal priority
}
```

### Conditional Interactions
Control when interactables are available:

```csharp
public bool CanInteract()
{
    // Only allow interaction during specific times
    if (!IsNightTime()) return false;
    if (IsPlayerInDialog()) return false;
    return true;
}
```

---

## ?? Checklist

### When Adding a New NPC
- [ ] InteractiveItem component added
- [ ] Collider2D added and set to Trigger
- [ ] Collider covers clickable area
- [ ] DialogBehaviour assigned
- [ ] DialogNodeGraph assigned
- [ ] Tested cursor changes when hovering
- [ ] Tested E key interaction works
- [ ] Tested right-click interaction works

### When Performance Is Slow
- [ ] Check how many NPCs are in player's trigger zone
- [ ] Verify raycastLayerMask excludes unnecessary layers
- [ ] Consider reducing directionalHoverMaxDistance
- [ ] Profile with Unity Profiler (look for UpdateMouseHover)
- [ ] Disable debug logs in production builds

---

## ?? Need Help?

### Documentation
1. `SYSTEM_OPTIMIZATIONS.md` - Full technical details
2. `CURSOR_DETECTION_IMPROVEMENTS.md` - Cursor-specific info
3. `HOVER_SETUP_GUIDE.md` - Step-by-step setup guide

### Common Questions

**Q: Cursor doesn't change for my NPC?**  
A: Check collider exists, is a trigger, and NPC layer is in raycastLayerMask

**Q: Wrong NPC gets selected when multiple are nearby?**  
A: Check interaction priorities - lower number = higher priority

**Q: Directional hover isn't working?**  
A: Verify enableDirectionalHover is checked and NPC is within directionalHoverMaxDistance

**Q: Performance issues with many NPCs?**  
A: Limit raycastLayerMask and reduce directionalHoverMaxDistance

---

## ?? That's It!

The system is designed to "just work" with sensible defaults. Most users won't need to change anything beyond basic setup.

Happy game developing! ??
