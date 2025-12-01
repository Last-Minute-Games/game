# Main Menu Save Flow Setup Guide

## Overview

This guide explains how to set up the main menu so that when the player clicks "Play", they see options to either:
1. **Create a new save** (enter a character name)
2. **Load an existing save** (from the load game menu)

## Flow Diagram

```
Main Menu
    ?
    ??? [Play Button]
    ?       ?
    ?       ??? Check if saves exist?
    ?       ?
    ?       ??? YES ? Show Choice Screen
    ?       ?           ??? [New Game] ? Name Prompt ? Start Game
    ?       ?           ??? [Load Game] ? Load Menu ? Select Save ? Start Game
    ?       ?
    ?       ??? NO ? Name Prompt ? Start Game
    ?
    ??? [Settings Button] ? Settings Menu
    ??? [Credits Button] ? Credits
    ??? [Quit Button] ? Exit Game
```

## Method 1: Simple Two-Button Approach (Recommended)

This is the cleanest approach - separate "New Game" and "Continue" buttons.

### Setup Steps

#### 1. Update Main Menu UI

Replace the single "Play" button with two buttons:

```
Main Menu Canvas
??? Title Logo
??? [New Game Button]      ? Creates new save
??? [Continue Button]      ? Loads existing save (disabled if no saves)
??? [Settings Button]
??? [Credits Button]
??? [Quit Button]
```

#### 2. Update MainMenu.cs (Startscreen)

The `MainMenu.cs` file already has the infrastructure, but needs updates:

```csharp
[Header("Main Menu Elements")]
public GameObject newGameButton;      // Renamed from playButton
public GameObject continueButton;     // Renamed from loadGameButton
public GameObject settingsButton;
public GameObject creditsButton;
public GameObject quitButton;

void Start()
{
    // Find buttons
    newGameButton = GameObject.Find("NewGameButton");
    continueButton = GameObject.Find("ContinueButton");
    settingsButton = GameObject.Find("SettingsButton");
    creditsButton = GameObject.Find("CreditsButton");
    quitButton = GameObject.Find("QuitButton");
    
    // ... fade canvas setup ...
    
    // Check if any saves exist
    UpdateContinueButton();
    
    // Start logo sequence
    StartCoroutine(LogoStartup());
}

/// <summary>
/// Enable/disable Continue button based on save existence
/// </summary>
private void UpdateContinueButton()
{
    if (continueButton != null)
    {
        // Check if any save files exist
        bool hasSaves = CheckIfAnySavesExist();
        
        Button continueBtn = continueButton.GetComponent<Button>();
        if (continueBtn != null)
        {
            continueBtn.interactable = hasSaves;
        }
        
        // Optionally gray out or hide the button
        CanvasGroup cg = continueButton.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = hasSaves ? 1f : 0.5f;
        }
    }
}

/// <summary>
/// Check if any save files exist
/// </summary>
private bool CheckIfAnySavesExist()
{
    string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    if (!Directory.Exists(saveDirectory))
        return false;
        
    string[] saveFiles = Directory.GetFiles(saveDirectory, "GameFlags_*.json");
    return saveFiles.Length > 0;
}

/// <summary>
/// New Game - Show name prompt
/// </summary>
public void OnNewGameClicked()
{
    if (saveNamePrompt != null)
    {
        saveNamePrompt.Show(OnSaveNameConfirmed, OnSaveNameCancelled);
    }
    else
    {
        Debug.LogError("[MainMenu] SaveNamePrompt not assigned!");
    }
}

/// <summary>
/// Continue - Show load game menu
/// </summary>
public void OnContinueClicked()
{
    if (loadGameUI != null)
    {
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

// Existing methods remain the same...
```

#### 3. Wire Up Buttons in Unity

1. **Select NewGameButton** in hierarchy
2. In Inspector ? Button component ? OnClick()
3. Drag your `MainMenu` GameObject to the object field
4. Select `Startscreen.OnNewGameClicked`

5. **Select ContinueButton** in hierarchy
6. In Inspector ? Button component ? OnClick()
7. Drag your `MainMenu` GameObject to the object field
8. Select `Startscreen.OnContinueClicked`

---

## Method 2: Single Play Button with Choice Menu

If you want to keep a single "Play" button that opens a submenu.

### Setup Steps

#### 1. Create Choice Menu UI

Create a new UI panel that appears when Play is clicked:

```
PlayChoicePanel (CanvasGroup)
??? Background (Dark semi-transparent)
??? Title Text ("Select Option")
??? [New Game Button]
?   ??? Text: "Start New Game"
??? [Load Game Button]
?   ??? Text: "Load Saved Game"
??? [Back Button]
    ??? Text: "Back"
```

