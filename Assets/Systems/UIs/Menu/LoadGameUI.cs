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
    
    private CanvasGroup _fadeCanvasGroup; // For scene transition fades
    
    private void Awake()
    {
        // Find the fade canvas group (same as MainMenu does)
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup")?.GetComponent<CanvasGroup>();
        if (_fadeCanvasGroup != null)
        {
            Debug.Log("[LoadGameUI] Found FadeCanvasGroup for scene transitions");
        }
        else
        {
            Debug.LogWarning("[LoadGameUI] FadeCanvasGroup not found - scene transitions will not fade");
        }
        
        // Setup the main canvas rect transform first
        SetupMainCanvasRect();
        
        // Setup button listeners
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            Debug.Log("[LoadGameUI] Back button listener added");
        }
        else
        {
            Debug.LogWarning("[LoadGameUI] Back button reference is null!");
        }
        
        // Setup ScrollView Content with proper layout components
        SetupScrollViewContent();
        
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
    /// Setup the main canvas RectTransform to fill the screen/parent
    /// </summary>
    private void SetupMainCanvasRect()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        
        // If this GameObject doesn't have a RectTransform, search in children
        if (rectTransform == null)
        {
            Debug.LogWarning("[LoadGameUI] LoadGameUI is not on a RectTransform GameObject. Searching for Container panel...");
            
            // Try to find a Container GameObject in children
            Transform container = transform.Find("Canvas/Container");
            if (container == null)
            {
                // Try alternative search
                Transform canvasTransform = GetComponentInChildren<Canvas>()?.transform;
                if (canvasTransform != null)
                {
                    container = canvasTransform.Find("Container");
                }
            }
            
            if (container != null)
            {
                rectTransform = container.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    Debug.LogError("[LoadGameUI] Container doesn't have a RectTransform! Cannot setup UI.");
                    return;
                }
                Debug.Log("[LoadGameUI] Found Container RectTransform");
            }
            else
            {
                Debug.LogError("[LoadGameUI] Could not find Container panel in hierarchy! Cannot setup UI.");
                return;
            }
        }
        
        // Log the current state before setup
        Debug.Log($"[LoadGameUI] Before setup - Container rect: {rectTransform.rect.width}x{rectTransform.rect.height}, SizeDelta: {rectTransform.sizeDelta}");
        
        // Stretch to fill parent (full screen)
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero; // Zero offset when stretched
        
        Debug.Log($"[LoadGameUI] Panel RectTransform setup - Anchors: {rectTransform.anchorMin} to {rectTransform.anchorMax}");
        
        // Also setup parent Canvas if it exists and needs fixing
        Transform canvasParent = rectTransform.parent;
        if (canvasParent != null)
        {
            RectTransform canvasRect = canvasParent.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Debug.Log($"[LoadGameUI] Canvas parent found - Current rect: {canvasRect.rect.width}x{canvasRect.rect.height}");
                
                // Only fix if it has 0 width/height
                if (canvasRect.rect.width == 0 || canvasRect.rect.height == 0)
                {
                    canvasRect.anchorMin = Vector2.zero;
                    canvasRect.anchorMax = Vector2.one;
                    canvasRect.pivot = new Vector2(0.5f, 0.5f);
                    canvasRect.anchoredPosition = Vector2.zero;
                    canvasRect.sizeDelta = Vector2.zero;
                    Debug.Log("[LoadGameUI] Fixed Canvas parent RectTransform");
                }
            }
        }
        
        // Force an immediate layout update
        Canvas.ForceUpdateCanvases();
        
        // Log the result
        Debug.Log($"[LoadGameUI] After setup - Container rect: {rectTransform.rect.width}x{rectTransform.rect.height}, SizeDelta: {rectTransform.sizeDelta}");
    }
    
    /// <summary>
    /// Ensure the scroll view content has proper layout components
    /// </summary>
    private void SetupScrollViewContent()
    {
        if (saveSlotContainer == null)
        {
            Debug.LogError("[LoadGameUI] Save slot container is null!");
            return;
        }
        
        // Ensure saveSlotContainer is a RectTransform
        RectTransform contentRect = saveSlotContainer as RectTransform;
        if (contentRect == null)
        {
            Debug.LogError("[LoadGameUI] Save slot container must be a RectTransform!");
            return;
        }
        
        Debug.Log($"[LoadGameUI] Content initial rect: {contentRect.rect.width}x{contentRect.rect.height}");
        
        // Setup the viewport (parent of content) to ensure proper width
        Transform viewport = contentRect.parent;
        if (viewport != null)
        {
            RectTransform viewportRect = viewport as RectTransform;
            if (viewportRect != null)
            {
                Debug.Log($"[LoadGameUI] Viewport before setup: {viewportRect.rect.width}x{viewportRect.rect.height}");
                
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.pivot = new Vector2(0.5f, 0.5f);
                viewportRect.anchoredPosition = Vector2.zero;
                viewportRect.sizeDelta = Vector2.zero;
                Debug.Log($"[LoadGameUI] Viewport RectTransform setup - Anchors: {viewportRect.anchorMin} to {viewportRect.anchorMax}");
                
                // Setup ScrollView (parent of viewport) as well
                Transform scrollView = viewport.parent;
                if (scrollView != null)
                {
                    RectTransform scrollViewRect = scrollView as RectTransform;
                    if (scrollViewRect != null)
                    {
                        Debug.Log($"[LoadGameUI] ScrollView before setup: {scrollViewRect.rect.width}x{scrollViewRect.rect.height}");
                        
                        // Only fix if it has sizing issues
                        if (scrollViewRect.rect.width == 0 || scrollViewRect.rect.height == 0)
                        {
                            scrollViewRect.anchorMin = new Vector2(0.1f, 0.2f); // Leave some margin
                            scrollViewRect.anchorMax = new Vector2(0.9f, 0.8f);
                            scrollViewRect.sizeDelta = Vector2.zero;
                            Debug.Log("[LoadGameUI] Fixed ScrollView RectTransform");
                        }
                    }
                }
            }
        }
        
        // Fix the RectTransform to fill the viewport horizontally
        contentRect.anchorMin = new Vector2(0, 1); // Top-left anchor
        contentRect.anchorMax = new Vector2(1, 1); // Top-right anchor (stretch horizontally)
        contentRect.pivot = new Vector2(0.5f, 1); // Pivot at top-center
        contentRect.anchoredPosition = Vector2.zero;
        
        // Set a proper initial size (will be adjusted by ContentSizeFitter)
        if (contentRect.sizeDelta.x == 0)
        {
            contentRect.sizeDelta = new Vector2(0, contentRect.sizeDelta.y); // Width will auto-stretch, keep height
        }
        
        Debug.Log($"[LoadGameUI] Content RectTransform setup - Anchors: {contentRect.anchorMin} to {contentRect.anchorMax}, SizeDelta: {contentRect.sizeDelta}");
        Debug.Log($"[LoadGameUI] Content after anchor setup: {contentRect.rect.width}x{contentRect.rect.height}");
        
        // Ensure Content has a VerticalLayoutGroup
        var layoutGroup = saveSlotContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = saveSlotContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            Debug.Log("[LoadGameUI] Added VerticalLayoutGroup to Content");
        }
        
        // Configure the layout group
        layoutGroup.spacing = 10f; // Space between save slots
        layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Padding around edges
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false; // IMPORTANT: Let LayoutElement control height
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        
        // Ensure Content has a ContentSizeFitter
        var contentFitter = saveSlotContainer.GetComponent<ContentSizeFitter>();
        if (contentFitter == null)
        {
            contentFitter = saveSlotContainer.gameObject.AddComponent<ContentSizeFitter>();
            Debug.Log("[LoadGameUI] Added ContentSizeFitter to Content");
        }
        
        // Configure the content fitter to expand vertically only
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        Debug.Log("[LoadGameUI] ScrollView content setup complete");
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
        Debug.Log("[LoadGameUI] Refreshing save list...");
        
        // Clear existing slots
        foreach (var slot in _saveSlots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        _saveSlots.Clear();
        
        // Get all save files
        string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        Debug.Log($"[LoadGameUI] Looking for saves in: {saveDirectory}");
        
        if (!Directory.Exists(saveDirectory))
        {
            Debug.Log("[LoadGameUI] Save directory doesn't exist");
            ShowNoSaves();
            return;
        }
        
        string[] saveFiles = Directory.GetFiles(saveDirectory, "GameFlags_*.json");
        Debug.Log($"[LoadGameUI] Found {saveFiles.Length} save files");
        
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
            
            Debug.Log($"[LoadGameUI] Creating slot for save: {saveName}");
            CreateSaveSlot(saveName, filePath);
        }
        
        Debug.Log($"[LoadGameUI] Created {_saveSlots.Count} save slots");
        
        // Force layout rebuild to ensure everything displays correctly
        StartCoroutine(ForceRebuildLayout());
    }
    
    /// <summary>
    /// Force the layout to rebuild after creating slots
    /// </summary>
    private IEnumerator ForceRebuildLayout()
    {
        // Wait multiple frames for Unity to process the instantiations and LayoutElements
        yield return null;
        yield return null;
        yield return null;
        
        if (saveSlotContainer != null)
        {
            RectTransform contentRect = saveSlotContainer as RectTransform;
            
            // Force layout rebuild multiple times to ensure it sticks
            for (int i = 0; i < 3; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                Canvas.ForceUpdateCanvases();
                yield return null;
            }
            
            Debug.Log("[LoadGameUI] Layout rebuilt");
            
            // Log the content size
            if (contentRect != null)
            {
                Debug.Log($"[LoadGameUI] Content size after rebuild - Width: {contentRect.sizeDelta.x}, Height: {contentRect.sizeDelta.y}");
                Debug.Log($"[LoadGameUI] Content rect size - Width: {contentRect.rect.width}, Height: {contentRect.rect.height}");
                
                // Log individual slot heights for debugging
                int slotIndex = 0;
                foreach (Transform child in contentRect)
                {
                    RectTransform childRect = child as RectTransform;
                    if (childRect != null)
                    {
                        LayoutElement le = child.GetComponent<LayoutElement>();
                        Debug.Log($"[LoadGameUI] Slot {slotIndex}: height={childRect.rect.height}, preferredHeight={le?.preferredHeight ?? -1}");
                    }
                    slotIndex++;
                }
                
                // If width is still 0, force it to match the viewport width
                if (contentRect.rect.width == 0 || contentRect.sizeDelta.x == 0)
                {
                    RectTransform viewportRect = contentRect.parent as RectTransform;
                    if (viewportRect != null)
                    {
                        float viewportWidth = viewportRect.rect.width;
                        Debug.Log($"[LoadGameUI] Forcing content width to match viewport: {viewportWidth}");
                        
                        // Disable ContentSizeFitter temporarily
                        ContentSizeFitter fitter = contentRect.GetComponent<ContentSizeFitter>();
                        if (fitter != null)
                        {
                            fitter.enabled = false;
                        }
                        
                        // Set a minimum width
                        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
                        
                        // Re-enable fitter
                        if (fitter != null)
                        {
                            fitter.enabled = true;
                        }
                        
                        Debug.Log($"[LoadGameUI] After fix - Content rect size: {contentRect.rect.width}x{contentRect.rect.height}");
                    }
                }
            }
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
        slotObj.SetActive(true); // Ensure the slot is active
        
        // Disable any nested Canvas components (they interfere with rendering)
        Canvas[] nestedCanvases = slotObj.GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in nestedCanvases)
        {
            if (canvas.gameObject != slotObj) // Don't disable if Canvas is on root
            {
                Debug.Log($"[LoadGameUI] Disabling nested Canvas on {canvas.gameObject.name}");
                canvas.enabled = false;
            }
        }
        
        // Ensure the prefab has a RectTransform
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        if (slotRect == null)
        {
            Debug.LogError("[LoadGameUI] SaveSlotPrefab must have a RectTransform!");
            Destroy(slotObj);
            return;
        }
        
        // Get the original prefab height before modifying anything
        float originalHeight = slotRect.rect.height;
        if (originalHeight == 0)
        {
            // If height is 0, check sizeDelta
            originalHeight = Mathf.Abs(slotRect.sizeDelta.y);
        }
        if (originalHeight == 0)
        {
            // Fallback to a generous default if still 0
            originalHeight = 200f; // INCREASED: Much larger to ensure visibility
        }
        
        // If the detected height is too small, force it to be larger
        if (originalHeight < 150f)
        {
            Debug.Log($"[LoadGameUI] Detected height too small ({originalHeight}), forcing to 200px");
            originalHeight = 200f;
        }
        
        Debug.Log($"[LoadGameUI] Using prefab height: {originalHeight}");
        
        // NUCLEAR OPTION: Set RectTransform directly and bypass LayoutElement entirely
        // Configure anchors for proper vertical sizing
        slotRect.anchorMin = new Vector2(0f, 0.5f);
        slotRect.anchorMax = new Vector2(1f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Set height directly via sizeDelta (this works when anchors are at same Y position)
        slotRect.sizeDelta = new Vector2(0f, 200f); // Width=0 (stretch), Height=200 (fixed)
        
        Debug.Log($"[LoadGameUI] Set RectTransform directly - Anchors: {slotRect.anchorMin} to {slotRect.anchorMax}, SizeDelta: {slotRect.sizeDelta}");
        
        // Get or add LayoutElement (but don't rely on it)
        LayoutElement layoutElement = slotObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = slotObj.AddComponent<LayoutElement>();
            Debug.Log("[LoadGameUI] Added LayoutElement to save slot");
        }
        else
        {
            Debug.Log($"[LoadGameUI] Using existing LayoutElement (preferredHeight: {layoutElement.preferredHeight})");
        }
        
        // Configure LayoutElement as backup
        layoutElement.ignoreLayout = false;
        layoutElement.minHeight = 200f;
        layoutElement.preferredHeight = 200f;
        layoutElement.flexibleHeight = 0f;
        layoutElement.minWidth = -1f;
        layoutElement.preferredWidth = -1f;
        layoutElement.flexibleWidth = 1f;
        
        Debug.Log($"[LoadGameUI] LayoutElement configured - minHeight: {layoutElement.minHeight}, preferredHeight: {layoutElement.preferredHeight}");
        
        // Force immediate layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotRect);
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[LoadGameUI] Slot final height: {slotRect.rect.height}");
        
        // FIX INTERNAL LAYOUT: Ensure child elements have proper constraints
        FixSlotInternalLayout(slotObj);
        
        // Reset the RectTransform to ensure proper positioning
        slotRect.localScale = Vector3.one;
        slotRect.localPosition = Vector3.zero;
        slotRect.localRotation = Quaternion.identity;
        
        // Ensure all child UI elements are active and visible
        Image[] images = slotObj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.gameObject.SetActive(true);
            img.enabled = true;
        }
        
        TextMeshProUGUI[] texts = slotObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            text.gameObject.SetActive(true);
            text.enabled = true;
        }
        
        Debug.Log($"[LoadGameUI] Slot RectTransform - AnchoredPosition: {slotRect.anchoredPosition}, SizeDelta: {slotRect.sizeDelta}");
        Debug.Log($"[LoadGameUI] Slot has {images.Length} images and {texts.Length} texts");
        
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
        
        if (slotUI == null)
        {
            Debug.LogWarning("[LoadGameUI] SaveSlotUI component not found on prefab, adding it...");
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
        
        Debug.Log($"[LoadGameUI] Save slot created and initialized for: {saveName}");
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
            Debug.Log($"[LoadGameUI] Successfully loaded save: {saveName}");
            
            // Start fade transition coroutine
            StartCoroutine(FadeAndLoadOverworld(saveName));
        }
        else
        {
            Debug.LogError($"[LoadGameUI] Failed to load save: {saveName}");
        }
    }
    
    /// <summary>
    /// Fade out, load overworld scene, then fade in
    /// </summary>
    private IEnumerator FadeAndLoadOverworld(string saveName)
    {
        Debug.Log("[LoadGameUI] Starting fade out before loading overworld");
        
        // Disable interaction on load game UI
        if (loadGameCanvasGroup != null)
        {
            loadGameCanvasGroup.interactable = false;
        }
        
        if (_fadeCanvasGroup != null)
        {
            // Fade to black using the fade canvas (same as MainMenu does)
            _fadeCanvasGroup.blocksRaycasts = true;
            
            float fadeTimer = 0f;
            float targetDuration = fadeDuration * 2.5f; // Longer fade like MainMenu
            
            while (fadeTimer < targetDuration)
            {
                fadeTimer += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / targetDuration);
                yield return null;
            }
            
            _fadeCanvasGroup.alpha = 1f;
            Debug.Log("[LoadGameUI] Fade to black complete");
        }
        else
        {
            Debug.LogWarning("[LoadGameUI] No fade canvas - loading scene immediately");
        }
        
        // Wait a brief moment for dramatic effect
        yield return new WaitForSeconds(0.3f);
        
        // Load the overworld scene - OverworldWakeUpCutscene will automatically:
        // 1. Play eyes opening animation (it handles its own fade)
        // 2. Play clock reconstruction animation (19 ? 13 ? 1)
        // 3. Play day-specific wake-up dialogue (if configured for current day)
        // 4. Start the clock timer with the saved time
        Debug.Log("[LoadGameUI] Loading overworld scene");
        
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("overworld");
        op.allowSceneActivation = true;
        
        // Wait until load is done
        while (!op.isDone)
            yield return null;
        
        Debug.Log("[LoadGameUI] Overworld scene loaded");
    }
    
    private void OnDeleteSave(string saveName)
    {
        Debug.Log($"[LoadGameUI] Deleting save: {saveName}");
        
        // Delete immediately without confirmation
        bool success = GameFlags.DeleteSaveFile(saveName);
        
        if (success)
        {
            Debug.Log($"[LoadGameUI] Successfully deleted save: {saveName}");
            // Refresh the list
            RefreshSaveList();
            
            // Notify that save was deleted
            SaveGameEvents.OnSaveDeleted?.Invoke(saveName);
        }
        else
        {
            Debug.LogError($"[LoadGameUI] Failed to delete save: {saveName}");
        }
    }
    
    private void OnBackClicked()
    {
        Debug.Log("[LoadGameUI] Back button clicked");
        _onBack?.Invoke();
        Hide();
    }
    
    private IEnumerator FadeIn()
    {
        if (loadGameCanvasGroup == null)
        {
            Debug.LogWarning("[LoadGameUI] Canvas group is null, skipping fade");
            yield break;
        }
            
        loadGameCanvasGroup.blocksRaycasts = true;
        loadGameCanvasGroup.interactable = false;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            loadGameCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        
        loadGameCanvasGroup.alpha = 1f;
        loadGameCanvasGroup.interactable = true;
        
        Debug.Log("[LoadGameUI] Fade in complete, UI is now interactable");
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

    /// <summary>
    /// Fix the internal layout of a save slot, ensuring child elements are properly anchored and sized.
    /// This is crucial after dynamically changing the slot's height.
    /// </summary>
    private void FixSlotInternalLayout(GameObject slotObj)
    {
        // Get RectTransform of the slot object
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        if (slotRect == null)
        {
            Debug.LogError("[LoadGameUI] FixSlotInternalLayout: Save slot object has no RectTransform!");
            return;
        }
        
        // Find all images (including character portrait)
        Image[] images = slotObj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            // Skip the background/main panel image
            if (img.gameObject == slotObj) continue;
            
            RectTransform imgRect = img.GetComponent<RectTransform>();
            if (imgRect == null) continue;
            
            // If this looks like a character portrait (small, likely square), add AspectRatioFitter
            if (imgRect.name.Contains("Portrait") || imgRect.name.Contains("Character") || imgRect.name.Contains("Icon"))
            {
                // Add AspectRatioFitter to maintain square aspect ratio
                AspectRatioFitter aspectFitter = img.GetComponent<AspectRatioFitter>();
                if (aspectFitter == null)
                {
                    aspectFitter = img.gameObject.AddComponent<AspectRatioFitter>();
                }
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspectFitter.aspectRatio = 1f; // Square
                
                Debug.Log($"[LoadGameUI] Added AspectRatioFitter to {imgRect.name}");
            }
        }
        
        // Find and fix text elements - ensure they don't stretch weirdly
        TextMeshProUGUI[] texts = slotObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            RectTransform textRect = text.GetComponent<RectTransform>();
            if (textRect == null) continue;
            
            // Enable TextMeshPro auto-sizing if text is too large
            text.enableAutoSizing = true;
            text.fontSizeMin = 10f;
            text.fontSizeMax = 48f;
            
            // Ensure text overflow is handled properly
            text.overflowMode = TextOverflowModes.Ellipsis;
        }
        
        // Find buttons and ensure they have proper layout
        Button[] buttons = slotObj.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect == null) continue;
            
            // Ensure buttons have a LayoutElement to control their size
            LayoutElement buttonLayout = button.GetComponent<LayoutElement>();
            if (buttonLayout == null)
            {
                buttonLayout = button.gameObject.AddComponent<LayoutElement>();
            }
            
            // Set reasonable button sizes
            buttonLayout.preferredWidth = 120f;
            buttonLayout.preferredHeight = 40f;
            buttonLayout.flexibleWidth = 0f;
            buttonLayout.flexibleHeight = 0f;
        }
        
        Debug.Log($"[LoadGameUI] Fixed internal layout for slot: {slotObj.name}");
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
        
        Debug.Log($"[SaveSlotUI] Initializing slot for: {saveName}");
        
        // Auto-find components if not assigned - search recursively in children
        if (saveNameText == null)
        {
            saveNameText = GetComponentInChildren<TextMeshProUGUI>(true);
            // Try to find specifically by name if multiple TextMeshProUGUI exist
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                if (text.name == "SaveNameText")
                {
                    saveNameText = text;
                    break;
                }
            }
        }
        
        if (dayInfoText == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                if (text.name == "DayInfoText")
                {
                    dayInfoText = text;
                    break;
                }
            }
        }
        
        if (clockTimeText == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                if (text.name == "ClockTimeText")
                {
                    clockTimeText = text;
                    break;
                }
            }
        }
        
        if (dateText == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                if (text.name == "DateText")
                {
                    dateText = text;
                    break;
                }
            }
        }
        
        if (loadButton == null)
        {
            var allButtons = GetComponentsInChildren<Button>(true);
            foreach (var button in allButtons)
            {
                if (button.name == "LoadButton")
                {
                    loadButton = button;
                    break;
                }
            }
        }
        
        if (deleteButton == null)
        {
            var allButtons = GetComponentsInChildren<Button>(true);
            foreach (var button in allButtons)
            {
                if (button.name == "DeleteButton")
                {
                    deleteButton = button;
                    break;
                }
            }
        }
        
        // Set save name
        if (saveNameText != null)
        {
            saveNameText.text = saveName;
            Debug.Log($"[SaveSlotUI] Set save name text to: {saveName}");
        }
        else
        {
            Debug.LogWarning("[SaveSlotUI] Could not find SaveNameText component in children");
        }
        
        // Set day info
        if (dayInfoText != null)
        {
            dayInfoText.text = dayInfo;
            Debug.Log($"[SaveSlotUI] Set day info to: {dayInfo}");
        }
        
        // Set clock time with clock icon
        if (clockTimeText != null)
        {
            int minutes = Mathf.FloorToInt(clockTime / 60f);
            int seconds = Mathf.FloorToInt(clockTime % 60f);
            clockTimeText.text = $"? {minutes:D2}:{seconds:D2}"; // Using unicode clock emoji
            Debug.Log($"[SaveSlotUI] Set clock time to: {minutes:D2}:{seconds:D2}");
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
            loadButton.onClick.AddListener(() => {
                Debug.Log($"[SaveSlotUI] Load button clicked for: {saveName}");
                _onLoad?.Invoke();
            });
            Debug.Log($"[SaveSlotUI] Load button configured for: {saveName}");
        }
        else
        {
            Debug.LogWarning($"[SaveSlotUI] Could not find LoadButton component in children");
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => {
                Debug.Log($"[SaveSlotUI] Delete button clicked for: {saveName}");
                _onDelete?.Invoke();
            });
            Debug.Log($"[SaveSlotUI] Delete button configured for: {saveName}");
        }
        else
        {
            Debug.LogWarning($"[SaveSlotUI] Could not find DeleteButton component in children");
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
