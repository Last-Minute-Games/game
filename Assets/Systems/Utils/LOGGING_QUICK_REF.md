# ?? Debug Logging - Quick Reference

## ? Updated Scripts (All OFF by default)

| Script | Location | Logs |
|--------|----------|------|
| **ClockTimer** | ClockTimer GameObject | Timer, frames, death |
| **RoomAudioZone** | Each audio zone | Player/NPC enter/exit |
| **InteractionDetector** | Player GameObject | E key, right-click, hover |
| **TeleportSystem** | Each door | Teleport triggers, range |
| **DialogTrigger** | Each NPC | Dialog start/end, pause |
| **TutorialTrigger** | Each tutorial trigger | Activation, flags |
| **GlobalPause** | Static class | Pause state changes |

---

## ?? How to Toggle

1. **Select** GameObject in Hierarchy
2. **Find** "Debug" section in Inspector  
3. **Check** "Enable Debug Logs"
4. **Play** - see logs!
5. **Uncheck** - logs stop!

---

## ?? Quick Tips

? Console clean by default
? Enable only what you need
? Works during Play mode
? Errors always show
? Zero cost in builds

---

## ?? Common Combos

**Debug Interactions:**
- InteractionDetector ?
- TeleportSystem ? (for doors)
- DialogTrigger ? (for NPCs)

**Debug Timer:**
- ClockTimer ?

**Debug Audio:**
- RoomAudioZone ?

**Debug All:**
- Check all boxes!

That's it! Clean console, powerful debugging! ??