#### 2. Update MainMenu.cs

```csharp
[Header("Play Choice Menu")]
public GameObject playChoicePanel;
public CanvasGroup playChoiceCanvasGroup;
public Button newGameChoiceButton;
public Button loadGameChoiceButton;
public Button backChoiceButton;

void Start()
{
    // ... existing setup ...
    
    // Hide play choice panel initially
    if (playChoicePanel != null)
    {
        playChoicePanel.SetActive(false);
    }
}

/// <summary>
/// Play button clicked - show choice menu or go straight to name prompt
/// </summary>
public void OnPlayClicked()
{
    bool hasSaves = CheckIfAnySavesExist();
    
    if (hasSaves)
    {
        // Show choice menu (New Game vs Load Game)
        ShowPlayChoiceMenu();
    }
    else
    {
        // No saves exist, go straight to new game
        OnNewGameClicked();
    }
}

/// <summary>
/// Show the play choice menu
/// </summary>
private void ShowPlayChoiceMenu()
{
    if (playChoicePanel != null)
    {
        playChoicePanel.SetActive(true);
        StartCoroutine(FadeInPlayChoice());
    }
}

/// <summary>
/// Hide the play choice menu
/// </summary>
private void HidePlayChoiceMenu()
{
    if (playChoicePanel != null)
    {
        StartCoroutine(FadeOutPlayChoice());
    }
}

private IEnumerator FadeInPlayChoice()
{
    if (playChoiceCanvasGroup == null) yield break;
    
    float duration = 0.3f;
    float timer = 0f;
    
    while (timer < duration)
    {
        timer += Time.deltaTime;
        playChoiceCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
        yield return null;
    }
    
    playChoiceCanvasGroup.alpha = 1f;
}

private IEnumerator FadeOutPlayChoice()
{
    if (playChoiceCanvasGroup == null) yield break;
    
    float duration = 0.3f;
    float timer = 0f;
    
    while (timer < duration)
    {
        timer += Time.deltaTime;
        playChoiceCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
        yield return null;
    }
    
    playChoiceCanvasGroup.alpha = 0f;
    playChoicePanel.SetActive(false);
}

/// <summary>
/// New Game clicked from choice menu
/// </summary>
public void OnNewGameFromChoice()
{
    HidePlayChoiceMenu();
    OnNewGameClicked();
}

/// <summary>
/// Load Game clicked from choice menu
/// </summary>
public void OnLoadGameFromChoice()
{
    HidePlayChoiceMenu();
    OnContinueClicked();
}

/// <summary>
/// Back clicked from choice menu
/// </summary>
public void OnBackFromChoice()
{
    HidePlayChoiceMenu();
}
```

#### 3. Wire Up Choice Menu Buttons

1. **Select NewGameChoiceButton**
   - OnClick() ? `Startscreen.OnNewGameFromChoice`

2. **Select LoadGameChoiceButton**
   - OnClick() ? `Startscreen.OnLoadGameFromChoice`

3. **Select BackChoiceButton**
   - OnClick() ? `Startscreen.OnBackFromChoice`

---

## Setting Up SaveNamePrompt

### 1. Create SaveNamePrompt UI

```
SaveNamePromptPanel (CanvasGroup)
??? Background (Dark overlay)
??? Dialog Box (Panel)
?   ??? Title Text ("Enter Character Name")
?   ??? Input Field (TMP_InputField)
?   ?   ??? Placeholder: "Enter name..."
?   ??? [Confirm Button] ("Start Game")
?   ??? [Cancel Button] ("Back")
```

### 2. Verify SaveNamePrompt.cs

