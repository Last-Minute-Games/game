using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class ScreenFadeUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI resultText;

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    
    [Header("Round Transition Settings")]
    public float roundTransitionFadeInTime = 0.5f;
    public float roundTransitionHoldTime = 1.0f;
    public float roundTransitionFadeOutTime = 0.5f;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowMessage(string message, Color color)
    {
        resultText.text = message;
        resultText.color = color;

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, fadeDuration);
    }
    
    public void HideMessage()
    {
        canvasGroup.DOFade(0f, fadeDuration);
    }

    /// <summary>
    /// Shows a wave transition (e.g., "WAVE 1", "WAVE 2") with fade in/out effect.
    /// If isFirstWave is true, it starts fully dark (no fade in), then only fades out.
    /// </summary>
    public IEnumerator ShowRoundTransition(int waveNumber, bool isFirstWave = false)
    {
        string message = $"WAVE {waveNumber}";
        Color color = new Color(1f, 0.5f, 0f); // Orange for waves
        
        resultText.text = message;
        resultText.color = color;

        if (isFirstWave)
        {
            // 🔴 FIRST WAVE: start fully dark, no fade in
            canvasGroup.alpha = 1f; // fully dark already

            // Hold while fully dark
            yield return new WaitForSeconds(roundTransitionHoldTime);

            // Fade out to reveal the scene
            canvasGroup.DOFade(0f, roundTransitionFadeOutTime);
            yield return new WaitForSeconds(roundTransitionFadeOutTime);
        }
        else
        {
            // 🔁 NORMAL WAVES: fade in → hold → fade out
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, roundTransitionFadeInTime);
            yield return new WaitForSeconds(roundTransitionFadeInTime);

            // Hold briefly while dark
            yield return new WaitForSeconds(roundTransitionHoldTime);

            // Fade out as the wave starts
            canvasGroup.DOFade(0f, roundTransitionFadeOutTime);
            yield return new WaitForSeconds(roundTransitionFadeOutTime);
        }
    }
}
