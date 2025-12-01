# Single Play Button with Smart Save Flow - Implementation Guide

## Your Current Setup

Based on your hierarchy, you have:
```
MainMenu
??? StartScreen
?   ??? Buttons
?   ?   ??? PlayButton ? (Already exists)
?   ?   ??? SettingsButton ?
?   ?   ??? CreditsButton ?
?   ?   ??? QuitButton ?
?   ??? SaveSystem
?       ??? Buttons
?       ?   ??? LoadButton ? (Already exists)
?       ?   ??? ContinueButton ? (Already exists)
```

## Flow Overview

```
???????????????????????????????????????????????????????????????????
?                         MAIN MENU                               ?
?                                                                 ?
?                     ?? Castle of Time                           ?
?                                                                 ?
?              ???????????????????????                           ?
?              ?   [? Play]          ? ???? SINGLE BUTTON        ?
?              ???????????????????????                           ?
?                        ?                                        ?
?              ???????????????????????                           ?
?              ?   [? Settings]      ?                           ?
?              ???????????????????????                           ?
?                                                                 ?
?              ???????????????????????                           ?
?              ?   [?? Credits]       ?                           ?
?              ???????????????????????                           ?
?                                                                 ?
?              ???????????????????????                           ?
?              ?   [? Quit]          ?                           ?
?              ???????????????????????                           ?
???????????????????????????????????????????????????????????????????
                        ?
        ?????????????????????????????????
        ? Check: Do saves exist?        ?
        ?????????????????????????????????
                ?               ?
        NO saves        YES saves exist
                ?               ?
                ?               ?
    ????????????????????   ????????????????????
    ? SKIP TO:         ?   ? SHOW:            ?
    ? Name Prompt      ?   ? Choice Menu      ?
    ????????????????????   ????????????????????
                ?               ?
                ?               ??? [New Game] ? Name Prompt
                ?               ?
                ?               ??? [Continue] ? Load Menu
                ?
                ??????????? Start New Game
```

## Smart Flow Logic

### When Player Clicks "Play":

1. **Check if saves exist:**
   - **NO saves** ? Skip choice menu, go directly to Name Prompt
   - **YES saves** ? Show choice menu (New Game vs Continue)

2. **After choosing:**
   - **New Game** ? Show Name Prompt ? Create save ? Start game
   - **Continue** ? Show Load Menu ? Select save ? Load game

---

## Step-by-Step Implementation

### Step 1: Create Choice Menu Panel (5 minutes)

Create a new panel that appears between main menu and game start:

#### In Unity Hierarchy:

1. **Right-click on `MainMenu`** (or `StartScreen`)
2. **UI ? Panel** ? Name it `PlayChoicePanel`
3. **Add CanvasGroup component** to `PlayChoicePanel`

#### Panel Structure:

```
PlayChoicePanel (with CanvasGroup)
??? Background (Image - dark semi-transparent)
?   ??? Set color: Black with 80% opacity
?
??? Title (TextMeshProUGUI)
?   ??? Text: "Welcome Back!"
?   ??? Font: Same style as your Castle title
?   ??? Size: Large (48-60pt)
?
??? NewGameButton (Button)
?   ??? Image (Background)
?   ??? Text (TextMeshProUGUI): "Start New Game"
?       ??? Font: Match your existing button style
?
??? ContinueButton (Button)
?   ??? Image (Background)
?   ??? Text (TextMeshProUGUI): "Continue"
?       ??? Font: Match your existing button style
?
??? BackButton (Button)
    ??? Image (Background)
    ??? Text (TextMeshProUGUI): "Back"
        ??? Font: Match your existing button style
```

#### Positioning (Vertical Stack):

```
??????????????????????????????????????
?                                    ?
?        Welcome Back!               ?
?                                    ?
?    ??????????????????????         ?
?    ?  Start New Game    ?         ?
?    ??????????????????????         ?
?                                    ?
?    ??????????????????????         ?
?    ?     Continue       ?         ?
?    ??????????????????????         ?
?                                    ?
?    ??????????????????????         ?
?    ?       Back         ?         ?
?    ??????????????????????         ?
?                                    ?
??????????????????????????????????????
```

