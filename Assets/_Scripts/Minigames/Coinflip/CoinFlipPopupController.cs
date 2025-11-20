using UnityEngine;
using UnityEngine.UI;

public class CoinFlipPopupController : MonoBehaviour
{
    [Header("UI")]
    public Button quitButton;

    private OverworldCoinGameLauncher launcher;

    void Awake()
    {
        // Find the launcher in the current scene (assumes only one)
        launcher = FindObjectOfType<OverworldCoinGameLauncher>();

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Hide popup on start (will be shown when OpenCoinFlipPopup is called)
        gameObject.SetActive(false);
    }

    public void OnQuitClicked()
    {
        if (launcher != null)
            launcher.CloseCoinFlipPopup();
        else
            gameObject.SetActive(false); // fallback: just hide it
    }
}
