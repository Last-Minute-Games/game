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
    [SerializeField] private GameObject hudGroup;

    void Awake()
    {
        if (quitButton != null) quitButton.onClick.AddListener(Hide);

        var game = window.GetComponentInChildren<BlackjackGame>();
        if (game) game.OnRequestClose += Hide;
        HideImmediate(); // ensure not visible at scene start
    }

    public void Show()
    {
        if (hudGroup != null)
            hudGroup.SetActive(false);   // hide HUD

        backdrop.SetActive(true);
        window.SetActive(true);

        // Show UI
        if (backdrop) backdrop.SetActive(true);
        if (window) window.SetActive(true);
        gameObject.SetActive(true);

        // Pause player input (still using player control scripts disable)
        foreach (var b in playerControlScripts)
            if (b) b.enabled = false;

        // Pause NPCs and ClockTimer (but NOT player input - minigame pause)
        GlobalPause.SetMinigamePaused(true);

        // Cursor for mouse-only minigame
        priorLockState = Cursor.lockState;
        wasCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {

        backdrop.SetActive(false);
        window.SetActive(false);

        if (hudGroup != null)
            hudGroup.SetActive(true);    // show HUD again

        // Resume NPCs and ClockTimer (using minigame pause)
        GlobalPause.SetMinigamePaused(false);

        GameFlags.SetFlag("minigame.blackjack.finish");
        Debug.Log("[Blackjack] Flag set: minigame.blackjack.finish");

        // If BlackjackGame needs to do cleanup, you can call a public method on it here.

        // Unpause player input
        foreach (var b in playerControlScripts)
            if (b) b.enabled = true;

        // Restore cursor
        Cursor.lockState = priorLockState;
        Cursor.visible = wasCursorVisible;

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
