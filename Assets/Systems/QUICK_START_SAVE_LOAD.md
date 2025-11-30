# QUICK START: Setting Up Save/Load Menu (5 Minutes)

## What You Need to Do

The code is done ? - you just need to set up the UI in Unity!

---

## Step 1: Choose Your Main Menu Style (30 seconds)

Pick ONE:

### Option A: Two Separate Buttons (Recommended ?)
```
Main Menu
??? [New Game]    ? Creates new save
??? [Continue]    ? Loads existing save
??? [Settings]
??? [Credits]
??? [Quit]
```

### Option B: Single Play Button with Submenu
```
Main Menu
??? [Play] ? Opens submenu:
?            ??? [New Game]
?            ??? [Load Game]
??? [Settings]
??? [Credits]
??? [Quit]
```

**?? Use Option A - it's simpler and cleaner**

---

## Step 2: Update MainMenu Scene UI (2 minutes)

### If Using Option A (Recommended):

1. **Rename existing button** "PlayButton" ? "NewGameButton"
2. **Duplicate it** to create "ContinueButton"
3. **Position buttons** vertically in menu

### If Using Option B:

1. Keep "PlayButton" as is
2. Create a new panel "PlayChoicePanel" (hidden by default)
3. Add two buttons inside: "NewGameButton" and "LoadGameButton"

---

## Step 3: Create SaveNamePrompt Panel (1 minute)

Right-click in Hierarchy ? UI ? Panel ? Name it "SaveNamePromptPanel"

Add these inside:
- **TMP_InputField** (name it "NameInputField")
- **Button** "ConfirmButton" with text "Start Game"
- **Button** "CancelButton" with text "Cancel"
- **TextMeshProUGUI** "ErrorText" (red color, initially hidden)

Add component: **CanvasGroup** to the panel

---

## Step 4: Create LoadGameUI Panel (1 minute)

Right-click in Hierarchy ? UI ? Panel ? Name it "LoadGameUIPanel"

Add these inside:
- **ScrollView** with Vertical Layout Group
- **TextMeshProUGUI** "NoSavesText" (shows when no saves)
- **Button** "BackButton"
- **Panel** "DeleteConfirmPanel" (hidden by default)
  - Add inside: "Yes" and "No" buttons

Add component: **CanvasGroup** to the panel

---

## Step 5: Create SaveSlot Prefab (30 seconds)

Right-click in Hierarchy ? UI ? Panel ? Name it "SaveSlotPrefab"

Add these TextMeshProUGUI inside (EXACT NAMES):
- **SaveNameText** (bold, large, white)
- **DayInfoText** (regular, smaller, gray)
- **ClockTimeText** (monospace, gold color)

Add buttons:
- **LoadButton**
- **DeleteButton**

Drag to Project folder to make it a prefab, then delete from scene.

---

## Step 6: Wire Everything Up in Inspector (30 seconds)

### Select MainMenu GameObject:

In **Startscreen** component:
- Drag **SaveNamePromptPanel** to `saveNamePrompt` field
- Drag **LoadGameUIPanel** to `loadGameUI` field

### Select LoadGameUIPanel:

In **LoadGameUI** component:
- Drag **SaveSlotPrefab** to `saveSlotPrefab` field
- Drag **ScrollView/Content** to `saveSlotContainer` field
- Drag **BackButton** to `backButton` field
- Drag **NoSavesText** to `noSavesText` field
- Drag **DeleteConfirmPanel** to `deleteConfirmPanel` field

### Select SaveNamePromptPanel:

In **SaveNamePrompt** component (add if missing):
- Drag all the child components to their fields

---

## Step 7: Connect Button Events (30 seconds)

### NewGameButton:
- Inspector ? Button ? OnClick()
- Drag **MainMenu** GameObject
- Select `Startscreen.OnNewGameClicked`

### ContinueButton:
- Inspector ? Button ? OnClick()
- Drag **MainMenu** GameObject
- Select `Startscreen.OnContinueClicked`

**That's it!** The rest auto-wires internally.

---

## Step 8: Update MainMenu.cs Code (Copy-Paste)

Open `Assets/Systems/UIs/Menu/MainMenu.cs` and replace the `Start()` method with:

