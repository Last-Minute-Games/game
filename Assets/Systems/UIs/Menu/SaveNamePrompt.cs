using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI component for prompting the player to enter a save name when starting a new game.
/// </summary>
public class SaveNamePrompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI errorText;
    
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Validation")]
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 20;
    
    private System.Action<string> _onConfirm;
    private System.Action _onCancel;
    private bool _isVisible = false;  // Track if prompt is currently showing
    
    private void Awake()
    {
        // Setup button listeners
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
            
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

    private void Update()
    {
        // Only run when prompt is visible
        if (!_isVisible) return;
        
        // Automatically re-focus input field if nothing is selected
        if (saveNameInput != null && UnityEngine.EventSystems.EventSystem.current != null)
        {
            var currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            // If nothing is selected, or if something other than our buttons/input is selected
            if (currentSelected == null)
            {
                saveNameInput.Select();
                saveNameInput.ActivateInputField();
            }
        }
        
        // Allow Enter to confirm if input is valid
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
            {
                OnConfirmClicked();
            }
        }
        
        // Allow Escape to cancel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClicked();
        }
    }
    
    /// <summary>
    /// Show the save name prompt with callbacks
    /// </summary>
    public void Show(System.Action<string> onConfirm, System.Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _isVisible = true;  // Mark as visible
        
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
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Hide the save name prompt
    /// </summary>
    public void Hide()
    {
        _isVisible = false;  // Mark as hidden
        StartCoroutine(FadeOut());
    }
    
    private void OnConfirmClicked()
    {
        Debug.Log("[SaveNamePrompt] Confirm button clicked!");
        
        if (saveNameInput == null)
        {
            Debug.LogWarning("[SaveNamePrompt] saveNameInput is null!");
            return;
        }
            
        string saveName = saveNameInput.text.Trim();
        Debug.Log($"[SaveNamePrompt] Save name entered: '{saveName}'");
        
        // Validate save name
        if (!ValidateSaveName(saveName, out string error))
        {
            Debug.LogWarning($"[SaveNamePrompt] Validation failed: {error}");
            ShowError(error);
            return;
        }
        
        // Check if save already exists
        if (GameFlags.HasSaveFile(saveName))
        {
            Debug.LogWarning($"[SaveNamePrompt] Save '{saveName}' already exists!");
            ShowError($"Save '{saveName}' already exists!");
            return;
        }
        
        // Success - invoke callback
        Debug.Log($"[SaveNamePrompt] Validation passed, invoking callback");
        _onConfirm?.Invoke(saveName);
        Hide();
    }
    
    private void OnCancelClicked()
    {
        Debug.Log("[SaveNamePrompt] Cancel button clicked!");
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
            gameObject.SetActive(false);
            yield break;
        }
            
        promptCanvasGroup.interactable = false;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        promptCanvasGroup.alpha = 0f;
        promptCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