#### Initial Setup:
- **Set `PlayChoicePanel` active state to FALSE** (hide by default)
- **CanvasGroup.alpha = 0** (will fade in when shown)
- **CanvasGroup.blocksRaycasts = false** (initially)
- **CanvasGroup.interactable = false** (initially)

---

### Step 2: Update MainMenu.cs with Smart Logic

Open `Assets/Systems/UIs/Menu/MainMenu.cs` and add these sections:

#### Add Fields at Top of Class:

```csharp
[Header("Play Choice Menu")]
public GameObject playChoicePanel;
public CanvasGroup playChoiceCanvasGroup;
public Button newGameChoiceButton;
public Button continueChoiceButton;
public Button backChoiceButton;

[Header("Save System References")]
public SaveNamePrompt saveNamePrompt;  // Already exists
public LoadGameUI loadGameUI;          // Already exists
```

#### Modify Start() Method:

Find your existing `Start()` method and add this after button initialization:

```csharp
void Start()
{
    // ... your existing button finding code ...
    
    // NEW: Hide play choice panel initially
    if (playChoicePanel != null)
    {
        playChoicePanel.SetActive(false);
        if (playChoiceCanvasGroup != null)
        {
            playChoiceCanvasGroup.alpha = 0f;
            playChoiceCanvasGroup.blocksRaycasts = false;
            playChoiceCanvasGroup.interactable = false;
        }
    }
    
    // ... rest of your existing Start() code ...
}
```

#### Replace/Update StartGame() Method:

Replace your existing `StartGame()` method with this smart version:

```csharp
/// <summary>
/// Called when Play button is clicked - smart flow based on save existence
/// </summary>
public void StartGame()
{
    Debug.Log("[MainMenu] Play button clicked - checking for saves");
    
    bool hasSaves = CheckIfAnySavesExist();
    
    if (hasSaves)
    {
        // Saves exist - show choice menu
        Debug.Log("[MainMenu] Saves detected - showing choice menu");
        ShowPlayChoiceMenu();
    }
    else
    {
        // No saves - skip to name prompt
        Debug.Log("[MainMenu] No saves detected - skipping to name prompt");
        ShowSaveNamePrompt();
    }
}
```

#### Add New Helper Methods:

Add these methods to your class (before the `QuitGame()` method):