The file should already exist at `Assets/Systems/UIs/Menu/SaveNamePrompt.cs`. Here's what it should contain:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SaveNamePrompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI errorText;
    
    [Header("Settings")]
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 16;
    [SerializeField] private float fadeDuration = 0.3f;
    
    private System.Action<string> _onConfirm;
    private System.Action _onCancel;
    
    private void Awake()
    {
        // Wire up buttons
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide initially
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (errorText != null)
            errorText.gameObject.SetActive(false);
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show the name prompt
    /// </summary>
    public void Show(System.Action<string> onConfirm, System.Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        
        gameObject.SetActive(true);
        
        // Clear input
        if (nameInputField != null)
            nameInputField.text = "";
        
        // Hide error
        if (errorText != null)
            errorText.gameObject.SetActive(false);
        
        // Fade in
        StartCoroutine(FadeIn());
        
        // Focus input field
        if (nameInputField != null)
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }
    
    /// <summary>
    /// Hide the name prompt
    /// </summary>
    public void Hide()
    {
        StartCoroutine(FadeOut());
    }
    
    private void OnConfirmClicked()
    {
        if (nameInputField == null) return;
        
        string saveName = nameInputField.text.Trim();
        
        // Validate name
        if (string.IsNullOrEmpty(saveName))
        {
            ShowError("Please enter a name");
            return;
        }
        
        if (saveName.Length < minNameLength)
        {
            ShowError($"Name must be at least {minNameLength} characters");
            return;
        }
        
        if (saveName.Length > maxNameLength)
        {
            ShowError($"Name must be {maxNameLength} characters or less");
            return;
        }
        
        // Check if save already exists
        if (GameFlags.HasSaveFile(saveName))
        {
            ShowError("A save with this name already exists");
            return;
        }
        
        // Valid name - proceed
        _onConfirm?.Invoke(saveName);
        Hide();
    }
    
    private void OnCancelClicked()
    {
        _onCancel?.Invoke();
        Hide();
    }
    
    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }
        
        canvasGroup.interactable = false;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
```

### 3. Assign SaveNamePrompt in Inspector

1. Select your `MainMenu` GameObject
2. Find the `Startscreen` component
3. Drag your `SaveNamePromptPanel` to the `saveNamePrompt` field

---

## Setting Up LoadGameUI

### 1. Create LoadGameUI Panel

```
LoadGameUIPanel (CanvasGroup)
??? Background (Dark overlay)
??? Content Panel
?   ??? Title Text ("Load Game")
?   ??? Scroll View
?   ?   ??? Content (Vertical Layout Group)
?   ?       ??? [Save Slots spawn here]
?   ??? No Saves Text ("No saved games found")
?   ??? [Back Button]
??? Delete Confirmation Panel
    ??? Title ("Delete Save?")
    ??? Message ("Delete save 'Name'?")
    ??? [Yes Button]
    ??? [No Button]
```

### 2. Create Save Slot Prefab

Create a prefab named `SaveSlotPrefab`:

```
SaveSlotPrefab
??? Background (Image)
??? NumberText (TextMeshProUGUI)
?   ??? Text: "1."
??? SaveNameText (TextMeshProUGUI)
?   ??? Font: Bold, Large
??? DayInfoText (TextMeshProUGUI)
?   ??? Font: Regular
??? ClockTimeText (TextMeshProUGUI)
?   ??? Font: Monospace
??? LoadButton (Button)
?   ??? Text: "Load"
??? DeleteButton (Button)
    ??? Text: "Delete"
