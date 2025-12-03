using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple button helper component for main menu save system integration.
/// Attach this to buttons and select the action you want them to perform.
/// </summary>
public class SaveSystemButton : MonoBehaviour
{
    public enum ButtonAction
    {
        NewGame,        // Show save name prompt
        LoadGame,       // Show load game UI
        SaveGame,       // Save current game
        ReturnToMainMenu // Return to main menu (for in-game pause menu)
    }
    
    [Header("Button Action")]
    [SerializeField] private ButtonAction action = ButtonAction.NewGame;
    
    [Header("Scene Loading (Optional)")]
    [SerializeField] private string gameSceneName = "NewTutorial";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private Button _button;
    private Startscreen _mainMenu;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("[SaveSystemButton] No Button component found!");
        }
    }
    
    private void OnButtonClicked()
    {
        switch (action)
        {
            case ButtonAction.NewGame:
                OnNewGame();
                break;
            case ButtonAction.LoadGame:
                OnLoadGame();
                break;
            case ButtonAction.SaveGame:
                OnSaveGame();
                break;
            case ButtonAction.ReturnToMainMenu:
                OnReturnToMainMenu();
                break;
        }
    }
    
    private void OnNewGame()
    {
        // Find the main menu component
        if (_mainMenu == null)
            _mainMenu = FindObjectOfType<Startscreen>();
            
        if (_mainMenu != null)
        {
            _mainMenu.StartGame();
        }
        else
        {
            Debug.LogError("[SaveSystemButton] Could not find Startscreen component!");
        }
    }
    
    private void OnLoadGame()
    {
        // Find the main menu component
        if (_mainMenu == null)
            _mainMenu = FindObjectOfType<Startscreen>();
            
        if (_mainMenu != null)
        {
            // Use the same smart flow as the Play button
            _mainMenu.StartGame();
        }
        else
        {
            Debug.LogError("[SaveSystemButton] Could not find Startscreen component!");
        }
    }
    
    private void OnSaveGame()
    {
        bool success = GameFlagsManager.SaveCurrentGame();
        
        if (success)
        {
            Debug.Log("[SaveSystemButton] Game saved successfully!");
            // You could show a "Game Saved!" notification here
        }
        else
        {
            Debug.LogError("[SaveSystemButton] Failed to save game!");
        }
    }
    
    private void OnReturnToMainMenu()
    {
        // Optional: Save before returning to main menu
        GameFlagsManager.SaveCurrentGame();
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
