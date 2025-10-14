using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [Header("UI")]
    public Image fadePanel;
    public float fadeDuration = 2f;

    [Header("Audio")]
    public AudioClip transitionClip; // drag MP3 or M4A here
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource audioSource;
    private static bool hasFadedIn = false; // prevents double fade on reloads
    private bool audioPlayed = false; // track if audio has played

    private void Awake()
    {
        if (fadePanel == null)
        {
            Debug.LogError("ScreenFader: Fade panel not assigned!");
            return;
        }

        // Setup Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = transitionClip;
        audioSource.volume = volume;

        // Keep this fader alive between scenes
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start fully black for the first scene
        fadePanel.color = new Color(0, 0, 0, 1);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasFadedIn)
        {
            hasFadedIn = true;
            StartCoroutine(FadeIn());
        }
    }

    // Fade to black smoothly
    public IEnumerator FadeOut()
    {
        if (!audioPlayed && transitionClip != null)
        {
            audioSource.Play();
            audioPlayed = true;
        }

        float t = 0f;
        Color c = fadePanel.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
    }

    // Fade from black to transparent
    public IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = fadePanel.color;
        c.a = 1f;
        fadePanel.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.SmoothStep(1f, 0f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        audioPlayed = false; // reset for next transition
    }

    public void SetPanelAlpha(float alpha)
    {
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = Mathf.Clamp01(alpha);
            fadePanel.color = c;
        }
    }

    // Fade out → Load next scene → Auto fade in
    public IEnumerator TransitionToScene(string nextSceneName)
    {
        hasFadedIn = false; // ensures next scene fades in again

        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;
    }
}