```csharp
/// <summary>
/// Check if any save files exist in the Saves folder
/// </summary>
private bool CheckIfAnySavesExist()
{
    string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    if (!System.IO.Directory.Exists(saveDirectory))
    {
        Debug.Log("[MainMenu] Save directory does not exist");
        return false;
    }
        
    string[] saveFiles = System.IO.Directory.GetFiles(saveDirectory, "GameFlags_*.json");
    Debug.Log($"[MainMenu] Found {saveFiles.Length} save files");
    return saveFiles.Length > 0;
}

/// <summary>
/// Show the play choice menu with fade animation
/// </summary>
private void ShowPlayChoiceMenu()
{
    if (playChoicePanel == null)
    {
        Debug.LogError("[MainMenu] PlayChoicePanel not assigned!");
        return;
    }
    
    playChoicePanel.SetActive(true);
    
    // Hide main menu buttons temporarily
    if (buttonsCanvasGroup != null)
    {
        buttonsCanvasGroup.interactable = false;
    }
    
    StartCoroutine(FadeInPlayChoice());
}

/// <summary>
/// Hide the play choice menu with fade animation
/// </summary>
private void HidePlayChoiceMenu()
{
    if (playChoicePanel == null) return;
    
    StartCoroutine(FadeOutPlayChoice());
    
    // Re-enable main menu buttons
    if (buttonsCanvasGroup != null)
    {
        buttonsCanvasGroup.interactable = true;
    }
}

/// <summary>
/// Fade in the play choice menu
/// </summary>
private IEnumerator FadeInPlayChoice()
{
    if (playChoiceCanvasGroup == null) yield break;
    
    playChoiceCanvasGroup.blocksRaycasts = true;
    playChoiceCanvasGroup.interactable = false;
    
    float duration = 0.3f;
    float timer = 0f;
    
    while (timer < duration)
    {
        timer += Time.deltaTime;
        playChoiceCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
        yield return null;
    }
    
    playChoiceCanvasGroup.alpha = 1f;
    playChoiceCanvasGroup.interactable = true;
}

/// <summary>
/// Fade out the play choice menu
/// </summary>
private IEnumerator FadeOutPlayChoice()
{
    if (playChoiceCanvasGroup == null)
    {
        playChoicePanel.SetActive(false);
        yield break;
    }
    
    playChoiceCanvasGroup.interactable = false;
    
    float duration = 0.3f;
    float timer = 0f;
    
    while (timer < duration)
    {
        timer += Time.deltaTime;
        playChoiceCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
        yield return null;
    }
    
    playChoiceCanvasGroup.alpha = 0f;
    playChoiceCanvasGroup.blocksRaycasts = false;
    playChoicePanel.SetActive(false);
}

/// <summary>
/// Show save name prompt (called when New Game is chosen)
/// </summary>
private void ShowSaveNamePrompt()
{
    if (saveNamePrompt != null)
    {
        Debug.Log("[MainMenu] Showing save name prompt");
        saveNamePrompt.Show(OnSaveNameConfirmed, OnSaveNameCancelled);
    }
    else
    {
        Debug.LogError("[MainMenu] SaveNamePrompt not assigned!");
    }
}

/// <summary>
/// NEW GAME button clicked from choice menu
/// </summary>
public void OnNewGameFromChoice()
{
    Debug.Log("[MainMenu] New Game selected from choice menu");
    HidePlayChoiceMenu();
    ShowSaveNamePrompt();
}

/// <summary>
/// CONTINUE button clicked from choice menu
/// </summary>
public void OnContinueFromChoice()
{
    Debug.Log("[MainMenu] Continue selected from choice menu");
    HidePlayChoiceMenu();
    ShowLoadGameMenu();
}

/// <summary>
/// BACK button clicked from choice menu
/// </summary>
public void OnBackFromChoice()
{
    Debug.Log("[MainMenu] Back clicked from choice menu");
    HidePlayChoiceMenu();
}

/// <summary>
/// Show the load game menu
/// </summary>
private void ShowLoadGameMenu()
{
    if (loadGameUI != null)
    {
        Debug.Log("[MainMenu] Opening load game UI");
        
        // Subscribe to load event to transition to game
        SaveGameEvents.OnSaveLoaded += OnGameLoaded;
        
        loadGameUI.Show(OnLoadGameBack);
    }
    else
    {
        Debug.LogError("[MainMenu] LoadGameUI not assigned!");
    }
}

/// <summary>
/// Called when a save is loaded from the load game UI
/// </summary>
private void OnGameLoaded(string saveName)
{
    Debug.Log($"[MainMenu] Game loaded: {saveName}");
    
    // Unsubscribe
    SaveGameEvents.OnSaveLoaded -= OnGameLoaded;
    
    // Load the game scene
    StartCoroutine(FadeAndLoad());
}

/// <summary>
/// Called when back button is clicked in load game UI
/// </summary>
private void OnLoadGameBack()
{
    Debug.Log("[MainMenu] Back from load game UI");
    // Unsubscribe in case we didn't load
    SaveGameEvents.OnSaveLoaded -= OnGameLoaded;
}
```

---

### Step 3: Wire Up Buttons in Unity Inspector

#### A. Play Button (Already Exists)

Your existing `PlayButton` should already be wired to `Startscreen.StartGame`:

1. **Select `PlayButton`** in hierarchy
2. **Inspector ? Button component ? OnClick()**
3. **Verify it points to:** `MainMenu GameObject ? Startscreen.StartGame`

If not set up:
- Drag `MainMenu` GameObject to the object field
- Select function: `Startscreen.StartGame`

#### B. New Game Choice Button

1. **Select `NewGameButton`** in `PlayChoicePanel`
2. **Inspector ? Button component ? OnClick()**
3. **Drag `MainMenu`** GameObject to object field
4. **Select function:** `Startscreen.OnNewGameFromChoice`

#### C. Continue Choice Button

1. **Select `ContinueButton`** in `PlayChoicePanel`
2. **Inspector ? Button component ? OnClick()**
3. **Drag `MainMenu`** GameObject to object field
4. **Select function:** `Startscreen.OnContinueFromChoice`

#### D. Back Choice Button

1. **Select `BackButton`** in `PlayChoicePanel`
2. **Inspector ? Button component ? OnClick()**
3. **Drag `MainMenu`** GameObject to object field
4. **Select function:** `Startscreen.OnBackFromChoice`

---

