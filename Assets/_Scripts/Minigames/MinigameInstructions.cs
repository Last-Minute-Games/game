using UnityEngine;

public class MinigameInstructions : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPanel;

    [Tooltip("Show automatically the first time this popup opens?")]
    [SerializeField] private bool showOnFirstOpen = true;

    bool _hasShownOnce = false;

    // This will be called by the launcher when the minigame popup is opened
    public void OnPopupOpened()
    {
        if (instructionsPanel == null) return;

        if (showOnFirstOpen && !_hasShownOnce)
        {
            _hasShownOnce = true;
            Show();
        }
        else
        {
            // After the first time, start with instructions hidden
            Hide();
        }
    }

    public void Show()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void Hide()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    // Hook this to the ? button
    public void OnHelpButtonClicked()
    {
        Show();
    }

    // Hook this to the "Got it" button
    public void OnGotItButtonClicked()
    {
        Hide();
    }
}
