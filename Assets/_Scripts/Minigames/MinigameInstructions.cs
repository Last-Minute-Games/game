using UnityEngine;

public class MinigameInstructions : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private bool showOnFirstOpen = true;

    bool _hasShownOnce = false;

    void OnEnable()
    {
        if (showOnFirstOpen && !_hasShownOnce)
        {
            _hasShownOnce = true;
            Show();
        }
        else
        {
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