```

**Important TextMeshProUGUI Names:**
- `SaveNameText` - Character name
- `DayInfoText` - Day info (e.g., "Day 2")
- `ClockTimeText` - Clock time (e.g., "?50:00")

### 3. Assign LoadGameUI in Inspector

1. Select your `MainMenu` GameObject
2. Find the `Startscreen` component
3. Drag `LoadGameUIPanel` to the `loadGameUI` field
4. Select `LoadGameUIPanel`
5. In the `LoadGameUI` component:
   - Assign `saveSlotPrefab` ? Your SaveSlotPrefab
   - Assign `saveSlotContainer` ? The Content object in ScrollView
   - Assign `backButton` ? The back button
   - Assign `noSavesText` ? The "no saves" text
   - Assign `deleteConfirmPanel` ? The confirmation dialog

---

## Complete Button Wiring Summary

### Method 1 (Two Buttons)

**Main Menu:**
- `NewGameButton.OnClick()` ? `Startscreen.OnNewGameClicked`
- `ContinueButton.OnClick()` ? `Startscreen.OnContinueClicked`

**SaveNamePrompt:**
- `ConfirmButton.OnClick()` ? Internal handler
- `CancelButton.OnClick()` ? Internal handler

**LoadGameUI:**
- `BackButton.OnClick()` ? Internal handler
- Individual save slots wire up automatically via `SaveSlotUI.Initialize()`

### Method 2 (Single Play Button)

**Main Menu:**
- `PlayButton.OnClick()` ? `Startscreen.OnPlayClicked`

**Play Choice Menu:**
- `NewGameChoiceButton.OnClick()` ? `Startscreen.OnNewGameFromChoice`
- `LoadGameChoiceButton.OnClick()` ? `Startscreen.OnLoadGameFromChoice`
- `BackChoiceButton.OnClick()` ? `Startscreen.OnBackFromChoice`

**SaveNamePrompt & LoadGameUI:** Same as Method 1

---

## Day Display Format

Update `LoadGameUI.cs` to remove "of Spring, Year 1":

```csharp
/// <summary>
/// Format day info for display (e.g., "Day 2")
/// </summary>
private string FormatDayInfo(string dayFlag)
{
    switch (dayFlag)
    {
        case "day.one": return "Day 1";
        case "day.two": return "Day 2";
        case "day.three": return "Day 3";
        case "day.four": return "Day 4";
        case "day.five": return "Day 5";
        default: return "Day 1";
    }
}
```

---

## Testing Workflow

### Test New Game Flow

1. **Start game** ? Main Menu appears
2. **Click New Game** (or Play if no saves)
3. **Name prompt appears**
4. **Enter name** "TestPlayer"
5. **Click Confirm**
6. **Game starts** with new save created
7. **Check save folder**: `GameFlags_TestPlayer.json` exists

### Test Continue Flow

1. **Create a save** first (follow above)
2. **Return to Main Menu**
3. **Continue button is enabled**
4. **Click Continue**
5. **Load Game UI appears** with your save
6. **Save shows**:
   - Name: "TestPlayer"
   - Day: "Day 1"
   - Clock: "?60:00"
7. **Click Load**
8. **Game resumes** with loaded data

### Test Multiple Saves

1. Create save "Player1"
2. Return to menu
3. Create save "Player2"
4. Return to menu
5. Click Continue
6. Both saves appear in list
7. Click Load on "Player1"
8. Verify correct save loads

---

## Quick Checklist

### Inspector Setup
- [ ] MainMenu GameObject has `Startscreen` component
- [ ] `saveNamePrompt` field assigned to SaveNamePromptPanel
- [ ] `loadGameUI` field assigned to LoadGameUIPanel
- [ ] LoadGameUI has `saveSlotPrefab` assigned
- [ ] LoadGameUI has `saveSlotContainer` assigned
- [ ] LoadGameUI has delete confirmation panel assigned

### Button Setup
- [ ] New Game button wired to `OnNewGameClicked`
- [ ] Continue button wired to `OnContinueClicked`
- [ ] SaveNamePrompt confirm button wired internally
- [ ] SaveNamePrompt cancel button wired internally
- [ ] LoadGameUI back button wired internally

### Prefab Setup
- [ ] SaveSlotPrefab has all TextMeshProUGUI components
- [ ] Components named exactly: `SaveNameText`, `DayInfoText`, `ClockTimeText`
- [ ] SaveSlotPrefab has LoadButton and DeleteButton
- [ ] Layout looks good (spacing, fonts, colors)

### Testing
- [ ] New game creates save file
- [ ] Continue button disabled when no saves
- [ ] Continue button enabled when saves exist
- [ ] Load game UI shows all saves
- [ ] Each save displays correct info
- [ ] Loading a save works
- [ ] Deleting a save works

---

## Troubleshooting

### Continue Button Always Disabled
**Problem:** Button stays grayed out even with saves
**Solution:** Call `UpdateContinueButton()` in `Start()` after logo sequence

### Save Name Prompt Doesn't Appear
**Problem:** Nothing happens when clicking New Game
**Solution:** Check if `saveNamePrompt` field is assigned in Inspector

### Load Game UI Empty
**Problem:** No saves show even though files exist
**Solution:** 
1. Check save folder path
2. Verify `saveSlotPrefab` is assigned
3. Check console for errors

### Save Slot Info Wrong
**Problem:** Day or time shows incorrectly
**Solution:** Verify `FormatDayInfo()` method updated (removed "Spring, Year 1")

### Buttons Don't Respond
**Problem:** Clicking buttons does nothing
**Solution:**
1. Check EventSystem exists in scene
2. Verify button OnClick() events wired correctly
3. Check for blocking UI elements

---

## File Locations Reference

```
Assets/
??? Systems/
?   ??? GameFlags.cs ? (Already updated)
?   ??? GameFlagsManager.cs ? (Already exists)
?   ??? UIs/
?       ??? Clock/
?       ?   ??? ClockTimer.cs ? (Already updated)
?       ??? Menu/
?           ??? MainMenu.cs (Startscreen) - UPDATE THIS
?           ??? SaveNamePrompt.cs ? (Should exist)
?           ??? LoadGameUI.cs ? (Already updated)
??? Scenes/
    ??? MainMenu.unity - UPDATE UI HERE
```

---

## Next Steps

1. **Choose Method 1 or Method 2** for your main menu flow
2. **Update MainMenu.cs** with the chosen implementation
3. **Create/update UI panels** in Unity scene
4. **Wire up buttons** in Inspector
5. **Test the flow** following the testing workflow
6. **Polish UI** (fonts, colors, animations)

Need help with any specific step? Let me know!
