using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;     // main pause panel (buttons)
    public GameObject settingsPanel;  // your Settings panel (can be the prefab you've built)

    [Header("Focus")]
    public GameObject firstSelected;  // e.g., Resume button for controller users

    void OnEnable()
    {
        if (firstSelected) EventSystem.current.SetSelectedGameObject(firstSelected);
        ShowPausePanel(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel && settingsPanel.activeSelf)
            {
                // from Settings back to Pause panel
                ShowSettings(false);
                ShowPausePanel(true);
                if (firstSelected) EventSystem.current.SetSelectedGameObject(firstSelected);
            }
            else
            {
                // toggle global pause
                PauseManager.I.TogglePause();
                gameObject.SetActive(PauseManager.I.IsPaused);
            }
        }
    }

    // Button hooks
    public void OnResume() { PauseManager.I.Resume(); gameObject.SetActive(false); }
    public void OnOpenSettings() { ShowPausePanel(false); ShowSettings(true); }
    public void OnBackFromSettings() { ShowSettings(false); ShowPausePanel(true); }
    public void OnQuitToMenu(string menuScene) { PauseManager.I.Resume(); UnityEngine.SceneManagement.SceneManager.LoadScene(menuScene); }

    void ShowPausePanel(bool on) { if (pausePanel) pausePanel.SetActive(on); }
    void ShowSettings(bool on) { if (settingsPanel) settingsPanel.SetActive(on); }
}
