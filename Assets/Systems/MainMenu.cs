using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class Startscreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    
    private CanvasGroup _fadeCanvasGroup;
    private CanvasGroup _logoCanvasGroup;
    
    public GameObject playButton;
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
        yield return new WaitForSeconds(1f);

        FadeCanvasGroup(_logoCanvasGroup, 0f, 1f, fadeDuration);
        
        _logoCanvasGroup.alpha = 1f;
        
        yield return new WaitForSeconds(fadeDuration + 3f);
        
        FadeCanvasGroup(_logoCanvasGroup, 1f, 0f, fadeDuration);
        
        yield return new WaitForSeconds(fadeDuration + 0.5f);
        _logoCanvasGroup.gameObject.SetActive(false);
        
        FadeCanvasGroup(_fadeCanvasGroup, 1f, 0f, 2f);
        
    }
    
    void Start()
    {
        playButton = GameObject.Find("PlayButton");
        quitButton = GameObject.Find("QuitButton");
        
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 1f; // Start transparent
        
        _logoCanvasGroup = GameObject.Find("LogoCanvasGroup").GetComponent<CanvasGroup>();
        _logoCanvasGroup.alpha = 0f; // Start transparent
        
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
        playButton.SetActive(false);
        
        // start flickering the play button
        StartCoroutine(FlickerButton(playButton, 0.3f));

        // Fade to black
        yield return StartCoroutine(FadeCoroutine(_fadeCanvasGroup, 0f, 1f, fadeDuration * 2.5f));

        // Load the next scene asynchronously while screen is black
        AsyncOperation op = SceneManager.LoadSceneAsync("Overworld");
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


