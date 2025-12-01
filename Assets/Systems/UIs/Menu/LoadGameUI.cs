using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

/// <summary>
/// UI component for displaying and loading saved games.
/// </summary>
public class LoadGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup loadGameCanvasGroup;
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI noSavesText;
    
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;
    
    private List<SaveSlotUI> _saveSlots = new List<SaveSlotUI>();
    private System.Action _onBack;
    
    private void Awake()
    {
        // Setup button listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        // Start hidden
        if (loadGameCanvasGroup != null)
        {
            loadGameCanvasGroup.alpha = 0f;
            loadGameCanvasGroup.interactable = false;
            loadGameCanvasGroup.blocksRaycasts = false;
        }
            
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show the load game UI
    /// </summary>
    public void Show(System.Action onBack = null)
    {
        _onBack = onBack;
        gameObject.SetActive(true);
        RefreshSaveList();
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Hide the load game UI
    /// </summary>
    public void Hide()
    {
        StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Refresh the list of available saves
    /// </summary>
    public void RefreshSaveList()
    {
        // Clear existing slots
        foreach (var slot in _saveSlots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        _saveSlots.Clear();
        
        // Get all save files
        string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            ShowNoSaves();
            return;
        }
        
        string[] saveFiles = Directory.GetFiles(saveDirectory, "GameFlags_*.json");
        
        if (saveFiles.Length == 0)
        {
            ShowNoSaves();
            return;
        }
        
        // Hide no saves text
        if (noSavesText != null)
            noSavesText.gameObject.SetActive(false);
        
        // Create save slots
        foreach (string filePath in saveFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            // Extract save name from "GameFlags_NAME.json"
            string saveName = fileName.Replace("GameFlags_", "");
            
            CreateSaveSlot(saveName, filePath);
        }
    }
    
    private void ShowNoSaves()
    {
        if (noSavesText != null)
        {
            noSavesText.gameObject.SetActive(true);
            noSavesText.text = "No saved games found";
        }
    }
    
    private void CreateSaveSlot(string saveName, string filePath)
    {
        if (saveSlotPrefab == null || saveSlotContainer == null)
        {
            Debug.LogError("[LoadGameUI] Save slot prefab or container not assigned!");
            return;
        }
        
        GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
        
        if (slotUI == null)
        {
            slotUI = slotObj.AddComponent<SaveSlotUI>();
        }
        
        // Get file info for last modified date
        FileInfo fileInfo = new FileInfo(filePath);
        
        // Get save metadata (day, clock time, etc.)
        GameFlagsSaveData saveData = GameFlags.GetSaveMetadata(saveName);
        string dayInfo = saveData != null ? FormatDayInfo(saveData.currentDay) : "Unknown";
        float clockTime = saveData != null ? saveData.clockTimeLeft : 0f;
        
        slotUI.Initialize(
            saveName, 
            dayInfo,
            clockTime,
            fileInfo.LastWriteTime, 
            () => OnLoadSave(saveName),
            () => OnDeleteSave(saveName)
        );
        
        _saveSlots.Add(slotUI);
    }
    
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
    
    private void OnLoadSave(string saveName)
    {
        Debug.Log($"[LoadGameUI] Loading save: {saveName}");
        
        // Set the active save name
        GameFlagsManager.SetCurrentSaveName(saveName);
        
        // Load the save
        bool success = GameFlags.LoadFromFile(saveName);
        
        if (success)
        {
            // Notify that save was loaded (can be used to trigger scene transition)
            SaveGameEvents.OnSaveLoaded?.Invoke(saveName);
            Hide();
        }
        else
        {
            Debug.LogError($"[LoadGameUI] Failed to load save: {saveName}");
        }
    }
    
    private void OnDeleteSave(string saveName)
    {
        Debug.Log($"[LoadGameUI] Deleting save: {saveName}");
        
        // Delete immediately without confirmation
        bool success = GameFlags.DeleteSaveFile(saveName);
        
        if (success)
        {
            // Refresh the list
            RefreshSaveList();
            
            // Notify that save was deleted
            SaveGameEvents.OnSaveDeleted?.Invoke(saveName);
        }
    }
    
    private void OnBackClicked()
    {
        _onBack?.Invoke();
        Hide();
    }
    
    private IEnumerator FadeIn()
    {
        if (loadGameCanvasGroup == null)
            yield break;
            
        loadGameCanvasGroup.interactable = false;
        loadGameCanvasGroup.blocksRaycasts = true;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            loadGameCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        
        loadGameCanvasGroup.alpha = 1f;
        loadGameCanvasGroup.interactable = true;
    }
    
    private IEnumerator FadeOut()
    {
        if (loadGameCanvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }
            
        loadGameCanvasGroup.interactable = false;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            loadGameCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        loadGameCanvasGroup.alpha = 0f;
        loadGameCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}

/// <summary>
/// Individual save slot UI component
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI dayInfoText;
    [SerializeField] private TextMeshProUGUI clockTimeText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    
    private System.Action _onLoad;
    private System.Action _onDelete;
    
    public void Initialize(string saveName, string dayInfo, float clockTime, System.DateTime lastModified, System.Action onLoad, System.Action onDelete)
    {
        _onLoad = onLoad;
        _onDelete = onDelete;
        
        // Auto-find components if not assigned
        if (saveNameText == null)
            saveNameText = transform.Find("SaveNameText")?.GetComponent<TextMeshProUGUI>();
        if (dayInfoText == null)
            dayInfoText = transform.Find("DayInfoText")?.GetComponent<TextMeshProUGUI>();
        if (clockTimeText == null)
            clockTimeText = transform.Find("ClockTimeText")?.GetComponent<TextMeshProUGUI>();
        if (dateText == null)
            dateText = transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        if (loadButton == null)
            loadButton = transform.Find("LoadButton")?.GetComponent<Button>();
        if (deleteButton == null)
            deleteButton = transform.Find("DeleteButton")?.GetComponent<Button>();
        
        // Set save name
        if (saveNameText != null)
            saveNameText.text = saveName;
        
        // Set day info
        if (dayInfoText != null)
            dayInfoText.text = dayInfo;
        
        // Set clock time with clock icon
        if (clockTimeText != null)
        {
            int minutes = Mathf.FloorToInt(clockTime / 60f);
            int seconds = Mathf.FloorToInt(clockTime % 60f);
            clockTimeText.text = $"?{minutes:D2}:{seconds:D2}";
        }
        
        // Set date (optional - can be hidden if not needed)
        if (dateText != null)
        {
            dateText.gameObject.SetActive(false); // Hide date to match design
        }
        
        // Setup button listeners
        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => _onLoad?.Invoke());
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => _onDelete?.Invoke());
        }
    }
}

/// <summary>
/// Static events for save game operations
/// </summary>
public static class SaveGameEvents
{
    public static System.Action<string> OnSaveLoaded;
    public static System.Action<string> OnSaveDeleted;
    public static System.Action<string> OnSaveCreated;
}
