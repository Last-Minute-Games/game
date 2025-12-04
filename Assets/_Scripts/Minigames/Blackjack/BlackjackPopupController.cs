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

    [Header("Instructions")]
    public MinigameInstructions instructions;

    [Header("Show Flags")]
    [Tooltip("Overworld objects (flag/entrance) to hide once Blackjack is finished.")]
    [SerializeField] private GameObject[] blackjackShowFlags;

    bool wasCursorVisible;
    CursorLockMode priorLockState;
    [SerializeField] private GameObject hudGroup;

    private BlackjackGame blackjackGame;
    void Awake()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(Hide);

        blackjackGame = window.GetComponentInChildren<BlackjackGame>();
        if (blackjackGame != null)
            blackjackGame.OnRequestClose += Hide;

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

        if (instructions == null)
        {
            // auto-find it if you forgot to wire it
            instructions = GetComponentInChildren<MinigameInstructions>(true);
        }
        if (instructions != null)
        {
            instructions.OnPopupOpened();
        }

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

        bool matchIsOver = (blackjackGame != null && blackjackGame.MatchOver);

        if (matchIsOver && blackjackGame != null)
        {
            //  Base flag: the Blackjack match is finished
            GameFlags.SetFlag("minigame.blackjack.finish");

            //  Outcome flags
            if (blackjackGame.PlayerWonMatch)
            {
                GameFlags.SetFlag("minigame.blackjack.win");
                Debug.Log("[Blackjack] Player won the match – setting win flag.");
            }
            else if (blackjackGame.DealerWonMatch)
            {
                GameFlags.SetFlag("minigame.blackjack.lose");
                Debug.Log("[Blackjack] Player lost the match – setting lose flag.");
            }
            else
            {
                // Optional: if you ever add a tie condition
                Debug.Log("[Blackjack] Match finished, but no clear winner (tie?).");
            }

            // Hide the entrance / flag so the minigame can't be replayed
            
            if (blackjackShowFlags != null)
            {
                foreach (var obj in blackjackShowFlags)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
            
        }
        else
        {
            Debug.Log("[Blackjack] Closed early – leaving entrance so player can retry.");
        }


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
