using UnityEngine;
using UnityEngine.UI;   // Needed for Button

public class MinigameWinUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;       // Fullscreen panel with "You Win" text + button
    public Button continueButton;     // "Continue" button

    void Awake()
    {
        if (winPanel != null)
            winPanel.SetActive(false);   // Hidden at start

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    // Call this from your WinConditionManager when the puzzle is solved
    public void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        // Optional: stop Sokoban movement while popup is open
        if (MinigameController.Instance != null &&
            MinigameController.Instance.sokobanPlayerScript != null)
        {
            MinigameController.Instance.sokobanPlayerScript.enabled = false;
        }
    }

    private void OnContinueClicked()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        // Use YOUR existing transition + exit logic:
        // EndSokoban(bool solved)
        if (MinigameController.Instance != null)
        {
            MinigameController.Instance.EndSokoban(true);
        }
    }
}
