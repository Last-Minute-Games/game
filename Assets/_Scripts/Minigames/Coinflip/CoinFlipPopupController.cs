using UnityEngine;
using UnityEngine.UI;

public class CoinFlipPopupController : MonoBehaviour
{
    [Header("UI")]
    public Button quitButton;

    [Header("Show Flags")]
    [Tooltip("Overworld objects (flag/entrance) to hide once the Coin Flip match has been finished at least once.")]
    [SerializeField] private GameObject[] coinFlipShowFlags;

    [Header("Logic")]
    [SerializeField] private GameManager gameManager;

    private OverworldCoinGameLauncher launcher;

    void Awake()
    {
        // Find the launcher in the current scene (assumes only one)
        launcher = FindObjectOfType<OverworldCoinGameLauncher>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Hide popup on start (will be shown when OpenCoinFlipPopup is called)
        gameObject.SetActive(false);
    }

    public void OnQuitClicked()
    {
        // Did the player complete at least one full match (player or AI reached targetScore)?
        bool finishedMatch = (gameManager != null && gameManager.HasCompletedMatch);

        if (finishedMatch)
        {
            // Optional: if you're using GameFlags like the other minigames:
            // GameFlags.SetFlag("minigame.coinflip.finish");
            GameFlags.SetFlag("minigame.coinflip.finish");

            Debug.Log("[CoinFlip] Match completed – hiding entrance flag.");

            if (gameManager.PlayerWonMatch)
            {
                GameFlags.SetFlag("minigame.coinflip.win");
                Debug.Log("[CoinFlip] Player won the match – setting win flag.");
            }

            else if (gameManager.AiWonMatch)
            {
                GameFlags.SetFlag("minigame.coinflip.lose");
                Debug.Log("[CoinFlip] Player lost the match – setting lose flag.");
            }

            if (coinFlipShowFlags != null)
            {
                foreach (var obj in coinFlipShowFlags)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("[CoinFlip] Closed before finishing match – leaving entrance so player can retry later.");
        }

        // Now actually close the popup
        if (launcher != null)
            launcher.CloseCoinFlipPopup();
        else
            gameObject.SetActive(false); // fallback: just hide it
    }
}
