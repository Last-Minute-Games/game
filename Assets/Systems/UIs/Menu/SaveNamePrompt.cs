using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI component for prompting the player to enter a save name when starting a new game.
/// </summary>
[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class SaveNamePrompt : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("UI References")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI errorText;
    
    [Header("Background (Optional - Manual Setup)")]
    [Tooltip("If you've manually created a background in the hierarchy, assign it here. Otherwise, one will be auto-created.")]
    [SerializeField] private GameObject manualBackground;
    
    [Header("Canvas Settings")]
    [Tooltip("Sort order for this canvas (should be higher than main menu, e.g., 100)")]
    [SerializeField] private int canvasSortOrder = 100;
    
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Validation")]
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 20;
    
    private System.Action<string> _onConfirm;
    private System.Action _onCancel;
    private bool _isVisible = false;  // Track if prompt is currently showing
    
    // Auto-created background blocker (only if manualBackground is not assigned)
    private GameObject _autoBackgroundBlocker;
    private Image _autoBackgroundBlockerImage;
    
    // Canvas components
    private Canvas _canvas;
    private GraphicRaycaster _raycaster;
    private bool _canvasSetupComplete = false;

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: Force Canvas settings when values change in Inspector
    /// </summary>
    private void OnValidate()
    {
        // This runs in the Editor when Inspector values change
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = canvasSortOrder;
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
    }
#endif
    
    private void Awake()
    {
        // Get canvas reference early but DON'T configure it yet
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = gameObject.AddComponent<Canvas>();
        }
        
        _raycaster = GetComponent<GraphicRaycaster>();
        if (_raycaster == null)
        {
            _raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Only create auto-background if no manual background is assigned
        if (manualBackground == null)
        {
            CreateBackgroundBlocker();
        }
        else
        {
            Debug.Log("[SaveNamePrompt] Using manual background from inspector");
            // Ensure manual background starts disabled
            manualBackground.SetActive(false);
            
            // CRITICAL: Disable raycastTarget on manual background so clicks pass through to buttons
            Image bgImage = manualBackground.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.raycastTarget = false; // CHANGED: false so clicks pass through
                Debug.Log("[SaveNamePrompt] Manual background Image raycastTarget DISABLED (allows button clicks)");
            }
            else
            {
                Debug.LogWarning("[SaveNamePrompt] Manual background has no Image component!");
            }
            
            // CRITICAL: Ensure manual background is FIRST CHILD (renders behind)
            manualBackground.transform.SetAsFirstSibling();
            Debug.Log($"[SaveNamePrompt] Manual background sibling index: {manualBackground.transform.GetSiblingIndex()}");
        }
        
        // Setup button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            Debug.Log("[SaveNamePrompt] Confirm button listener added");
        }
        else
        {
            Debug.LogError("[SaveNamePrompt] Confirm button is NULL!");
        }
            
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
            Debug.Log("[SaveNamePrompt] Cancel button listener added");
        }
        else
        {
            Debug.LogError("[SaveNamePrompt] Cancel button is NULL!");
        }
            
        // Setup input field listener
        if (saveNameInput != null)
        {
            saveNameInput.onValueChanged.AddListener(OnInputChanged);
            saveNameInput.characterLimit = maxNameLength;
        }
        
        // Start hidden
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = true;  // Keep true so children work
            promptCanvasGroup.blocksRaycasts = false;  // Don't block when hidden
        }
        
        // Hide error text
        if (errorText != null)
            errorText.gameObject.SetActive(false);
            
        gameObject.SetActive(false);
    }

    private void Start()
    {
        // NOW configure the Canvas after Unity has finished initialization
        // This happens AFTER Inspector values are set, so we can override them
        SetupCanvasForTopRendering();
    }

    private void OnEnable()
    {
        // Force canvas settings every time the object is enabled
        // BUT ONLY if setup is complete (after Start() has run)
        if (_canvasSetupComplete && _canvas != null)
        {
            ForceCanvasSettings();
            // Also verify after one frame in case Unity resets it
            StartCoroutine(VerifyCanvasSettingsNextFrame());
        }
    }

    /// <summary>
    /// Force canvas settings to be correct
    /// </summary>
    private void ForceCanvasSettings()
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = canvasSortOrder;
        Debug.Log($"[SaveNamePrompt] Forced Canvas overrideSorting=true, sortingOrder={canvasSortOrder}");
    }

    /// <summary>
    /// Verify canvas settings stick after one frame
    /// </summary>
    private IEnumerator VerifyCanvasSettingsNextFrame()
    {
        yield return null; // Wait one frame
        
        if (_canvas != null)
        {
            if (!_canvas.overrideSorting || _canvas.sortingOrder != canvasSortOrder)
            {
                Debug.LogWarning($"[SaveNamePrompt] Canvas settings were reset! Re-applying... (was: override={_canvas.overrideSorting}, sort={_canvas.sortingOrder})");
                ForceCanvasSettings();
            }
            else
            {
                Debug.Log($"[SaveNamePrompt] Canvas settings verified: overrideSorting={_canvas.overrideSorting}, sortingOrder={_canvas.sortingOrder}");
            }
        }
    }

    /// <summary>
    /// Set up Canvas component to ensure this UI renders on top of main menu
    /// </summary>
    private void SetupCanvasForTopRendering()
    {
        if (_canvas == null)
        {
            Debug.LogError("[SaveNamePrompt] Canvas is null in SetupCanvasForTopRendering!");
            return;
        }
        
        // FORCE configure canvas to render on top
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = canvasSortOrder;
        
        _canvasSetupComplete = true;
        
        Debug.Log($"[SaveNamePrompt] Canvas configured in Start(): overrideSorting={_canvas.overrideSorting}, sortingOrder={_canvas.sortingOrder}");
    }

    /// <summary>
    /// Create a full-screen background blocker to prevent clicks behind the prompt
    /// </summary>
    private void CreateBackgroundBlocker()
    {
        // Create blocker as first child of this GameObject (renders behind prompt content)
        _autoBackgroundBlocker = new GameObject("AutoBackgroundBlocker");
        _autoBackgroundBlocker.transform.SetParent(transform, false);
        _autoBackgroundBlocker.transform.SetAsFirstSibling(); // Render behind everything else
        
        // Add RectTransform and stretch to fill parent
        RectTransform blockerRect = _autoBackgroundBlocker.AddComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.sizeDelta = Vector2.zero;
        blockerRect.anchoredPosition = Vector2.zero;
        
        // Add Image component with semi-transparent black
        _autoBackgroundBlockerImage = _autoBackgroundBlocker.AddComponent<Image>();
        _autoBackgroundBlockerImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        _autoBackgroundBlockerImage.raycastTarget = true; // CRITICAL: Must block raycasts
        
        // Start disabled
        _autoBackgroundBlocker.SetActive(false);
        
        Debug.Log("[SaveNamePrompt] Auto background blocker created successfully");
    }

    private void Update()
    {
        // Only run when prompt is visible
        if (!_isVisible) return;
        
        #if UNITY_EDITOR
        // Debug: Log mouse clicks to see if they're being registered (EDITOR ONLY)
        if (Input.GetMouseButtonDown(0) && enableDebugLogs)
        {
            LogMouseClickDebug();
        }
        #endif
        
        // Production input handling
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
            {
                OnConfirmClicked();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClicked();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug logging for mouse clicks (Editor only - stripped from builds)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogMouseClickDebug()
    {
        Debug.Log("[SaveNamePrompt] ========== MOUSE CLICK DETECTED ==========");
        
        // Check what's currently selected
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            var currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            Debug.Log($"[SaveNamePrompt] Currently selected object: {(currentSelected != null ? currentSelected.name : "NULL")}");
            
            // Check pointer over what UI element
            var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = Input.mousePosition;
            
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
            
            Debug.Log($"[SaveNamePrompt] Raycast found {results.Count} UI elements under cursor:");
            foreach (var result in results)
            {
                // Get the canvas sort order if available
                Canvas resultCanvas = result.gameObject.GetComponentInParent<Canvas>();
                int sortOrder = resultCanvas != null && resultCanvas.overrideSorting ? resultCanvas.sortingOrder : 0;
                bool hasOverride = resultCanvas != null && resultCanvas.overrideSorting;
                
                Debug.Log($"  - {result.gameObject.name} (layer: {result.gameObject.layer}, canvas sortOrder: {sortOrder}, override: {hasOverride})");
            }
        }
        
        // Check button states
        if (confirmButton != null)
            Debug.Log($"[SaveNamePrompt] Confirm button: interactable={confirmButton.interactable}, activeInHierarchy={confirmButton.gameObject.activeInHierarchy}");
        else
            Debug.LogError("[SaveNamePrompt] Confirm button is NULL!");
            
        if (cancelButton != null)
            Debug.Log($"[SaveNamePrompt] Cancel button: interactable={cancelButton.interactable}, activeInHierarchy={cancelButton.gameObject.activeInHierarchy}");
        else
            Debug.LogError("[SaveNamePrompt] Cancel button is NULL!");
            
        // Check canvas group state
        if (promptCanvasGroup != null)
            Debug.Log($"[SaveNamePrompt] Canvas Group: alpha={promptCanvasGroup.alpha}, interactable={promptCanvasGroup.interactable}, blocksRaycasts={promptCanvasGroup.blocksRaycasts}");
            
        // Check canvas sort order
        if (_canvas != null)
            Debug.Log($"[SaveNamePrompt] Canvas: overrideSorting={_canvas.overrideSorting}, sortingOrder={_canvas.sortingOrder}");
            
        // Check background
        if (manualBackground != null)
        {
            Debug.Log($"[SaveNamePrompt] Manual BG active: {manualBackground.activeSelf}, siblingIndex: {manualBackground.transform.GetSiblingIndex()}");
            Image img = manualBackground.GetComponent<Image>();
            if (img != null)
                Debug.Log($"[SaveNamePrompt] Manual BG Image raycastTarget: {img.raycastTarget}");
        }
        else if (_autoBackgroundBlocker != null)
        {
            Debug.Log($"[SaveNamePrompt] Auto BG active: {_autoBackgroundBlocker.activeSelf}, siblingIndex: {_autoBackgroundBlocker.transform.GetSiblingIndex()}");
        }
    }
#endif

    /// <summary>
    /// Show the save name prompt with callbacks
    /// </summary>
    public void Show(System.Action<string> onConfirm, System.Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _isVisible = true;  // Mark as visible
        
        Debug.Log("[SaveNamePrompt] Show() called");
        
        // Show the appropriate background
        if (manualBackground != null)
        {
            manualBackground.SetActive(true);
            // FORCE it to be first sibling every time we show
            manualBackground.transform.SetAsFirstSibling();
            Debug.Log($"[SaveNamePrompt] Manual background activated, siblingIndex: {manualBackground.transform.GetSiblingIndex()}");
        }
        else if (_autoBackgroundBlocker != null)
        {
            _autoBackgroundBlocker.SetActive(true);
            Debug.Log("[SaveNamePrompt] Auto background blocker activated");
        }
        
        // Reset input field
        if (saveNameInput != null)
        {
            saveNameInput.text = "";
            saveNameInput.Select();
            saveNameInput.ActivateInputField();
        }
        
        // Hide error text
        if (errorText != null)
            errorText.gameObject.SetActive(false);
            
        // Show the prompt
        gameObject.SetActive(true);
        
        // CRITICAL: Force canvas settings RIGHT AFTER activation
        // This ensures they're set AFTER Unity's activation logic runs
        StartCoroutine(ForceCanvasSettingsAfterActivation());
        
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Force canvas settings after GameObject activation completes
    /// </summary>
    private IEnumerator ForceCanvasSettingsAfterActivation()
    {
        // Wait for end of frame to ensure Unity's activation is complete
        yield return new WaitForEndOfFrame();
        
        if (_canvas != null)
        {
            ForceCanvasSettings();
        }
    }
    
    /// <summary>
    /// Hide the save name prompt
    /// </summary>
    public void Hide()
    {
        _isVisible = false;  // Mark as hidden
        Debug.Log("[SaveNamePrompt] Hide() called");
        StartCoroutine(FadeOut());
    }
    
    private void OnConfirmClicked()
    {
        Debug.Log("[SaveNamePrompt] ============ CONFIRM BUTTON CLICKED! ============");
        Debug.Log($"[SaveNamePrompt] saveNameInput null? {saveNameInput == null}");
        
        if (saveNameInput == null)
        {
            Debug.LogWarning("[SaveNamePrompt] saveNameInput is null!");
            return;
        }
            
        string saveName = saveNameInput.text.Trim();
        Debug.Log($"[SaveNamePrompt] Save name entered: '{saveName}'");
        Debug.Log($"[SaveNamePrompt] Text length: {saveName.Length}");
        
        // Validate save name
        if (!ValidateSaveName(saveName, out string error))
        {
            Debug.LogWarning($"[SaveNamePrompt] Validation failed: {error}");
            ShowError(error);
            return;
        }
        
        Debug.Log("[SaveNamePrompt] Validation passed!");
        
        // Check if save already exists
        if (GameFlags.HasSaveFile(saveName))
        {
            Debug.LogWarning($"[SaveNamePrompt] Save '{saveName}' already exists!");
            ShowError($"Save '{saveName}' already exists!");
            return;
        }
        
        // Success - invoke callback
        Debug.Log($"[SaveNamePrompt] Invoking callback with save name: '{saveName}'");
        _onConfirm?.Invoke(saveName);
        Hide();
    }
    
    private void OnCancelClicked()
    {
        Debug.Log("[SaveNamePrompt] ============ CANCEL BUTTON CLICKED! ============");
        _onCancel?.Invoke();
        Hide();
    }
    
    private void OnInputChanged(string value)
    {
        // Hide error when user starts typing again
        if (errorText != null && errorText.gameObject.activeSelf)
            errorText.gameObject.SetActive(false);
            
        // Update confirm button state
        if (confirmButton != null)
        {
            bool isValid = ValidateSaveName(value.Trim(), out _);
            confirmButton.interactable = isValid;
        }
    }
    
    private bool ValidateSaveName(string name, out string error)
    {
        error = "";
        
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Save name cannot be empty";
            return false;
        }
        
        if (name.Length < minNameLength)
        {
            error = $"Save name must be at least {minNameLength} characters";
            return false;
        }
        
        if (name.Length > maxNameLength)
        {
            error = $"Save name cannot exceed {maxNameLength} characters";
            return false;
        }
        
        // Check for invalid characters
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            if (name.Contains(c.ToString()))
            {
                error = "Save name contains invalid characters";
                return false;
            }
        }
        
        return true;
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
        if (promptCanvasGroup == null)
            yield break;
            
        // IMPORTANT: Set blocksRaycasts FIRST to prevent clicks beneath
        promptCanvasGroup.blocksRaycasts = true;
        promptCanvasGroup.interactable = true;  // Keep TRUE so child elements work
        
        // Start from 0 alpha
        promptCanvasGroup.alpha = 0f;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        
        promptCanvasGroup.alpha = 1f;
        
        // IMPORTANT: Re-focus the input field after fade completes
        if (saveNameInput != null)
        {
            yield return null; // Wait one frame
            saveNameInput.Select();
            saveNameInput.ActivateInputField();
        }
    }
    
    private IEnumerator FadeOut()
    {
        if (promptCanvasGroup == null)
        {
            // Hide backgrounds
            if (manualBackground != null)
                manualBackground.SetActive(false);
            if (_autoBackgroundBlocker != null)
                _autoBackgroundBlocker.SetActive(false);
            
            gameObject.SetActive(false);
            yield break;
        }
            
        promptCanvasGroup.interactable = false;
        
        // Faster fade out for cancel (0.2s instead of fadeDuration)
        float duration = 0.2f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }
        
        promptCanvasGroup.alpha = 0f;
        promptCanvasGroup.blocksRaycasts = false;
        
        // Hide backgrounds
        if (manualBackground != null)
            manualBackground.SetActive(false);
        if (_autoBackgroundBlocker != null)
            _autoBackgroundBlocker.SetActive(false);
        
        gameObject.SetActive(false);
    }
}
