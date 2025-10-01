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
        playButton.SetActive(false); // Hide Play button
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
        SceneManager.LoadSceneAsync("Overworld");
    }
}