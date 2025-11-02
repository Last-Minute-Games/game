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
    public AudioClip transitionClip;
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource audioSource;
    private bool audioPlayed = false;

    private void Awake()
    {
        if (fadePanel == null)
        {
            Debug.LogError("ScreenFader: Fade panel not assigned!");
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = transitionClip;
        audioSource.volume = volume;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        fadePanel.color = new Color(0, 0, 0, 1f); // fully black at game start
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in automatically when a new scene loads
        StartCoroutine(FadeIn());
    }

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

        fadePanel.color = Color.black;
    }

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

        fadePanel.color = new Color(0, 0, 0, 0f);
        audioPlayed = false;
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

    public IEnumerator TransitionToScene(string nextScene)
    {
        yield return StartCoroutine(FadeOut());
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
            yield return null;
    }
}
