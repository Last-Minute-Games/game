using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class Startscreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    
    private CanvasGroup _fadeCanvasGroup;
    private CanvasGroup _logoCanvasGroup;
    
    // Drag your main menu Canvas (the one containing Play/Settings/Credits/Quit) here
    [SerializeField] private GraphicRaycaster mainMenuRaycaster;
    
    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    
    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    private void FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        StartCoroutine(FadeCoroutine(canvasGroup, startAlpha, endAlpha, duration));
    }
    private IEnumerator LogoStartup()
    {
        // Disable the main menu raycaster to block all clicks/hover on that canvas while the logo plays
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = false;

        // Make sure the logo overlay is active and catching input
        _logoCanvasGroup.gameObject.SetActive(true);
        _logoCanvasGroup.blocksRaycasts = true;   // <- blocks all UI beneath
        _logoCanvasGroup.interactable = true;   // if you have a Skip button, etc.
        _logoCanvasGroup.alpha = 0f;

        // Fade logo in
        yield return StartCoroutine(FadeCoroutine(_logoCanvasGroup, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(3f); // your logo hold time (no need to add fadeDuration again)

        // Fade logo out
        yield return StartCoroutine(FadeCoroutine(_logoCanvasGroup, 1f, 0f, fadeDuration));

        // Stop blocking and hide
        _logoCanvasGroup.blocksRaycasts = false;
        _logoCanvasGroup.interactable = false;
        _logoCanvasGroup.gameObject.SetActive(false);

        // While the screen is black, also block input on the fade CG
        _fadeCanvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeCoroutine(_fadeCanvasGroup, 1f, 0f, 2f));
        _fadeCanvasGroup.blocksRaycasts = false;

        // Re-enable menu input and UI audio
        if (mainMenuRaycaster) mainMenuRaycaster.enabled = true;
        UIButtonFX.globalAudioEnabled = true;
    }

    void Start()
    {
        playButton = GameObject.Find("PlayButton");
        quitButton = GameObject.Find("QuitButton");
        settingsButton = GameObject.Find("SettingsButton");
        creditsButton = GameObject.Find("CreditsButton");
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 1f; // Start transparent
        
        _logoCanvasGroup = GameObject.Find("LogoCanvasGroup").GetComponent<CanvasGroup>();
        _logoCanvasGroup.alpha = 0f; // Start transparent

        // Suppress UI sounds during logo loading and suppress clicks on menu buttons
        UIButtonFX.globalAudioEnabled = false; // disable hover/click audio while loading logo
        UIButtonFX.suppressClickInMainMenu = true; // ensure clicks in main menu don't play click sounds
        
        StartCoroutine(LogoStartup());
    }

    public void StartGame()
    {
        StartCoroutine(FadeAndLoad());
    }
    
    private IEnumerator FlickerButton(GameObject button, float interval)
    {
        bool visible = true;

        // run until the fade finishes (you can break when alpha reaches 1)
        while (_fadeCanvasGroup.alpha < 1f)
        {
            visible = !visible;
            button.SetActive(visible);
            yield return new WaitForSeconds(interval);
        }

        // make sure it's visible again at the end (optional)
        button.SetActive(true);
    }

    private IEnumerator FadeAndLoad()
    {
        quitButton.SetActive(false);
        settingsButton.SetActive(false);
        creditsButton.SetActive(false); 
        playButton.SetActive(false);
        
        // start flickering the play button
        StartCoroutine(FlickerButton(playButton, 0.3f));

        // Fade to black
        yield return StartCoroutine(FadeCoroutine(_fadeCanvasGroup, 0f, 1f, fadeDuration * 2.5f));

        // Load the next scene asynchronously while screen is black
        AsyncOperation op = SceneManager.LoadSceneAsync("NewTutorial");
        op.allowSceneActivation = true; // or set false if you want to gate activation

        // Optionally wait until load is done (it’s already black)
        while (!op.isDone)
            yield return null;

        // If you want to fade back in *after* the new scene is ready,
        // you’ll need a fade canvas in the new scene too, or mark this object as persistent:
        // DontDestroyOnLoad(gameObject);  (then manage the fade-out there)
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
                // This makes the stop button in the editor work properly
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}


