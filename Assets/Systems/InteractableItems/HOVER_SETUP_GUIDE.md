# Stardew Valley Style Hover Detection - Setup Guide

## ? How It Works Now

### NPCs & Interactable Items (Blackjack, etc.)
- **Hover** your mouse cursor over the NPC/item
- Cursor **changes** to show you can interact
- **Right-click** to interact

### Doors/Teleports
- Walk near them
- Right-click **anywhere** (no hover needed)
- Instant teleport!

### Priority System
If hovering over an NPC while near a door:
- **Hovered item takes priority** (NPC will be interacted with)
- Doors only activate if you're NOT hovering over something else

---

## ?? Setup in Unity

### Step 1: Assign Cursor Textures
1. Select your **Player** GameObject
2. Find the **InteractionDetector** component
3. Assign these fields:
   - **Interact Cursor**: A hand icon or highlight cursor (shows when hovering)
   - **Default Cursor**: Your normal cursor
   - **Cursor Hotspots**: Usually `(16, 16)` for center of a 32x32 cursor

### Step 2: Enable/Disable Hover Detection
- **Enable Hover Detection**: ? Checked (default - Stardew Valley style)
- **Hover Check Radius**: `0.5` to `1.5` (how close mouse needs to be)

### Step 3: Test It!
1. Play the game
2. Walk near an NPC or Blackjack entrance
3. Move your mouse over them
4. **Watch for**:
   - Cursor changes to interact cursor ?
   - Yellow circle appears in Scene view (if Gizmos enabled)
5. Right-click to interact!

---

## ?? Troubleshooting

### Cursor Not Changing

**Check**:
1. Cursor textures assigned on InteractionDetector?
2. `Enable Hover Detection` is checked?
3. Texture import settings:
   - Texture Type: **Default** or **Cursor**
   - Read/Write Enabled: **TRUE**

### NPCs Hard to Click

**Fix**: Increase `Hover Check Radius`:
- Try `1.0` - easier to click
- Try `1.5` - even easier
- Try `0.5` - more precise (default)

### Collider Too Small

**Add/Adjust Collider**:
1. Select NPC GameObject
2. Add **CircleCollider2D** or **BoxCollider2D**
3. Set **Is Trigger** = TRUE
4. Adjust size to match sprite

---

## ?? How Hover Detection Works

### Method 1: Collider Check (Precise)
- Checks if mouse is over the object's Collider2D
- Works great for items with precise colliders
- Best for complex shapes

### Method 2: Distance Check (Fallback)
- If no collider or collider missed
- Checks if mouse is within `hoverCheckRadius` of object center
- Perfect for NPCs/items with small colliders

### Both Methods Work Together!
The system tries collider first, then distance. You don't need to do anything special!

---

## ?? Visual Debugging

In Scene View with Gizmos enabled, you'll see:
- **Yellow wireframe circles**: Around hoverable interactables
- **Green filled circle**: Around the currently hovered interactable
- Adjust circle size with `Hover Check Radius`

---

## ?? Toggle System

Don't like hover detection? Turn it off!

**In Inspector**: Uncheck `Enable Hover Detection`
- System falls back to click-anywhere mode
- Works like the simple version
- Useful for testing or if you prefer simpler controls

---

## ?? What Each Interactable Does

| Type | Hover Required? | Cursor Changes? | Click Anywhere? |
|------|----------------|-----------------|-----------------|
| NPCs | ? Yes | ? Yes | ? No |
| Blackjack Entrance | ? Yes | ? Yes | ? No |
| Items | ? Yes | ? Yes | ? No |
| Doors/Teleports | ? No | ? No | ? Yes |

---

## ?? Player Experience

### Feels Like Stardew Valley!
- Move mouse over NPCs ? cursor changes ? click!
- Move mouse over Blackjack entrance ? cursor changes ? click!
- Near a door ? click anywhere ? teleport!
- Near both NPC and door ? hover NPC ? NPC takes priority!

---

## ?? Debug Console Messages

Look for these messages:
- `[InteractionDetector] Cursor changed - hovering over: [Name]` = Hover working! ?
- `[InteractionDetector] Right-click on hovered item: DialogTrigger` = Hovered interaction! ?
- `[InteractionDetector] Right-click interacting with: TeleportSystem` = Door (no hover) ?

---

## ? Performance Notes

? **Optimized**:
- Camera.main cached (called once, not every frame)
- Only checks hover for nearby interactables
- Skips hover check if detection is disabled
- Efficient collider checks

---

That's it! You now have Stardew Valley style interactions! ??
