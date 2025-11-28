using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Example main menu integration with GameFlags save system.
/// Shows how to implement New Game / Continue functionality.
/// </summary>
public class MainMenuExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button deleteButton;
    
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Optional")]
    [SerializeField] private GameObject confirmDeletePanel;
    
    private void Start()
    {
        UpdateContinueButton();
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
            
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnRequestDelete);
    }
    
    /// <summary>
    /// Update the continue button based on whether save data exists
    /// </summary>
    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            bool hasSave = GameFlags.HasSavedFlags();
            continueButton.interactable = hasSave;
            
            // Optional: change button appearance
            var colors = continueButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            continueButton.colors = colors;
            
            Debug.Log($"[MainMenu] Save data exists: {hasSave}");
        }
    }
    
    /// <summary>
    /// Start a new game (resets flags to defaults and saves)
    /// </summary>
    private void OnNewGame()
    {
        Debug.Log("[MainMenu] Starting new game");
        
        // If save data exists, you might want to show a confirmation dialog
        if (GameFlags.HasSavedFlags())
        {
            // Optional: Show "This will overwrite your save" warning
            ConfirmNewGame();
        }
        else
        {
            StartNewGame();
        }
    }
    
    /// <summary>
    /// Continue from saved game (loads saved flags)
    /// </summary>
    private void OnContinue()
    {
        Debug.Log("[MainMenu] Continuing from saved game");
        
        // Flags are already loaded automatically in GameFlags.Awake
        // We just need to load the game scene
        LoadGameScene();
    }
    
    /// <summary>
    /// Request to delete save data (shows confirmation)
    /// </summary>
    private void OnRequestDelete()
    {
        if (confirmDeletePanel != null)
        {
            confirmDeletePanel.SetActive(true);
        }
        else
        {
            // Direct delete without confirmation
            ConfirmDelete();
        }
    }
    
    /// <summary>
    /// Confirm and execute new game start
    /// </summary>
    public void ConfirmNewGame()
    {
        // Reset to default flags
        GameFlags.ResetToDefaults();
        
        // Save the defaults
        GameFlags.SaveFlags();
        
        Debug.Log("[MainMenu] New game confirmed - flags reset and saved");
        
        // Load game scene
        LoadGameScene();
    }
    
    /// <summary>
    /// Confirm and execute save deletion
    /// </summary>
    public void ConfirmDelete()
    {
        GameFlags.DeleteSavedFlags();
        Debug.Log("[MainMenu] Save data deleted");
        
        // Update UI
        UpdateContinueButton();
        
        // Hide confirmation panel
        if (confirmDeletePanel != null)
            confirmDeletePanel.SetActive(false);
    }
    
    /// <summary>
    /// Cancel delete confirmation
    /// </summary>
    public void CancelDelete()
    {
        if (confirmDeletePanel != null)
            confirmDeletePanel.SetActive(false);
    }
    
    /// <summary>
    /// Internal method to start a new game
    /// </summary>
    private void StartNewGame()
    {
        GameFlags.ResetToDefaults();
        GameFlags.SaveFlags();
        LoadGameScene();
    }
    
    /// <summary>
    /// Load the game scene
    /// </summary>
    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[MainMenu] Game scene name not set!");
        }
    }
    
    // ========== DEBUG METHODS ==========
    
    /// <summary>
    /// Debug: Print all active flags to console
    /// </summary>
    [ContextMenu("Debug: Print All Flags")]
    public void DebugPrintFlags()
    {
        GameFlags.PrintAllFlags();
    }
    
    /// <summary>
    /// Debug: Force save current flags
    /// </summary>
    [ContextMenu("Debug: Force Save")]
    public void DebugForceSave()
    {
        GameFlags.SaveFlags();
        Debug.Log("[MainMenu] Forced save completed");
    }
    
    /// <summary>
    /// Debug: Force load flags
    /// </summary>
    [ContextMenu("Debug: Force Load")]
    public void DebugForceLoad()
    {
        bool success = GameFlags.LoadFlags();
        Debug.Log($"[MainMenu] Forced load {(success ? "successful" : "failed")}");
    }
    
    /// <summary>
    /// Debug: Add a test flag
    /// </summary>
    [ContextMenu("Debug: Add Test Flag")]
    public void DebugAddTestFlag()
    {
        GameFlags.SetFlag("test.flag.example");
        Debug.Log("[MainMenu] Added test flag");
    }
}
