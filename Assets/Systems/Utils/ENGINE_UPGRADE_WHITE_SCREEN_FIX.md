# Unity 6000.0 ? 6000.0.3 Engine Upgrade - White Screen Fix

## ? Problem
After upgrading from Unity 6000.0 to 6000.0.3, the camera shows **all white** instead of rendering the scene.

## ?? Root Cause
This is a **Universal Render Pipeline (URP)** compatibility issue, NOT related to camera caching code. Unity engine upgrades often reset or break URP settings.

---

## ? FIXES (Try in Order)

### **Fix 1: Check URP 2D Renderer Asset**

1. **Open Project Settings:**
   - Edit ? Project Settings ? Graphics

2. **Check if URP Renderer is assigned:**
   - Look for "Scriptable Render Pipeline Settings"
   - Should point to a **UniversalRenderPipelineAsset**

3. **If missing or broken:**
   ```
   Assets ? Create ? Rendering ? URP Asset (2D Renderer)
   ```
   - Assign the new asset in Project Settings ? Graphics

4. **Check Camera Clear Flags:**
   - Select Main Camera in scene
   - Change "Clear Flags" to **Solid Color** (NOT Skybox)
   - Set Background color to Black or your desired color

---

### **Fix 2: Fix Global Light2D Conflicts**

Your project has a tool to fix this:

1. **Run the auto-fix:**
   ```
   Tools ? 2D Lighting ? Auto-Fix Duplicates (Assign Unique Blend Styles)
   ```

2. **Or manually find duplicates:**
   ```
   Tools ? 2D Lighting ? Find Duplicate Global Lights
   ```

3. **Why this matters:**
   - Unity 6 enforces stricter light rules
   - Multiple Global Light2D with same blend style = white screen
   - Each scene needs a unique blend style index (0-3)

---

### **Fix 3: Recreate 2D Renderer Data**

If URP asset exists but still white:

1. **Create new 2D Renderer:**
   ```
   Assets ? Create ? Rendering ? URP 2D Renderer
   ```

2. **Assign to URP Asset:**
   - Select your UniversalRenderPipelineAsset
   - In Inspector ? Renderer List
   - Add the new 2D Renderer

3. **Configure 2D Renderer:**
   - Enable "Transparency Sort Mode: Custom Axis"
   - Set Transparency Sort Axis: (0, 1, 0) for Y-axis sorting

---

### **Fix 4: Check Camera Component Settings**

Select **Main Camera** in your scenes and verify:

```plaintext
Camera Component:
?? Clear Flags: Solid Color
?? Background: Black (or desired color)
?? Culling Mask: Everything (or your desired layers)
?? Projection: Orthographic (for 2D)
?? Renderer: <Your 2D Renderer> (in URP additional camera data)
```

**Important for URP:**
- Camera should have **Universal Additional Camera Data** component
- Renderer Type should match your URP 2D Renderer

---

### **Fix 5: Reimport TextMesh Pro**

Your shaders show TMP is included. After Unity upgrades, TMP can break:

1. **Reimport TMP:**
   ```
   Window ? TextMeshPro ? Import TMP Essential Resources
   ```

2. **If that doesn't work, delete and reimport:**
   ```
   Delete: Assets/TextMesh Pro folder
   Window ? TextMeshPro ? Import TMP Essential Resources
   ```

---

### **Fix 6: Clear Library and Reimport**

Last resort if nothing else works:

1. **Close Unity**

2. **Delete these folders:**
   ```
   Library/
   Temp/
   obj/
   ```

3. **Reopen project** (Unity will reimport everything - takes 5-10 minutes)

---

## ?? Quick Diagnostic Checklist

Run through these quickly to identify the issue:

- [ ] Check Console for errors (especially shader/URP errors)
- [ ] Main Camera has "Clear Flags: Solid Color" (NOT Skybox)
- [ ] URP Asset is assigned in Project Settings ? Graphics
- [ ] No duplicate Global Light2D errors in console
- [ ] Camera has "Universal Additional Camera Data" component
- [ ] Scene view shows correctly (if Game view is white but Scene view works = camera issue)

---

## ?? Most Common Fix

**90% of the time it's this:**

1. Select **Main Camera**
2. Change **Clear Flags** from "Skybox" to **"Solid Color"**
3. Set **Background** to **Black** (or desired color)
4. Done!

---

## ?? If Still White After All Fixes

Check these advanced issues:

### **Issue: Shader Compilation Errors**
- Open Console (Ctrl+Shift+C)
- Filter by "Error"
- Look for shader compilation failures
- **Fix:** Reimport shaders or update URP package

### **Issue: Post-Processing Stack**
- Disable Post-Processing on camera temporarily
- If that fixes it, your post-processing broke in upgrade

### **Issue: Multiple Cameras**
- Check if you have multiple cameras in scene
- Ensure they have different depths (Base = 0, Overlay = 1+)
- One camera must be "Base", others "Overlay"

---

## ?? Debug Commands

### **Check URP Asset:**
```csharp
// Add this to a MonoBehaviour and check Inspector
UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset asset = 
    (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)
    UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
Debug.Log("URP Asset: " + (asset != null ? asset.name : "NULL"));
```

### **Check Camera:**
```csharp
Camera cam = Camera.main;
Debug.Log("Camera: " + (cam != null ? cam.name : "NULL"));
Debug.Log("Clear Flags: " + cam.clearFlags);
Debug.Log("Background Color: " + cam.backgroundColor);
```

---

## ? Expected Result

After fixing:
- Game view shows **black** background (or your chosen color)
- Sprites/UI render correctly
- No white screen

---

## ?? Prevention for Future Upgrades

Before upgrading Unity:

1. **Backup Project** (obviously)
2. **Export URP settings:**
   - Right-click URP Asset ? Export Package
3. **Document current settings:**
   - Screenshot Project Settings ? Graphics
   - Screenshot Camera settings
4. **After upgrade:**
   - Compare settings
   - Reassign if needed

---

**Status:** Ready to fix white screen issue  
**Not caused by:** Performance optimizations (those are fine)  
**Caused by:** Unity 6000.0.3 upgrade breaking URP settings  
**Solution:** Follow Fix 1 first (camera clear flags), then Fix 2 (Global Light2D)

---

**Need More Help?**
1. Check Console for specific errors
2. Open Scene view - if that works, it's definitely a camera/URP issue
3. Try Safe Mode: Hold Alt while opening Unity to skip scripts