```csharp
void Start()
{
    // Find buttons
    newGameButton = GameObject.Find("NewGameButton");
    continueButton = GameObject.Find("ContinueButton");
    settingsButton = GameObject.Find("SettingsButton");
    creditsButton = GameObject.Find("CreditsButton");
    quitButton = GameObject.Find("QuitButton");
    
    _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
    _fadeCanvasGroup.alpha = 1f;
    
    _logoCanvasGroup = GameObject.Find("LogoCanvasGroup").GetComponent<CanvasGroup>();
    _logoCanvasGroup.alpha = 0f;

    // Setup canvas groups
    if (mainMenuLogo && !mainMenuLogoCanvasGroup)
        mainMenuLogoCanvasGroup = mainMenuLogo.GetComponent<CanvasGroup>() ?? mainMenuLogo.AddComponent<CanvasGroup>();
    
    if (buttonsParent && !buttonsCanvasGroup)
        buttonsCanvasGroup = buttonsParent.GetComponent<CanvasGroup>() ?? buttonsParent.AddComponent<CanvasGroup>();

    if (mainMenuLogoCanvasGroup) mainMenuLogoCanvasGroup.alpha = 1f;
    if (buttonsCanvasGroup) buttonsCanvasGroup.alpha = 1f;

    // Credits setup
    if (creditLogo) _creditLogoStartPos = creditLogo.anchoredPosition;
    if (creditText) _creditTextStartPos = creditText.anchoredPosition;
    if (creditsCanvasGroup)
    {
        creditsCanvasGroup.alpha = 0f;
        creditsCanvasGroup.gameObject.SetActive(false);
    }

    // Update continue button state
    UpdateContinueButton();

    UIButtonFX.globalAudioEnabled = false;
    UIButtonFX.suppressClickInMainMenu = true;
    
    StartCoroutine(LogoStartup());
    InitializeMainMenuMusic();
}
```

Add these new methods at the end of the class:

```csharp
/// <summary>
/// Enable/disable Continue button based on save existence
/// </summary>
private void UpdateContinueButton()
{
    if (continueButton == null) return;
    
    bool hasSaves = CheckIfAnySavesExist();
    
    Button btn = continueButton.GetComponent<Button>();
    if (btn != null)
        btn.interactable = hasSaves;
    
    CanvasGroup cg = continueButton.GetComponent<CanvasGroup>();
    if (cg == null) cg = continueButton.AddComponent<CanvasGroup>();
    cg.alpha = hasSaves ? 1f : 0.5f;
}

/// <summary>
/// Check if any save files exist
/// </summary>
private bool CheckIfAnySavesExist()
{
    string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    if (!System.IO.Directory.Exists(saveDirectory))
        return false;
        
    string[] saveFiles = System.IO.Directory.GetFiles(saveDirectory, "GameFlags_*.json");
    return saveFiles.Length > 0;
}

/// <summary>
/// New Game button clicked
/// </summary>
public void OnNewGameClicked()
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
/// Continue button clicked
/// </summary>
public void OnContinueClicked()
{
    if (loadGameUI != null)
    {
        Debug.Log("[MainMenu] Opening load game UI");
        
        // Subscribe to load event
        SaveGameEvents.OnSaveLoaded += OnGameLoaded;
        
        // Show load game UI
        loadGameUI.Show(OnLoadGameBack);
    }
    else
    {
        Debug.LogError("[MainMenu] LoadGameUI not assigned!");
    }
}
```

Rename variables at the top of the class:
```csharp
// OLD:
public GameObject playButton;
public GameObject loadGameButton;

// NEW:
public GameObject newGameButton;
public GameObject continueButton;
```

---

## Test It! (1 minute)

1. **Press Play** in Unity
2. **Click New Game**
3. **Enter a name** "Test"
4. **Click Start Game**
5. **Save created!** ?

6. **Return to menu**
7. **Continue button enabled!** ?
8. **Click Continue**
9. **Your save appears!** ?

---

## What Happens Automatically

? Game auto-saves when day progresses  
? Clock time is saved  
? Load menu shows: name, day, clock time  
? Continue button disabled when no saves  
? Delete button has confirmation dialog  

---

## Troubleshooting

**Continue button not working?**
- Make sure you renamed the variable from `loadGameButton` to `continueButton`

**Save name prompt doesn't appear?**
- Check if SaveNamePrompt component exists on the panel
- Verify `saveNamePrompt` field is assigned in Inspector

**Load game UI empty?**
- Check if `saveSlotPrefab` is assigned
- Verify save folder exists (create a save first!)

**Buttons don't respond?**
- Check EventSystem exists in scene
- Verify OnClick() events are wired correctly

---

## That's It!

Your save/load system is now complete! The game will:
- ? Auto-save when days progress
- ? Show beautiful save slots with day and time
- ? Handle multiple saves per player
- ? Restore clock time when loading

**Want more details?** Check:
- `MAIN_MENU_SAVE_FLOW_SETUP.md` - Full setup guide
- `SAVE_FLOW_DIAGRAMS.md` - Visual flowcharts
- `SAVE_SLOT_UI_REFERENCE.md` - UI styling guide
