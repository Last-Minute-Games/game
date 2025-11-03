using UnityEngine;
using UnityEngine.UI;

public class BlackjackPopupController : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Root window panel that contains the BlackjackGame UI.")]
    public GameObject window;
    [Tooltip("Optional: a semi-opaque full-screen Image to block clicks behind.")]
    public GameObject backdrop;
    [Tooltip("Hook the Quit button from the Blackjack UI here.")]
    public Button quitButton;

    [Header("Player Control")]
    [Tooltip("Add your player movement scripts here to disable during the popup.")]
    public Behaviour[] playerControlScripts; // e.g., FirstPersonController, CharacterController wrapper, etc.

    bool wasCursorVisible;
    CursorLockMode priorLockState;

    void Awake()
    {
        if (quitButton != null) quitButton.onClick.AddListener(Hide);

        var game = window.GetComponentInChildren<BlackjackGame>();
        if(game == null)
            game.OnRequestClose += Hide;
        HideImmediate(); // ensure not visible at scene start
    }

    public void Show()
    {
        // Show UI
        if (backdrop) backdrop.SetActive(true);
        if (window) window.SetActive(true);
        gameObject.SetActive(true);

        // Pause player
        foreach (var b in playerControlScripts)
            if (b) b.enabled = false;

        // Cursor for mouse-only minigame
        priorLockState = Cursor.lockState;
        wasCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optionally pause the game world:
        // Time.timeScale = 0f;  // if your overworld has moving NPCs and you want them to pause
    }

    public void Hide()
    {
        // If BlackjackGame needs to do cleanup, you can call a public method on it here.

        // Unpause player
        foreach (var b in playerControlScripts)
            if (b) b.enabled = true;

        // Restore cursor
        Cursor.lockState = priorLockState;
        Cursor.visible = wasCursorVisible;

        // Resume time if you paused it
        // Time.timeScale = 1f;

        // Hide UI
        HideImmediate();
    }

    void HideImmediate()
    {
        if (window) window.SetActive(false);
        if (backdrop) backdrop.SetActive(false);
        gameObject.SetActive(false);
    }
}