### Step 4: Assign References in MainMenu Inspector

Select the `MainMenu` GameObject (or wherever `Startscreen` component is):

#### In Startscreen Component:

**Play Choice Menu section:**
- Drag `PlayChoicePanel` ? `playChoicePanel` field
- Drag `PlayChoicePanel` ? `playChoiceCanvasGroup` field (it will get the component)
- Drag `NewGameButton` ? `newGameChoiceButton` field
- Drag `ContinueButton` ? `continueChoiceButton` field
- Drag `BackButton` ? `backChoiceButton` field

**Save System References section:**
- Drag `SaveNamePromptPanel` ? `saveNamePrompt` field
- Drag `LoadGameUIPanel` ? `loadGameUI` field

---

### Step 5: Style Your Choice Menu (Match Your Game's Aesthetic)

Based on your screenshots, your game has a medieval/gothic fantasy style. Here's how to style the choice menu:

#### Text Style:
```
Font: Same decorative font as "Castle" title
Title ("Welcome Back!"):
  - Size: 48-60pt
  - Color: White or Cream (#FFF8E1)
  - Outline: Dark brown/black

Button Text:
  - Size: 24-32pt
  - Font: Match your existing buttons
  - Color: Dark brown or black
```

#### Button Style:
```
Background:
  - Use same button background as your existing buttons
  - Or create parchment/medieval style
  
Colors:
  - Normal: Cream/tan (#D7CCC8)
  - Highlighted: Lighter cream (#FFF8E1)
  - Pressed: Darker brown (#8D6E63)
```

#### Background Panel:
```
Color: Black
Alpha: 0.8 (80% opacity)
```

---

## Complete Flow Diagram

```
???????????????????????????????????????????????????????????????????
?                         ?? MAIN MENU                            ?
?                                                                 ?
?                      [? Play]                                   ?
?                      [? Settings]                               ?
?                      [?? Credits]                                ?
?                      [? Quit]                                   ?
???????????????????????????????????????????????????????????????????
                          ? Click Play
                          ?
          ?????????????????????????????????
          ? CheckIfAnySavesExist()        ?
          ?????????????????????????????????
                      ?
        ?????????????????????????????
        ? NO SAVES          YES SAVES?
        ?                           ?
?????????????????         ????????????????????
? Skip to:      ?         ? Show:            ?
? Name Prompt   ?         ? Choice Menu      ?
?               ?         ?                  ?
? ??????????????         ????????????????????
? ?Enter Name ??         ?? Start New Game ??
? ??????????????         ????????????????????
?      ?        ?         ?        ?         ?
?      ?        ?         ?        ?         ?
? Create Save   ?         ?  Name Prompt     ?
?               ?         ?        ?         ?
?               ?         ?        ?         ?
?               ?         ?  Create Save     ?
?               ?         ?                  ?
?               ?         ????????????????????
?               ?         ??   Continue     ??
?               ?         ????????????????????
?               ?         ?        ?         ?
?               ?         ?        ?         ?
?               ?         ?  Load Menu       ?
?               ?         ?        ?         ?
?               ?         ?        ?         ?
?               ?         ?  Select Save     ?
?               ?         ?                  ?
?               ?         ????????????????????
?               ?         ??     Back       ??
?               ?         ????????????????????
?               ?         ?        ?         ?
?               ?         ?        ?         ?
?               ?         ?  Return to Menu  ?
?????????????????         ????????????????????
        ?                          ?
        ????????????????????????????
                   ?
         ????????????????????
         ?  START GAME       ?
         ????????????????????
```

---

## Testing Checklist

### First Time Playing (No Saves):

1. ? **Launch game**
2. ? **Click "Play"**
3. ? **Name prompt appears immediately** (skips choice menu)
4. ? **Enter name "TestPlayer"**
5. ? **Click Confirm**
6. ? **Game starts**
7. ? **Check save folder:** `GameFlags_TestPlayer.json` exists

### With Existing Saves:

1. ? **Launch game**
2. ? **Click "Play"**
3. ? **Choice menu appears** (New Game vs Continue)
4. ? **Click "New Game"**
5. ? **Name prompt appears**
6. ? **Enter different name**
7. ? **Game starts with new save**

### Testing Continue Flow:

