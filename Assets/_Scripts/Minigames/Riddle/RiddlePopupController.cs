using UnityEngine;
using UnityEngine.UI;

public class RiddlePopupController : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Root window panel that shows the riddle.")]
    [SerializeField] private GameObject window;
    [Tooltip("Optional: semi-opaque full-screen image behind the window.")]
    [SerializeField] private GameObject backdrop;
    [Tooltip("Close / Got It button on the riddle window.")]
    [SerializeField] private Button closeButton;

    [Header("Player Control")]
    [Tooltip("Add your overworld movement scripts here to disable during the riddle.")]
    [SerializeField] private Behaviour[] playerControlScripts;

    [Header("HUD")]
    [Tooltip("HUD root object that should be hidden while the riddle is open.")]
    [SerializeField] private GameObject hudGroup;

    [Header("Show Flags")]
    [Tooltip("Overworld objects (item / flag / trigger) to hide once the riddle is read.")]
    [SerializeField] private GameObject[] riddleShowFlags;

    bool wasCursorVisible;
    CursorLockMode priorLockState;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        HideImmediate();   // start fully hidden
    }

    // Called by OverworldRiddleItem when player presses E
    public void Show()
    {
        // Hide HUD
        if (hudGroup != null)
            hudGroup.SetActive(false);

        // Show UI
        if (backdrop != null) backdrop.SetActive(true);
        if (window != null) window.SetActive(true);
        gameObject.SetActive(true);

        // Disable player controls
        foreach (var b in playerControlScripts)
            if (b != null) b.enabled = false;

        // Pause world time / NPCs (same system Blackjack uses)
        GlobalPause.SetMinigamePaused(true);

        // Unlock and show cursor
        priorLockState = Cursor.lockState;
        wasCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        // Mark flag so other code knows riddle was seen
        GameFlags.SetFlag("minigame.riddle.show");

        // Hide the floor item / flag permanently for this playthrough
        if (riddleShowFlags != null)
        {
            foreach (var obj in riddleShowFlags)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        // Show HUD again
        if (hudGroup != null)
            hudGroup.SetActive(true);

        // Unpause world
        GlobalPause.SetMinigamePaused(false);

        // Re-enable player controls
        foreach (var b in playerControlScripts)
            if (b != null) b.enabled = true;

        // Restore cursor state
        Cursor.lockState = priorLockState;
        Cursor.visible = wasCursorVisible;

        // Finally hide UI
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (window != null) window.SetActive(false);
        if (backdrop != null) backdrop.SetActive(false);
        gameObject.SetActive(false);
    }
}
