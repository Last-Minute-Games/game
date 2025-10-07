using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class Startscreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    private CanvasGroup _fadeCanvasGroup;
    public GameObject playButton;
    void Start()
    {
        _fadeCanvasGroup = GameObject.Find("FadeCanvasGroup").GetComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 0f; // Start transparent
    }

    public void StartGame()
    {
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            _fadeCanvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }
        _fadeCanvasGroup.alpha = 1f;
        SceneManager.LoadScene("Overworld");
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