1. ? **Launch game**
2. ? **Click "Play"**
3. ? **Choice menu appears**
4. ? **Click "Continue"**
5. ? **Load menu appears with all saves**
6. ? **Each save shows:** Name, Day, Clock Time
7. ? **Click Load on a save**
8. ? **Game loads with that save's data**

### Testing Back Button:

1. ? **Launch game**
2. ? **Click "Play"**
3. ? **Choice menu appears**
4. ? **Click "Back"**
5. ? **Returns to main menu**
6. ? **All main menu buttons work**

---

## Debug Logs to Watch

When testing, watch the Console for these logs:

### First Play (No Saves):
```
[MainMenu] Play button clicked - checking for saves
[MainMenu] Save directory does not exist
[MainMenu] Found 0 save files
[MainMenu] No saves detected - skipping to name prompt
[MainMenu] Showing save name prompt
[MainMenu] Save name confirmed: TestPlayer
[GameFlagsManager] New save created: TestPlayer
```

### With Saves:
```
[MainMenu] Play button clicked - checking for saves
[MainMenu] Found 2 save files
[MainMenu] Saves detected - showing choice menu
[MainMenu] New Game selected from choice menu
[MainMenu] Showing save name prompt
```

OR

```
[MainMenu] Continue selected from choice menu
[MainMenu] Opening load game UI
[LoadGameUI] Refreshing save list
[LoadGameUI] Found save: TestPlayer
```

---

## Troubleshooting

### Issue: Choice Menu Doesn't Appear

**Symptom:** Clicking Play does nothing or skips directly to name prompt even with saves

**Solutions:**
1. Check `playChoicePanel` is assigned in Inspector
2. Verify `PlayChoicePanel` exists in hierarchy
3. Check Console for: `[MainMenu] PlayChoicePanel not assigned!`
4. Make sure save files exist in correct location:
   - Windows: `C:/Users/[User]/AppData/LocalLow/[Company]/[Game]/Saves/`

### Issue: Choice Menu Appears But Is Black/Invisible

**Symptom:** Screen dims but no menu visible

**Solutions:**
1. Check `CanvasGroup.alpha` is 0 initially (will fade in)
2. Verify all child objects (Title, Buttons) are active
3. Check z-position of panel (should be in front)
4. Verify Canvas sorting order

### Issue: Buttons Don't Respond

**Symptom:** Can't click buttons in choice menu

**Solutions:**
1. Check `EventSystem` exists in scene
2. Verify `CanvasGroup.interactable = true` after fade in
3. Check `CanvasGroup.blocksRaycasts = true`
4. Verify button OnClick() events are wired correctly

### Issue: Always Shows Choice Menu Even With No Saves

**Symptom:** Choice menu appears on first play

**Solutions:**
1. Check `CheckIfAnySavesExist()` implementation
2. Verify save folder path is correct
3. Delete any test saves from previous testing
4. Check Console logs for save count

### Issue: Name Prompt Doesn't Appear

**Symptom:** Choice menu closes but nothing happens

**Solutions:**
1. Check `saveNamePrompt` field is assigned in Inspector
2. Verify `SaveNamePromptPanel` exists in scene
3. Check `SaveNamePrompt` component is attached to panel
4. Look for Console error: `[MainMenu] SaveNamePrompt not assigned!`

---

## Summary

### What You Have Now:

? Single "Play" button on main menu  
? Smart flow based on save existence  
? Choice menu for players with saves  
? Direct to name prompt for new players  
? Fade animations for smooth transitions  

### Files Modified:

- ? `Assets/Systems/UIs/Menu/MainMenu.cs` - Added smart flow logic

### Files to Reference:

- `SaveNamePrompt.cs` - Already handles name input
- `LoadGameUI.cs` - Already handles save loading
- `GameFlags.cs` - Already handles save file operations

### Time to Complete:

- Creating UI: **5 minutes**
- Adding code: **10 minutes**
- Wiring buttons: **5 minutes**
- Testing: **5 minutes**
- **Total: ~25 minutes**

---

## Next Steps

1. ? **Create `PlayChoicePanel` in Unity**
2. ? **Add the code to `MainMenu.cs`**
3. ? **Wire up all button events**
4. ? **Assign references in Inspector**
5. ? **Test both flows** (with and without saves)
6. ? **Style to match your game's aesthetic**

Your implementation is ready to go! ????
